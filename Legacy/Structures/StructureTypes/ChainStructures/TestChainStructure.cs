using SpawnHouses.Legacy.Helpers;
using SpawnHouses.Legacy.Structures.StructureParts;

namespace SpawnHouses.Legacy.Structures.StructureTypes.ChainStructures;

public sealed class TestChainStructure : LegacyChainStructure {
    public TestChainStructure(ushort x = 0, ushort y = 0, byte status = StructureStatus.NotGenerated, sbyte cost = -1,
        ushort weight = 10) :
        base("StructureCommon/Assets/StructureFiles/chainTest.shstruct",
            15,
            13,
            [
                // top
                [],

                // bottom
                [],

                // left
                [
                    new ChainConnectPoint(0, 6, LegacyDirections.Left, null, true)
                ],

                // right
                [
                    new ChainConnectPoint(14, 6, LegacyDirections.Right),
                    new ChainConnectPoint(14, 12, LegacyDirections.Right)
                ]
            ],
            x, y, status, cost, weight) {
    }
}