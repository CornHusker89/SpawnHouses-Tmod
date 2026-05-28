using SpawnHouses.Legacy.Structures;
using SpawnHouses.Structures.StructureParts;

namespace SpawnHouses.Structures.ChainStructures;

public sealed class TestChainStructure2 : CustomChainStructure {
    public TestChainStructure2(ushort x = 0, ushort y = 0, byte status = StructureStatus.NotGenerated, sbyte cost = -1,
        ushort weight = 10) :
        base("Structures/StructureFiles/chainTest2",
            15,
            8,
            [
                // top
                [
                    new ChainConnectPoint(7, 0, LegacyDirections.Up)
                ],

                // bottom
                [
                    new ChainConnectPoint(7, 7, LegacyDirections.Down)
                ],

                // left
                [
                    new ChainConnectPoint(0, 7, LegacyDirections.Left, null, true)
                ],

                // right
                [
                    new ChainConnectPoint(14, 7, LegacyDirections.Right)
                ]
            ],
            x, y, status, cost, weight) {
    }
}