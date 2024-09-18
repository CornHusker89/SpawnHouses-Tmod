using SpawnHouses.Structures.StructureParts;

namespace SpawnHouses.Structures.Structures.ChainStructures.caveTown1;

public sealed class CaveTown1_Test2 : CustomChainStructure
{
    // constants
    private static readonly string _filePath = "Structures/StructureFiles/caveTown1/caveTown1_Test2";
    private static readonly ushort _structureXSize = 25;
    private static readonly ushort _structureYSize = 13;
    
    private static readonly ChainConnectPoint[][] _connectPoints =
    [
        // top
        [],
        
        // bottom
        [],
        
        // left
        [
            new ChainConnectPoint(0, 12, Directions.Left, rootPoint: true)
        ],
        
        // right
        [
            new ChainConnectPoint(24, 12, Directions.Right)
        ]
    ];

    public CaveTown1_Test2(sbyte cost, ushort weight, Bridge[] childBridgeType, byte status = StructureStatus.NotGenerated,
        ushort x = 1000, ushort y = 1000) :
        base(_filePath, _structureXSize, _structureYSize,
            CopyChainConnectPoints(_connectPoints), childBridgeType, status, x, y, cost, weight)
    {
        ID = StructureID.CaveTown1_Test2;
        SetSubstructurePositions();
    }
    
    public override CaveTown1_Test2 Clone()
    {
        return new CaveTown1_Test2(Cost, Weight, ChildBridgeTypes, Status, X, Y);
    }
}