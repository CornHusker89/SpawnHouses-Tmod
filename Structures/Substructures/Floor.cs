using System;
using Terraria;
using Terraria.ID;
using Terraria.WorldBuilding;
using Microsoft.Xna.Framework;

namespace SpawnHouses.Structures.Substructures;

public class Floor {
    public ushort X { get; set; }
    public ushort Y { get; set; }
    private short _YOffset { get; set; }
    private short _XOffset { get; set; }

    public ushort FloorLength { get; set; }
    
    public Floor(short xOffset, short yOffset, ushort floorLength)
    {
        _XOffset = xOffset;
        _YOffset = yOffset;
        this.FloorLength = floorLength;
    }

    public void SetPosition(ushort mainStructureX, ushort mainStructureY)
    {
        X = Convert.ToUInt16(mainStructureX + _XOffset);
        Y = Convert.ToUInt16(mainStructureY + _YOffset);
    }

    public void GenerateBeams(ushort tileID, ushort beamInterval, ushort beamsCount, byte tileColor = 0, ushort beamsXOffset = 0,
        ushort maxBeamSize = 50, bool debug = false) 
    {
            for (int i = 0; i < beamsCount; i++)
            {
                bool validBeamLocation = true;
                int y2 = 1; //put us 1 below the floor
                int x2 = X + beamsXOffset + (i * beamInterval);

                //if there's a tile there already, dont place a beam
                if (Terraria.WorldGen.SolidTile(x2, Y + y2)) { continue; }
				
                while (!Terraria.WorldGen.SolidTile(x2, Y + y2))
                {
                    if (y2 >= maxBeamSize)
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
                        tile.HasTile = true;
                        tile.BlockType = BlockType.Solid;
                        tile.TileType = tileID;
                        tile.TileColor = tileColor;
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
    }

    public void GenerateFoundation(ushort tileID = 0, ushort foundationRadius = 0, ushort foundationXOffset = 0,
        ushort foundationYOffset = 0, bool debug = false) 
    {
        
        if (tileID == 9999) 
        {
            int x2 = X + (FloorLength / 2);
            int y2 = Y + (foundationRadius / 2);
            while (!Terraria.WorldGen.SolidTile(X, y2))
            {
                y2++;
            }
    
            Tile tile = Main.tile[x2, y2];
            tile.Slope = SlopeType.Solid;
            tile.IsHalfBlock = false;
            tileID = tile.TileType;
        }

        int centerX = X + foundationXOffset + (FloorLength / 2);
        int centerY = Y + foundationYOffset;

        if (foundationRadius == 0)
        {
            foundationRadius = Convert.ToUInt16(FloorLength / 2);
        }

        if (debug)
        {
            Main.NewText($"cx: {centerX} cy: {centerY} rad: {foundationRadius} x: {X} y: {Y} FoundationYOffset: {foundationYOffset}");
        }

        WorldUtils.Gen(new Point(centerX, centerY), new Shapes.Circle(foundationRadius),
            new Actions.SetTile(type: tileID));
    }
}