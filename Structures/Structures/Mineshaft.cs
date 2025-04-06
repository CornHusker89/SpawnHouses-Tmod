using Microsoft.Xna.Framework;
using SpawnHouses.Structures.StructureParts;
using Terraria;
using Terraria.ID;
using Terraria.WorldBuilding;

namespace SpawnHouses.Structures.Structures;

public sealed class Mineshaft : CustomStructure {
    // constants
    public static readonly string _filePath = "Structures/StructureFiles/mineshaft";
    public static readonly ushort _structureXSize = 21;
    public static readonly ushort _structureYSize = 22;

    public static readonly ConnectPoint[][] _connectPoints = [
        // top
        [],

        // bottom
        [],

        // left
        [
            new ConnectPoint(0, 13, Directions.Left)
        ],

        // right
        [
            new ConnectPoint(20, 13, Directions.Right)
        ]
    ];

    public readonly bool IsLeftSide;

    public Mineshaft(ushort x = 0, ushort y = 0, byte status = StructureStatus.NotGenerated) :
        base(_filePath, _structureXSize, _structureYSize,
            CopyConnectPoints(_connectPoints), status, x, y) {
        if (SpawnHousesSystem.MainHouse is not null && SpawnHousesSystem.MainHouse.X > X)
            IsLeftSide = true;
    }

    public override void Generate() {
        StructureGenHelper.Blend(ConnectPoints[2][0], 7, TileID.Grass);
        StructureGenHelper.Blend(ConnectPoints[3][0], 7, TileID.Grass, blendLeftSide: false);

        _GenerateStructure();

        var tunnelSteps = Terraria.WorldGen.genRand.Next(7, 11);
        WorldUtils.Gen(new Point(X + 9, Y + 13), // make sure rope can fully generate
            new Shapes.Rectangle(2, 10 + tunnelSteps * 15),
            new Actions.ClearTile(true)
        );
        StructureGenHelper.DigVerticalTunnel(new Point(X + 10, Y + 14), 3, tunnelSteps);

        // place rope
        for (var i = 5; i < 300; i++) {
            var tile = Main.tile[X + 10, Y + i];

            if (Terraria.WorldGen.SolidTile(X + 10, Y + i + 3)) break;

            tile.HasTile = true;
            tile.Slope = SlopeType.Solid;
            tile.IsHalfBlock = false;
            tile.TileType = TileID.Rope;
        }

        var leftBushX = X - Terraria.WorldGen.genRand.Next(-2, 2);
        var surfaceY = Y + 5;
        while (!Terraria.WorldGen.SolidTile(leftBushX, surfaceY))
            surfaceY++;
        StructureGenHelper.PlaceBush(new Point(leftBushX, surfaceY - 1));

        var rightBushX = X + _structureXSize + Terraria.WorldGen.genRand.Next(-2, 2);
        surfaceY = Y + 5;
        while (!Terraria.WorldGen.SolidTile(rightBushX, surfaceY))
            surfaceY++;
        StructureGenHelper.PlaceBush(new Point(rightBushX, surfaceY - 1));
        FrameTiles(X + 10, Y + 160, 180);

        Status = StructureStatus.GeneratedAndFound;
    }
}