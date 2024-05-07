using System;
using Terraria;
using Terraria.ID;
using Terraria.WorldBuilding;
using Terraria.DataStructures;
using Microsoft.Xna.Framework;
using Terraria.ModLoader;

namespace SpawnHouses.Structures.Substructures;

public class ConnectPoint {
    private readonly Mod _mod = ModContent.GetInstance<SpawnHouses>();

    public ushort X { get; set; }
    public ushort Y { get; set; }
    private short _YOffset { get; set; }
    private short _XOffset { get; set; }
    
    public ConnectPoint(short xOffset, short yOffset)
    {
        _XOffset = xOffset;
        _YOffset = yOffset;
    }
    
    public void SetPosition(int mainStructureX, int mainStructureY)
    {
        X = Convert.ToUInt16(mainStructureX + _XOffset);
        Y = Convert.ToUInt16(mainStructureY + _YOffset);
    }
    
    public void BlendLeft(ushort topTileID, ushort blendDistance, ushort fillTileID = 0,
        bool canUsePartialTiles = true, bool removeWalls = true, bool reverseDirection = false, bool debug = false)
    {
        int startX;
        int startY;

        int endX;
        int endY;
        
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
            topTileID = TileID.Adamantite;
            fillTileID = TileID.Cobalt;
        }
        else
        {
            if (fillTileID == 0)
            {
                if (topTileID == TileID.Grass)
                {
                    fillTileID = TileID.Dirt;
                } 
                else if (topTileID == TileID.JungleGrass)
                {
                    fillTileID = TileID.Mud;
                }
                else
                {
                    fillTileID = topTileID;
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
            tile.HasTile = true;
            tile.BlockType = BlockType.Solid;
            tile.TileType = topTileID;
            if (removeWalls)
            {
                tile.WallType = WallID.None;
            }
            
            // give the top tile a random slope if it's in the right spot
            int nextTileDx = 1; 
            if (slope < 0) nextTileDx = -1; 
            int nextTileY = startY + (int)Math.Round((dX + nextTileDx) * slope);
            
            if (canUsePartialTiles && topTileY != nextTileY )
            {
                // val of 0-2 --> full tile, 3 --> slope right/left, 4 --> half tile
                int randomVal = Terraria.WorldGen.genRand.Next(minValue: 2, maxValue: 6);
                if (randomVal == 3 || randomVal == 4)
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
                else if (randomVal == 5)
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
            if (removeWalls)
            {
                while (Main.tile[startX - dX, topTileY - dYUp].HasTile || Main.tile[startX - dX, topTileY - dYUp].WallType != 0) 
                {
                    Tile upperTile = Main.tile[startX - dX, topTileY - dYUp];
                    upperTile.HasTile = false;
                    upperTile.WallType = WallID.None;
                    dYUp++;
                }
            }
            else
            {
                while (Main.tile[startX - dX, topTileY - dYUp].HasTile)
                {
                    Tile upperTile = Main.tile[startX - dX, topTileY - dYUp];
                    upperTile.HasTile = false;
                    dYUp++;
                }
            }


            // get/change the tiles beneath the top tiles. make sure we establish a min/max depth based on the last tile
            ushort dYDown = 1;
            while ( (!Terraria.WorldGen.SolidTile(startX - dX, topTileY + dYDown) && dYDown <= (lastTileVerticalFillLen * 1.3) + 2) || dYDown <= (lastTileVerticalFillLen * 0.6))
            {
                Tile lowerTile = Main.tile[startX - dX, topTileY + dYDown];
                lowerTile.HasTile = true;
                lowerTile.BlockType = BlockType.Solid;
                lowerTile.TileType = fillTileID;
                
                dYDown++;
            }

            lastTileVerticalFillLen = dYDown;
            
            // make sure that the tile we found at the bottom is full
            // 'dyDown' will end up as the y-coord of the lowest tile
            Tile lowestTile = Main.tile[startX - dX, topTileY + dYDown];

            bool overwriteTileType = !lowestTile.HasTile;
            lowestTile.HasTile = true;
            lowestTile.BlockType = BlockType.Solid;
            if (overwriteTileType)
            {
                lowestTile.TileType = fillTileID;
            }
            
        }
        WorldUtils.Gen(new Point(frameCenterX, frameCenterY), new Shapes.Circle(radius: (int)(blendDistance * 6)), new Actions.SetFrames());
    }

    public void BlendRight(ushort topTileID, ushort blendDistance, ushort fillTileID = 0,
        bool canUsePartialTiles = true, bool debug = false)
    {
        BlendLeft(topTileID: topTileID, blendDistance: blendDistance, fillTileID: fillTileID,
            canUsePartialTiles: canUsePartialTiles, reverseDirection: true, debug: debug);
    }

    private Tuple<double, double, double, ushort, ushort> _CalculateBridge(ConnectPoint other, double maxSlope)
    {
        ushort startX;
        ushort endX;
        ushort startY;
        ushort endY;
        if (X < other.X)
        {
            startX = X;
            endX = other.X;
            startY = Y;
            endY = other.Y;
        }
        else
        {
            startX = other.X;
            endX = X;
            startY = other.Y;
            endY = Y;
        }

        // straight up no clue how this works but make coefficients for ax^2 + bx + c parabola
        double a = -1 * Math.Abs((maxSlope - (2.0 * (endY - startY) / (endX - startX))) / (endX - startX));
        double b = (endY - startY - a * (endX * endX - startX * startX)) / (endX - startX);
        double c = startY - a * startX * startX - b * startX;
        return Tuple.Create(a, b, c, startX, endX);
    }
    
    public void GenerateBridge(ConnectPoint other, ushort tileID, double attemptSlope)
    {
        var parabola = _CalculateBridge(other, attemptSlope);
        double a = parabola.Item1;
        double b = parabola.Item2;
        double c = parabola.Item3;
        ushort startX = parabola.Item4;
        ushort endX = parabola.Item5;

        for (ushort bridgeTileX = (ushort)(startX + 1); bridgeTileX < endX; bridgeTileX++)
        {
            Tile tile = Main.tile[bridgeTileX, (ushort)Math.Floor(a * bridgeTileX * bridgeTileX + b * bridgeTileX + c)];
            tile.HasTile = true;
            tile.BlockType = BlockType.Solid;
            tile.TileType = tileID;
        }

        ushort centerX = (ushort)((X + other.X) / 2);
        ushort centerY = (ushort)((Y + other.Y) / 2);
        int radius = Math.Abs(X - other.X) + 4;
        WorldUtils.Gen(new Point(centerX, centerY), new Shapes.Circle(radius), new Actions.SetFrames());
    }

    public void GenerateBridge(ConnectPoint other, string structureFilePath, ushort structureLength,
        short structureYOffset, double attemptSlope)
    {
        if ((Math.Abs(X - other.X) - 1) % structureLength != 0)
        {
            throw new Exception("Bridge length cannot be resolved with the given BridgeStructure's length");
        }

        var parabola = _CalculateBridge(other, attemptSlope);
        double a = parabola.Item1;
        double b = parabola.Item2;
        double c = parabola.Item3;
        ushort startX = parabola.Item4;
        ushort endX = parabola.Item5;

        for (ushort bridgeStructureX = (ushort)(startX + 1); bridgeStructureX < endX; bridgeStructureX += structureLength)
        {
            ushort bridgeStructureY = (ushort)Math.Floor(a * bridgeStructureX * bridgeStructureX + b * bridgeStructureX + c + structureYOffset);
            StructureHelper.Generator.GenerateStructure(structureFilePath, new Point16(X:bridgeStructureX, Y:bridgeStructureY), _mod);
        }

        ushort centerX = (ushort)((X + other.X) / 2);
        ushort centerY = (ushort)((Y + other.Y) / 2);
        int radius = Math.Abs(X - other.X) + 4;
        WorldUtils.Gen(new Point(centerX, centerY), new Shapes.Circle(radius), new Actions.SetFrames());
    }
    
}