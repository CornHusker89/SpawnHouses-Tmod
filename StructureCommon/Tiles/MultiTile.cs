using SpawnHouses.StructureCommon.Palette;
using SpawnHouses.StructureCommon.Types.Geometry;
using Terraria.DataStructures;
using Terraria.Utilities;

namespace SpawnHouses.StructureCommon.Tiles;

public class MultiTile {
    public static readonly Point16 DoorOrigin = new(0, 1);
    public bool FacingRight;
    public Point16 Origin;
    public byte PaintType;
    public int Style;
    public ushort TileType;

    public Shape Volume;

    public MultiTile(Point16 topLeftPos, Point16 size, TilePaintedType tilePaintedType, UnifiedRandom random, Point16? origin = null, bool facingRight = true) {
        Volume = new Shape(
            topLeftPos,
            topLeftPos + new Point16(size.X - 1, 0),
            topLeftPos + new Point16(size.X - 1, size.Y - 1),
            topLeftPos + new Point16(0, size.Y - 1)
        );
        (TileType, PaintType, Style) = tilePaintedType.GetIds(random);
        Origin = origin ?? Point16.NegativeOne;
        FacingRight = facingRight;
    }
}