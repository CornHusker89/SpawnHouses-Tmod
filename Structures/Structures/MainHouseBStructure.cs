using System;
using Terraria.ID;
using SpawnHouses.Structures;



using SpawnHouses.Structures.StructureParts;

namespace SpawnHouses.Structures.Structures;

public class MainHouseBStructure : CustomStructure
{
    // constants
    private static readonly string _filePath = "Structures/StructureFiles/mainHouse/mainHouse_B_v3";
    private static readonly ushort _structureXSize = 63;
    private static readonly ushort _structureYSize = 40;
    
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
    
    public MainHouseBStructure(ushort x = 0, ushort y = 0)
    {
        Floors = _floors;
        ConnectPoints = _connectPoints;
        
        X = x;
        Y = y;
        SetSubstructurePositions();
    }

    public override void Generate()
    {
        Floors[0].GenerateFoundation(TileID.Dirt, foundationRadius: 31, foundationYOffset: 5);
        ConnectPoints[2][0].BlendLeft(topTileID: TileID.Grass, blendDistance: 20, maxFillCount: 25);
        ConnectPoints[3][0].BlendRight(topTileID: TileID.Grass, blendDistance: 20, maxFillCount: 25);

        _GenerateStructure();
        
        // int signIndex = Terraria.Sign.ReadSign(X, Y);
        // Console.WriteLine(signIndex);
        // if (signIndex != -1)
        //     Terraria.Sign.TextSign(signIndex, "aaa");
        
        FrameTiles();
    }
}   