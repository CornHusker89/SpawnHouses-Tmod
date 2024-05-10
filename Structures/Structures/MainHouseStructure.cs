using Terraria.ID;

using SpawnHouses.Structures;
using SpawnHouses.Structures.StructureParts;

namespace SpawnHouses.Structures.Structures;

public class MainHouseStructure : CustomStructure
{
    public override string FilePath => "Structures/StructureFiles/mainHouse";
    public override ushort StructureXSize => 63;
    public override ushort StructureYSize => 33;

    public MainHouseStructure(ushort x = 0, ushort y = 0, sbyte cost = -1)
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
        Cost = cost;
        SetSubstructurePositions();
    }

    private void Generate()
    {
        Floors[0].GenerateFoundation(TileID.Dirt, foundationRadius: 31, foundationYOffset: 5);
        ConnectPoints[0].BlendLeft(topTileID: TileID.Grass, blendDistance: 20);
        ConnectPoints[1].BlendRight(topTileID: TileID.Grass, blendDistance: 20);

        GenerateStructure();
        FrameTiles();
    }

    public override MainHouseStructure Clone()
    {
        return new MainHouseStructure(X, Y, Cost);
    }
}   