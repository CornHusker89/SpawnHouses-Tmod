using System;
using System.Collections;
using Terraria.ID;
using Terraria;

using SpawnHouses.Structures;
using SpawnHouses.Structures.StructureParts;

namespace SpawnHouses.Structures.ChainStructures.MainBasement;

public class MainBasement_Room3 : CustomChainStructure
{
    // constants
    public static readonly string _filePath = "Structures/StructureFiles/mainBasement/mainBasement_Room3";
    public static readonly ushort _structureXSize = 10;
    public static readonly ushort _structureYSize = 7;

    public static readonly sbyte _boundingBoxMargin = 0;
    
    public static readonly Floor[] _floors = 
    [
        new Floor(0, 6, 10)
    ];
    
    public static readonly ChainConnectPoint[][] _connectPoints =
    [
        // top
        [],
        
        // bottom
        [],
        
        // left
        [
            new ChainConnectPoint(0, 6, Directions.Left, new Seal.MainBasement_SealWall(), true),
        ],
        
        // right
        [
            new ChainConnectPoint(9, 6, Directions.Right, new Seal.MainBasement_SealWall(), false),
        ]
    ];
    
    public MainBasement_Room3(sbyte cost, ushort weight, Bridge[] childBridgeType, ushort x = 1, ushort y = 1) : 
        base(_filePath, _structureXSize, _structureYSize, CopyFloors(_floors), 
            CopyChainConnectPoints(_connectPoints), childBridgeType, x, y, cost, weight)
    {
        FilePath = _filePath;
        StructureXSize = _structureXSize;
        StructureYSize = _structureYSize;
        
        X = x;
        Y = y;
        Cost = cost;
        Weight = weight;
        BoundingBoxMargin = (byte)_boundingBoxMargin;
            
        SetSubstructurePositions();
    }
    
    public override void Generate()
    {
        _GenerateStructure();
        Floors[0].GenerateCobwebs(StructureYSize);
        FrameTiles();
    }

    public override MainBasement_Room3 Clone()
    {
        return new MainBasement_Room3(Cost, Weight, ChildBridgeTypes, X, Y);
    }
}