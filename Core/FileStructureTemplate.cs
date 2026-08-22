#nullable enable

using System.Collections.Generic;
using SpawnHouses.Core.DataStructures;
using SpawnHouses.Core.RootStructureTypes;
using SpawnHouses.Core.Tagging;
using Terraria.DataStructures;

namespace SpawnHouses.Core;

using StructureInfo = (Dictionary<string, Point16> positionIdsToPositions, EntryPoint[] entryPoints, TagMap tags);

/// <summary>
///     loadable structure template, which gets turned into placeable <see cref="FileStructure" />s handled internally.
///     all children must not have an explicit constructor
/// </summary>
public abstract class FileStructureTemplate {
    /// <summary>
    ///     all substructures that could be used to generate a <see cref="FileStructure" />
    /// </summary>
    public abstract FileSubstructureData[] Substructures { get; }

    /// <summary>
    ///     all possible position ids for the substructures
    /// </summary>
    public abstract string[] PositionIds { get; }

    /// <summary>
    ///     gets the relative positions associated with each position ID, the entry points, and tags for a given variant of substructures.
    ///     if a position ID's position is (-1, -1), that position ID will not be generated
    /// </summary>
    /// <param name="posIdsToStruct">mapping of position IDs to friendly substructure names</param>
    /// <returns></returns>
    /// <remarks>this function MUST be deterministic</remarks>
    public abstract StructureInfo GetStructureInfo(Dictionary<string, FileSubstructureData> posIdsToStruct);

    /// <summary>
    ///     gives a list of upgrade structures this variation has. return null for autofill from this template.
    ///     otherwise override for specific structures or an empty list.
    /// </summary>
    /// <param name="structure"></param>
    /// <returns></returns>
    public virtual StructureRoot[]? GetUpgrades(FileStructure structure) => null;

    /// <inheritdoc cref="StructureRoot.IsFound" />
    public virtual bool IsFound(FileStructure structure, Point16 playerPos) => true;

    /// <inheritdoc cref="StructureRoot.OnFound" />
    public virtual void OnFound(FileStructure structure) {
    }

    /// <inheritdoc cref="FileStructure._onTilemapLoaded" />
    /// . will be called outside the context of a loaded world,
    /// must not be reliant on Main.tile[] or any other world-dependent factors
    public virtual void OnTilemapLoaded(FileStructure structure) {
    }
}