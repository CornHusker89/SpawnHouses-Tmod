using System.Collections;
using Terraria.ID;
using Terraria;

using SpawnHouses.Structures;
using SpawnHouses.Structures.StructureParts;

namespace SpawnHouses.Structures.Structures;

public class BridgeTestStructure : CustomStructure
{
    // constants
    private static readonly string _filePath = "Structures/StructureFiles/bridgeTest";
    private static readonly ushort _structureXSize = 8;
    private static readonly ushort _structureYSize = 9;
    
    private static readonly Floor[] _floors = 
    [
        new Floor(0, 8, 8)
    ];

    private static readonly ConnectPoint[][] _connectPoints =
    [
        // top
        [], 
        
        // bottom
        [],
        
        // left
        [
            new ConnectPoint(0, 0, true)
        ],
        
        // right
        [
            new ConnectPoint(7, 0, false)
        ]
    ];
    
    public override string FilePath => _filePath;
    public sealed override ushort StructureXSize => _structureXSize;
    public sealed override ushort StructureYSize => _structureYSize;

    public BridgeTestStructure(ushort x = 0, ushort y = 0)
    {
        Floors = _floors;
        ConnectPoints = _connectPoints;
        
        X = x;
        Y = y;
        SetSubstructurePositions();
    }

    public override void Generate()
    {
        Floors[0].GenerateFoundation(TileID.Dirt, 4, 0, 1);

        _GenerateStructure();
        FrameTiles();
    }
}