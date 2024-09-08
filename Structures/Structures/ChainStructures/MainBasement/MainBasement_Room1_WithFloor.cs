using Microsoft.Xna.Framework;
using SpawnHouses.Structures.StructureParts;
using Terraria.ID;
using Terraria.WorldBuilding;

namespace SpawnHouses.Structures.Structures.ChainStructures.MainBasement;

public sealed class MainBasement_Room1_WithFloor : CustomChainStructure
{
    // constants
    public static readonly string _filePath = "Structures/StructureFiles/mainBasement/mainBasement_Room1_WithFloor";
    public static readonly ushort _structureXSize = 22;
    public static readonly ushort _structureYSize = 9;

    public static readonly Floor[] _floors = 
    [
        new Floor(0, 8, 22)
    ];
    
    public static readonly ChainConnectPoint[][] _connectPoints =
    [
        // top
        [],
        
        // bottom
        [
            new ChainConnectPoint(10, 8, Directions.Down, new Seal.MainBasement_SealFloor(), false),
        ],
        
        // left
        [
            new ChainConnectPoint(0, 8, Directions.Left, new Seal.MainBasement_SealWall(), true),
        ],
        
        // right
        [
            new ChainConnectPoint(21, 8, Directions.Right, new Seal.MainBasement_SealWall(), false),
        ]
    ];

    public MainBasement_Room1_WithFloor(sbyte cost, ushort weight, Bridge[] childBridgeType, ushort x = 1000,
        ushort y = 1000) :
        base(_filePath, _structureXSize, _structureYSize, CopyFloors(_floors),
            CopyChainConnectPoints(_connectPoints), childBridgeType, x, y, cost, weight)
    {
        ID = StructureID.MainHouseBasement_Room1_WithFloor;
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

    public override MainBasement_Room1_WithFloor Clone()
    {
        return new MainBasement_Room1_WithFloor(Cost, Weight, ChildBridgeTypes, X, Y);
    }
}