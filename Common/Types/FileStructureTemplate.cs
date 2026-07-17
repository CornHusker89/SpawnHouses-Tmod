using System.Collections.Generic;
using SpawnHouses.Common.Tagging;
using SpawnHouses.Common.Types.DataStructures;
using SpawnHouses.Common.Types.Interfaces;
using SpawnHouses.Common.Types.RootStructureTypes;
using Terraria.DataStructures;

namespace SpawnHouses.Common.Types;

using StructureInfo = (Dictionary<string, Point16> positionIdsToPositions, EntryPoint[] entryPoints, TagMap tags);

/// <summary>
///     loadable structure template, which gets turned into placeable <see cref="FileStructure" />s handled internally.
///     all children must not have an explicit constructor
/// </summary>
public abstract class FileStructureTemplate {
    public abstract FileSubstructureData[] Substructures { get; }

    public abstract string[] PositionIds { get; }

    /// <summary>
    ///     gets the relative positions associated with each position ID, the entry points, and tags for a given variant of substructures.
    ///     if a position ID's position is (-1, -1), that position ID will not be generated
    /// </summary>
    /// <param name="positionIdsToNames">mapping of position IDs to friendly substructure names</param>
    /// <returns></returns>
    public abstract StructureInfo GetStructureInfo(Dictionary<string, string> positionIdsToNames);

    /// <inheritdoc cref="IStructureRoot.IsFound" />
    public virtual bool IsFound(FileStructure structure, Point16 playerPos) => true;

    /// <inheritdoc cref="IStructureRoot.OnFound" />
    public virtual void OnFound(FileStructure structure) {
    }

    /// <inheritdoc cref="FileStructure.OnTilemapLoaded" />
    public virtual void OnTilemapLoaded(FileStructure structure) {
    }
}