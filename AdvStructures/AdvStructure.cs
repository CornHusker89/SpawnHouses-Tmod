using System;
using System.Collections.Generic;
using System.Reflection;
using SpawnHouses.AdvStructures.AdvStructureParts;
using SpawnHouses.AdvStructures.Generation;
using SpawnHouses.Helpers;
using SpawnHouses.Types;
using SpawnHouses.Types.Palette;
using SpawnHouses.Types.TagTypes;
using Terraria.Utilities;

namespace SpawnHouses.AdvStructures;

public class AdvStructure : Generatable<StructureTag, StructureParams, StructureLayoutGenerator> {
    public static readonly List<StructureLayoutGenerator> StructureLayoutGenerators = [];

    /// <summary>type corresponds to the final component's type</summary>
    public static readonly Dictionary<Type, List<ComponentGenerator>> ComponentGenerators = new();

    public readonly UnifiedRandom RandomGen;

    protected readonly int Seed;
    public List<IComponent> Components;

    public ExternalLayout ExternalLayout;
    public RoomLayout Layout;
    public TilePalette Palette;
    public StructureTilemap Tilemap;

    /// <summary>
    /// </summary>
    /// <param name="param"></param>
    /// <param name="palette"></param>
    /// <param name="seed">if -1, will create a new random seed from the base terraria random generator</param>
    /// <param name="generate">
    ///     if true, will call <see cref="ApplyLayoutMethod" />, <see cref="FillComponents" /> and
    ///     <see cref="PlaceTilemap" />
    /// </param>
    public AdvStructure(StructureParams param, TilePalette palette, int seed = -1, bool generate = true) : base(param) {
        Seed = seed == -1 ? Terraria.WorldGen.genRand.Next() : seed;
        RandomGen = new UnifiedRandom(Seed);
        Palette = palette;
        if (generate) {
            ApplyLayoutMethod();
            FillComponents();
            PlaceTilemap();
        }
    }

    public bool HasSetComponents { get; private set; }

    /// <summary>
    ///     sets the outside/exterior component/inside tile data within the tilemap, based on the current external layout
    /// </summary>
    public void SetTilesExternalStatus() {
        foreach (Shape shape in ExternalLayout.Floors.Select(floor => floor.Volume))
            shape.ExecuteInArea((x, y) => {
                StructureTile tile = Tilemap[x, y];
                tile.IsExteriorComponent = true;
                tile.IsFloor = true;
            });

        foreach (Shape shape in ExternalLayout.Walls.Select(wall => wall.Volume))
            shape.ExecuteInArea((x, y) => {
                StructureTile tile = Tilemap[x, y];
                tile.IsExteriorComponent = true;
                tile.IsWall = true;
            });

        foreach (Shape shape in ExternalLayout.Gaps.Select(gap => gap.Volume))
            shape.ExecuteInArea((x, y) => {
                StructureTile tile = Tilemap[x, y];
                tile.IsExteriorComponent = true;
                tile.IsGap = true;
            });

        SearchOutside(0, 0);
        for (int x = 0; x < Tilemap.Width; x++)
        for (int y = 0; y < Tilemap.Height; y++) {
            StructureTile tile = Tilemap[x, y];
            if (!tile.IsOutside && !tile.IsExteriorComponent) tile.IsInside = true;
        }

        return;

        void SearchOutside(int x, int y) {
            StructureTile thisTile = Tilemap[x, y];
            thisTile.IsOutside = true;
            thisTile.IsNullTile = true;
            thisTile.IsNullWall = true;

            foreach ((int dx, int dy) in ((int, int)[]) [(1, 0), (-1, 0), (0, 1), (0, -1)]) {
                if (!Tilemap.InBounds(x + dx, y + dy)) continue;
                StructureTile nextTile = Tilemap[x + dx, y + dy];
                if (nextTile.IsOutside || nextTile.IsExteriorComponent) continue;

                SearchOutside(x + dx, y + dy);
            }
        }
    }

    /// <summary>
    ///     assigns room objects to the gaps in the external layout
    /// </summary>
    public void CompleteExternalGaps() {
        foreach (Gap gap in ExternalLayout.Gaps)
            gap.LowerRoom = RoomLayoutHelper.GetClosestRoom(Layout.Rooms, gap.Volume.Center);
    }

