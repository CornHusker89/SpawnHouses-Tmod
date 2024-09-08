using Microsoft.Xna.Framework;
using SpawnHouses.Structures.StructureParts;
using Terraria.ID;
using Terraria.WorldBuilding;

namespace SpawnHouses.Structures.Structures.ChainStructures.MainBasement;

public sealed class MainBasement_Room2_WithRoof : CustomChainStructure
{
    // constants
    public static readonly string _filePath = "Structures/StructureFiles/mainBasement/mainBasement_Room2_WithRoof";
    public static readonly ushort _structureXSize = 23;
    public static readonly ushort _structureYSize = 7;

    public static readonly Floor[] _floors = 
    [
        new Floor(0, 6, 23)
    ];
    
    public static readonly ChainConnectPoint[][] _connectPoints =
    [
        // top
        [
            new ChainConnectPoint(3, 0, Directions.Up, new Seal.MainBasement_SealRoof(), false),
        ],
        
        // bottom
        [],
        
        // left
        [
            new ChainConnectPoint(0, 6, Directions.Left, new Seal.MainBasement_SealWall(), true),
        ],
        
        // right
        [
            new ChainConnectPoint(22, 6, Directions.Right, new Seal.MainBasement_SealWall(), false),
        ]
    ];

    public MainBasement_Room2_WithRoof(sbyte cost, ushort weight, Bridge[] childBridgeType, ushort x = 1000,
        ushort y = 1000) :
        base(_filePath, _structureXSize, _structureYSize, CopyFloors(_floors),
            CopyChainConnectPoints(_connectPoints), childBridgeType, x, y, cost, weight)
    {
        ID = StructureID.MainHouseBasement_Room2_WithRoof;
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

    public override MainBasement_Room2_WithRoof Clone()
    {
        return new MainBasement_Room2_WithRoof(Cost, Weight, ChildBridgeTypes, X, Y);
    }
}