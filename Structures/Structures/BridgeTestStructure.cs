using System.Collections;
using Terraria.ID;
using Terraria;

using SpawnHouses.Structures;
using SpawnHouses.Structures.StructureParts;

namespace SpawnHouses.Structures.Structures;

public class BridgeTestStructure : CustomStructure
{
    public override string FilePath => "Structures/StructureFiles/bridgeTestStructure";
    public override ushort StructureXSize => 8;
    public override ushort StructureYSize => 9;

    public BridgeTestStructure(ushort x, ushort y, bool generate = true)
    {
        Floors =
        [
            new Floor(0, 8, 8)
        ];

        ConnectPoints =
        [
            new ConnectPoint(0, 0, true, true),
            new ConnectPoint(7, 0, false, true)
        ];

        X = x;
        Y = y;
        SetSubstructurePositions();
        
        if (generate)
            Generate();
    }

    private void Generate()
    {
        Floors[0].GenerateFoundation(TileID.Dirt, 4, 0, 1);

        GenerateStructure();
        FrameTiles();
    }
}