using Microsoft.Xna.Framework;
using SpawnHouses.Structures.StructureParts;
using Terraria;
using Terraria.ID;
using Terraria.WorldBuilding;

namespace SpawnHouses.Structures.Structures;

public sealed class Firepit : CustomStructure {
    // constants
    public static readonly string _filePath = "Structures/StructureFiles/firepit";
    public static readonly ushort _structureXSize = 7;
    public static readonly ushort _structureYSize = 3;

    public static readonly ConnectPoint[][] _connectPoints = [
        // top
        [],

        // bottom
        [],

        // left
        [
            new ConnectPoint(-1, 2, Directions.Left)
        ],

        // right
        [
            new ConnectPoint(7, 2, Directions.Right)
        ]
    ];

    public Firepit(ushort x = 0, ushort y = 0, byte status = StructureStatus.NotGenerated) :
        base(_filePath, _structureXSize, _structureYSize,
            CopyConnectPoints(_connectPoints), status, x, y) {
    }

    public override void Generate() {
        WorldUtils.Gen(new Point(X, Y - 9), new Shapes.Rectangle(7, 9),
            new Actions.ClearTile());

        var blendTileID = Main.tile[X + 3, Y + 7].TileType;
        if (blendTileID == TileID.ShellPile)
            blendTileID = TileID.Sand;

        StructureGenHelper.Blend(ConnectPoints[2][0], 5, blendTileID);
        StructureGenHelper.Blend(ConnectPoints[3][0], 5, blendTileID, blendLeftSide: false);

        // make sure that blending doesn't fuck up the tiles next to the chairs
        var tile = Main.tile[X - 1, Y + 2];
        tile.Slope = SlopeType.Solid;
        tile.IsHalfBlock = false;
        tile = Main.tile[X + 7, Y + 2];
        tile.Slope = SlopeType.Solid;
        tile.IsHalfBlock = false;

        var leftX = (ushort)(X - Terraria.WorldGen.genRand.Next(2, 6));
        var rightX = (ushort)(X + 6 + Terraria.WorldGen.genRand.Next(2, 6));
        var curLeftY = (ushort)(Y - 8);
        var curRightY = (ushort)(Y - 8);
        while (!Terraria.WorldGen.SolidTile(leftX, curLeftY))
            curLeftY++;
        while (!Terraria.WorldGen.SolidTile(rightX, curRightY))
            curRightY++;

        if (Terraria.WorldGen.genRand.Next(0, 3) != 0) // 2/3 chance
            Terraria.WorldGen.PlaceTile(leftX, curLeftY - 1, TileID.BeachPiles, true);
        if (Terraria.WorldGen.genRand.Next(0, 3) != 0)
            Terraria.WorldGen.PlaceTile(rightX, curRightY - 1, TileID.BeachPiles, true);

        _GenerateStructure();
        FrameTiles(X + 3, Y + 1, 3);
    }
}