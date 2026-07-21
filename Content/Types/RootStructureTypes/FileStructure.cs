#nullable enable

using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using SpawnHouses.Content.Debug;
using SpawnHouses.Content.Tagging;
using SpawnHouses.Content.Tiles;
using SpawnHouses.Content.Types.Enums;
using SpawnHouses.Content.Types.Interfaces;
using SpawnHouses.Helpers;
using StructureHelper.API;
using Terraria;
using Terraria.DataStructures;

namespace SpawnHouses.Content.Types.RootStructureTypes;

/// <summary>
///     placeable instance of a structure. not to be created directly, structure manager handles the creation of these instances
/// </summary>
public sealed class FileStructure : IGeneratable, IStructureRoot, IStructureTags {
    // IGeneratable
    public ushort Id { get; init; }

    // IStructureRoot
    public DebugInfoLevel DebugInfoVisibility { get; set; }
    public string Name { get; }
    public StructureTilemap Tilemap { get; }
    public EntryPoint[] EntryPoints { get; }
    public bool HasBeenFound { get; set; }
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

    public bool Standalone { get; internal set; }

    public bool Depreciated { get; internal set; }

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
        if (entryPoints.Count(entryPoint => entryPoint.Purpose is EntryPointPurpose.GroundLevel) > 2)
            throw new ArgumentException("Cannot have more than 2 ground-level entry points");

        _isFound = isFound;
        _onFound = onFound;
        _onTilemapLoaded = onTilemapLoaded;
        DebugInfoVisibility = StructureManager.DefaultDebugInfoLevel.Clone();
        Id = StructureManager.NextGeneratableId();
        Tilemap = new StructureTilemap(this, (ushort)size.X, (ushort)size.Y);
        EntryPoints = entryPoints;
        TagsCurrent = tagMap;
        Name = name;
    }

    public Color GetDrawColor() => DrawHelper.GetColor(Id);

    public List<DebugLabel> DrawDebugGeometry() {
        // draw geometry
        if (DebugInfoVisibility.DisplayBounds) {
            DrawHelper.DrawWorldBasedRectangularPath(Tilemap.BoundingBox.GetDrawPath(), GetDrawColor(), DrawHelper.DebugDrawWidth);

            if (HasMultipleSubstructures)
                foreach (string positionId in PositionIdToFilename.Keys) {
                    Point worldPos = PositionIdToPosition[positionId].ToPoint() * new Point(16, 16);
                    Point worldSize = Generator.GetStructureDimensions(PositionIdToFilename[positionId], SpawnHousesMod.Instance).ToPoint();
                    Point[] path = [
                        worldPos,
                        worldPos + new Point(worldSize.X, 0),
                        worldPos + new Point(worldSize.X, worldSize.Y),
                        worldPos + new Point(0, worldSize.Y)
                    ];
                    DrawHelper.DrawWorldBasedRectangularPath(path, DrawHelper.GetColor((ushort)PositionIdToFilename[positionId].GetHashCode()), DrawHelper.DebugDrawWidth);
                }
        }

        // create labels
        DebugLabel mainLabel = new(Tilemap.GlobalTileOffset, this);
        if (mainLabel.IsVisible(Tilemap.BoundingBox)) {
            List<DebugLabel> labels = [mainLabel];
            if (HasMultipleSubstructures)
                foreach (string positionId in PositionIdToFilename.Keys)
                    labels.Add(new DebugLabel(
                        PositionIdToPosition[positionId],
                        DebugInfoVisibility,
                        DrawHelper.GetColor((ushort)PositionIdToFilename[positionId].GetHashCode()),
                        positionId,
                        PositionIdToFilename[positionId].Replace("Content/Assets/StructureFiles/", "")
                    ));

            return labels;
        }

        return [];
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
    ///     set the position of the structure in the world. this is the inclusive top-left corner of the structure
    /// </summary>
    /// <param name="position"></param>
    public void SetPosition(Point16 position) {
        Tilemap.SetPosition(position);
        foreach (EntryPoint entryPoint in EntryPoints) entryPoint.SetOffset(position);
    }
}