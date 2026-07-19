using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using SpawnHouses.Helpers;
using SpawnHouses.StructureCommon.Debug;
using SpawnHouses.StructureCommon.Modules;
using SpawnHouses.StructureCommon.Palette;
using SpawnHouses.StructureCommon.Parameters;
using SpawnHouses.StructureCommon.Tiles;
using SpawnHouses.StructureCommon.Types.Interfaces;
using Terraria;
using Terraria.DataStructures;
using Terraria.Utilities;
using IComponent = SpawnHouses.StructureCommon.Types.Interfaces.IComponent;

namespace SpawnHouses.StructureCommon.Types.RootStructureTypes;

/// <summary>
///     the central object that everything for adv. structures revolve around
/// </summary>
public class AdvStructure : ICanDebugDraw, IStructureRoot {
    
    // ICanDebugDraw
    public DebugInfoLevel DebugInfoVisibility { get; set; }
    public string Name { get; init; }

    // IStructureRoot
    public ushort Id { get; init; }
    public StructureTilemap Tilemap { get; set; }
    public EntryPoint[] EntryPoints => LayoutParam.EntryPoints;
    public bool HasBeenFound { get; set; }
    
    

    public readonly Dictionary<Type, List<IAdvGenerator>> InstanceGeneratorQueue = [];

    /// <summary>
    ///     this random advGenerator should only be used for things that ARE related to layouts
    /// </summary>
    public readonly UnifiedRandom LayoutRandom;

    /// <summary>
    ///     this random advGenerator should only be used for things that are NOT related to layouts
    /// </summary>
    public readonly UnifiedRandom OtherRandom;
    
    public readonly int Seed;
    public StructureLayout StructureLayout;
    public readonly StructureLayoutParams LayoutParam;
    public TilePalette Palette;
    public bool FailedLayoutGeneration;

    /// <summary>
    /// </summary>
    /// <param name="name"></param>
    /// <param name="layoutParam"></param>
    /// <param name="palette"></param>
    /// <param name="seed">if -1, creates a new random seed from the normal terraria random advGenerator</param>
    /// <param name="generate">
    ///     if true, will call <see cref="ApplyLayoutMethod" />, <see cref="LoadTilemap" />,
    ///     <see cref="Register"/>, and <see cref="ApplyTilemap" /> 
    /// </param>
    public AdvStructure(string name, StructureLayoutParams layoutParam, TilePalette palette, int seed = -1, bool generate = true) {
        Name = name;
        Id = StructureManager.NextGeneratableId();
        Seed = seed == -1 ? WorldGen.genRand.Next() : seed;
        LayoutRandom = new UnifiedRandom(Seed);
        OtherRandom = new UnifiedRandom(Seed + 1);
        LayoutParam = layoutParam;
        LayoutParam.Structure = this;
        Palette = palette;
        
        if (generate) {
            ApplyLayoutMethod();
            LoadTilemap();
            Register();
            ApplyTilemap();
        }

        DebugInfoVisibility = StructureManager.DefaultDebugInfoLevel.Clone();
        UpdateDebugVisibility();
    }

    /// <summary>
    ///     fills current layout with tiles/walls
    /// </summary>
    /// <exception cref="Exception">Throws when no layout has been set</exception>
    public void LoadTilemap() {
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
    public void ApplyTilemap() {
        if (StructureLayout == null)
            throw new Exception("No layout has been set");
        if (FailedLayoutGeneration)
            throw new Exception("layout generation was called but failed, aborting placing tilemap");

        Tilemap.ApplyTilemap();
    }

    /// <summary>
    ///     adds this structure to the global structure list and initializes some debug fields
    /// </summary>
    public void Register() => StructureManager.RegisterAdvStructure(this);

    public bool IsFound(Point16 playerPos) {
        if (!Tilemap.IsTilesPlaced)
            return false;
        return playerPos.X > StructureLayout.BoundingBox.Left - 20 &&
               playerPos.X < StructureLayout.BoundingBox.Right + 20 &&
               playerPos.X > StructureLayout.BoundingBox.Top - 20 &&
               playerPos.X < StructureLayout.BoundingBox.Bottom + 20;
    }

    public void OnFound() {
    }

    public Color GetDrawColor() => DrawHelper.GetColor((ushort)GetHashCode());

    public List<DebugLabel> DrawDebugGeometry() {
        if (FailedLayoutGeneration)
            return [];

        List<DebugLabel> labels = [];
        if (Tilemap != null)
            labels.AddRange(Tilemap.DrawDebugGeometry());
        if (StructureLayout != null)
            labels.AddRange(StructureLayout.DrawDebugGeometry());
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
    /// <param name="advGenerator">layout advGenerator to be used. leave null for a random method</param>
    public void ApplyLayoutMethod(StructureLayoutAdvGenerator advGenerator = null) {
        if (StructureLayout != null) throw new Exception("this AdvStructure already has a layout set");

        StructureLayout = new StructureLayout(LayoutParam, "Layout_Type");
        StructureLayout.ExecuteGenerator();
        if (StructureLayout == null || Tilemap == null) {
            FailedLayoutGeneration = true;
            SpawnHousesMod.Instance.Logger.Warn($"structure with see {Seed} failed layout generation");
        }
    }
}