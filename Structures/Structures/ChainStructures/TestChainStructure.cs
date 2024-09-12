using System;
using System.Collections;
using Terraria.ID;
using Terraria;

using SpawnHouses.Structures;
using SpawnHouses.Structures.StructureParts;

namespace SpawnHouses.Structures.ChainStructures;

public sealed class TestChainStructure : CustomChainStructure
{
    // constants
    private static readonly string _filePath = "Structures/StructureFiles/chainTest";
    private static readonly ushort _structureXSize = 15;
    private static readonly ushort _structureYSize = 13;

    private static readonly Floor[] _floors = [];
    
    private static readonly ChainConnectPoint[][] _connectPoints =
    [
        // top
        [],
        
        // bottom
        [],
        
        // left
        [
            new ChainConnectPoint(0, 6, Directions.Left, null, true)
        ],
        
        // right
        [
            new ChainConnectPoint(14, 6, Directions.Right),
            new ChainConnectPoint(14, 12, Directions.Right)
        ]
    ];

    public TestChainStructure(sbyte cost, ushort weight, Bridge[] childBridgeType, byte status = StructureStatus.NotGenerated,
        ushort x = 1000, ushort y = 1000) :
        base(_filePath, _structureXSize, _structureYSize, CopyFloors(_floors),
            CopyChainConnectPoints(_connectPoints), childBridgeType, status, x, y, cost, weight)
    {
        ID = StructureID.TestChainStructure;
        SetSubstructurePositions();
    }
    
    public override TestChainStructure Clone()
    {
        return new TestChainStructure(Cost, Weight, ChildBridgeTypes, Status, X, Y);
    }
}