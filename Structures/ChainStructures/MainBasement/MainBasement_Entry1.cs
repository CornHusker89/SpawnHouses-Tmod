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
    private static readonly string _filePath = "Structures/StructureFiles/mainBasement/mainBasement_Entry1";
    private static readonly ushort _structureXSize = 10;
    private static readonly ushort _structureYSize = 16;

    private static readonly byte _boundingBoxMargin = 0;
    
    private static readonly Floor[] _floors = [];
    
    private static readonly ChainConnectPoint[][] _connectPoints =
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
    
    public override string FilePath => _filePath;
    public sealed override ushort StructureXSize => _structureXSize;
    public sealed override ushort StructureYSize => _structureYSize;
    
    public MainBasement_Entry1(sbyte cost, ushort weight, Bridge[] childBridgeType, ushort x = 1, ushort y = 1) : 
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