using SpawnHouses.Helpers;
using SpawnHouses.Types;

namespace SpawnHouses.Structures.Structures.ChainStructures;

public sealed class TestChainStructure : CustomChainStructure {
    public TestChainStructure(ushort x = 0, ushort y = 0, byte status = StructureStatus.NotGenerated, sbyte cost = -1,
        ushort weight = 10) :
        base("Assets/StructureFiles/chainTest.shstruct",
            15,
            13,
            [
                // top
                [],

                // bottom
                [],

                // left
                [
                    new ChainConnectPoint(0, 6, Directions.Left, null, true)
                ],

                // right
                [
                    new ChainConnectPoint(14, 6, Directions.Right),
                    new ChainConnectPoint(14, 12, Directions.Right)
                ]
            ],
            x, y, status, cost, weight) {
    }
}