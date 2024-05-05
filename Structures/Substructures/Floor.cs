using System;
using Terraria;
using Terraria.ID;
using Terraria.WorldBuilding;
using Microsoft.Xna.Framework;

namespace SpawnHouses.Structures.Substructures;

public class Floor {
    public virtual int X { get; set; } = 0;
    public virtual int Y { get; set; } = 0;

    public virtual ushort floorLength { get; set; } = 1;

    public bool GenerateBeams(Tile beamTile, ushort beamInterval, ushort beamsCount, ushort beamsXOffset = 0,
        ushort maxBeamSize = 50, bool debug = false) 
    {
            for (int i = 0; i < beamsCount; i++)
            {
                bool validBeamLocation = true;
                int y2 = Y + 1; //put us 1 below the floor
                int x2 = X + beamsXOffset + (i * beamInterval);

                //if there's a tile there already, dont place a beam
                if (Terraria.WorldGen.SolidTile(x2, Y + y2)) { continue; }
				
                while (!Terraria.WorldGen.SolidTile(x2, Y + y2))
                {
                    if (y2 >= maxBeamSize + Y)
                    {
                        validBeamLocation = false;
                        break;
                    }
                    y2++;
                }

                if (debug)
                {
                    Main.NewText($"X:{x2}, Y:{Y}, X2:{x2}, Y2:{y2}, i:{i}, interval:{beamInterval}");
                }

                if (validBeamLocation)
                {
                    for (int j = 0; j < y2; j++)
                    {
                        Tile tile = Main.tile[x2, Y + j];
                        tile = beamTile;
                        tile.HasTile = true;
                    }

                    //make the tile beneath the beam (and the one just above) a full block
                    Tile bottomBeamTile = Main.tile[x2, Y + y2 - 1];
                    bottomBeamTile.Slope = SlopeType.Solid;
                    bottomBeamTile.IsHalfBlock = false;
                    
                    Tile bottomTerrainTile = Main.tile[x2, Y + y2];
                    bottomTerrainTile.Slope = SlopeType.Solid;
                    bottomTerrainTile.IsHalfBlock = false;
                }
            }
            // set the tile frames
            WorldUtils.Gen(new Point(X + ( (beamInterval + 1) * beamsCount), Y + (maxBeamSize / 2)),
                new Shapes.Circle(radius: maxBeamSize), new Actions.SetFrames());
            
            return true;
    }

    public bool GenerateFoundation(Tile? foundationTile = null, short foudationRadius = 0, ushort foudationXOffset = 0,
        ushort foudationYOffset = 0, bool debug = false) 
    {

        
        if (!foundationTile.HasValue) 
        {
            int x2 = X + (floorLength / 2);
            int y2 = Y + (foudationRadius / 2);
            while (!Terraria.WorldGen.SolidTile(X, Y))
            {
                y2++;
            }
    
            Tile tile = Main.tile[x2, y2];
            tile.Slope = SlopeType.Solid;
            tile.IsHalfBlock = false;
            foundationTile = tile;
        }
        
        
        int centerX = 0;
        
        if (foudationXOffset == 0)
        {
            centerX = X + (floorLength / 2);
        }
        else
        {
            centerX = X + foudationXOffset;
        }
        
        int centerY = 0;

        if (foudationYOffset == 0)
        {
            centerY = Y + foudationYOffset + (foudationYOffset / 2);
        }
        else
        {
            centerY = Y + foudationYOffset;
        }
        

        if (foudationRadius == 0)
        {
            foudationRadius = Convert.ToInt16(floorLength / 2);
        }

        if (debug)
        {
            Main.NewText($"cx: {centerX} cy: {centerY} rad: {foudationRadius} y: {Y} FoudationYOffset: {foudationYOffset}");
        }

        WorldUtils.Gen(new Point(centerX, centerY), new Shapes.Circle(foudationRadius),
            new Actions.SetTile(foundationTile.GetValueOrDefault().TileType));

        WorldUtils.Gen(new Point(centerX, centerY), new Shapes.Circle(foudationRadius + 5),
            new Actions.SetFrames());
    
        return true;
    }
}