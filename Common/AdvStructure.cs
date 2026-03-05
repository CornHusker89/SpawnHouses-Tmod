using System;
using System.Collections.Generic;
using System.Reflection;
using SpawnHouses.Common.Modules;
using SpawnHouses.Common.Palette;
using SpawnHouses.Common.Parameters;
using SpawnHouses.Common.Tiles;
using SpawnHouses.Common.Types;
using Terraria;
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

    /// <summary>
    ///     this random generator should only be used for things that ARE related to layouts
    /// </summary>
    public readonly UnifiedRandom LayoutRandom;

    /// <summary>
    ///     this random generator should only be used for things that are NOT related to layouts
    /// </summary>
    public readonly UnifiedRandom OtherRandom;
    
    public readonly int Seed;
    public StructureLayout StructureLayout;
    public StructureLayoutParams LayoutParam;
    public TilePalette Palette;
    public StructureTilemap Tilemap;

    /// <summary>
    /// </summary>
    /// <param name="layoutParam"></param>
    /// <param name="palette"></param>
    /// <param name="seed">if -1, creates a new random seed from the base terraria random generator</param>
    /// <param name="generate">
    ///     if true, will call <see cref="ApplyLayoutMethod" />, <see cref="FillComponents" /> and
    ///     <see cref="PlaceTilemap" />
    /// </param>
    public AdvStructure(StructureLayoutParams layoutParam, TilePalette palette, int seed = -1, bool generate = true) {
        Seed = seed == -1 ? WorldGen.genRand.Next() : seed;
        LayoutRandom = new UnifiedRandom(Seed);
        OtherRandom = new UnifiedRandom(Seed + 1);
        LayoutParam = layoutParam;
        LayoutParam.Structure = this;
        Palette = palette;
        StructureManager.StructureList.Add(this);
        if (generate) {
            ApplyLayoutMethod();
            FillComponents();
            PlaceTilemap();
        }
    }
    
    /// <summary>
    ///     calculates a structure's layout but does not apply component generators
    /// </summary>
    /// <param name="generator">layout generator to be used. leave null for a random method</param>
    public void ApplyLayoutMethod(StructureLayoutGenerator generator = null) {
        if (StructureLayout != null) throw new Exception("this AdvStructure already has a layout set");

        StructureLayout = new StructureLayout(LayoutParam);
        StructureLayout.ExecuteGenerator();
    }

    /// <summary>
    ///     Fills current layout with tiles/walls
    /// </summary>
    /// <exception cref="Exception">Throws when no layout has been set</exception>
    public void FillComponents() {
        if (StructureLayout == null)
            throw new Exception("No layout has been set");

        foreach (IComponent component in StructureLayout.AllComponents)
            component.ExecuteGenerator();
    }

    /// <summary>
    ///     paste tiles from adv structure's tilemap into game tilemap
    /// </summary>
    public void PlaceTilemap() {
        if (StructureLayout == null)
            throw new Exception("No layout has been set");

        Tilemap.ApplyTilemap();
    }

    #region Generators

    public static void LoadGenerators(Assembly assembly) {
        var pluginTypes = assembly.GetTypes();
        foreach (Type type in pluginTypes) {
            ModuleGenerator moduleInfo = type.GetCustomAttribute<ModuleGenerator>();

            if (moduleInfo != null) {
                if (!InstanceGenerators.TryGetValue(moduleInfo.ModuleType, out var generatorList))
                    InstanceGenerators[moduleInfo.ModuleType] = generatorList = [];
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
public class ModuleGenerator(Type moduleType) : Attribute {
    public readonly Type ModuleType = moduleType;
}