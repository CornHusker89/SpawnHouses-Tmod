using Microsoft.Xna.Framework;
using SpawnHouses.Structures.StructureParts;
using Terraria.ID;
using Terraria.WorldBuilding;

namespace SpawnHouses.Structures.Structures.ChainStructures.MainBasement;

public sealed class MainBasement_Hallway4 : CustomChainStructure
{
    // constants
    public static readonly string _filePath = "Structures/StructureFiles/mainBasement/mainBasement_Hallway4";
    public static readonly ushort _structureXSize = 6;
    public static readonly ushort _structureYSize = 11;

    public static readonly Floor[] _floors = [];
    
    public static readonly ChainConnectPoint[][] _connectPoints =
    [
        // top
        [
            new ChainConnectPoint(2, 0, Directions.Down, new Seal.MainBasement_SealFloor(), true, GenerateChances.Guaranteed)
        ],
        
        // bottom
        [],
        
        // left
        [
            new ChainConnectPoint(0, 10, Directions.Left, new Seal.MainBasement_SealWall(), false, GenerateChances.Guaranteed)
        ],
        
        // right
        [
            new ChainConnectPoint(5, 10, Directions.Right, new Seal.MainBasement_SealWall(), false, GenerateChances.Guaranteed),
        ]
    ];

    public MainBasement_Hallway4(sbyte cost, ushort weight, Bridge[] childBridgeType, byte status = StructureStatus.NotGenerated,
        ushort x = 1000, ushort y = 1000) :
        base(_filePath, _structureXSize, _structureYSize, CopyFloors(_floors),
            CopyChainConnectPoints(_connectPoints), childBridgeType, status, x, y, cost, weight)
    {
        ID = StructureID.MainHouseBasement_Hallway4;
        SetSubstructurePositions();
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

    public override MainBasement_Hallway4 Clone()
    {
        return new MainBasement_Hallway4(Cost, Weight, ChildBridgeTypes, Status, X, Y);
    }
}