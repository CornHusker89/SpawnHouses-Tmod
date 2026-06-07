using System;
using System.Collections.Generic;
using SpawnHouses.Common.Debug;
using SpawnHouses.Common.Modules;
using SpawnHouses.Common.Palette;
using SpawnHouses.Common.Parameters;
using SpawnHouses.Common.Tiles;
using SpawnHouses.Common.Types;
using Terraria;
using Terraria.ModLoader;
using Terraria.Utilities;
using IComponent = SpawnHouses.Common.Modules.IComponent;

namespace SpawnHouses.Common;

/// <summary>
///     the central object that everything for adv. structures revolve around
/// </summary>
public class AdvStructure : ICanDebugDraw {
    
    public DebugInfoLevel DebugInfoVisibility { get; set; }
    public string Name { get; init; }
    
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
    public bool FailedLayoutGeneration;

    /// <summary>
    /// </summary>
    /// <param name="name"></param>
    /// <param name="layoutParam"></param>
    /// <param name="palette"></param>
    /// <param name="seed">if -1, creates a new random seed from the normal terraria random generator</param>
    /// <param name="generate">
    ///     if true, will call <see cref="ApplyLayoutMethod" />, <see cref="FillComponents" />,
    ///     <see cref="Register"/>, and <see cref="PlaceTilemap" /> 
    /// </param>
    public AdvStructure(string name, StructureLayoutParams layoutParam, TilePalette palette, int seed = -1, bool generate = true) {
        Name = name;
        
        Seed = seed == -1 ? WorldGen.genRand.Next() : seed;
        LayoutRandom = new UnifiedRandom(Seed);
        OtherRandom = new UnifiedRandom(Seed + 1);
        LayoutParam = layoutParam;
        LayoutParam.Structure = this;
        Palette = palette;
        
        if (generate) {
            ApplyLayoutMethod();
            FillComponents();
            Register();
            PlaceTilemap();
        }

        DebugInfoVisibility = StructureManager.DefaultDebugInfoLevel.Clone();
        UpdateDebugVisibility();
    }

    public List<DebugLabel> DrawDebugInfo() {
        if (FailedLayoutGeneration)
            return [];

        List<DebugLabel> labels = [];
        if (Tilemap != null)
            labels.AddRange(Tilemap.DrawDebugInfo());
        if (StructureLayout != null)
            labels.AddRange(StructureLayout.DrawDebugInfo());
        return labels;
    }

    /// <summary>
    ///     recursively sets this <see cref="DebugInfoLevel" /> for everything in this structure
    /// </summary>
    /// <param name="infoLevel"></param>
    public void SetDebugVisibility(DebugInfoLevel infoLevel) {
        Tilemap.DebugInfoVisibility = infoLevel;
        StructureLayout.DebugInfoVisibility = infoLevel;
        foreach (IComponent component in StructureLayout.AllComponents!) component.DebugInfoVisibility = infoLevel;
    }

    /// <summary>
    ///     recursively sets this <see cref="DebugInfoLevel" /> for everything in this structure to this <see cref="AdvStructure" />'s <see cref="DebugInfoVisibility" />
    /// </summary>
    public void UpdateDebugVisibility() => SetDebugVisibility(DebugInfoVisibility);
    
    /// <summary>
    ///     calculates a structure's layout but does not apply component generators
    /// </summary>
    /// <param name="generator">layout generator to be used. leave null for a random method</param>
    public void ApplyLayoutMethod(StructureLayoutGenerator generator = null) {
        if (StructureLayout != null) throw new Exception("this AdvStructure already has a layout set");

        StructureLayout = new StructureLayout(LayoutParam, "Layout_Type");
        StructureLayout.ExecuteGenerator();
        if (StructureLayout == null || Tilemap == null) {
            FailedLayoutGeneration = true;
            ModContent.GetInstance<SpawnHouses>().Logger.Warn($"structure with see {Seed} failed layout generation");
        }
    }

    /// <summary>
    ///     fills current layout with tiles/walls
    /// </summary>
    /// <exception cref="Exception">Throws when no layout has been set</exception>
    public void FillComponents() {
        if (StructureLayout == null)
            throw new Exception("no layout has been set");
        if (FailedLayoutGeneration)
            throw new Exception("layout generation was called but failed, aborting filling components");

        foreach (IComponent component in StructureLayout.AllComponents!)
            component.ExecuteGenerator();
    }

    /// <summary>
    ///     paste tiles from adv structure's tilemap into game tilemap
    /// </summary>
    public void PlaceTilemap() {
        if (StructureLayout == null)
            throw new Exception("No layout has been set");
        if (FailedLayoutGeneration)
            throw new Exception("layout generation was called but failed, aborting placing tilemap");

        Tilemap.ApplyTilemap();
    }

    /// <summary>
    ///     adds this structure to the global structure list and initializes some debug fields
    /// </summary>
    public void Register() => StructureManager.RegisterStructure(this);
}