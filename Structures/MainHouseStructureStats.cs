using System.Collections;
using Terraria.ID;
using Terraria;

using SpawnHouses.Structures;
using SpawnHouses.Structures.Substructures;

namespace SpawnHouses.Structures;

public class MainHouseStructure : CustomStructure
{
    public override string FilePath => "Structures/PrebuiltStructures/mainHouse";
    public override int StructureXSize => 63;
    public override int StructureYSize => 33;

    public MainHouseStructure(int x, int y)
    {
        Floors = 
        [
            new Floor(11, 26, 42)
        ];

        ConnectPoints =
        [
            new ConnectPoint(0, 26),
            new ConnectPoint(63, 26)
        ];
        
        X = x;
        Y = y;
        SetSubstructurePositions();
        
        Floors[0].GenerateFoundation(TileID.Dirt, foundationRadius: 31, foundationYOffset: 5);
        ConnectPoints[0].BlendLeft(topTileID: TileID.Grass, blendDistance: 20);
        ConnectPoints[1].BlendRight(topTileID: TileID.Grass, blendDistance: 20);
        
        GenerateStructure();
        FrameTiles();
    }
}   