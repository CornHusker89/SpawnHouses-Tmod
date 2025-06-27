using System;
using Terraria.DataStructures;

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

    /// <summary>
    ///     tests if the coordinates have a valid, initialized tile that is in bounds
    /// </summary>
    /// <param name="x"></param>
    /// <param name="y"></param>
    /// <returns></returns>
    public bool IsValidTile(int x, int y) {
        if (x < 0 || x >= Width || y < 0 || y >= Height) return false;

        return _tiles[x, y] != null;
    }

    /// <summary>
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
        Point16 localPos = pos - WorldTileOffset;
        return this[localPos.X, localPos.Y];
    }

    /// <summary>
    ///     adds rows and columns to the tilemap
    /// </summary>
    /// <param name="tileOffset">
    ///     the amount to shift each tile. the way to think about this is shifting the top left tile this
    ///     much, and filling in the resulting gap to expand the tilemap
    /// </param>
    public void Expand(Point16 tileOffset) {
        throw new NotImplementedException();
    }
}