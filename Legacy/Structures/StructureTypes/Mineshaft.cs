using Microsoft.Xna.Framework;
using SpawnHouses.Legacy.Helpers;
using SpawnHouses.Legacy.Structures.StructureParts;
using Terraria;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.WorldBuilding;

namespace SpawnHouses.Legacy.Structures.StructureTypes;

public sealed class Mineshaft : LegacyStructure {
    // constants
    public static readonly string _filePath = "Content/Assets/StructureFiles/mineshaft.shstruct";
    public static readonly ushort _structureXSize = 21;
    public static readonly ushort _structureYSize = 22;

    public static readonly ConnectPoint[][] _connectPoints = [
        // top
        [],

        // bottom
        [],

        // left
        [
            new ConnectPoint(0, 13, LegacyDirections.Left)
        ],

        // right
        [
            new ConnectPoint(20, 13, LegacyDirections.Right)
        ]
    ];

    public readonly bool IsLeftSide;

    public Mineshaft(ushort x = 0, ushort y = 0, byte status = StructureStatus.NotGenerated) :
        base(_filePath, _structureXSize, _structureYSize,
            CopyConnectPoints(_connectPoints), status, x, y) {
        // if (StructureManager.LegacyStructures.Find(s => s is MainHouse) is MainHouse mainHouse && mainHouse.BoundingBox.Left > BoundingBox.Left)
        //     IsLeftSide = true;
    }

    public override void Generate(bool bare = false) {
        StructureGenHelper.Blend(ConnectPoints[2][0], 7, TileID.Grass);
        StructureGenHelper.Blend(ConnectPoints[3][0], 7, TileID.Grass, blendLeftSide: false);

        _GenerateStructure();

        int tunnelSteps = Terraria.WorldGen.genRand.Next(7, 11);
        WorldUtils.Gen(new Point(BoundingBox.Left + 9, BoundingBox.Top + 13), // make sure rope can fully generate
            new Shapes.Rectangle(2, 10 + tunnelSteps * 15),
            new Actions.ClearTile(true)
        );
        StructureGenHelper.DigVerticalTunnel(new Point16(BoundingBox.Left + 10, BoundingBox.Top + 14), 3, tunnelSteps);

        // place rope
        for (int i = 5; i < 300; i++) {
            Tile tile = Main.tile[BoundingBox.Left + 10, BoundingBox.Top + i];

            if (Terraria.WorldGen.SolidTile(BoundingBox.Left + 10, BoundingBox.Top + i + 3)) break;

            tile.HasTile = true;
            tile.Slope = SlopeType.Solid;
            tile.IsHalfBlock = false;
            tile.TileType = TileID.Rope;
        }

        int leftBushX = BoundingBox.Left - Terraria.WorldGen.genRand.Next(-2, 2);
        int surfaceY = BoundingBox.Top + 5;
        while (!Terraria.WorldGen.SolidTile(leftBushX, surfaceY))
            surfaceY++;
        StructureGenHelper.PlaceBush(new Point16(leftBushX, surfaceY - 1));

        int rightBushX = BoundingBox.Left + _structureXSize + Terraria.WorldGen.genRand.Next(-2, 2);
        surfaceY = BoundingBox.Top + 5;
        while (!Terraria.WorldGen.SolidTile(rightBushX, surfaceY))
            surfaceY++;
        StructureGenHelper.PlaceBush(new Point16(rightBushX, surfaceY - 1));
        FrameTiles(BoundingBox.Left + 10, BoundingBox.Top + 160, 180);

        Status = StructureStatus.GeneratedAndFound;
    }
}