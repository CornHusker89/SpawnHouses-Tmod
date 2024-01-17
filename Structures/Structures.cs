
using System;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.DataStructures;
using Microsoft.Xna.Framework;
using Terraria.WorldBuilding;

namespace SpawnHouses.Structures
{
    public class CustomStructure
    {
        private Mod _mod = ModContent.GetInstance<SpawnHouses>();

        public virtual int X { get; set; } = 0;
        public virtual int Y { get; set; } = 0;
        
        public virtual bool Debug { get; set; } = false;
        public virtual string FilePath { get; set; } = "Structures/_";
        public virtual int StructureXSize { get; set; } = 1;
        public virtual int StructureFloorLength { get; set; } = 1;
        public virtual int StructureFloorYOffset { get; set; } = 0;
        public virtual int StructureFloorXOffset { get; set; } = 1;
        public virtual bool CanHaveBeams { get; set; } = false;
        public virtual int BeamInterval { get; set; } = 1;
        public virtual int BeamsXOffset { get; set; } = 0;
        public virtual int BeamsCount { get; set; } = 0;
        public virtual int MaxBeamSize { get; set; } = 50;
        public virtual ushort BeamTileID { get; set; } = TileID.WoodenBeam;
        public virtual byte BeamTilePaintID { get; set; } = PaintID.None;
        public virtual bool CanHaveFoundation { get; set; } = false;
        public virtual bool CanSampleFoundation { get; set; } = false;
        public virtual ushort FoundationTileID { get; set; } = TileID.Dirt;
        public virtual int FoudationRadiusOverride { get; set; } = 0;
        public virtual int FoudationXOffsetOverride { get; set; } = 0;
        public virtual int FoudationYOffsetOverride { get; set; } = 0;
        public virtual bool CanBlendLeft { get; set; } = false;
        public virtual bool CanBlendRight { get; set; } = false;
        public virtual int BlendDistance { get; set; } = 6;

        private bool FrameTiles()    
        {
            int centerX = X + StructureFloorXOffset + (StructureFloorLength / 2);
            int centerY = Y + StructureFloorYOffset + (StructureFloorLength / 2);
            
            WorldUtils.Gen(new Point(centerX, centerY), new Shapes.Circle((int)(StructureXSize * 1.33)), new Actions.SetFrames());

            return true;
        }
        
        private bool FrameTiles(int centerX, int centerY, int radius)    
        {
            WorldUtils.Gen(new Point(centerX, centerY), new Shapes.Circle(radius), new Actions.SetFrames());

            return true;
        }
        
        public bool GenerateStructure()
        {
            bool result = StructureHelper.Generator.GenerateStructure(FilePath, new Point16(X:X, Y:Y), _mod);

            FrameTiles();
            
            return result;
        }
        
        public bool GenerateBeams()
        {
            if (!CanHaveBeams)
            {
                throw new Exception("structure can not have beams");
            }
            
            for (int i = 0; i < BeamsCount; i++)
            {
                bool validBeamLocation = true;
                int y2 = StructureFloorYOffset + 1; //put us 1 below the floor
                int x2 = X + BeamsXOffset + (i * BeamInterval);

                //if there's a tile there already, dont place a beam
                if (Terraria.WorldGen.SolidTile(x2, Y + y2))
                {
                    continue;
                }
				
                while (!Terraria.WorldGen.SolidTile(x2, Y + y2))
                {
                    if (y2 >= MaxBeamSize + StructureFloorYOffset)
                    {
                        validBeamLocation = false;
                        break;
                    }
                    y2++;
                }

                if (Debug)
                {
                    Main.NewText($"X:{x2}, Y:{Y}, X2:{x2}, Y2:{y2}, i:{i}, interval:{BeamInterval}");
                }

                if (validBeamLocation)
                {
                    for (int j = 0; j < y2 - StructureFloorYOffset; j++)
                    {
                        Tile tile = Main.tile[x2, Y + j + StructureFloorYOffset];
                        tile.HasTile = true;
                        tile.TileType = BeamTileID;
                        tile.TileColor = BeamTilePaintID;
                    }

                    //make the tile beneath the beam (and the one just above) a full block
                    Tile bottomTile = Main.tile[x2, Y + y2 - 1];
                    bottomTile.Slope = SlopeType.Solid;
                    bottomTile = Main.tile[x2, Y + y2];
                    bottomTile.Slope = SlopeType.Solid;
                }
            }
            FrameTiles();
            
            return true;
        }
        
