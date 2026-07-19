using SpawnHouses.Legacy.Helpers;
using SpawnHouses.Legacy.Structures.StructureParts;
using Terraria.DataStructures;
using Terraria.ID;

namespace SpawnHouses.Legacy.Structures.StructureTypes;

public sealed class BridgeTest : LegacyStructure {
    // constants
    public static readonly string _filePath = "StructureCommon/Assets/StructureFiles/bridgeTest.shstruct";
    public static readonly ushort _structureXSize = 8;
    public static readonly ushort _structureYSize = 9;

    public static readonly ConnectPoint[][] _connectPoints = [
        // top
        [],

        // bottom
        [],

        // left
        [
            new ConnectPoint(0, 0, LegacyDirections.Left)
        ],

        // right
        [
            new ConnectPoint(7, 0, LegacyDirections.Right)
        ]
    ];

    public BridgeTest(ushort x = 0, ushort y = 0, byte status = StructureStatus.NotGenerated) :
        base(_filePath, _structureXSize, _structureYSize,
            CopyConnectPoints(_connectPoints), status, x, y) {
    }

    public override void Generate(bool bare = false) {
        StructureGenHelper.GenerateFoundation(new Point16(BoundingBox.Left, BoundingBox.Top + 9), TileID.Dirt, 4);

        base.Generate();
    }
}