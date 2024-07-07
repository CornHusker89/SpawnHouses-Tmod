using System;
using System.Collections;
using Terraria.ID;
using Terraria;

using SpawnHouses.Structures;
using SpawnHouses.Structures.StructureParts;

namespace SpawnHouses.Structures.ChainStructures;

public class TestChainStructure : CustomChainStructure
{
    // constants
    private static readonly string _filePath = "Structures/StructureFiles/chainTest";
    private static readonly ushort _structureXSize = 15;
    private static readonly ushort _structureYSize = 13;

    private static readonly byte _boundingBoxMargin = 0;
    
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

    public TestChainStructure(sbyte cost, ushort weight, Bridge[] childBridgeType, ushort x = 1, ushort y = 1) : 
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

    public override TestChainStructure Clone()
    {
        return new TestChainStructure(Cost, Weight, ChildBridgeTypes, X, Y);
    }
}