using System.Collections;
using Terraria.ID;
using Terraria;

using SpawnHouses.Structures;
using SpawnHouses.Structures.StructureParts;

namespace SpawnHouses.Structures.Structures;

public class ChainTestStructure : CustomStructure
{
    public override string FilePath => "Structures/StructureFiles/bridgeTest";
    public override ushort StructureXSize => 8;
    public override ushort StructureYSize => 9;
    
    public ChainTestStructure() {}

    public ChainTestStructure(ushort x = 0, ushort y = 0, sbyte cost = -1)
    {
        Floors =
        [
        ];

        ConnectPoints =
        [
            new ConnectPoint(0, 6, true, true),
            new ConnectPoint(14, 6, false, true),
            new ConnectPoint(14, 12, false, true)
        ];

        X = x;
        Y = y;
        Cost = cost;
        SetSubstructurePositions();
    }

    private void Generate()
    {
        Floors[0].GenerateFoundation(TileID.Dirt, 4, 0, 1);

        GenerateStructure();
        FrameTiles();
    }
}