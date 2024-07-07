using System;
using System.Collections;
using Terraria.ID;
using Terraria;

using SpawnHouses.Structures;
using SpawnHouses.Structures.StructureParts;

namespace SpawnHouses.Structures.ChainStructures.MainBasement;

public class MainBasement_Entry1 : CustomChainStructure
{
    // constants
    public static readonly string _filePath = "Structures/StructureFiles/mainBasement/mainBasement_Entry1";
    public static readonly ushort _structureXSize = 10;
    public static readonly ushort _structureYSize = 16;

    public static readonly byte _boundingBoxMargin = 0;
    
    public static readonly Floor[] _floors = [];
    
    public static readonly ChainConnectPoint[][] _connectPoints =
    [
        // top
        [
            new ChainConnectPoint(4, 0, Directions.Up, null, true),
        ],
        
        // bottom
        [],
        
        // left
        [
            new ChainConnectPoint(0, 15, Directions.Left, new Seal.MainBasement_SealWall(), false),
        ],
        
        // right
        [
            new ChainConnectPoint(9, 15, Directions.Right, new Seal.MainBasement_SealWall(), false),
        ]
    ];
    
    public MainBasement_Entry1(sbyte cost, ushort weight, Bridge[] childBridgeType, ushort x = 1, ushort y = 1) : 
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
    
    protected override void SetSubstructurePositions()
    {
        foreach (var floor in Floors)
            floor.SetPosition(mainStructureX: X, mainStructureY: Y);
        for (byte direction = 0; direction < 4; direction++)
            foreach (var connectPoint in ConnectPoints[direction])
                connectPoint.SetPosition(mainStructureX: X, mainStructureY: Y);
        
        StructureBoundingBoxes =
        [
            new BoundingBox(X - BoundingBoxMargin - 100, Y - BoundingBoxMargin, X + StructureXSize + 100 + BoundingBoxMargin - 1, Y + 6 + BoundingBoxMargin - 1),
            new BoundingBox(X - BoundingBoxMargin, Y - 7, X + StructureXSize + BoundingBoxMargin - 1, Y + StructureYSize + BoundingBoxMargin - 1)
        ];
    }
    
    public override void Generate()
    {
        _GenerateStructure();
        FrameTiles();
    }

    public override MainBasement_Entry1 Clone()
    {
        return new MainBasement_Entry1(Cost, Weight, ChildBridgeTypes, X, Y);
    }
}