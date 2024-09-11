using SpawnHouses.Structures.Bridges;
using SpawnHouses.Structures.ChainStructures;
using Terraria.DataStructures;

namespace SpawnHouses.Structures.StructureChains;

public class TestStructure : StructureChain
{
    public static Bridge _bridge = new ParabolaBridge.TestBridgeSmall();
    
    public static CustomChainStructure[] _structureList =
    [
        new TestChainStructure(10, 100, [_bridge])
    ];
    
    public TestStructure(ushort x, ushort y) : 
        base(100, 60, _structureList, x, y, 3, 7) {}
}