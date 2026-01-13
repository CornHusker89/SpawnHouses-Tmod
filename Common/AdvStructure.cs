using System;
using System.Collections.Generic;
using System.Reflection;
using SpawnHouses.Common.Modules;
using SpawnHouses.Common.Modules.Components;
using SpawnHouses.Common.Parameters;
using SpawnHouses.Common.Tagging;
using SpawnHouses.Common.Tiles;
using SpawnHouses.Common.Types;
using SpawnHouses.Types.Palette;
using Terraria.Utilities;
using IComponent = SpawnHouses.Common.Modules.IComponent;

namespace SpawnHouses.Common;

/// <summary>
///     the central object that everything for adv. structures revolve around
/// </summary>
public class AdvStructure {
    /// <summary>type corresponds to the final component's type</summary>
    public static readonly Dictionary<Type, List<IGenerator>> InstanceGenerators = new();

    public readonly Dictionary<Type, List<IGenerator>> InstanceGeneratorQueue = [];
    public readonly UnifiedRandom RandomGen;
    public readonly int Seed;

    public StructureLayout StructureLayout;

    public StructureLayoutParams LayoutParam;
    public TagMap TagsRequired;
    public TilePalette Palette;
    public StructureTilemap Tilemap;

    /// <summary>
    /// </summary>
    /// <param name="layoutParam"></param>
    /// <param name="tagsRequired"></param>
    /// <param name="palette"></param>
    /// <param name="seed">if -1, will create a new random seed from the base terraria random generator</param>
    /// <param name="generate">
    ///     if true, will call <see cref="ApplyLayoutMethod" />, <see cref="FillComponents" /> and
    ///     <see cref="PlaceTilemap" />
    /// </param>
    public AdvStructure(StructureLayoutParams layoutParam, TagMap tagsRequired, TilePalette palette, int seed = -1, bool generate = true) {
        Seed = seed == -1 ? Terraria.WorldGen.genRand.Next() : seed;
        RandomGen = new UnifiedRandom(Seed);
        LayoutParam = layoutParam;
        TagsRequired = tagsRequired;
        Palette = palette;
        if (generate) {
            ApplyLayoutMethod();
            FillComponents();
            PlaceTilemap();
        }
    }

    public bool HasSetComponents { get; private set; }

    /// <summary>
    ///     assigns room objects to the gaps in the external layout
    /// </summary>
    public void CompleteExternalGaps() {
        foreach (Gap gap in StructureLayout.Gaps)
            gap.LowerRoom = RoomLayoutHelper.GetClosestRoom(RoomSections, gap.Geometry.Center);
    }

    /// <summary>
    ///     calculates a structure's layout but does not apply component generators
    /// </summary>
    /// <param name="generator">layout generator to be used. leave null for a random method</param>
    public void ApplyLayoutMethod(StructureLayoutGenerator generator = null) {
        if (HasSetComponents) throw new Exception("this AdvStructure already has a layout set");

        StructureLayout = new StructureLayout(LayoutParam);
        StructureLayout.SetGenerator();
        StructureLayout.TagsCurrent.AddRange(StructureLayout.Generator.Generate(LayoutParam));

        Components = [];
        Components.AddRange(StructureLayout.Floors);
        Components.AddRange(StructureLayout.Walls);
        Components.AddRange(StructureLayout.Gaps);
        Components.AddRange(StructureLayout.Roofs);
        foreach (RoomLayout roomLayout in RoomSections) {
            Components.AddRange(roomLayout.Floors);
            Components.AddRange(roomLayout.Walls);
            Components.AddRange(roomLayout.Gaps);
        }

        // fill rooms last because the furniture needs to be placed specifically
        foreach (RoomLayout roomLayout in RoomSections) Components.AddRange(roomLayout.Rooms);

        // set component generators
        foreach (IComponent component in Components) {
            component.SetGenerator();
        }
    }

    /// <summary>
    ///     Fills current layout with tiles/walls
    /// </summary>
    /// <exception cref="Exception">Throws when no layout has been set</exception>
    public void FillComponents() {
        if (!HasSetComponents)
            throw new Exception("No layout has been set");

        foreach (IComponent component in Components)
            component.TagsCurrent.AddRange(component.Generator.Generate(component.Params));
    }

    /// <summary>
    ///     paste tiles from adv structure's tilemap into game tilemap
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
            InstanceGenerator instanceInfo = type.GetCustomAttribute<InstanceGenerator>();

            if (instanceInfo != null) {
                if (!InstanceGenerators.TryGetValue(instanceInfo.InstanceType, out var generatorList))
                    InstanceGenerators[instanceInfo.InstanceType] = generatorList = [];
                generatorList.Add((IGenerator)Activator.CreateInstance(type));
            }
        }
    }

    internal static void LoadGenerators() {
        LoadGenerators(Assembly.GetExecutingAssembly());
    }

    #endregion
}

[AttributeUsage(AttributeTargets.Class)]
public class InstanceGenerator(Type instanceType) : Attribute {
    public readonly Type InstanceType = instanceType;
}