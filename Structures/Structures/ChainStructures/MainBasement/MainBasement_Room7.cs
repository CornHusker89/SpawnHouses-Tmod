using Microsoft.Xna.Framework;
using SpawnHouses.Structures.StructureParts;
using Terraria.ID;
using Terraria.WorldBuilding;

namespace SpawnHouses.Structures.Structures.ChainStructures.MainBasement;

public class MainBasement_Room7 : CustomChainStructure
{
    // constants
    public static readonly string _filePath = "Structures/StructureFiles/mainBasement/mainBasement_Room7";
    public static readonly ushort _structureXSize = 27;
    public static readonly ushort _structureYSize = 12;

    public static readonly sbyte _boundingBoxMargin = 0;
    
    public static readonly Floor[] _floors = 
    [
        new Floor(0, 11, 27)
    ];
    
    public static readonly ChainConnectPoint[][] _connectPoints =
    [
        // top
        [],
        
        // bottom
        [],
        
        // left
        [
            new ChainConnectPoint(0, 11, Directions.Left, new Seal.MainBasement_SealWall(), true),
        ],
        
        // right
        [
            new ChainConnectPoint(26, 11, Directions.Right, new Seal.MainBasement_SealWall(), false),
        ]
    ];
    
    public MainBasement_Room7(sbyte cost, ushort weight, Bridge[] childBridgeType, ushort x = 1000, ushort y = 1000) : 
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
        base.Generate();
        Floors[0].GenerateCobwebs(StructureYSize);
        
        int centerX = X + (StructureXSize / 2);
        int centerY = Y + (StructureXSize / 2);
        WorldUtils.Gen(new Point(centerX, centerY), new Shapes.Circle(StructureXSize + StructureYSize), Actions.Chain(
            new Modifiers.OnlyWalls(WallID.DirtUnsafe, WallID.GrassUnsafe),
            new Actions.PlaceTile(TileID.Dirt)
        ));
        
        FrameTiles();
    }

    public override MainBasement_Room7 Clone()
    {
        return new MainBasement_Room7(Cost, Weight, ChildBridgeTypes, X, Y);
    }
}