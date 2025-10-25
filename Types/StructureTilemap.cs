using System;
using SpawnHouses.AdvStructures.AdvStructureParts;
using SpawnHouses.Helpers;
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
        if (paintedType.Style != -1) throw new NotImplementedException();

        StructureTile tile = this[x, y];
        tile.HasTile = true;
        tile.BlockType = blockType;
        tile.TileType = paintedType.Type;
        tile.TileColor = paintedType.PaintType;
        tile.IsNullTile = false;
    }

    public void PlaceTile(int x, int y, PaintedType paintedType, SlopingAlgorithm slopingAlgorithm, SlopeModifier slopeModifier = SlopeModifier.GlobalSloping) {
        if (paintedType.Style != -1) {
            throw new NotImplementedException();
        }

        StructureTile tile = this[x, y];
        tile.HasTile = true;
        tile.TileType = paintedType.Type;
        tile.TileColor = paintedType.PaintType;
        tile.IsNullTile = false;
        tile.SlopingAlg = slopingAlgorithm;
        tile.SlopeModifier = slopeModifier;
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

    /// <summary>
    ///     applies this tilemap (with it's offset) onto main game tilemap
    /// </summary>
    public void ApplyTilemap() {
        bool[,] globalTilemap = new bool[Width, Height];
        bool[,] globalNonLocalTilemap = new bool[Width, Height];
        for (int x = 0; x < Width; x++)
        for (int y = 0; y < Height; y++) {
            StructureTile tile = this[x, y];
            tile.PasteTile(ConvertToGlobal(x, y));
            globalTilemap[x, y] = tile.HasTile;
        }

        for (int x = 0; x < Width; x++)
        for (int y = 0; y < Height; y++) {
            StructureTile tile = this[x, y];
            if (tile.SlopingAlg != null) {
                bool[,] tilemap;
                if (tile.SlopeModifier == SlopeModifier.GlobalSloping)
                    tilemap = globalTilemap;
                else if (tile.SlopeModifier == SlopeModifier.GlobalOnlySloping)
                    tilemap = globalNonLocalTilemap;
                else
                    throw new Exception($"tile has unsupported slope modifier {tile.SlopeModifier} for standalone placement");

                tile.BlockType = tile.SlopingAlg(x, y, tilemap);
                tile.SlopingAlg = null;
            }

            if (tile.HasTile)
                tile.ApplySlopes(ConvertToGlobal(x, y));
        }

        for (int x = 0; x < Width; x++)
        for (int y = 0; y < Height; y++)
            StructureTile.SetFrames(ConvertToGlobal(x, y));
    }
}