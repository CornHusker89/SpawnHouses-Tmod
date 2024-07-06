using Terraria;
using Terraria.ID;
using System;
using SpawnHouses.Structures;



using SpawnHouses.Structures.StructureParts;

namespace SpawnHouses.Structures.Structures;

public class MainHouseStructure : CustomStructure
{
    // constants
    private static readonly string _filePath = "Structures/StructureFiles/mainHouse/mainHouse_v3";
    private static readonly ushort _structureXSize = 63;
    private static readonly ushort _structureYSize = 33;
    
    private static readonly Floor[] _floors = 
    [
        new Floor(11, 26, 42)
    ];

    private static readonly ConnectPoint[][] _connectPoints =
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
    
    public override string FilePath => _filePath;
    public sealed override ushort StructureXSize => _structureXSize;
    public sealed override ushort StructureYSize => _structureYSize;
    public bool InUnderworld;
    
    public MainHouseStructure(ushort x = 0, ushort y = 0, bool inUnderworld = false)
    {
        Floors = _floors;
        ConnectPoints = _connectPoints;

        InUnderworld = inUnderworld;
        
        X = x;
        Y = y;
        SetSubstructurePositions();
    }

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
    }
}   