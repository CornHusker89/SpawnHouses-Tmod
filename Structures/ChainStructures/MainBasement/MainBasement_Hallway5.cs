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
    private static readonly ushort _structureYSize = 22;

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
            new ChainConnectPoint(0, 6, Directions.Left, new Seal.MainBasement_SealWall(), true, GenerateChances.Guaranteed),
            new ChainConnectPoint(1, 21, Directions.Left, new Seal.MainBasement_SealWall(), false, GenerateChances.Guaranteed)
        ],
        
        // right
        [
            new ChainConnectPoint(7, 6, Directions.Right, new Seal.MainBasement_SealWall(), false, GenerateChances.Guaranteed),
            new ChainConnectPoint(6, 21, Directions.Right, new Seal.MainBasement_SealWall(), false, GenerateChances.Guaranteed),
        ]
    ];
    
    public override string FilePath => _filePath;
    public sealed override ushort StructureXSize => _structureXSize;
    public sealed override ushort StructureYSize => _structureYSize;
    
    public MainBasement_Hallway5(sbyte cost, ushort weight, Bridge[] childBridgeType, ushort x = 1, ushort y = 1) : 
        base(_filePath,  _structureXSize,  _structureYSize, CopyFloors(_floors), 
            CopyChainConnectPoints(_connectPoints), childBridgeType, x, y, cost, weight)
    {
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
            new BoundingBox(X - _boundingBoxMargin, Y - _boundingBoxMargin, X + StructureXSize + _boundingBoxMargin - 1, Y + 7 + _boundingBoxMargin - 1),
            new BoundingBox(X + 1 - _boundingBoxMargin, Y + 7 - _boundingBoxMargin, X - 1 + StructureXSize + _boundingBoxMargin - 1, Y + StructureYSize + _boundingBoxMargin - 1)
        ];
    }
    
    public override void Generate()
    {
        _GenerateStructure();
        FrameTiles();
    }

    public override MainBasement_Hallway5 Clone()
    {
        return new MainBasement_Hallway5(Cost, Weight, ChildBridgeTypes, X, Y);
    }
}