        public bool GenerateFoundation()
        {
            if (!CanHaveFoundation)
            {
                throw new Exception("structure cannot not have foundation");
            }
            ushort foundationTileId = FoundationTileID;
            if (CanSampleFoundation)
            {
                int x2 = X + (StructureXSize / 2);
                int y2 = Y - 2;
                while (!Terraria.WorldGen.SolidTile(X, Y))
                {
                    y2++;
                }
        
                Tile tile = Main.tile[x2, y2];
                foundationTileId = tile.TileType;
            }
            
            int centerX = 0;
            
            if (FoudationXOffsetOverride == 0)
            {
                centerX = X + StructureFloorXOffset + (StructureFloorLength / 2);
            }
            else
            {
                centerX = X + FoudationXOffsetOverride;
            }
            
            int centerY = 0;

            if (FoudationYOffsetOverride == 0)
            {
                centerY = Y + StructureFloorYOffset + (StructureFloorLength / 2);
            }
            else
            {
                centerY = Y + FoudationYOffsetOverride;
            }

            int radius = 0;

            if (FoudationRadiusOverride == 0)
            {
                radius = (int)(StructureFloorLength * 1.33 / 2);
            }
            else
            {
                radius = FoudationRadiusOverride;
            }

            if (Debug)
            {
                Main.NewText($"cx: {centerX} cy: {centerY} rad: {radius} y: {Y} FoudationYOffsetOverride: {FoudationYOffsetOverride}");
            }

            WorldUtils.Gen(new Point(centerX, centerY), new Shapes.Circle(radius), new Actions.SetTile(foundationTileId));

            // centerY = y + StructureFloorYOffset / 2;
            // WorldUtils.Gen(new Point(centerX, centerY), new Shapes.Rectangle(), new Actions.ClearTile());

            FrameTiles();
        
            return true;
        }

        public bool BlendLeft()
        {
            if (!CanBlendLeft)
            {
                throw new Exception("structure can not blend terrain on left");
            }

            int startX = X + StructureFloorXOffset;
            int startY = Y + StructureFloorYOffset;

            int endX = startX - BlendDistance;
            int endY = startY - 35; // to make sure that it starts outside the terrain
            
            while (!Main.tile[endX, endY].HasTile) {
                endY++;
            }

            ushort topTileType = Main.tile[endX, endY].TileType;
            ushort fillTileType = 0;
            if (topTileType == TileID.Grass)
            {
                fillTileType = TileID.Grass;
            } 
            else if (topTileType == TileID.JungleGrass)
            {
                fillTileType = TileID.Grass;
            }
            else
            {
                fillTileType = topTileType;
            }

            double slope = (double) (endY - startY) / (endX - startX);
            double changePerTile = slope / BlendDistance;
            
            for (int i = 0; i < BlendDistance; i++)
            {
                // get the top tile, change its values
                int topTileY = Y + (int)Math.Round(i * changePerTile);
                Tile tile = Main.tile[X - i, topTileY];
                tile.HasTile = true;
                tile.Slope = SlopeType.Solid;
                tile.TileType = topTileType;

                // get/change the tiles beneath the top tiles
                int y = 0;
                while (!Main.tile[X - i, topTileY + y].HasTile || Main.tile[X - i, topTileY + y].Slope != SlopeType.Solid)
                {
                    Tile lowerTile = Main.tile[X - i, topTileY + y];
                    lowerTile.HasTile = true;
                    lowerTile.Slope = SlopeType.Solid;
                    lowerTile.TileType = fillTileType;
                    
                    y++;
                }
            }
            
            return true;
        }
    }
    
    public class TestHouseStructure : CustomStructure
    {
        public override string FilePath => "Structures/testHouse";
        public override int StructureXSize => 24;
        public override int StructureFloorLength => 19;
        public override int StructureFloorYOffset => 12;
        public override int StructureFloorXOffset => 2;
        public override bool CanHaveBeams => true;
        public override int BeamInterval => 4;
        public override int BeamsXOffset => 3;
        public override int BeamsCount => 5;
        public override bool CanHaveFoundation => true; 
        public override ushort FoundationTileID => TileID.Dirt;
    }
    
    public class MainHouseStructure : CustomStructure
    {
        public override string FilePath => "Structures/mainHouse";
        public override int StructureXSize => 63;
        public override int StructureFloorLength => 41;
        public override int StructureFloorYOffset => 27;
        public override int StructureFloorXOffset => 11;
        public override bool CanHaveFoundation => true; 
        public override ushort FoundationTileID => TileID.Dirt;
        public override int FoudationRadiusOverride => 31;
        public override int FoudationYOffsetOverride => 31;
        public override bool CanBlendLeft => true;
        public override bool CanBlendRight => true;
        public override int BlendDistance => 12;
    }
    
    public class BeachHouseStructure : CustomStructure
    {
        public override string FilePath => "Structures/beachHouse";
        public override int StructureXSize => 35;
        public override int StructureFloorLength => 23;
        public override int StructureFloorYOffset => 26;
        public override int StructureFloorXOffset => 11;
        public override bool CanHaveBeams => true;
        public override int BeamInterval => 4;
        public override int BeamsXOffset => 1;
        public override int BeamsCount => 3;
        public override ushort BeamTileID => TileID.RichMahoganyBeam;
        public override byte BeamTilePaintID => PaintID.BrownPaint;
        public override bool CanHaveFoundation => true; 
        public override ushort FoundationTileID => TileID.Sand;
        public override int FoudationXOffsetOverride => 25;
        public override int FoudationYOffsetOverride => 39;
        public override int FoudationRadiusOverride => 14;
        public override bool CanBlendRight => true;
    }
}