    /// <summary>
    ///     calculates a structure's layout but does not apply component generators
    /// </summary>
    /// <param name="generator">layout generator to be used. leave null for a random method</param>
    public void ApplyLayoutMethod(StructureLayoutGenerator generator = null) {
        if (HasSetComponents) throw new Exception("this AdvStructure already has a layout set");

        generator ??= GetGenerator(StructureLayoutGenerators);
        Tags.AddCurrentTags(generator.Generate(Params));

        List<IComponent> components = [];
        components.AddRange(ExternalLayout.Floors);
        components.AddRange(ExternalLayout.Walls);
        components.AddRange(ExternalLayout.Gaps);
        // components.AddRange(ExternalLayout.Roofs);
        components.AddRange(Layout.Floors);
        components.AddRange(Layout.Walls);
        components.AddRange(Layout.Gaps);
        components.AddRange(Layout.Rooms); // fill rooms last because the furniture needs to be placed specifically

        // set component generators
        Dictionary<Type, List<ComponentGenerator>> generatorQueue = [];
        for (int i = 0; i < components.Count; i++) {
            IComponent component = components[i];
            component.Id = (short)i;
            ComponentParams componentParams = ComponentUtils.CreateParamsForComponent(component, this);
            Components.Add(component);

            int generatorIndex = 0;
            Type componentType = component.GetType();
            if (!ComponentGenerators.TryGetValue(componentType, out var generators)) throw new Exception($"component type {component.GetType().FullName} generators not found");
            if (!generatorQueue.TryGetValue(componentType, out var generatorList)) {
                generatorList = [component.GetGenerator(componentParams, generators)];
                generatorQueue[componentType] = generatorList;
            }
            else {
                while (!generatorList[generatorIndex].CanGenerate(componentParams)) {
                    generatorIndex++;
                    if (generatorIndex >= generatorQueue.Count) generatorList.Add(component.GetGenerator(componentParams, generators));
                }
            }

            component.Generator = generatorList[generatorIndex];
        }

        HasSetComponents = true;
    }

    /// <summary>
    ///     Fills current layout with tiles/walls
    /// </summary>
    /// <exception cref="Exception">Throws when no layout has been set</exception>
    public void FillComponents() {
        if (!HasSetComponents)
            throw new Exception("No layout has been set");

        foreach (Component component in Components)
            component.AddCurrentTags(component.Generator.Generate(component.Params));
    }

    /// <summary>
    ///     paste tiles from this tilemap into game tilemap
    /// </summary>
    public void PlaceTilemap() {
        if (!HasSetComponents)
            throw new Exception("No layout has been set");

        Tilemap.ApplyTilemap();
    }

    #region Generators

    public static void LoadGenerators(Assembly assembly) {
        var pluginTypes = assembly.GetTypes();
        foreach (Type type in pluginTypes) {
            ComponentGeneratorAttribute componentInfo = type.GetCustomAttribute<ComponentGeneratorAttribute>();
            StructureLayoutGeneratorAttribute structureLayoutInfo = type.GetCustomAttribute<StructureLayoutGeneratorAttribute>();

            if (componentInfo != null && structureLayoutInfo != null) throw new Exception($"{type.FullName} has both structure layout and component generator attributes, which are mutually exclusive");

            if (componentInfo != null) {
                if (!type.IsSubclassOf(typeof(VolumeComponentGenerator)) && !type.IsSubclassOf(typeof(PathComponentGenerator)))
                    throw new Exception($"component generator \"{type.FullName}\" must derive from either {nameof(VolumeComponentGenerator)} or {nameof(PathComponentGenerator)}");
                if (!ComponentGenerators.TryGetValue(componentInfo.ComponentType, out var generatorList))
                    ComponentGenerators[componentInfo.ComponentType] = generatorList = [];
                generatorList.Add((ComponentGenerator)Activator.CreateInstance(type));
            }

            if (structureLayoutInfo != null) {
                if (!typeof(StructureLayoutGenerator).IsAssignableFrom(type))
                    throw new Exception($"structure layout generator \"{type.FullName}\" must derive from {nameof(StructureLayoutGenerator)}");
                StructureLayoutGenerators.Add((StructureLayoutGenerator)Activator.CreateInstance(type));
            }
        }
    }

    internal static void LoadGenerators() {
        LoadGenerators(Assembly.GetExecutingAssembly());
    }

    #endregion
}

[AttributeUsage(AttributeTargets.Class)]
public class ComponentGeneratorAttribute(Type componentType) : Attribute {
    public readonly Type ComponentType = componentType;
}

[AttributeUsage(AttributeTargets.Class)]
public class StructureLayoutGeneratorAttribute : Attribute;