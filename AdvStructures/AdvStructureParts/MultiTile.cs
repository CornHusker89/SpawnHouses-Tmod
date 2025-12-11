using SpawnHouses.Types.Palette;
using Terraria.DataStructures;

namespace SpawnHouses.AdvStructures.AdvStructureParts;

public class MultiTile {
    public static readonly Point16 DoorOrigin = new(0, 1);

    public Shape Volume;
    public ushort TileType;
    public int Style;
    public byte PaintType;
    public Point16 Origin;
    public bool FacingRight;

    public MultiTile(Point16 topLeftPos, Point16 size, TilePaintedType tilePaintedType, Point16? origin = null, bool facingRight = true) {
        Volume = new Shape(
            topLeftPos,
            topLeftPos + new Point16(size.X - 1, 0),
            topLeftPos + new Point16(size.X - 1, size.Y - 1),
            topLeftPos + new Point16(0, size.Y - 1)
        );
        (TileType, PaintType, Style) = tilePaintedType.Ids;
        Origin = origin ?? Point16.NegativeOne;
        FacingRight = facingRight;
    }
}