using System;
using Terraria.ID;

using SpawnHouses.Structures;
using SpawnHouses.Structures.StructureParts;

namespace SpawnHouses.Structures.Structures;

public class BeachHouseStructure : CustomStructure
{
    public override string FilePath => "Structures/StructureFiles/beachHouse";
    public override ushort StructureXSize => 35;
    public override ushort StructureYSize => 26;

    public readonly bool Reverse;
    
    public BeachHouseStructure(ushort x = 0, ushort y = 0, bool reverse = false)
    {
        Reverse = reverse;
        if (!reverse)
        {
            Floors =
            [
                new Floor(0, 30, 30)
            ];

            ConnectPoints =
            [
                new ConnectPoint(34, 31, false)
            ];

            X = x;
            Y = y;
            SetSubstructurePositions();
        }
        else
        {
            Floors =
            [
                new Floor(5, 30, 30)
            ];

            ConnectPoints =
            [
                new ConnectPoint(0, 31, true)
            ];

            X = x;
            Y = y;
            SetSubstructurePositions();
        }

    }

    public override void Generate()
    {
        if (!Reverse)
        {
            ConnectPoints[0].BlendRight(TileID.Sand, 8);
            Floors[0].GenerateBeams(TileID.RichMahoganyBeam, 4, 3, tileColor: PaintID.BrownPaint, 1);
            Floors[0].GenerateFoundation(TileID.Sand, 11, 8, 4);
        }
        else
        {
            ConnectPoints[0].BlendLeft(TileID.Sand, 8);
            Floors[0].GenerateBeams(TileID.RichMahoganyBeam, 4, 3, tileColor: PaintID.BrownPaint, 20);
            Floors[0].GenerateFoundation(TileID.Sand, 11, -8, 4);
        }

        _GenerateStructure(Reverse);
        FrameTiles();
    }
}
