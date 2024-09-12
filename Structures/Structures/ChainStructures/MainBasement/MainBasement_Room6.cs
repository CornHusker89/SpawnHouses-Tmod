using Microsoft.Xna.Framework;
using SpawnHouses.Structures.StructureParts;
using Terraria.ID;
using Terraria.WorldBuilding;
using BoundingBox = SpawnHouses.Structures.StructureParts.BoundingBox;

namespace SpawnHouses.Structures.Structures.ChainStructures.MainBasement;

public sealed class MainBasement_Room6 : CustomChainStructure
{
    // constants
    public static readonly string _filePath = "Structures/StructureFiles/mainBasement/mainBasement_Room6";
    public static readonly ushort _structureXSize = 28;
    public static readonly ushort _structureYSize = 15;

    public static readonly Floor[] _floors = 
    [
        new Floor(0, 7, 16),
        new Floor(0, 14, 28)
    ];
    
    public static readonly ChainConnectPoint[][] _connectPoints =
    [
        // top
        [],
        
        // bottom
        [
            new ChainConnectPoint(15, 14, Directions.Down, new Seal.MainBasement_SealFloor(), false),
        ],
        
        // left
        [
            new ChainConnectPoint(0, 6, Directions.Left, new Seal.MainBasement_SealWall(), true),
        ],
        
        // right
        [
            new ChainConnectPoint(15, 6, Directions.Right, new Seal.MainBasement_SealWall(), false),
        ]
    ];

    public MainBasement_Room6(sbyte cost, ushort weight, Bridge[] childBridgeType, byte status = StructureStatus.NotGenerated,
        ushort x = 1000, ushort y = 1000) :
        base(_filePath, _structureXSize, _structureYSize, CopyFloors(_floors),
            CopyChainConnectPoints(_connectPoints), childBridgeType, status, x, y, cost, weight)
    {
        ID = StructureID.MainHouseBasement_Room6;
        SetSubstructurePositions();
    }
    
    protected override void SetSubstructurePositions()
    {
        base.SetSubstructurePositions();
        
        StructureBoundingBoxes =
        [
            new BoundingBox(X - BoundingBoxMargin, Y - BoundingBoxMargin, X + 16 + BoundingBoxMargin - 1, Y + 7 + BoundingBoxMargin - 1),
            new BoundingBox(X - BoundingBoxMargin, Y + 8 - BoundingBoxMargin, X + StructureXSize + BoundingBoxMargin - 1, Y + StructureYSize + BoundingBoxMargin - 1)
        ];
    }
    
    public override void Generate()
    {
        base.Generate();
        Floors[0].GenerateCobwebs(8);
        Floors[1].GenerateCobwebs(8);
        
        int centerX = X + (StructureXSize / 2);
        int centerY = Y + (StructureXSize / 2);
        WorldUtils.Gen(new Point(centerX, centerY), new Shapes.Circle(StructureXSize + StructureYSize), Actions.Chain(
            new Modifiers.OnlyWalls(WallID.DirtUnsafe, WallID.GrassUnsafe),
            new Actions.PlaceTile(TileID.Dirt)
        ));
        
        FrameTiles();
    }

    public override MainBasement_Room6 Clone()
    {
        return new MainBasement_Room6(Cost, Weight, ChildBridgeTypes, Status, X, Y);
    }
}