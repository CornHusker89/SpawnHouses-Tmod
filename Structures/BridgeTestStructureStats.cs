using System.Collections;
using Terraria.ID;
using Terraria;

using SpawnHouses.Structures;
using SpawnHouses.Structures.Substructures;

namespace SpawnHouses.Structures;

public class BridgeTestStructure : CustomStructure
{
    public override string FilePath => "Structures/PrebuiltStructures/bridgeTestStructure";
    public override ushort StructureXSize => 8;
    public override ushort StructureYSize => 9;

    public BridgeTestStructure(ushort x, ushort y)
    {
        Floors =
        [
            new Floor(0, 8, 8)
        ];

        ConnectPoints =
        [
            new ConnectPoint(0, 0),
            new ConnectPoint(7, 0)
        ];

        X = x;
        Y = y;
        SetSubstructurePositions();
        Floors[0].GenerateFoundation(TileID.Dirt, 4, 0, 1);

        GenerateStructure();
        FrameTiles();
    }
}