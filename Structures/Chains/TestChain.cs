using SpawnHouses.Structures.Bridges;
using SpawnHouses.Structures.ChainStructures;
using SpawnHouses.Types;

namespace SpawnHouses.Structures.Chains;

public class TestChain : StructureChain {
    public static Bridge _bridge = new ParabolaBridge.TestBridgeSmall();

    public static CustomChainStructure[] _structureList = [
        new TestChainStructure(cost: 10, weight: 100)
    ];

    public TestChain(ushort x, ushort y) :
        base(60, 100, 3, 7, x, y, _structureList, [_bridge]) {
    }
}