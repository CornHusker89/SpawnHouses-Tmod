using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using SpawnHouses.AdvStructures.AdvStructureParts;
using SpawnHouses.AdvStructures.Generation;
using SpawnHouses.Helpers;
using SpawnHouses.Structures;
using SpawnHouses.Types;
using Terraria;

namespace SpawnHouses.AdvStructures;

public class AdvStructure {
    public ExternalLayout ExternalLayout;
    public bool HasFilledComponents;
    public RoomLayout Layout;
    public StructureParams Params;
    public StructureTilemap Tilemap;

    /// <summary>
    /// </summary>
    /// <param name="structureParams"></param>
    /// <param name="generate">
    ///     if true, will call <see cref="ApplyLayoutMethod" />, <see cref="FillComponents" /> and
    ///     <see cref="PlaceTilemap" />
    /// </param>
    public AdvStructure(StructureParams structureParams, bool generate = true) {
        Params = structureParams;
        if (generate) {
            ApplyLayoutMethod();
            FillComponents();
            FillFurniture();
            PlaceTilemap();
        }
    }

    public bool HasLayout => Layout != null;

    /// <summary>
    ///     calculates a structure's layout but does not apply component generators
    /// </summary>
    /// <param name="generator">layout generator to be used. leave null for a random method</param>
    public void ApplyLayoutMethod(IStructureLayoutGenerator generator = null) {
        TagUtils.ValidateTagDataTypes(Params.TagsRequired);
        if (generator == null) {
            var validGenerators = StructureLayoutGenerators.Where(gen => gen.CanGenerate(Params)
                                                                         && Params.TagsRequired.Keys.ToHashSet().IsSubsetOf(gen.GetPossibleTags())
                                                                         && !Params.TagsBlocklist.Overlaps(gen.GetPossibleTags()))
                .ToArray();

            if (validGenerators.Length == 0) throw new Exception($"No structure layout generators found were compatible with the given parameters. required tags: {EnumHelper.ToString(Params.TagsRequired)}, blocklisted tags: {EnumHelper.ToString(Params.TagsBlocklist)}");
            generator = Terraria.WorldGen.genRand.NextFromList(validGenerators);
        }

        bool result = generator.Generate(this);
        if (!result) throw new Exception("error in structure layout generator");
    }

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

    private ComponentGenerator GetComponentGenerator(ComponentParams componentParams, List<ComponentGenerator> generators) {
        var validGenerators = generators.Where(gen => gen.CanGenerate(componentParams)
                                                      && componentParams.Component.TagsRequired.Keys.ToHashSet().IsSubsetOf(gen.PossibleTags)
                                                      && !componentParams.Component.TagsBlocklist.Overlaps(gen.PossibleTags))
            .ToArray();

        if (validGenerators.Length == 0)
            throw new Exception($"No component generators were found that are compatible with given parameters. type: {componentParams.Component.GetType().FullName}, required tags: {EnumHelper.ToString(componentParams.Component.TagsRequired)}, blocklisted tags: {EnumHelper.ToString(componentParams.Component.TagsBlocklist)}");

        return Terraria.WorldGen.genRand.NextFromList(validGenerators);
    }

    /// <summary>
    ///     assigns room objects to the gaps in the external layout
    /// </summary>
    public void CompleteExternalGaps() {
        foreach (Gap gap in ExternalLayout.Gaps) gap.LowerRoom = RoomLayoutHelper.GetClosestRoom(Layout.Rooms, gap.Volume.Center);
    }

    /// <summary>
    ///     Fills current layout with tiles/walls
    /// </summary>
    /// <exception cref="Exception">Throws when no layout has been set</exception>
    public void FillComponents() {
        if (!HasLayout)
            throw new Exception("No layout has been set");

        List<Component> components = [];
        components.AddRange(ExternalLayout.Floors);
        components.AddRange(ExternalLayout.Walls);
        components.AddRange(ExternalLayout.Gaps);
        components.AddRange(ExternalLayout.Roofs);
        components.AddRange(Layout.Floors);
        components.AddRange(Layout.Walls);
        components.AddRange(Layout.Gaps);
        components.AddRange(Layout.Rooms);

        Dictionary<Type, List<ComponentGenerator>> generatorQueue = [];
        for (int i = 0; i < components.Count; i++) {
            Component component = components[i];
            ComponentUtils.ValidateComponent(component);
            ComponentParams componentParams = ComponentUtils.CreateComponentParamsForType(component, Params.Palette, Tilemap);
            component.Id = (ushort)i;

            int generatorIndex = 0;
            Type componentType = component.GetType();
            if (!ComponentGenerators.TryGetValue(componentType, out var generators)) throw new Exception($"component type {component.GetType().FullName} generators not found");
            if (!generatorQueue.TryGetValue(componentType, out var generatorList)) {
                generatorList = [GetComponentGenerator(componentParams, generators)];
                generatorQueue[componentType] = generatorList;
            }
            else {
                while (!generatorList[generatorIndex].CanGenerate(componentParams)) {
                    generatorIndex++;
                    if (generatorIndex >= generatorQueue.Count) generatorList.Add(GetComponentGenerator(componentParams, generators));
                }
            }

            ComponentGenerator generator = generatorList[generatorIndex];
            component.GeneratorId = (generator.GetType().FullName ?? generator.GetType().Name).GetHashCode();
            generator.Generate(componentParams);
        }

        HasFilledComponents = true;
    }

    /// <summary>
    ///     paste tiles from this tilemap into game tilemap 
    /// </summary>
    public void PlaceTilemap() {
        if (!HasFilledComponents)
            throw new Exception("No filled components have been set");

        Tilemap.ApplyTilemap();
    }

    public void FillFurniture() {
    }

    #region Generators

    public static readonly List<IStructureLayoutGenerator> StructureLayoutGenerators = [];

    /// <summary>type corresponds to the final component's type</summary>
    public static readonly Dictionary<Type, List<ComponentGenerator>> ComponentGenerators = new();

    internal static void LoadGenerators() {
        LoadGenerators(Assembly.GetExecutingAssembly());
    }

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
                ComponentGenerator instance = (ComponentGenerator)Activator.CreateInstance(type);
                if (type.GetField("PossibileTags")?.GetValue(instance) == null)
                    throw new Exception($"property \"PossibleTags\" must be set for the component generator \"{type.FullName}\"");
                generatorList.Add(instance);
            }

            if (structureLayoutInfo != null) {
                if (!type.IsAssignableFrom(typeof(IStructureLayoutGenerator)))
                    throw new Exception($"structure layout generator \"{type.FullName}\" must derive from {nameof(IStructureLayoutGenerator)}");
                IStructureLayoutGenerator instance = (IStructureLayoutGenerator)Activator.CreateInstance(type);
                if (type.GetField("PossibileTags")?.GetValue(instance) == null)
                    throw new Exception($"property \"PossibleTags\" must be set for the structure layout generator \"{type.FullName}\"");
                StructureLayoutGenerators.Add(instance);
            }
        }
    }

    #endregion
}

[AttributeUsage(AttributeTargets.Class)]
public class ComponentGeneratorAttribute(Type componentType) : Attribute {
    public readonly Type ComponentType = componentType;
}

[AttributeUsage(AttributeTargets.Class)]
public class StructureLayoutGeneratorAttribute : Attribute;