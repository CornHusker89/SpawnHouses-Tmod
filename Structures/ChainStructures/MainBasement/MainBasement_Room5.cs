using System;
using System.Collections;
using Terraria.ID;
using Terraria;

using SpawnHouses.Structures;
using SpawnHouses.Structures.StructureParts;
using Terraria.ModLoader;

namespace SpawnHouses.Structures.ChainStructures.MainBasement;

public class MainBasement_Room5 : CustomChainStructure
{
    // constants
    public static readonly string _filePath = "Structures/StructureFiles/mainBasement/mainBasement_Room5";
    public static readonly string _filePath_MagicStorage = "Structures/StructureFiles/mainBasement/mainBasement_Room5_MagicStorage";
    
    public static readonly ushort _structureXSize = 22;
    public static readonly ushort _structureYSize = 9;

    public static readonly sbyte _boundingBoxMargin = 0;
    
    public static readonly Floor[] _floors = 
    [
        new Floor(0, 8, 22)
    ];
    
    public static readonly ChainConnectPoint[][] _connectPoints =
    [
        // top
        [],
        
        // bottom
        [],
        
        // left
        [
            new ChainConnectPoint(0, 8, Directions.Left, new Seal.MainBasement_SealWall(), true),
        ],
        
        // right
        [
            new ChainConnectPoint(21, 8, Directions.Right, new Seal.MainBasement_SealWall(), false),
        ]
    ];
    
    public MainBasement_Room5(sbyte cost, ushort weight, Bridge[] childBridgeType, ushort x = 1000, ushort y = 1000) : 
        base(ModLoader.HasMod("MagicStorage")? _filePath_MagicStorage : _filePath, 
            _structureXSize, _structureYSize, CopyFloors(_floors), 
            CopyChainConnectPoints(_connectPoints), childBridgeType, x, y, cost, weight)
    {
        FilePath = ModLoader.HasMod("MagicStorage")? _filePath_MagicStorage : _filePath;
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

    public override MainBasement_Room5 Clone()
    {
        return new MainBasement_Room5(Cost, Weight, ChildBridgeTypes, X, Y);
    }
}