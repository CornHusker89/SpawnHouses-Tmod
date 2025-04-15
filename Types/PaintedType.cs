using Terraria;
using Terraria.DataStructures;
using Terraria.ID;

namespace SpawnHouses.Types;

public struct PaintedType(ushort type, byte paintType = PaintID.None, short style = -1) {
    public ushort Type = type;
    public byte PaintType = paintType;
    public short Style = style;

    public static PaintedType PickRandom(PaintedType[] paintedTypes) {
        var index = Terraria.WorldGen.genRand.Next(paintedTypes.Length);
        return paintedTypes[index];
    }

    public static void PlaceTile(int x, int y, PaintedType paintedType, BlockType blockType = BlockType.Solid) {
        if (paintedType.Style == -1) {
            var tile = Main.tile[x, y];
            tile.HasTile = true;
            tile.BlockType = blockType;
            tile.TileType = paintedType.Type;
            tile.TileColor = paintedType.PaintType;
        }
        else {
            Terraria.WorldGen.PlaceTile(x, y, paintedType.Type, true, true, style: paintedType.Style);
            var tile = Main.tile[x, y];
            tile.TileColor = paintedType.PaintType;
        }
    }

    public static void PlaceTile(Point16 position, PaintedType paintedType, BlockType blockType = BlockType.Solid) {
        PlaceTile(position.X, position.Y, paintedType, blockType);
    }

    public static void PlaceWall(int x, int y, PaintedType paintedType) {
        var tile = Main.tile[x, y];
        tile.WallType = paintedType.Type;
        tile.WallColor = paintedType.PaintType;
    }

    public static void PlaceWall(Point16 position, PaintedType paintedType) {
        PlaceWall(position.X, position.Y, paintedType);
    }
}