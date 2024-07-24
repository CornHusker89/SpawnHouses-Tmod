using SpawnHouses.Structures.Bridges;
using SpawnHouses.Structures.ChainStructures;
using Terraria.DataStructures;

namespace SpawnHouses.Structures.StructureChains;

public class TestStructureChain : StructureChain
{
    public static Bridge _bridge = new ParabolaBridge.TestBridge();
    
    public static CustomChainStructure[] _structureList =
    [
        new TestChainStructure(10, 100, [_bridge])
    ];
    
    public TestStructureChain(ushort x, ushort y) : 
        base(100, 60, _structureList, x, y, 3, 7, null, null, false) {}
}