using System;
using Microsoft.Xna.Framework;
using Terraria.ID;

using SpawnHouses.Structures;
using SpawnHouses.Structures.StructureParts;
using Terraria;
using Terraria.WorldBuilding;

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
        ushort bottomTileID = Main.tile[Floors[0].X + Floors[0].FloorLength / 2, Floors[0].Y + 10].TileType;
        
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
        
        if (bottomTileID != TileID.Sand && bottomTileID != TileID.ShellPile)
        {
            WorldUtils.Gen(new Point(Floors[0].X + Floors[0].FloorLength / 2, Floors[0].Y),
                new Shapes.Circle(Floors[0].FloorLength + 16), // +16 for the blendDistance
                Actions.Chain(
                    new Modifiers.OnlyTiles(TileID.Sand),
                    new Actions.Custom((i, j, args) => { Main.tile[i, j].TileType = bottomTileID; return true; })
                )
            );
        }
        
        FrameTiles();
    }
}
