
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
        public virtual ushort BlendTileID { get; set; } = 0;

        public bool FrameTiles()    
        {
            int centerX = X + StructureFloorXOffset + (StructureFloorLength / 2);
            int centerY = Y + StructureFloorYOffset + (StructureFloorLength / 2);
            
            WorldUtils.Gen(new Point(centerX, centerY), new Shapes.Circle(StructureXSize * 3), new Actions.SetFrames());

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

        public bool BlendLeft(bool reverse = false)
        {
            if (!CanBlendLeft && !reverse)
            {
                throw new Exception("structure can not blend terrain on left");
            } 
            if (!CanBlendRight && reverse)
            {
                throw new Exception("structure can not blend terrain on right");
            }

            int startX = 0;
            int startY = 0;

            int endX = 0;
            int endY = 0;
            
            if (reverse)
            {
                endX = X + StructureFloorXOffset + StructureFloorLength;
                endY = Y + StructureFloorYOffset;
                
                startX = endX + BlendDistance;
                startY = endY - 35; // to make sure that it starts outside the terrain
                
                while (!Terraria.WorldGen.SolidTile(startX, startY)) {
                    startY++;
                }
            }
            else
            {
                startX = X + StructureFloorXOffset;
                startY = Y + StructureFloorYOffset;

                endX = startX - BlendDistance;
                endY = startY - 35; // to make sure that it starts outside the terrain
                
                while (!Terraria.WorldGen.SolidTile(endX, endY)) {
                    endY++;
                }
            }

            


            ushort topTileType = 0;
            if (BlendTileID == 0)
            {
                topTileType = Main.tile[endX, endY].TileType;
            }
            else
            {
                topTileType = BlendTileID;
            }
            
            
            ushort fillTileType = 0;
            if (topTileType == TileID.Grass)
            {
                fillTileType = TileID.Dirt;
            } 
            else if (topTileType == TileID.JungleGrass)
            {
                fillTileType = TileID.Mud;
            }
            else
            {
                fillTileType = topTileType;
            }
            

            double slope = (double) (endY - startY) / (endX - startX) * -1;
            
            if (Debug)
            {
                Main.NewText($"sX: {startX} sY: {startY} eX: {endX} eY: {endY} slope: {slope}");
            }

            int frameCenterX = 0;
            int frameCenterY = 0;
            
            for (int i = BlendDistance; i >= 0; i--)
            {
                // get the top tile, change its values
                int topTileY = startY + (int)Math.Round(i * slope);

                if (Debug)
                {
                    Main.NewText($"x: {startX - i} y: {topTileY}");
                }

                if (i == BlendDistance / 2 || i - 1 == BlendDistance / 2)
                {
                    frameCenterX = startX - i;
                    frameCenterY = topTileY;
                }
                
                Tile tile = Main.tile[startX - i, topTileY];
                tile.HasTile = true;
                tile.Slope = SlopeType.Solid;
                tile.TileType = topTileType;

                //remove the tiles above
                for (int j = 1; j < 55; j++)
                {
                    Tile lowerTile = Main.tile[startX - j, topTileY - j];
                    lowerTile.HasTile = false;
                }

                // get/change the tiles beneath the top tiles
                int y = 1;
                while (!Terraria.WorldGen.SolidTile(startX - i, topTileY + y)) 
                {
                    Tile lowerTile = Main.tile[startX - i, topTileY + y];
                    lowerTile.HasTile = true;
                    lowerTile.Slope = SlopeType.Solid;
                    lowerTile.TileType = fillTileType;
                    
                    y++;
                }
                
                // make sure that the tile we found at the bottom is full
                Tile lowestTile = Main.tile[startX - i, topTileY + y];
                lowestTile.HasTile = true;
                lowestTile.Slope = SlopeType.Solid;
                
                lowestTile = Main.tile[startX - i, topTileY + y - 1];
                lowestTile.Slope = SlopeType.Solid;
            }

            FrameTiles(frameCenterX, frameCenterY, (int)(BlendDistance * 6));
            
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
        public override bool Debug => true;
        public override string FilePath => "Structures/mainHouse";
        public override int StructureXSize => 63;
        public override int StructureFloorLength => 63; //41
        public override int StructureFloorYOffset => 26;
        public override int StructureFloorXOffset => 0;
        public override bool CanHaveFoundation => true; 
        public override ushort FoundationTileID => TileID.Dirt;
        public override int FoudationRadiusOverride => 31;
        public override int FoudationYOffsetOverride => 36;
        public override int FoudationXOffsetOverride => 31;
        public override bool CanBlendLeft => true;
        public override bool CanBlendRight => true;
        public override int BlendDistance => 20;
        public override ushort BlendTileID => TileID.Grass;
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
