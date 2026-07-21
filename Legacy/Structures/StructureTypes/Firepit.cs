using Microsoft.Xna.Framework;
using SpawnHouses.Legacy.Helpers;
using SpawnHouses.Legacy.Structures.StructureParts;
using Terraria;
using Terraria.ID;
using Terraria.WorldBuilding;

namespace SpawnHouses.Legacy.Structures.StructureTypes;

public sealed class Firepit : LegacyStructure {
    // constants
    public static readonly string _filePath = "Content/Assets/StructureFiles/firepit.shstruct";
    public static readonly ushort _structureXSize = 7;
    public static readonly ushort _structureYSize = 3;

    public static readonly ConnectPoint[][] _connectPoints = [
        // top
        [],

        // bottom
        [],

        // left
        [
            new ConnectPoint(-1, 2, LegacyDirections.Left)
        ],

        // right
        [
            new ConnectPoint(7, 2, LegacyDirections.Right)
        ]
    ];

    public Firepit(ushort x = 0, ushort y = 0, byte status = StructureStatus.NotGenerated) :
        base(_filePath, _structureXSize, _structureYSize,
            CopyConnectPoints(_connectPoints), status, x, y) {
    }

    public override void Generate(bool bare = false) {
        WorldUtils.Gen(new Point(BoundingBox.Left, BoundingBox.Top - 9), new Shapes.Rectangle(7, 9),
            new Actions.ClearTile());

        ushort blendTileID = Main.tile[BoundingBox.Left + 3, BoundingBox.Top + 7].TileType;
        if (blendTileID == TileID.ShellPile)
            blendTileID = TileID.Sand;

        StructureGenHelper.Blend(ConnectPoints[2][0], 5, blendTileID);
        StructureGenHelper.Blend(ConnectPoints[3][0], 5, blendTileID, blendLeftSide: false);

        // make sure that blending doesn't fuck up the tiles next to the chairs
        Tile tile = Main.tile[BoundingBox.Left - 1, BoundingBox.Top + 2];
        tile.Slope = SlopeType.Solid;
        tile.IsHalfBlock = false;
        tile = Main.tile[BoundingBox.Left + 7, BoundingBox.Top + 2];
        tile.Slope = SlopeType.Solid;
        tile.IsHalfBlock = false;

        ushort leftX = (ushort)(BoundingBox.Left - Terraria.WorldGen.genRand.Next(2, 6));
        ushort rightX = (ushort)(BoundingBox.Left + 6 + Terraria.WorldGen.genRand.Next(2, 6));
        ushort curLeftY = (ushort)(BoundingBox.Top - 8);
        ushort curRightY = (ushort)(BoundingBox.Top - 8);
        while (!Terraria.WorldGen.SolidTile(leftX, curLeftY))
            curLeftY++;
        while (!Terraria.WorldGen.SolidTile(rightX, curRightY))
            curRightY++;

        if (Terraria.WorldGen.genRand.Next(0, 3) != 0) // 2/3 chance
            Terraria.WorldGen.PlaceTile(leftX, curLeftY - 1, TileID.BeachPiles, true);
        if (Terraria.WorldGen.genRand.Next(0, 3) != 0)
            Terraria.WorldGen.PlaceTile(rightX, curRightY - 1, TileID.BeachPiles, true);

        _GenerateStructure();
        FrameTiles(BoundingBox.Left + 3, BoundingBox.Top + 1, 3);
    }
}