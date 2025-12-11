#nullable enable
using System;
using System.Collections.Generic;
using SpawnHouses.AdvStructures.AdvStructureParts;
using SpawnHouses.Helpers;
using SpawnHouses.Types.Palette;
using Terraria;
using Terraria.DataStructures;
using Terraria.ID;

namespace SpawnHouses.Types;

public class StructureTilemap {
    private readonly StructureTile[,] _tiles;

    /// <summary>the actual world tile coordinates of the top left tile in this tilemap</summary>
    public Point16 WorldTileOffset;

    public List<MultiTile> MultiTiles;

    public StructureTilemap(ushort width, ushort height, Point16? worldTileOffset = null) {
        Width = width;
        Height = height;
        _tiles = new StructureTile[width, height];
        WorldTileOffset = worldTileOffset ?? new Point16(0, 0);
        MultiTiles = [];
    }

    public ushort Width { get; }
    public ushort Height { get; }

    /// <summary>
    ///     uses coordinates relative to this tilemap
    /// </summary>
    /// <param name="x"></param>
    /// <param name="y"></param>
    /// <exception cref="IndexOutOfRangeException"></exception>
    public StructureTile this[int x, int y] {
        get {
            if (x < 0 || x >= Width || y < 0 || y >= Height) throw new IndexOutOfRangeException();

            // ReSharper disable once NullCoalescingConditionIsAlwaysNotNullAccordingToAPIContract
            return _tiles[x, y] ?? (_tiles[x, y] = new StructureTile());
        }
    }

    public StructureTile this[Point16 point] => this[point.X, point.Y];

    public bool InBounds(int x, int y) {
        return x >= 0 && x < Width && y >= 0 && y < Height;
    }

    public bool InBounds(Point16 point) {
        return InBounds(point.X, point.Y);
    }

    public bool InInterior(int x, int y) {
        return InBounds(x, y) && _tiles[x, y].IsInside;
    }

    public bool InInterior(Point16 point) {
        return InInterior(point.X, point.Y);
    }

    /// <summary>
    ///     gets tile from this tilemap using global world coordinates
    /// </summary>
    /// <param name="x"></param>
    /// <param name="y"></param>
    /// <returns></returns>
    public StructureTile GetTileByGlobalPos(int x, int y) {
        return this[x - WorldTileOffset.X, y - WorldTileOffset.Y];
    }

    /// <summary>
    ///     gets tile from this tilemap using global world coordinates
    /// </summary>
    /// <param name="pos"></param>
    /// <returns></returns>
    public StructureTile GetTileByGlobalPos(Point16 pos) {
        return this[pos - WorldTileOffset];
    }

    public int ConvertToRelative(int coordinate, bool isX) {
        return coordinate - (isX ? WorldTileOffset.X : WorldTileOffset.Y);
    }

    public Point16 ConvertToRelative(int x, int y) {
        return new Point16(x - WorldTileOffset.X, y - WorldTileOffset.Y);
    }

    public Point16 ConvertToRelative(Point16 point) {
        return ConvertToRelative(point.X, point.Y);
    }

    public int ConvertToGlobal(int coordinate, bool isX) {
        return coordinate + (isX ? WorldTileOffset.X : WorldTileOffset.Y);
    }

    public Point16 ConvertToGlobal(int x, int y) {
        return new Point16(x + WorldTileOffset.X, y + WorldTileOffset.Y);
    }

    public Point16 ConvertToGlobal(Point16 point) {
        return ConvertToGlobal(point.X, point.Y);
    }

    /// <summary>
    ///     offsets given <see cref="ExternalLayout" /> by this tilemap's tile offset
    /// </summary>
    /// <param name="externalLayout"></param>
    public void OffsetExternalLayout(ExternalLayout externalLayout) {
        Point16 offset = WorldTileOffset * Point16.NegativeOne;
        foreach (Floor floor in externalLayout.Floors) floor.Volume.Offset(offset);
        foreach (Wall wall in externalLayout.Walls) wall.Volume.Offset(offset);
        foreach (Gap gap in externalLayout.Gaps) gap.Volume.Offset(offset);
        foreach (Roof roof in externalLayout.Roofs) roof.Line.Offset(offset);
    }

    /// <summary>
    ///     offsets given <see cref="EntryPoint" /> by this tilemap's tile offset
    /// </summary>
    /// <param name="entryPoint"></param>
    public void OffsetEntryPoint(EntryPoint entryPoint) {
        entryPoint.Offset = WorldTileOffset * Point16.NegativeOne;
    }

    public void PlaceTile(int x, int y, TilePaintedType? paintedType, BlockType blockType = BlockType.Solid) {
        if (paintedType == null) return;
        if (paintedType.Style != -1) throw new NotImplementedException();

        StructureTile tile = this[x, y];
        tile.HasTile = true;
        tile.BlockType = blockType;
        (tile.TileType, tile.TileColor) = paintedType.Ids;
        tile.IsNullTile = false;
    }

