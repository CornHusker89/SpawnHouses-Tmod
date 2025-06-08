using Microsoft.Xna.Framework;
using SpawnHouses.Helpers;
using SpawnHouses.Types;
using Terraria;
using Terraria.ID;

namespace SpawnHouses.Structures.Structures;

public sealed class BeachHouse : CustomStructure {
    // constants
    public static readonly string _filePath = "Assets/StructureFiles/beachHouse/beachHouse_v2";
    public static readonly string _filePath_r = "Assets/StructureFiles/beachHouse/beachHouse_v2_r";
    public static readonly ushort _structureXSize = 35;
    public static readonly ushort _structureYSize = 26;

    public static readonly ConnectPoint[][] _connectPoints = [
        // top
        [],

        // bottom
        [],

        // left
        [],

        // right
        [
            new ConnectPoint(34, 31, Directions.Right)
        ]
    ];

    public static readonly ConnectPoint[][] _connectPoints_r = [
        // top
        [],

        // bottom
        [],

        // left
        [
            new ConnectPoint(0, 31, Directions.Left)
        ],

        // right
        []
    ];


    public readonly bool Reverse;

    public BeachHouse(ushort x = 0, ushort y = 0, byte status = StructureStatus.NotGenerated, bool reverse = false) :
        base(!reverse ? _filePath : _filePath_r, _structureXSize, _structureYSize,
            CopyConnectPoints(!reverse ? _connectPoints : _connectPoints_r), status, x, y) {
        Reverse = reverse;
        Status = status;
    }

    public override void OnFound() {
        Status = StructureStatus.GeneratedAndFound;

        if (!Reverse) {
            Terraria.WorldGen.PlaceTile(X + 16, Y + 20, TileID.Beds, true, true, style: 22);
            NetMessage.SendTileSquare(-1, X + 15, Y + 19, 4, 2);

            Terraria.WorldGen.PlaceTile(X + 14, Y + 28, TileID.Chairs, true, true, style: 0);
            NetMessage.SendTileSquare(-1, X + 14, Y + 27, 1, 2);
        }
        else {
            Terraria.WorldGen.PlaceTile(X + 17, Y + 20, TileID.Beds, true, true, style: 22);
            NetMessage.SendTileSquare(-1, X + 16, Y + 19, 4, 2);

            Terraria.WorldGen.PlaceTile(X + 20, Y + 28, TileID.Chairs, true, true, style: 0);
            NetMessage.SendTileSquare(-1, X + 20, Y + 27, 1, 2);
        }
    }

    public override void Generate(bool bare = false) {
        Tile beamTile = new() {
            HasTile = true,
            TileType = TileID.RichMahoganyBeam,
            TileColor = PaintID.BrownPaint
        };
        if (!Reverse) {
            StructureGenHelper.Blend(ConnectPoints[3][0], 8, TileID.Sand, blendLeftSide: false);
            StructureGenHelper.GenerateBeams(new Point(X + 1, Y + 30), beamTile, 4, 3);
            StructureGenHelper.GenerateFoundation(new Point(X + 22, Y + 34), TileID.Sand, 11);
        }
        else {
            StructureGenHelper.Blend(ConnectPoints[2][0], 8, TileID.Sand);
            StructureGenHelper.GenerateBeams(new Point(X + 25, Y + 30), beamTile, 4, 3);
            StructureGenHelper.GenerateFoundation(new Point(X + 12, Y + 34), TileID.Sand, 11);
        }

        _GenerateStructure();
        FrameTiles();

        Status = StructureStatus.GeneratedButNotFound;
    }
}