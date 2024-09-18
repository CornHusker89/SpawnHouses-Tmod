using System;
using System.Collections;
using Terraria.ID;
using Terraria;

using SpawnHouses.Structures;
using SpawnHouses.Structures.StructureParts;

namespace SpawnHouses.Structures.ChainStructures;

public sealed class TestChainStructure2 : CustomChainStructure
{
    // constants
    private static readonly string _filePath = "Structures/StructureFiles/chainTest2";
    private static readonly ushort _structureXSize = 15;
    private static readonly ushort _structureYSize = 8;

    private static readonly ChainConnectPoint[][] _connectPoints =
    [
        // top
        [
            new ChainConnectPoint(7, 0, Directions.Up)
        ],
        
        // bottom
        [
            new ChainConnectPoint(7, 7, Directions.Down)
        ],
        
        // left
        [
            new ChainConnectPoint(0, 7, Directions.Left, null, true)
        ],
        
        // right
        [
            new ChainConnectPoint(14, 7, Directions.Right)
        ]
    ];

    public TestChainStructure2(sbyte cost, ushort weight, Bridge[] childBridgeType, byte status = StructureStatus.NotGenerated, ushort x = 1000, ushort y = 1000) :
        base(_filePath, _structureXSize, _structureYSize,
            CopyChainConnectPoints(_connectPoints), childBridgeType, status, x, y, cost, weight)
    {
        ID = StructureID.TestChainStructure2;
        SetSubstructurePositions();
    }
    
    public override TestChainStructure2 Clone()
    {
        return new TestChainStructure2(Cost, Weight,  ChildBridgeTypes, Status, X, Y);
    }
}