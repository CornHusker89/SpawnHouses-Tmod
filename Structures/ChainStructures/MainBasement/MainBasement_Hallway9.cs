using System;
using System.Collections;
using Terraria.ID;
using Terraria;

using SpawnHouses.Structures;
using SpawnHouses.Structures.StructureParts;

namespace SpawnHouses.Structures.ChainStructures.MainBasement;

public class MainBasement_Hallway9 : CustomChainStructure
{
    // constants
    private static readonly string _filePath = "Structures/StructureFiles/mainBasement/mainBasement_Hallway9";
    private static readonly ushort _structureXSize = 6;
    private static readonly ushort _structureYSize = 11;

    private static readonly byte _boundingBoxMargin = 0;
    
    private static readonly Floor[] _floors = [];
    
    private static readonly ChainConnectPoint[][] _connectPoints =
    [
        // top
        [],
        
        // bottom
        [
            new ChainConnectPoint(2, 10, Directions.Up, new Seal.MainBasement_SealWall(), false, GenerateChances.Guaranteed)
        ],
        
        // left
        [
            new ChainConnectPoint(0, 5, Directions.Left, new Seal.MainBasement_SealWall(), true, GenerateChances.Guaranteed)
        ],
        
        // right
        [
            new ChainConnectPoint(5, 5, Directions.Right, new Seal.MainBasement_SealWall(), false, GenerateChances.Guaranteed)
        ]
    ];
    
    public override string FilePath => _filePath;
    public sealed override ushort StructureXSize => _structureXSize;
    public sealed override ushort StructureYSize => _structureYSize;
    
    public MainBasement_Hallway9(sbyte cost, ushort weight, Bridge[] childBridgeType, ushort x = 1, ushort y = 1) : 
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

    public override MainBasement_Hallway9 Clone()
    {
        return new MainBasement_Hallway9(Cost, Weight, ChildBridgeTypes, X, Y);
    }
}