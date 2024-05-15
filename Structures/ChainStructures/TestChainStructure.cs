using System.Collections;
using Terraria.ID;
using Terraria;

using SpawnHouses.Structures;
using SpawnHouses.Structures.StructureParts;

namespace SpawnHouses.Structures.StructureChains;

public class TestChainStructure : CustomChainStructure
{
    // constants
    private static readonly string _filePath = "Structures/StructureFiles/chainTest";
    private static readonly ushort _structureXSize = 15;
    private static readonly ushort _structureYSize = 13;
    
    private static readonly Floor[] _floors =
    [
    ];

    private static readonly ChainConnectPoint[] _connectPoints =
    [
        new ChainConnectPoint(0, 6, true, true),
        new ChainConnectPoint(14, 6, false, true),
        new ChainConnectPoint(14, 12, false, true)
    ];
    
    
        
    public override string FilePath => _filePath;
    public sealed override ushort StructureXSize => _structureXSize;
    public sealed override ushort StructureYSize => _structureYSize;
    
    public TestChainStructure(sbyte cost, ushort x = 1, ushort y = 1) : 
        base(_filePath,  _structureXSize,  _structureYSize, _floors, _connectPoints, x, y, cost)
    {
        X = x;
        Y = y;
        Cost = cost;
        StructureBoundingBox = new BoundingBox(x - 3, y - 3, x + StructureXSize + 3, y + StructureYSize + 3);
        SetSubstructurePositions();
    }

    public override TestChainStructure Clone()
    {
        return new TestChainStructure(Cost, X, Y);
    }
}