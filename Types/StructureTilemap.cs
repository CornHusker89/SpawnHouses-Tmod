using System;
using SpawnHouses.AdvStructures.AdvStructureParts;
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
    ///     offsets given <see cref="ExternalLayout" /> by this tilemap's tile offset
    /// </summary>
    /// <param name="externalLayout"></param>
    public void OffsetExternalLayout(ExternalLayout externalLayout) {
        foreach (Floor floor in externalLayout.Floors) floor.Volume.Offset(WorldTileOffset);
        foreach (Wall wall in externalLayout.Walls) wall.Volume.Offset(WorldTileOffset);
        foreach (Gap gap in externalLayout.Gaps) gap.Volume.Offset(WorldTileOffset);
    }
}