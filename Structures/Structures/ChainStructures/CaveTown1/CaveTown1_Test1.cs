using SpawnHouses.Structures.StructureParts;

namespace SpawnHouses.Structures.Structures.ChainStructures.caveTown1;

public sealed class CaveTown1_Test1 : CaveTown1_CustomChainStructure
{
    // constants
    private static readonly string _filePath = "Structures/StructureFiles/caveTown1/caveTown1_Test1";
    private static readonly ushort _structureXSize = 30;
    private static readonly ushort _structureYSize = 16;
    
    private static readonly Floor[] _floors = [];
    
    private static readonly ChainConnectPoint[][] _connectPoints =
    [
        // top
        [],
        
        // bottom
        [],
        
        // left
        [
            new ChainConnectPoint(0, 7, Directions.Left),
            new ChainConnectPoint(0, 15, Directions.Left, rootPoint: true)
        ],
        
        // right
        [
            new ChainConnectPoint(29, 15, Directions.Right)
        ]
    ];

    public CaveTown1_Test1(sbyte cost, ushort weight, Bridge[] childBridgeType, ushort x = 1000, ushort y = 1000) :
        base(_filePath, _structureXSize, _structureYSize, CopyFloors(_floors),
            CopyChainConnectPoints(_connectPoints), childBridgeType, x, y, cost, weight)
    {
        SetSubstructurePositions();
    }
    
    public override CaveTown1_Test1 Clone()
    {
        return new CaveTown1_Test1(Cost, Weight, ChildBridgeTypes, X, Y);
    }
}