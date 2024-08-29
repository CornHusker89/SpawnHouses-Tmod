using Microsoft.Xna.Framework;
using SpawnHouses.Structures.StructureParts;
using Terraria.ID;
using Terraria.WorldBuilding;
using BoundingBox = SpawnHouses.Structures.StructureParts.BoundingBox;

namespace SpawnHouses.Structures.Structures.ChainStructures.MainBasement;

public class MainBasement_Hallway5 : CustomChainStructure
{
    // constants
    public static readonly string _filePath = "Structures/StructureFiles/mainBasement/mainBasement_Hallway5";
    public static readonly ushort _structureXSize = 8;
    public static readonly ushort _structureYSize = 22;

    public static readonly byte _boundingBoxMargin = 0;
    
    public static readonly Floor[] _floors = [];
    
    public static readonly ChainConnectPoint[][] _connectPoints =
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
    
    public MainBasement_Hallway5(sbyte cost, ushort weight, Bridge[] childBridgeType, ushort x = 1000, ushort y = 1000) : 
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
            new BoundingBox(X - _boundingBoxMargin, Y - _boundingBoxMargin, X + StructureXSize + _boundingBoxMargin - 1, Y + 7 + _boundingBoxMargin - 1),
            new BoundingBox(X + 1 - _boundingBoxMargin, Y + 7 - _boundingBoxMargin, X - 1 + StructureXSize + _boundingBoxMargin - 1, Y + StructureYSize + _boundingBoxMargin - 1)
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

    public override MainBasement_Hallway5 Clone()
    {
        return new MainBasement_Hallway5(Cost, Weight, ChildBridgeTypes, X, Y);
    }
}