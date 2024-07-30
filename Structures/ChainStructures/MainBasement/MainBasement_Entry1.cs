using System;
using System.Collections;
using Microsoft.Xna.Framework;
using Terraria.ID;
using Terraria;

using SpawnHouses.Structures;
using SpawnHouses.Structures.StructureParts;
using Terraria.WorldBuilding;
using BoundingBox = SpawnHouses.Structures.StructureParts.BoundingBox;

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
    
    public MainBasement_Entry1(sbyte cost, ushort weight, Bridge[] childBridgeType, ushort x = 1000, ushort y = 1000) : 
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
        base.SetSubstructurePositions();
        
        StructureBoundingBoxes =
        [
            new BoundingBox(X - BoundingBoxMargin - 100, Y - BoundingBoxMargin, X + StructureXSize + 100 + BoundingBoxMargin - 1, Y + 6 + BoundingBoxMargin - 1),
            new BoundingBox(X - BoundingBoxMargin, Y + 7, X + StructureXSize + BoundingBoxMargin - 1, Y + StructureYSize + BoundingBoxMargin - 1)
        ];
    }
    
    public override void Generate()
    {
        base.Generate();
        
        int centerX = X + (StructureXSize / 2);
        int centerY = Y + (StructureXSize / 2);
        WorldUtils.Gen(new Point(centerX, centerY), new Shapes.Circle(StructureXSize + StructureYSize), Actions.Chain(
            new Modifiers.OnlyWalls(WallID.DirtUnsafe, WallID.GrassUnsafe),
            new Actions.PlaceTile(TileID.Dirt)
        ));
        
        FrameTiles();
    }

    public override MainBasement_Entry1 Clone()
    {
        return new MainBasement_Entry1(Cost, Weight, ChildBridgeTypes, X, Y);
    }
}