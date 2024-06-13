using Terraria.ID;
using SpawnHouses.Structures;



using SpawnHouses.Structures.StructureParts;

namespace SpawnHouses.Structures.Structures;

public class MainHouseStructure : CustomStructure
{
    // constants
    private static readonly string _filePath = "Structures/StructureFiles/mainHouse_v2";
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
            new ConnectPoint(0, 26, true)
        ],
        
        // right
        [
            new ConnectPoint(63, 26, false)
        ]
    ];
    
    public override string FilePath => _filePath;
    public sealed override ushort StructureXSize => _structureXSize;
    public sealed override ushort StructureYSize => _structureYSize;
    
    public MainHouseStructure(ushort x = 0, ushort y = 0)
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
        FrameTiles();
    }
}   