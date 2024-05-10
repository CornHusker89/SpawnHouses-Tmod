using System.Collections;
using Terraria.ID;
using Terraria;

using SpawnHouses.Structures;
using SpawnHouses.Structures.StructureParts;

namespace SpawnHouses.Structures.Structures;

public class ChainTestStructure : CustomChainStructure
{
    public override string FilePath => "Structures/StructureFiles/chainTest";
    public sealed override ushort StructureXSize => 8;
    public sealed override ushort StructureYSize => 9;
    
    public ChainTestStructure(ushort x = 0, ushort y = 0, sbyte cost = -1)
    {
        X = x;
        Y = y;
        Cost = cost;
        
        Floors =
        [
        ];

        ConnectPoints =
        [
            new ConnectPoint(0, 6, true, true),
            new ConnectPoint(14, 6, false, true),
            new ConnectPoint(14, 12, false, true)
        ];

        BoundingBox = new BoundingBox(x - 3, y - 3, x + StructureXSize + 3, y + StructureYSize + 3);
        
        SetSubstructurePositions();
    }

    private void Generate()
    {
        Floors[0].GenerateFoundation(TileID.Dirt, 4, 0, 1);

        GenerateStructure();
        FrameTiles();
    }

    public override ChainTestStructure Clone()
    {
        return new ChainTestStructure(X, Y, Cost);
    }
}