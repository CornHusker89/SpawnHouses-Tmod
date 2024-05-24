using System.Collections;
using Terraria.ID;
using Terraria;

using SpawnHouses.Structures;
using SpawnHouses.Structures.StructureParts;

namespace SpawnHouses.Structures.Structures;

public class BridgeTestStructure : CustomStructure
{
    public override string FilePath => "Structures/StructureFiles/bridgeTest";
    public override ushort StructureXSize => 8;
    public override ushort StructureYSize => 9;

    public BridgeTestStructure(ushort x = 0, ushort y = 0)
    {
        Floors =
        [
            new Floor(0, 8, 8)
        ];

        ConnectPoints =
        [
            new ConnectPoint(0, 0, true),
            new ConnectPoint(7, 0, false)
        ];

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