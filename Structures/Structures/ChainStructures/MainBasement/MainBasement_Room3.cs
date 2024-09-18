using Microsoft.Xna.Framework;
using SpawnHouses.Structures.StructureParts;
using Terraria.ID;
using Terraria.WorldBuilding;

namespace SpawnHouses.Structures.Structures.ChainStructures.MainBasement;

public sealed class MainBasement_Room3 : CustomChainStructure
{
    // constants
    public static readonly string _filePath = "Structures/StructureFiles/mainBasement/mainBasement_Room3";
    public static readonly ushort _structureXSize = 10;
    public static readonly ushort _structureYSize = 7;
    
    public static readonly ChainConnectPoint[][] _connectPoints =
    [
        // top
        [],
        
        // bottom
        [],
        
        // left
        [
            new ChainConnectPoint(0, 6, Directions.Left, new Seal.MainBasement_SealWall(), true),
        ],
        
        // right
        [
            new ChainConnectPoint(9, 6, Directions.Right, new Seal.MainBasement_SealWall(), false),
        ]
    ];

    public MainBasement_Room3(sbyte cost, ushort weight, Bridge[] childBridgeType, byte status = StructureStatus.NotGenerated,
        ushort x = 1000, ushort y = 1000) :
        base(_filePath, _structureXSize, _structureYSize,
            CopyChainConnectPoints(_connectPoints), childBridgeType, status, x, y, cost, weight)
    {
        ID = StructureID.MainHouseBasement_Room3;
        SetSubstructurePositions();
    }
    
    public override void Generate()
    {
        base.Generate();
        GenHelper.GenerateCobwebs(new Point(X, Y), StructureXSize, _structureYSize);
        
        int centerX = X + (StructureXSize / 2);
        int centerY = Y + (StructureXSize / 2);
        WorldUtils.Gen(new Point(centerX, centerY), new Shapes.Circle(StructureXSize + StructureYSize), Actions.Chain(
            new Modifiers.OnlyWalls(WallID.DirtUnsafe, WallID.GrassUnsafe),
            new Actions.PlaceTile(TileID.Dirt)
        ));
        
        FrameTiles();
    }

    public override MainBasement_Room3 Clone()
    {
        return new MainBasement_Room3(Cost, Weight, ChildBridgeTypes, Status, X, Y);
    }
}