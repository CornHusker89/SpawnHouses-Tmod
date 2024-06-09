using Terraria.ID;
using SpawnHouses.Structures;



using SpawnHouses.Structures.StructureParts;

namespace SpawnHouses.Structures.Structures;

public class MainHouseStructure : CustomStructure
{
    public override string FilePath => "Structures/StructureFiles/mainHouse_v2";
    public override ushort StructureXSize => 63;
    public override ushort StructureYSize => 33;

    public MainHouseStructure(ushort x = 0, ushort y = 0)
    {
        Floors =
        [
            new Floor(11, 26, 42)
        ];

        ConnectPoints =
        [
            new ConnectPoint(0, 26, true),
            new ConnectPoint(63, 26, false)
        ];

        X = x;
        Y = y;
        SetSubstructurePositions();
    }

    public override void Generate()
    {
        Floors[0].GenerateFoundation(TileID.Dirt, foundationRadius: 31, foundationYOffset: 5);
        ConnectPoints[0].BlendLeft(topTileID: TileID.Grass, blendDistance: 20, maxFillCount: 25);
        ConnectPoints[1].BlendRight(topTileID: TileID.Grass, blendDistance: 20, maxFillCount: 25);

        _GenerateStructure();
        FrameTiles();
    }
}   