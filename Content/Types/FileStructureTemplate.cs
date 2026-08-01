using System.Collections.Generic;
using SpawnHouses.Content.Tagging;
using SpawnHouses.Content.Types.DataStructures;
using SpawnHouses.Content.Types.RootStructureTypes;
using Terraria.DataStructures;

namespace SpawnHouses.Content.Types;

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

    /// <inheritdoc cref="StructureRoot.IsFound" />
    public virtual bool IsFound(FileStructure structure, Point16 playerPos) => true;

    /// <inheritdoc cref="StructureRoot.OnFound" />
    public virtual void OnFound(FileStructure structure) {
    }

    /// <inheritdoc cref="FileStructure.OnTilemapLoaded" />
    public virtual void OnTilemapLoaded(FileStructure structure) {
    }
}