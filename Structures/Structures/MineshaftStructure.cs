using Microsoft.Xna.Framework;
using Terraria.ID;
using SpawnHouses.Structures;



using SpawnHouses.Structures.StructureParts;
using Terraria;
using Terraria.WorldBuilding;
using Actions = Terraria.GameContent.Animations.Actions;

namespace SpawnHouses.Structures.Structures;

public sealed class MineshaftStructure : CustomStructure
{
    
    // constants
    public static readonly string _filePath = "Structures/StructureFiles/mineshaft";
    public static readonly ushort _structureXSize = 21;
    public static readonly ushort _structureYSize = 22;

    public static readonly ConnectPoint[][] _connectPoints =
    [
        // top
        [],
        
        // bottom
        [],
        
        // left
        [
            new ConnectPoint(0, 13, Directions.Left)
        ],
        
        // right
        [
            new ConnectPoint(20, 13, Directions.Right)
        ]
    ];
    
    public MineshaftStructure(ushort x = 0, ushort y = 0, byte status = StructureStatus.NotGenerated) :
        base(_filePath, _structureXSize, _structureYSize, 
            CopyConnectPoints(_connectPoints), status, x, y)
    {
        ID = StructureID.Well;
        SetSubstructurePositions();
    }
    
    public override void Generate()
    {
        ConnectPoints[2][0].BlendLeft(TileID.Grass, 7);
        ConnectPoints[3][0].BlendRight(TileID.Grass, 7);
        
        _GenerateStructure();
        
        Terraria.WorldGen.digTunnel(X + 10, Y + 32, 0, 12, 
            Terraria.WorldGen.genRand.Next(18, 25), Terraria.WorldGen.genRand.Next(7, 9));

        // place rope
        for (int i = 5; i < 300; i++)
        {
            Tile tile = Main.tile[X + 10, Y + i];

            if (Terraria.WorldGen.SolidTile(X + 10, Y + i + 1)) break;
 
            tile.HasTile = true;
            tile.Slope = SlopeType.Solid;
            tile.IsHalfBlock = false;
            tile.TileType = TileID.Rope;
        }
        
        int leftBushX = X - Terraria.WorldGen.genRand.Next(-2, 2);
        int surfaceY = Y + 5;
        while (!Terraria.WorldGen.SolidTile(leftBushX, surfaceY))
            surfaceY++;
        GenHelper.PlaceBush(new Point(leftBushX, surfaceY - 1));
        
        int rightBushX = X + _structureXSize + Terraria.WorldGen.genRand.Next(-2, 2);
        surfaceY = Y + 5;
        while (!Terraria.WorldGen.SolidTile(rightBushX, surfaceY))
            surfaceY++;
        GenHelper.PlaceBush(new Point(rightBushX, surfaceY - 1));
        
        FrameTiles(X + 10, Y + 160, 180);
    }
}