    public void PlaceTile(int x, int y, TilePaintedType? paintedType, SlopingAlgorithm? slopingAlgorithm, SlopeModifier slopeModifier = SlopeModifier.GlobalSloping) {
        if (paintedType == null) return;
        if (paintedType.Style != -1) throw new NotImplementedException();

        StructureTile tile = this[x, y];
        tile.HasTile = true;
        (tile.TileType, tile.TileColor) = paintedType.Ids;
        tile.IsNullTile = false;
        tile.SlopingAlg = slopingAlgorithm;
        tile.SlopeModifier = slopeModifier;
    }

    public void PlaceMultiTile(Point16 topLeftPos, Point16 size, TilePaintedType tilePaintedType, bool isFurniture,
        Point16? origin = null, bool facingRight = true) {
        MultiTile multiTile = new(topLeftPos, size, tilePaintedType, origin, facingRight);
        PlaceMultiTile(multiTile, isFurniture);
    }

    public void PlaceMultiTile(MultiTile multiTile, bool isFurniture) {
        multiTile.Volume.ExecuteInArea((x, y) => {
            StructureTile tile = this[x, y];
            tile.HasTile = true;
            tile.IsNullTile = false;
            tile.TileType = multiTile.TileType;
            tile.TileColor = multiTile.PaintType;
            tile.IsFurniture = isFurniture;
            tile.IsFakeTile = true;
        });
        MultiTiles.Add(multiTile);
    }

    /// <summary>
    ///     changes the tile at this position to be the <paramref name="paintedType"/>,
    ///     does not change <see cref="StructureTile.BlockType" />, <see cref="StructureTile.HasTile" />, or <see cref="StructureTile.IsNullTile" />
    /// </summary>
    /// <param name="x"></param>
    /// <param name="y"></param>
    /// <param name="paintedType"></param>
    public void SoftPlaceTile(int x, int y, TilePaintedType paintedType) {
        StructureTile tile = this[x, y];
        (tile.TileType, tile.TileColor) = paintedType.Ids;
    }

    public void PlaceWall(int x, int y, WallPaintedType paintedType) {
        StructureTile tile = this[x, y];
        (tile.WallType, tile.WallColor) = paintedType.Ids;
        tile.IsNullWall = false;
    }

    /// <summary>
    /// </summary>
    /// <param name="x"></param>
    /// <param name="y"></param>
    /// <param name="nullTile">If true, when the tilemap is pasted, the original tile here will remain</param>
    public void ClearTile(int x, int y, bool nullTile) {
        this[x, y].ClearTile(nullTile);
    }

    /// <summary>
    ///     applies this tilemap (with it's offset) onto main game tilemap
    /// </summary>
    public void ApplyTilemap() {
        bool[,] globalTilemap = new bool[Width, Height];
        bool[,] globalNonLocalTilemap = new bool[Width, Height];

        // place normal tiles
        for (int x = 0; x < Width; x++)
        for (int y = 0; y < Height; y++) {
            StructureTile tile = this[x, y];
            tile.PasteTile(ConvertToGlobal(x, y));
            globalTilemap[x, y] = tile.HasTile;
            globalNonLocalTilemap[x, y] = tile.HasTile && tile.SlopeModifier != SlopeModifier.LocalSloping;
        }

        // slope tiles
        for (int x = 0; x < Width; x++)
        for (int y = 0; y < Height; y++) {
            StructureTile tile = this[x, y];
            if (tile.SlopingAlg != null) {
                bool[,] tilemap;
                switch (tile.SlopeModifier) {
                    case SlopeModifier.GlobalSloping:
                        tilemap = globalTilemap;
                        tile.BlockType = tile.SlopingAlg(x, y, tilemap);
                        break;
                    case SlopeModifier.GlobalOnlySloping:
                        tilemap = globalNonLocalTilemap;
                        tile.BlockType = tile.SlopingAlg(x, y, tilemap);
                        break;
                    case SlopeModifier.LocalSloping:
                        break;
                    default:
                        throw new Exception($"tile has unsupported slope modifier {tile.SlopeModifier} for standalone placement");
                }
            }

            if (tile.HasTile)
                tile.ApplySlopes(ConvertToGlobal(x, y));
        }

        // place MultiTiles
        foreach (MultiTile multiTile in MultiTiles) {
            Point16 originPoint = multiTile.Volume.BoundingBox.topLeft + multiTile.Origin;
            if (multiTile.FacingRight)
                Terraria.WorldGen.PlaceTile(originPoint.X, originPoint.Y, multiTile.TileType, true, style: multiTile.Style);
            else
                Terraria.WorldGen.PlaceObject(originPoint.X, originPoint.Y, multiTile.TileType, true, multiTile.Style, direction: multiTile.FacingRight ? 1 : -1);
            multiTile.Volume.ExecuteInArea((x, y) => {
                Tile tile = Main.tile[ConvertToGlobal(x, y)];
                tile.TileColor = multiTile.PaintType;
            });
        }

        // set frames
        for (int x = 0; x < Width; x++)
        for (int y = 0; y < Height; y++)
            StructureTile.SetFrames(ConvertToGlobal(x, y));
    }
}