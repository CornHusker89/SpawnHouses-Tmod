using System;
using System.Collections.Generic;
using SpawnHouses.Core.AdvGeneratables;
using SpawnHouses.Core.Debug;
using SpawnHouses.Core.Interfaces;
using SpawnHouses.Core.Palette;
using SpawnHouses.Core.Parameters;
using SpawnHouses.Core.Tagging;
using Terraria;
using Terraria.DataStructures;
using Terraria.Utilities;
using IComponent = SpawnHouses.Core.Interfaces.IComponent;

namespace SpawnHouses.Core.RootStructureTypes;

/// <summary>
///     the central object that everything for adv. structures revolve around
/// </summary>
public sealed class AdvStructure : StructureRoot {
    public override string InternalName { get; protected set; }

    /// <summary>
    ///     same instance as the <see cref="StructureLayout" />'s <see cref="SpawnHouses.Core.AdvGeneratable{SpawnHouses.Core.AdvGeneratables.StructureLayout,SpawnHouses.Core.Parameters.StructureLayoutParams,SpawnHouses.Core.StructureLayoutAdvGenerator}.TagsCurrent" />
    /// </summary>
    public override TagMap TagsCurrent {
        get => StructureLayout.TagsCurrent;
        protected set => StructureLayout.TagsCurrent = value;
    }

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
    /// <param internalName="internalName"></param>
    /// <param internalName="layoutParam"></param>
    /// <param internalName="palette"></param>
    /// <param internalName="seed">if -1, creates a new random seed from the normal terraria random generator</param>
    /// <param internalName="generate">
    ///     if true, will call <see cref="LoadTilemap" />, <see cref="StructureManager.RegisterAdvStructure"/>, and <see cref="ApplyTilemap" /> 
    /// </param>
    // ReSharper disable once NotNullOrRequiredMemberIsNotInitialized
    public AdvStructure(string internalName, StructureLayoutParams layoutParam, TilePalette palette, int seed = -1, bool generate = false) {
        // StructureRoot
        InternalName = internalName;
        Id = StructureManager.NextGeneratableId();
        EntryPoints = layoutParam.EntryPoints;
        HasBeenFound = false;
        
        Seed = seed == -1 ? WorldGen.genRand.Next() : seed;
        LayoutRandom = new UnifiedRandom(Seed);
        OtherRandom = new UnifiedRandom(Seed + 1);
        LayoutParam = layoutParam;
        LayoutParam.Structure = this;
        Palette = palette;

        ApplyLayoutMethod();
        
        if (generate) {
            LoadTilemap();
            StructureManager.RegisterAdvStructure(this);
            ApplyTilemap();
        }
        
        UpdateDebugVisibility();
    }

    /// <summary>
    ///     fills current layout with tiles/walls
    /// </summary>
    /// <exception cref="Exception">Throws when no layout has been set</exception>
    public override void LoadTilemap() {
        if (StructureLayout == null)
            throw new Exception("no layout has been set");
        if (FailedLayoutGeneration)
            throw new Exception("layout generation was called but failed, aborting filling components");
        if (Tilemap.IsAllTilesLoaded)
            return;

        foreach (IComponent component in StructureLayout.AllComponents!)
            component.ExecuteGenerator();
    }

    /// <summary>
    ///     paste tiles from adv structure's tilemap into game tilemap
    /// </summary>
    public override void ApplyTilemap() {
        if (StructureLayout == null)
            throw new Exception("No layout has been set");
        if (FailedLayoutGeneration)
            throw new Exception("layout generation was called but failed, aborting placing tilemap");

        Tilemap.ApplyTilemap();
    }

    public override bool IsFound(Point16 playerPos) {
        if (!Tilemap.IsTilesPlaced)
            return false;
        return playerPos.X > StructureLayout.BoundingBox.Left - 20 &&
               playerPos.X < StructureLayout.BoundingBox.Right + 20 &&
               playerPos.X > StructureLayout.BoundingBox.Top - 20 &&
               playerPos.X < StructureLayout.BoundingBox.Bottom + 20;
    }

    public override void OnFound() {
    }

    public override List<DebugLabel> DrawDebugGeometry() {
        if (FailedLayoutGeneration)
            return [];

        List<DebugLabel> labels = [];
        // ReSharper disable once ConditionIsAlwaysTrueOrFalse
        if (Tilemap != null)
            labels.AddRange(Tilemap.DrawDebugGeometry());
        if (StructureLayout != null)
            labels.AddRange(StructureLayout.DrawDebugGeometry());
        return labels;
    }

    public override void SetPosition(Point16 position) {
        Point16 delta = position - StructureLayout.BoundingBox.TopLeftPoint16;

        Tilemap.SetPosition(Tilemap.BoundingBox.TopLeftPoint16 + delta);
        StructureLayout.Offset(delta);
        foreach (EntryPoint entryPoint in LayoutParam.EntryPoints) entryPoint.SetOffset(position);
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
        // ReSharper disable once ConditionIsAlwaysTrueOrFalse
        if (StructureLayout == null || Tilemap == null) {
            FailedLayoutGeneration = true;
            SpawnHousesMod.Instance.Logger.Warn($"structure with see {Seed} failed layout generation");
        }
    }
}