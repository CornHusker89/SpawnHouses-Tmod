using SpawnHouses.Legacy.Structures;
using SpawnHouses.Structures.StructureParts;

namespace SpawnHouses.Structures.ChainStructures;

public sealed class TestChainStructure : CustomChainStructure {
    public TestChainStructure(ushort x = 0, ushort y = 0, byte status = StructureStatus.NotGenerated, sbyte cost = -1,
        ushort weight = 10) :
        base("Structures/StructureFiles/chainTest",
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