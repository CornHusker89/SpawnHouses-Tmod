using Microsoft.Xna.Framework;
using Terraria.ID;
using SpawnHouses.Structures;



using SpawnHouses.Structures.StructureParts;
using Terraria;
using Terraria.WorldBuilding;
using Actions = Terraria.GameContent.Animations.Actions;

namespace SpawnHouses.Structures.Structures;

public class FirepitStructure : CustomStructure
{
    
    // constants
    private static readonly string _filePath = "Structures/StructureFiles/firepit";
    private static readonly ushort _structureXSize = 7;
    private static readonly ushort _structureYSize = 3;
    
    private static readonly Floor[] _floors = [];

    private static readonly ConnectPoint[][] _connectPoints =
    [
        // top
        [],
        
        // bottom
        [],
        
        // left
        [
            new ConnectPoint(-1, 2, Directions.Left)
        ],
        
        // right
        [
            new ConnectPoint(7, 2, Directions.Right)
        ]
    ];
    
    public override string FilePath => _filePath;
    public sealed override ushort StructureXSize => _structureXSize;
    public sealed override ushort StructureYSize => _structureYSize;

    public FirepitStructure(ushort x = 0, ushort y = 0)
    {
        Floors = _floors;
        ConnectPoints = _connectPoints;
        
        X = x;
        Y = y;
        SetSubstructurePositions();
    }

    public override void Generate()
    {
        WorldUtils.Gen(new Point(X, Y - 9), new Shapes.Rectangle(7, 9),
            new Terraria.WorldBuilding.Actions.ClearTile());
        
        ushort blendTileID = Main.tile[X + 3, Y + 7].TileType;
        if (blendTileID == TileID.ShellPile)
            blendTileID = TileID.Sand;
        
        ConnectPoints[2][0].BlendLeft(topTileID: blendTileID, blendDistance: 5);
        ConnectPoints[3][0].BlendRight(topTileID: blendTileID, blendDistance: 5);

        // make sure that blending doesn't fuck up the tiles next to the chairs
        Tile tile = Main.tile[X - 1, Y + 2];
        tile.Slope = SlopeType.Solid;
        tile.IsHalfBlock = false;
        tile = Main.tile[X + 7, Y + 2];
        tile.Slope = SlopeType.Solid;
        tile.IsHalfBlock = false;
        
        ushort leftX = (ushort)(X - Terraria.WorldGen.genRand.Next(2, 6));
        ushort rightX = (ushort)(X + 6 + Terraria.WorldGen.genRand.Next(2, 6));
        ushort curLeftY = (ushort)(Y - 8);
        ushort curRightY = (ushort)(Y - 8);
        while (!Terraria.WorldGen.SolidTile(leftX, curLeftY))
            curLeftY++; 
        while (!Terraria.WorldGen.SolidTile(rightX, curRightY))
            curRightY++;

        if (Terraria.WorldGen.genRand.Next(0, 3) != 0) // 2/3 chance
            Terraria.WorldGen.PlaceTile(leftX, curLeftY - 1, TileID.BeachPiles, true);
        if (Terraria.WorldGen.genRand.Next(0, 3) != 0)
            Terraria.WorldGen.PlaceTile(rightX, curRightY - 1, TileID.BeachPiles, true);
        
        _GenerateStructure();
        FrameTiles(X + 3, Y + 1, 3);
    }
}