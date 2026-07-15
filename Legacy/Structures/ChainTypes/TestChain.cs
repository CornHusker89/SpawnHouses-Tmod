using SpawnHouses.Legacy.Structures.BridgeTypes;
using SpawnHouses.Legacy.Structures.StructureTypes.ChainStructures;

namespace SpawnHouses.Legacy.Structures.ChainTypes;

public class TestChain : LegacyStructureChain {
    public static Bridge _bridge = new ParabolaBridge.TestBridgeSmall();

    public static LegacyChainStructure[] _structureList = [
        new TestChainStructure(cost: 10, weight: 100)
    ];

    public TestChain(ushort x, ushort y) :
        base(60, 100, 3, 7, x, y, _structureList, [_bridge]) {
    }
}