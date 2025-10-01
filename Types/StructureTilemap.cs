using System;
using SpawnHouses.AdvStructures.AdvStructureParts;
using Terraria.DataStructures;
using Terraria.ID;

namespace SpawnHouses.Types;

public class StructureTilemap {
    private readonly StructureTile[,] _tiles;

    /// <summary>the actual world tile coordinates of the top left tile in this tilemap</summary>
    public Point16 WorldTileOffset;

    public StructureTilemap(ushort width, ushort height, Point16? worldTileOffset = null) {
        Width = width;
        Height = height;
        _tiles = new StructureTile[width, height];
        WorldTileOffset = worldTileOffset ?? new Point16(0, 0);
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
    ///     tests if the coordinates have a valid, initialized tile that is in bounds
    /// </summary>
    /// <param name="x"></param>
    /// <param name="y"></param>
    /// <returns></returns>
    public bool IsInitializedTile(int x, int y) {
        return InBounds(x, y) && _tiles[x, y] != null;
    }

    /// <summary>
    ///     gets tile from this tilemap using global world coordinates
    /// </summary>
    /// <param name="x"></param>
    /// <param name="y"></param>
    /// <returns></returns>
    public StructureTile GetTileByGlobalPos(int x, int y) {
        x -= WorldTileOffset.X;
        y -= WorldTileOffset.Y;
        return this[x, y];
    }

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

    public void PlaceTile(int x, int y, PaintedType paintedType, BlockType blockType = BlockType.Solid) {
        if (paintedType.Style == -1) {
            StructureTile tile = this[x, y];
            tile.HasTile = true;
            tile.BlockType = blockType;
            tile.TileType = paintedType.Type;
            tile.TileColor = paintedType.PaintType;
            tile.IsNullTile = false;
        }
        else {
            throw new NotImplementedException();

            // can't use this because it needs a custom tilemap
            // Terraria.WorldGen.PlaceTile(x, y, paintedType.Type, true, true, style: paintedType.Style);
            // StructureTile tile = tilemap[x, y];
            // tile.TileColor = paintedType.PaintType;
        }
    }

    /// <summary>
    ///     changes the tile at this position to be the <see cref="paintedType" />,
    ///     does not change <see cref="StructureTile.BlockType" />, <see cref="StructureTile.HasTile" />, or <see cref="StructureTile.IsNullTile" />
    /// </summary>
    /// <param name="x"></param>
    /// <param name="y"></param>
    /// <param name="paintedType"></param>
    public void SoftPlaceTile(int x, int y, PaintedType paintedType) {
        StructureTile tile = this[x, y];
        tile.TileType = paintedType.Type;
        tile.TileColor = paintedType.PaintType;
    }

    public void PlaceWall(int x, int y, PaintedType paintedType) {
        StructureTile tile = this[x, y];
        tile.WallType = paintedType.Type;
        tile.WallColor = paintedType.PaintType;
        tile.IsNullWall = false;
    }
}