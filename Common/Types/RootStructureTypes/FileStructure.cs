#nullable enable

using System;
using System.Collections.Generic;
using System.Linq;
using SpawnHouses.Common.Tagging;
using SpawnHouses.Common.Tiles;
using SpawnHouses.Common.Types.Enums;
using SpawnHouses.Common.Types.Interfaces;
using Terraria.DataStructures;

namespace SpawnHouses.Common.Types.RootStructureTypes;

/// <summary>
///     placeable instance of a structure. not to be created directly, structure manager handles the creation of these instances
/// </summary>
public sealed class FileStructure : IGeneratable, IStructureRoot, IStructureTags {
    // IGeneratable
    public ushort Id { get; init; }

    // IStructureRoot
    public StructureTilemap Tilemap { get; }
    public EntryPoint[] EntryPoints { get; }
    public bool HasBeenFound { get; }
    public bool IsFound(Point16 playerPos) => _isFound?.Invoke(this, playerPos) ?? true;
    public void OnFound() => _onFound?.Invoke(this);

    // IStructureTags
    public TagMap TagsCurrent { get; }


    /// <inheritdoc cref="IStructureRoot.IsFound" />
    private readonly Func<FileStructure, Point16, bool>? _isFound;

    /// <inheritdoc cref="IStructureRoot.OnFound" />
    private readonly Action<FileStructure>? _onFound;

    /// <inheritdoc cref="OnTilemapLoaded" />
    private readonly Action<FileStructure>? _onTilemapLoaded;

    public readonly string Name;

    /// <summary>any special data this structure needs to store</summary>
    public readonly Dictionary<string, object> Data = [];

    /// <summary>map of the substructures with each positionID being associated with a filename</summary>
    public readonly Dictionary<string, string> PositionIdToFilename = [];

    /// <summary>map of the substructures with each positionID being associated with a position</summary>
    public readonly Dictionary<string, Point16> PositionIdToPosition = [];

    public bool HasMultipleSubstructures => PositionIdToFilename.Keys.Count > 1;
    public Point16 Size => new(Tilemap.Width, Tilemap.Height);
    public Point16 Position => Tilemap.GlobalTileOffset;

    public FileStructure(string name, Point16 size, EntryPoint[] entryPoints, TagMap tagMap, Func<FileStructure, Point16, bool>? isFound = null,
        Action<FileStructure>? onFound = null, Action<FileStructure>? onTilemapLoaded = null) {
        if (EntryPoints.Count(entryPoint => entryPoint.Purpose is EntryPointPurpose.GroundLevel) > 2)
            throw new ArgumentException("Cannot have more than 2 ground-level entry points");

        Id = StructureManager.NextGeneratableId();
        Tilemap = new StructureTilemap(this, (ushort)size.X, (ushort)size.Y);
        EntryPoints = entryPoints;
        TagsCurrent = tagMap;
        _isFound = isFound;
        _onFound = onFound;
        _onTilemapLoaded = onTilemapLoaded;
        Name = name;
    }

    /// <summary>called just after the structure files are loaded into the tilemap</summary>
    public void OnTilemapLoaded() => _onTilemapLoaded?.Invoke(this);

    public void LoadTilemap() {
        foreach (var substructure in PositionIdToFilename) Tilemap.PlaceFile(PositionIdToPosition[substructure.Key], substructure.Value);

        _onTilemapLoaded?.Invoke(this);
    }

    public void ApplyTilemap() {
        Tilemap.ApplyTilemap();
    }

    /// <summary>
    ///     set the position of the structure in the world. this is the inclusive top-left corner of the structure, not including padding
    /// </summary>
    /// <param name="position"></param>
    public void SetPosition(Point16 position) {
        Tilemap.GlobalTileOffset = position;
        foreach (EntryPoint entryPoint in EntryPoints) entryPoint.SetOffset(position);
    }
}