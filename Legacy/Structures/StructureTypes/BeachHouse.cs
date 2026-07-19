using SpawnHouses.Legacy.Helpers;
using SpawnHouses.Legacy.Structures.StructureParts;
using Terraria;
using Terraria.DataStructures;
using Terraria.ID;

namespace SpawnHouses.Legacy.Structures.StructureTypes;

public sealed class BeachHouse : LegacyStructure {
    public static readonly string _filePath = "StructureCommon/Assets/StructureFiles/beachHouse/beachHouse_v2.shstruct";
    public static readonly string _filePath_r = "StructureCommon/Assets/StructureFiles/beachHouse/beachHouse_v2_r.shstruct";
    public static readonly ushort _structureXSize = 35;
    public static readonly ushort _structureYSize = 26;

    public static readonly string _filePathNoDeck = "StructureCommon/Assets/StructureFiles/beachHouse/beachHouse_v2_NoDeck.shstruct";
    public static readonly ushort _structureXSizeNoDeck = 24;
    public static readonly ushort _structureYSizeNoDeck = 34;


    public static readonly ConnectPoint[][] _connectPoints = [
        // top
        [],

        // bottom
        [],

        // left
        [],

        // right
        [
            new ConnectPoint(34, 31, LegacyDirections.Right)
        ]
    ];

    public static readonly ConnectPoint[][] _connectPoints_r = [
        // top
        [],

        // bottom
        [],

        // left
        [
            new ConnectPoint(0, 31, LegacyDirections.Left)
        ],

        // right
        []
    ];


    public readonly bool Reverse;
    public bool HasDeck;

    public BeachHouse(ushort x = 0, ushort y = 0, byte status = StructureStatus.NotGenerated, bool reverse = false, bool hasDeck = true) :
        base(hasDeck ? !reverse ? _filePath : _filePath_r : _filePathNoDeck, _structureXSize, _structureYSize,
            CopyConnectPoints(!reverse ? _connectPoints : _connectPoints_r), status, x, y) {
        Reverse = reverse;
        HasDeck = hasDeck;
        Status = status;
    }

    public override bool IsFound(Point16 playerPos) {
        Point16 center = BoundingBox.CenterPoint16;
        return playerPos.X > center.X - 70
               && playerPos.X < center.X + 70
               && playerPos.Y > center.Y - 44
               && playerPos.Y < center.Y + 44;
    }

    public override void OnFound() {
        Status = StructureStatus.GeneratedAndFound;

        if (HasDeck) {
            if (!Reverse) {
                Terraria.WorldGen.PlaceTile(BoundingBox.Left + 16, BoundingBox.Top + 20, TileID.Beds, true, true, style: 22);
                NetMessage.SendTileSquare(-1, BoundingBox.Left + 15, BoundingBox.Top + 19, 4, 2);

                Terraria.WorldGen.PlaceTile(BoundingBox.Left + 14, BoundingBox.Top + 28, TileID.Chairs, true, true, style: 0);
                NetMessage.SendTileSquare(-1, BoundingBox.Left + 14, BoundingBox.Top + 27, 1, 2);
            }
            else {
                Terraria.WorldGen.PlaceTile(BoundingBox.Left + 17, BoundingBox.Top + 20, TileID.Beds, true, true, style: 22);
                NetMessage.SendTileSquare(-1, BoundingBox.Left + 16, BoundingBox.Top + 19, 4, 2);

                Terraria.WorldGen.PlaceTile(BoundingBox.Left + 20, BoundingBox.Top + 28, TileID.Chairs, true, true, style: 0);
                NetMessage.SendTileSquare(-1, BoundingBox.Left + 20, BoundingBox.Top + 27, 1, 2);
            }
        }
    }

    public override void Generate(bool bare = false) {
        if (!bare) {
            Tile beamTile = new() {
                HasTile = true,
                TileType = TileID.RichMahoganyBeam,
                TileColor = PaintID.BrownPaint
            };
            if (!Reverse) {
                StructureGenHelper.Blend(ConnectPoints[3][0], 15, TileID.Sand, blendLeftSide: false);
                StructureGenHelper.GenerateBeams(new Point16(BoundingBox.Left + 1, BoundingBox.Top + 30), beamTile, 4, 3);
                StructureGenHelper.GenerateFoundation(new Point16(BoundingBox.Left + 22, BoundingBox.Top + 34), TileID.Sand, 11);
            }
            else {
                StructureGenHelper.Blend(ConnectPoints[2][0], 15, TileID.Sand);
                StructureGenHelper.GenerateBeams(new Point16(BoundingBox.Left + 25, BoundingBox.Top + 30), beamTile, 4, 3);
                StructureGenHelper.GenerateFoundation(new Point16(BoundingBox.Left + 12, BoundingBox.Top + 34), TileID.Sand, 11);
            }
        }

        _GenerateStructure();
        FrameTiles();

        Status = StructureStatus.GeneratedButNotFound;
    }
}