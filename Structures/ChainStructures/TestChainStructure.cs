using System.Collections;
using Microsoft.Build.Tasks;
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
    
    private static readonly Floor[] _floors = [];

    private static readonly ChainConnectPoint[] _connectPoints =
    [
        new ChainConnectPoint(0, 6, true),
        new ChainConnectPoint(14, 6, false),
        new ChainConnectPoint(14, 12, false)
    ];
    
    private static readonly ChainConnectPoint[] _topConnectPoints =
    [
    ];
    
    private static readonly ChainConnectPoint[] _bottomConnectPoints =
    [
    ];
    
    private static readonly ChainConnectPoint[] _leftConnectPoints =
    [
        _connectPoints[0]
    ];
    
    private static readonly ChainConnectPoint[] _rightConnectPoints =
    [
        _connectPoints[1],
        _connectPoints[2]
    ];
    
    
    public override string FilePath => _filePath;
    public sealed override ushort StructureXSize => _structureXSize;
    public sealed override ushort StructureYSize => _structureYSize;
    
    public TestChainStructure(sbyte cost, Bridge childBridgeType, ushort x = 1, ushort y = 1) : 
        base(_filePath,  _structureXSize,  _structureYSize, CopyFloors(_floors), 
            CopyConnectPoints(_connectPoints), CopyConnectPoints(_topConnectPoints),
            CopyConnectPoints(_bottomConnectPoints), CopyConnectPoints(_leftConnectPoints),
            CopyConnectPoints(_rightConnectPoints), childBridgeType, x, y, cost)
    {
        X = x;
        Y = y;
        Cost = cost;
        StructureBoundingBox = new BoundingBox(x - 3, y - 3, x + StructureXSize + 3, y + StructureYSize + 3);
        SetSubstructurePositions();
    }

    public override TestChainStructure Clone()
    {
        return new TestChainStructure(Cost, ChildBridgeType, X, Y);
    }
}