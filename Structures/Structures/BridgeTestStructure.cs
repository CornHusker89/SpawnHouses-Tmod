using System.Collections;
using Terraria.ID;
using Terraria;

using SpawnHouses.Structures;
using SpawnHouses.Structures.StructureParts;

namespace SpawnHouses.Structures.Structures;

public class BridgeTestStructure : CustomStructure
{
    // constants
    public static readonly string _filePath = "Structures/StructureFiles/bridgeTest";
    public static readonly ushort _structureXSize = 8;
    public static readonly ushort _structureYSize = 9;
    
    public static readonly Floor[] _floors = 
    [
        new Floor(0, 8, 8)
    ];

    public static readonly ConnectPoint[][] _connectPoints =
    [
        // top
        [], 
        
        // bottom
        [],
        
        // left
        [
            new ConnectPoint(0, 0, Directions.Left)
        ],
        
        // right
        [
            new ConnectPoint(7, 0, Directions.Right)
        ]
    ];

    public BridgeTestStructure(ushort x = 0, ushort y = 0)
    {
        Floors = _floors;
        ConnectPoints = _connectPoints;
        
        FilePath = _filePath;
        StructureXSize = _structureXSize;
        StructureYSize = _structureYSize;
        
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