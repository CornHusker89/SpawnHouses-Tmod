using Terraria;
using Terraria.ID;
using System;
using SpawnHouses.Structures;



using SpawnHouses.Structures.StructureParts;

namespace SpawnHouses.Structures.Structures;

public class MainHouseStructure : CustomStructure
{
    // constants
    public static readonly string _filePath1 = "Structures/StructureFiles/mainHouse/mainHouse_v3";
    public static readonly string _filePath2 = "Structures/StructureFiles/mainHouse/mainHouse_B_v3";
    public static readonly ushort _structureXSize = 63;
    public static readonly ushort _structureYSize = 40;
    
    public static readonly Floor[] _floors = 
    [
        new Floor(11, 26, 42)
    ];

    public static readonly ConnectPoint[][] _connectPoints =
    [
        // top
        [], 
        
        // bottom
        [],
        
        // left
        [
            new ConnectPoint(0, 26, Directions.Left)
        ],
        
        // right
        [
            new ConnectPoint(62, 26, Directions.Right)
        ]
    ];
    
    public readonly bool InUnderworld;
    public readonly bool HasBasement;
    
    public MainHouseStructure(ushort x = 500, ushort y = 500, byte status = StructureStatus.NotGenerated, bool hasBasement = false, bool inUnderworld = false)
    {
        InUnderworld = inUnderworld;
        HasBasement = hasBasement;
        
        Floors = _floors;
        ConnectPoints = _connectPoints;
        
        FilePath = !HasBasement ? _filePath1 : _filePath2;

        StructureXSize = _structureXSize;
        StructureYSize = _structureYSize;
        
        X = x;
        Y = y;
        Status = status;
        SetSubstructurePositions();
    }

    public override void OnFound() {}

    public override void Generate()
    {
        Floors[0].GenerateFoundation(TileID.Dirt, foundationRadius: 31, foundationYOffset: 5);

        if (!InUnderworld)
        {
            ConnectPoints[2][0].BlendLeft(topTileID: TileID.Grass, blendDistance: 20, maxFillCount: 25);
            ConnectPoints[3][0].BlendRight(topTileID: TileID.Grass, blendDistance: 20, maxFillCount: 25);
        }
        else
        {
            ConnectPoints[2][0].BlendLeft(topTileID: TileID.Grass, blendDistance: 20, maxFillCount: 25, maxHeight: 10);
            ConnectPoints[3][0].BlendRight(topTileID: TileID.Grass, blendDistance: 20, maxFillCount: 25, maxHeight: 10);
        }
        
        _GenerateStructure();
        FrameTiles();
        
        int signIndex = Sign.ReadSign(X + 7, Y + 21);
        if (signIndex != -1)
            Sign.TextSign(signIndex, "All good adventures start in a tavern...To bad this isn't a tavern :(");
        
        Status = StructureStatus.GeneratedAndFound;
    }
}   