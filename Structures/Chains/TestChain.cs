using SpawnHouses.Structures.Bridges;
using SpawnHouses.Structures.ChainStructures;
using Terraria.DataStructures;

namespace SpawnHouses.Structures.Chains;

public class TestChain : StructureChain
{
    public static Bridge _bridge = new ParabolaBridge.TestBridgeSmall();
    
    public static CustomChainStructure[] _structureList =
    [
        new TestChainStructure(cost: 10, weight: 100)
    ];
    
    public TestChain(ushort x, ushort y) : 
        base(100, 60, _structureList, x, y, 3, 7, [_bridge]) {}
}