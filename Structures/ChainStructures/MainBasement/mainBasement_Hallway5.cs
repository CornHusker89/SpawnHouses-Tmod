using System;
using System.Collections;
using Terraria.ID;
using Terraria;

using SpawnHouses.Structures;
using SpawnHouses.Structures.StructureParts;

namespace SpawnHouses.Structures.ChainStructures.MainBasement;

public class MainBasement_Hallway5 : CustomChainStructure
{
    // constants
    private static readonly string _filePath = "Structures/StructureFiles/mainBasement/mainBasement_Hallway5";
    private static readonly ushort _structureXSize = 8;
    private static readonly ushort _structureYSize = 17;

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
            new ChainConnectPoint(0, 6, true, true),
            new ChainConnectPoint(1, 16, true, false)
        ],
        
        // right
        [
            new ChainConnectPoint(7, 6, false, false),
            new ChainConnectPoint(6, 16, false, false),
        ]
    ];
    
    public override string FilePath => _filePath;
    public sealed override ushort StructureXSize => _structureXSize;
    public sealed override ushort StructureYSize => _structureYSize;
    
    public MainBasement_Hallway5(sbyte cost, Bridge[] childBridgeType, ushort x = 1, ushort y = 1) : 
        base(_filePath,  _structureXSize,  _structureYSize, CopyFloors(_floors), 
            CopyChainConnectPoints(_connectPoints), childBridgeType, x, y, cost)
    {
        X = x;
        Y = y;
        Cost = cost;
        BoundingBoxMargin = _boundingBoxMargin;
        
        StructureBoundingBoxes =
        [
            new BoundingBox(x - _boundingBoxMargin, y - _boundingBoxMargin, x + StructureXSize + _boundingBoxMargin - 1, y + StructureYSize + _boundingBoxMargin - 1)
        ];
            
        SetSubstructurePositions();
    }
    
    public override void Generate()
    {
        _GenerateStructure();
        FrameTiles();
    }

    public override MainBasement_Hallway5 Clone()
    {
        return new MainBasement_Hallway5(Cost, ChildBridgeTypes, X, Y);
    }
}