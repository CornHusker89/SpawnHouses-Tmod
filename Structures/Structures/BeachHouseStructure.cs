using Terraria.ID;

using SpawnHouses.Structures;
using SpawnHouses.Structures.StructureParts;

namespace SpawnHouses.Structures.Structures;

public class BeachHouseStructure : CustomStructure
{
    public override string FilePath => "Structures/StructureFiles/beachHouse";
    public override ushort StructureXSize => 35;
    public override ushort StructureYSize => 24;
    
    public BeachHouseStructure(ushort x = 0, ushort y = 0, sbyte cost = -1)
    {
        Floors =
        [
            new Floor(0, 26, 30)
        ];

        ConnectPoints =
        [
            new ConnectPoint(34, 29, false)
        ];

        X = x;
        Y = y;
        Cost = cost;
        SetSubstructurePositions();
    }

    private void Generate()
    {
        Floors[0].GenerateBeams(TileID.RichMahoganyBeam, 4, 3, tileColor: PaintID.BrownPaint, 1);
        Floors[0].GenerateFoundation(TileID.Sand, 11, 8, 4);
        ConnectPoints[0].BlendRight(TileID.Sand, 10);

        GenerateStructure();
        FrameTiles();
    }
}
