using System;
using System.Collections;
using Terraria.ID;
using Terraria;

using SpawnHouses.Structures;
using SpawnHouses.Structures.StructureParts;

namespace SpawnHouses.Structures.ChainStructures;

public class TestChainStructure2 : CustomChainStructure
{
    // constants
    private static readonly string _filePath = "Structures/StructureFiles/chainTest2";
    private static readonly ushort _structureXSize = 15;
    private static readonly ushort _structureYSize = 8;

    private static readonly byte _boundingBoxMargin = 0;
    
    private static readonly Floor[] _floors = [];
    
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

    public TestChainStructure2(sbyte cost, ushort weight, Bridge[] childBridgeType, ushort x = 1000, ushort y = 1000) : 
        base(_filePath,  _structureXSize,  _structureYSize, CopyFloors(_floors), 
            CopyChainConnectPoints(_connectPoints), childBridgeType, x, y, cost, weight)
    {
        FilePath = _filePath;
        StructureXSize = _structureXSize;
        StructureYSize = _structureYSize;
        
        X = x;
        Y = y;
        Cost = cost;
        Weight = weight;
        BoundingBoxMargin = _boundingBoxMargin;
        
        SetSubstructurePositions();
    }
    
    public override void Generate()
    {
        _GenerateStructure();
        FrameTiles();
    }

    public override TestChainStructure2 Clone()
    {
        return new TestChainStructure2(Cost, Weight,  ChildBridgeTypes, X, Y);
    }
}