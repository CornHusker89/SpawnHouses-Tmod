using System.Collections;
using Terraria.ID;
using Terraria;

using SpawnHouses.Structures;
using SpawnHouses.Structures.Substructures;

namespace SpawnHouses.Structures;

public class BeachHouseStructure : CustomStructure
{
    public override string FilePath => "Structures/PrebuiltStructures/beachHouse";
    public override int StructureXSize => 35;
    public override int StructureYSize => 24;

    public BeachHouseStructure(int x, int y)
    {
        Floors =
        [
            new Floor(0, 26, 30)
        ];

        ConnectPoints =
        [
            new ConnectPoint(34, 29)
        ];

        X = x;
        Y = y;
        SetSubstructurePositions();
        Floors[0].GenerateBeams(TileID.RichMahoganyBeam, 4, 3, tileColor: PaintID.BrownPaint, 1);
        Floors[0].GenerateFoundation(TileID.Sand, 11, 8, 4);
        ConnectPoints[0].BlendRight(TileID.Sand, 10);

        GenerateStructure();
        FrameTiles();
    }
}
