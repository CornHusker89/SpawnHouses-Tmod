using System;
using Terraria;
using Terraria.ID;
using Terraria.WorldBuilding;
using Terraria.DataStructures;
using Microsoft.Xna.Framework;

namespace SpawnHouses.Structures.Substructures;

public class ConnectPoint {
    
    
    public virtual int X { get; set; } = 0;
    public virtual int Y { get; set; } = 0;
    
#nullable enable
    public virtual Bridge? bridgeInfo { get; set; } = null;
#nullable disable
    
    public bool GenerateBridge(ConnectPoint other)
    {
        if (bridgeInfo == null || other.bridgeInfo == null) {
            return false;
        }

        return true;
    }

    public bool BlendLeft(Tile blendTileTop, ushort blendDistance, Tile? blendFillTile = null,
        bool canUsePartialTiles = true, bool reverseDirection = false, bool debug = false)
    {
        int startX = 0;
        int startY = 0;

        int endX = 0;
        int endY = 0;
        
        if (reverseDirection)
        {
            endX = X;
            endY = Y;
            
            startX = endX + blendDistance;
            startY = endY - 38; // to make sure that it starts outside the terrain
            
            while (!Terraria.WorldGen.SolidTile(startX, startY)) {
                startY++;
            }
        }
        else
        {
            startX = X;
            startY = Y;

            endX = startX - blendDistance;
            endY = startY - 38; // to make sure that it starts outside the terrain
            
            while (!Terraria.WorldGen.SolidTile(endX, endY)) {
                endY++;
            }
        }

        if (debug)
        {
            blendTileTop = new Tile();
            blendTileTop.TileType = TileID.Adamantite;
            blendFillTile = new Tile();
            blendFillTile.GetValueOrDefault().TileType = TileID.Cobalt;
        }
        else
        {
            if (!blendFillTile.HasValue)
            {
                ushort topTileType = blendTileTop.TileType;
                if (topTileType == TileID.Grass)
                {
                    blendFillTile = new Tile();
                    blendFillTile.GetValueOrDefault().TileType = TileID.Dirt;
                } 
                else if (topTileType == TileID.JungleGrass)
                {
                    blendFillTile = new Tile();
                    blendFillTile.GetValueOrDefault().TileType = TileID.Mud;
                }
                else
                {
                    blendFillTile = blendTileTop;
                }
            }
        }
        
        double slope = (double) (endY - startY) / (endX - startX) * -1;
        
        if (debug)
        {
            Main.NewText($"sX: {startX} sY: {startY} eX: {endX} eY: {endY} slope: {slope}");
        }

        // initialize the center tiles for when we call frametiles()
        int frameCenterX = 0;
        int frameCenterY = 0;
        
        // keep track of how far we filled the last tile down
        int lastTileVerticalFillLen = 1;
        
        for (int dX = blendDistance; dX >= 0; dX--)
        {
            // get the top tile of the final slope, change its values
            int topTileY = startY + (int)Math.Round(dX * slope);

            if (debug)
            {
                Main.NewText($"x: {startX - dX} y: {topTileY}");
            }
            
            // when we're roughly in the center of the blend, make the center of the frame 
            // for when we call frametiles()
            if (dX == blendDistance / 2 || dX - 1 == blendDistance / 2)
            {
                frameCenterX = startX - dX;
                frameCenterY = topTileY;
            }
            
            // make the top tile
            Tile tile = Main.tile[startX - dX, topTileY];
            tile = blendTileTop;
            tile.HasTile = true;
            
            // give the top tile a random slope if it's in the right spot
            int nextTileDx = 1; 
            if (slope < 0) nextTileDx = -1; 
            int nextTileY = startY + (int)Math.Round((dX + nextTileDx) * slope);
            
            if (canUsePartialTiles && topTileY != nextTileY )
            {
                // val of 0-2 --> full tile, 3 --> slope right/left, 4 --> half tile
                int randomVal = Terraria.WorldGen.genRand.Next(minValue: 1, maxValue: 5);
                if (randomVal == 3)
                {
                    if (slope > 0)
                    {
                        tile.IsHalfBlock = false;
                        tile.Slope = SlopeType.SlopeDownRight;
                    }
                    else
                    {
                        tile.IsHalfBlock = false;
                        tile.Slope = SlopeType.SlopeDownLeft;
                    }
                }
                else if (randomVal == 4)
                {
                    tile.Slope = SlopeType.Solid;
                    tile.IsHalfBlock = true;
                }
            }
            else
            {
                tile.Slope = SlopeType.Solid;
                tile.IsHalfBlock = false;
            }

            //remove the tiles above. start at 1 because we don't want to remove the top tile itself
            ushort dYUp = 1;
            while (Main.tile[startX - dX, topTileY - dYUp].HasTile)
            {
                Tile lowerTile = Main.tile[startX - dX, topTileY - dYUp];
                lowerTile.HasTile = false;
                dYUp++;
            }

            // get/change the tiles beneath the top tiles. make sure we establish a min/max depth based on the last tile
            ushort dYDown = 1;
            while ( (!Terraria.WorldGen.SolidTile(startX - dX, topTileY + dYDown) && dYDown <= (lastTileVerticalFillLen * 1.3) + 2) || dYDown <= (lastTileVerticalFillLen * 0.6))
            {
                Tile lowerTile = Main.tile[startX - dX, topTileY + dYDown];
                lowerTile = blendFillTile.GetValueOrDefault();
                lowerTile.HasTile = true;
                
                dYDown++;
            }

            lastTileVerticalFillLen = dYDown;
            
            // make sure that the tile we found at the bottom is full
            // 'dyDown' will end up as the y-coord of the lowest tile
            Tile lowestTile = Main.tile[startX - dX, topTileY + dYDown];
            lowestTile.HasTile = true;
            lowestTile.Slope = SlopeType.Solid;
            lowestTile.IsHalfBlock = false;
        }
        WorldUtils.Gen(new Point(frameCenterX, frameCenterY), new Shapes.Circle(radius: (int)(blendDistance * 6)), new Actions.SetFrames());
        
        return true;
    }

    public bool BlendRight(Tile blendTileTop, ushort blendDistance, Tile? blendFillTile = null,
        bool canUsePartialTiles = true, bool debug = false)
    {
        return BlendLeft(blendTileTop: blendTileTop, blendDistance: blendDistance, blendFillTile: blendFillTile,
            canUsePartialTiles: canUsePartialTiles, debug: debug);
    }
}