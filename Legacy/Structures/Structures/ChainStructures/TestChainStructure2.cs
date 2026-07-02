using SpawnHouses.Helpers;
using SpawnHouses.Types;

namespace SpawnHouses.Structures.Structures.ChainStructures;

public sealed class TestChainStructure2 : CustomChainStructure {
    public TestChainStructure2(ushort x = 0, ushort y = 0, byte status = StructureStatus.NotGenerated, sbyte cost = -1,
        ushort weight = 10) :
        base("Assets/StructureFiles/chainTest2.shstruct",
            15,
            8,
            [
                // top
                [
                    new ChainConnectPoint(7, 0, Directions.Up)
                ],

                // bottom
                [
                    new ChainConnectPoint(7, 7, Directions.Down)
                ],

                // left
                [
                    new ChainConnectPoint(0, 7, Directions.Left, null, true)
                ],

                // right
                [
                    new ChainConnectPoint(14, 7, Directions.Right)
                ]
            ],
            x, y, status, cost, weight) {
    }
}