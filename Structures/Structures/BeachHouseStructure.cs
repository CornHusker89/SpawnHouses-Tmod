using System;
using Microsoft.Xna.Framework;
using Terraria.ID;
using SpawnHouses.Structures;
using SpawnHouses.Structures.StructureParts;
using Terraria;
using Terraria.WorldBuilding;

namespace SpawnHouses.Structures.Structures;

public class BeachHouseStructure : CustomStructure
{
    // constants
    public static readonly string _filePath = "Structures/StructureFiles/beachHouse/beachHouse_v2";
    public static readonly string _filePath_r = "Structures/StructureFiles/beachHouse/beachHouse_v2_r";
    public static readonly ushort _structureXSize = 35;
    public static readonly ushort _structureYSize = 26;
    
    public static readonly Floor[] _floors = 
    [
        new Floor(0, 30, 30)
    ];

    public static readonly ConnectPoint[][] _connectPoints =
    [
        // top
        [],
        
        // bottom
        [],
        
        // left
        [],
        
        // right
        [
            new ConnectPoint(34, 31, Directions.Right)
        ]
    ];
    
    public static readonly Floor[] _floors_r = 
    [
        new Floor(5, 30, 30)
    ];

    public static readonly ConnectPoint[][] _connectPoints_r =
    [
        // top
        [],
        
        // bottom
        [],
        
        // left
        [
            new ConnectPoint(0, 31, Directions.Left)
        ],
        
        // right
        []
    ];


    public readonly bool Reverse;
    
    public BeachHouseStructure(ushort x = 500, ushort y = 500, byte status = StructureStatus.NotGenerated, bool reverse = false)
    {
        Reverse = reverse;
        
        if (!reverse)
        {
            FilePath = _filePath;
            Floors = _floors;
            ConnectPoints = _connectPoints;
        }
        else
        {
            FilePath = _filePath_r;
            Floors = _floors_r;
            ConnectPoints = _connectPoints_r;
        }
        
        StructureXSize = _structureXSize;
        StructureYSize = _structureYSize;
        
        X = x;
        Y = y;
        Status = status;
        SetSubstructurePositions();
    }
    
    public override void OnFound()
    {
        Status = StructureStatus.GeneratedAndFound;

        if (!Reverse)
        {
            Terraria.WorldGen.PlaceTile(X + 16, Y + 20, TileID.Beds, true, true, style: 22);
            NetMessage.SendTileSquare(-1, X + 15, Y + 19, 4, 2);
            
            Terraria.WorldGen.PlaceTile(X + 14, Y + 28, TileID.Chairs, true, true, style: 0);
            NetMessage.SendTileSquare(-1, X + 14, Y + 27, 1, 2);
        }
        else
        {
            Terraria.WorldGen.PlaceTile(X + 17, Y + 20, TileID.Beds, true, true, style: 22);
            NetMessage.SendTileSquare(-1, X + 16, Y + 19, 4, 2);
            
            Terraria.WorldGen.PlaceTile(X + 20, Y + 28, TileID.Chairs, true, true, style: 0);
            NetMessage.SendTileSquare(-1, X + 20, Y + 27, 1, 2);
        }
    }

    public override void Generate()
    {
        ushort bottomTileID = Main.tile[Floors[0].X + Floors[0].FloorLength / 2, Floors[0].Y + 10].TileType;
        
        if (!Reverse)
        {
            ConnectPoints[3][0].BlendRight(TileID.Sand, 8);
            Floors[0].GenerateBeams(TileID.RichMahoganyBeam, 4, 3, tileColor: PaintID.BrownPaint, 1);
            Floors[0].GenerateFoundation(TileID.Sand, 11, 8, 4);
        }
        else
        {
            ConnectPoints[2][0].BlendLeft(TileID.Sand, 8);
            Floors[0].GenerateBeams(TileID.RichMahoganyBeam, 4, 3, tileColor: PaintID.BrownPaint, 20);
            Floors[0].GenerateFoundation(TileID.Sand, 11, -8, 4);
        }

        _GenerateStructure();
        
        if (bottomTileID != TileID.Sand && bottomTileID != TileID.ShellPile)
        {
            WorldUtils.Gen(new Point(Floors[0].X + Floors[0].FloorLength / 2, Floors[0].Y),
                new Shapes.Circle(Floors[0].FloorLength + 16), // +16 for the blendDistance
                Actions.Chain(
                    new Modifiers.OnlyTiles(TileID.Sand),
                    new Actions.Custom((i, j, args) => { Main.tile[i, j].TileType = bottomTileID; return true; })
                )
            );
        }
        
        FrameTiles();
        
        Status = StructureStatus.GeneratedButNotFound;
    }
}
