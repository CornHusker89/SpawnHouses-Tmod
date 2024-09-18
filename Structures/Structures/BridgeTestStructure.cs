using System.Collections;
using Microsoft.Xna.Framework;
using Terraria.ID;
using Terraria;

using SpawnHouses.Structures;
using SpawnHouses.Structures.StructureParts;

namespace SpawnHouses.Structures.Structures;

public sealed class BridgeTestStructure : CustomStructure
{
    // constants
    public static readonly string _filePath = "Structures/StructureFiles/bridgeTest";
    public static readonly ushort _structureXSize = 8;
    public static readonly ushort _structureYSize = 9;
    
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

    public BridgeTestStructure(ushort x = 0, ushort y = 0, byte status = StructureStatus.NotGenerated) : 
        base(_filePath, _structureXSize, _structureYSize, 
            CopyConnectPoints(_connectPoints), status, x, y)
    {
        ID = StructureID.BridgeTestStructure;
        SetSubstructurePositions();
    }

    public override void Generate()
    {
        GenHelper.GenerateFoundation(new Point(X, Y + 9), TileID.Dirt, 4);

        base.Generate();
    }
}