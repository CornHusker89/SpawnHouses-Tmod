using System;
using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
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
    public ushort Id { get; private set; }

    // IStructureRoot
    public StructureTilemap Tilemap { get; }
    public EntryPoint[] EntryPoints { get; }
    public bool HasBeenFound { get; }
    public bool IsFound(Point16 playerPos) => _isFound?.Invoke(playerPos) ?? true;
    public void OnFound() => _onFound?.Invoke();

    // IStructureTags
    public TagMap TagsCurrent { get; }


    /// <summary>called to see if structure should now be considered "found"</summary>
    [CanBeNull]
    private readonly Func<Point16, bool> _isFound;

    /// <summary>called when structure is found</summary>
    [CanBeNull]
    private readonly Action _onFound;

    /// <summary>called just after the structure files are loaded into the tilemap</summary>
    [CanBeNull]
    private readonly Action _onFilesLoaded;

    public readonly string Name;

    public int TilemapPadding;

    /// <summary> map of the substructures with each of their root (top-left) positions</summary>
    public readonly Dictionary<string, (string positionID, Point16 position)> Substructures = [];

    public bool HasMultipleSubstructures => Substructures.Keys.Count > 1;
    public Point16 Size => new(Tilemap.Width - 2 * TilemapPadding, Tilemap.Height - 2 * TilemapPadding);
    public Point16 Position => Tilemap.GlobalTileOffset + new Point16(TilemapPadding, TilemapPadding);

    public FileStructure(string name, Point16 size, EntryPoint[] entryPoints, TagMap tagMap, Func<Point16, bool> isFound,
        Action onFound, Action onTilemapApplied, int tilemapPadding = 1) {
        if (EntryPoints.Count(entryPoint => entryPoint.Purpose is EntryPointPurpose.GroundLevel) > 2)
            throw new ArgumentException("Cannot have more than 2 ground-level entry points");

        Id = StructureManager.NextGeneratableId();
        Tilemap = new StructureTilemap(this, (ushort)(size.X + 2 * tilemapPadding), (ushort)(size.Y + 2 * tilemapPadding));
        EntryPoints = entryPoints;
        TagsCurrent = tagMap;
        _isFound = isFound;
        _onFound = onFound;
        _onFilesLoaded = onTilemapApplied;
        Name = name;
        TilemapPadding = tilemapPadding;
    }

    public void LoadTilemap() {
        Tilemap.loadfiles();

        _onFilesLoaded?.Invoke();
    }

    public void ApplyTilemap() {
        Tilemap.ApplyTilemap();
    }

    /// <summary>
    ///     set the position of the structure in the world. this is the inclusive top-left corner of the structure, not including padding
    /// </summary>
    /// <param name="position"></param>
    public void SetPosition(Point16 position) {
        Tilemap.GlobalTileOffset = position - new Point16(TilemapPadding, TilemapPadding);
        foreach (EntryPoint entryPoint in EntryPoints) entryPoint.SetOffset(position);
    }
}