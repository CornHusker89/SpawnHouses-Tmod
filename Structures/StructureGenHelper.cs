using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using ReLogic.Utilities;
using SpawnHouses.Structures.StructureParts;
using Terraria;
using Terraria.DataStructures;
using Terraria.GameContent.Generation;
using Terraria.ID;
using Terraria.WorldBuilding;

namespace SpawnHouses.Structures;

public static class StructureGenHelper
{
    private class ClearTileSafe : GenAction
    {
        private bool _frameNeighbors;

        public ClearTileSafe(bool frameNeighbors = false)
        {
            _frameNeighbors = frameNeighbors;
        }

        public override bool Apply(Point origin, int x, int y, params object[] args)
        {
            ClearChest(x, y);
            WorldUtils.ClearTile(x, y, _frameNeighbors);
            return UnitApply(origin, x, y, args);
        }
    }

    /// <summary>
    /// If any part of a chest is at (x, y), it will completely remove it
    /// </summary>
    /// <param name="x"></param>
    /// <param name="y"></param>
    public static void ClearChest(int x, int y)
    {
        void Clear(int chestX, int chestY)
        {
            int id = Chest.FindChest(chestX, chestY);
            Chest.DestroyChestDirect(chestX, chestY, id);
            WorldUtils.ClearTile(chestX, chestY, true);
            WorldUtils.ClearTile(chestX + 1, chestY, true);
            WorldUtils.ClearTile(chestX, chestY + 1, true);
            WorldUtils.ClearTile(chestX + 1, chestY + 1, true);
        }

        if (Chest.FindChest(x, y) != -1)
            Clear(x, y);
        if (Chest.FindChest(x - 1, y) != -1)
            Clear(x - 1, y);
        if (Chest.FindChest(x + 1, y) != -1)
            Clear(x + 1, y);
        if (Chest.FindChest(x, y - 1) != -1)
            Clear(x, y - 1);
        if (Chest.FindChest(x - 1, y - 1) != -1)
            Clear(x - 1, y - 1);
        if (Chest.FindChest(x + 1, y - 1) != -1)
            Clear(x + 1, y - 1);
        if (Chest.FindChest(x, y - 2) != -1)
            Clear(x, y - 2);
        if (Chest.FindChest(x - 1, y - 2) != -1)
            Clear(x - 1, y - 2);
        if (Chest.FindChest(x + 1, y - 2) != -1)
            Clear(x + 1, y - 2);
    }

    /// <summary>
    /// Places a bush (walls) with many variants, from 1x1 to 2x3 at the coordinates given
    /// </summary>
    /// <param name="start"></param>
    /// <param name="tileID"></param>
    /// <param name="wallBlacklistIDs"></param>
    public static void PlaceBush(Point start, ushort tileID = WallID.LivingLeaf, params ushort[] wallBlacklistIDs)
    {
        void PlaceWall(int i, int j)
        {
            Tile tile = Main.tile[i, j];
            if (!wallBlacklistIDs.Contains(tile.WallType))
                tile.WallType = tileID;
        }

        int x = start.X;
        int y = start.Y;

        switch (Terraria.WorldGen.genRand.Next(0, 6))
        {
            case 0:
                PlaceWall(x, y);
                PlaceWall(x, y + 1);
                PlaceWall(x - 1, y + 1);
                PlaceWall(x, y + 2);
                PlaceWall(x - 1, y + 2);
                break;

            case 1:
                PlaceWall(x, y);
                PlaceWall(x, y + 1);
                PlaceWall(x + 1, y);
                PlaceWall(x + 1, y + 1);
                PlaceWall(x, y + 2);
                PlaceWall(x + 1, y + 2);
                break;

            case 2:
                PlaceWall(x, y);
                PlaceWall(x, y + 1);
                PlaceWall(x + 1, y);
                PlaceWall(x + 1, y + 1);
                PlaceWall(x + 1, y - 1);
                PlaceWall(x + 1, y + 2);
                break;

            case 3:
                PlaceWall(x, y);
                PlaceWall(x, y - 1);
                PlaceWall(x, y + 1);
                PlaceWall(x, y + 2);
                break;

            case 4:
                PlaceWall(x, y);
                PlaceWall(x, y - 1);
                PlaceWall(x - 1, y - 1);
                PlaceWall(x - 2, y);
                PlaceWall(x, y + 1);
                PlaceWall(x - 2, y + 1);
                PlaceWall(x - 1, y + 2);
                break;

            case 5:
                PlaceWall(x - 1, y);
                PlaceWall(x + 1, y);
                PlaceWall(x, y - 1);
                PlaceWall(x + 1, y - 1);
                break;
        }
    }

    /// <summary>
    /// 65% chance of placing cobweb on every tile in designated area
    /// </summary>
    /// <param name="start"></param>
    /// <param name="length"></param>
    /// <param name="height"></param>
    /// <param name="wallWhitelistIDs"></param>
    public static void GenerateCobwebs(Point start, ushort length, ushort height, params ushort[] wallWhitelistIDs)
    {
        WorldUtils.Gen(start, new Shapes.Rectangle(length, height),
            Actions.Chain(
                new Modifiers.Dither(0.35),
                new Actions.Custom((i, j, args) =>
                {
                    if (!Main.tile[i, j].HasTile)
                    {
                        if (wallWhitelistIDs.Length > 0) // if whitelist mode is on
                        {
                            if (wallWhitelistIDs.Contains(Main.tile[i, j].WallType))
                            {
                                Tile tile = Main.tile[i, j];
                                tile.HasTile = true;
                                tile.TileType = TileID.Cobweb;
                                tile.Slope = SlopeType.Solid;
                                tile.IsHalfBlock = false;
                            }
                        }
                        else
                        {
                            Tile tile = Main.tile[i, j];
                            tile.HasTile = true;
                            tile.TileType = TileID.Cobweb;
                            tile.Slope = SlopeType.Solid;
                            tile.IsHalfBlock = false;
                        }
                    }

                    return true;
                }),
                new Actions.SetFrames()
            )
        );
    }

    /// <summary>
    /// Generates beams of a specific tile type
    /// </summary>
    /// <param name="start"></param>
    /// <param name="beamTile"></param>
    /// <param name="beamInterval"></param>
    /// <param name="beamsCount"></param>
    /// <param name="maxBeamSize"></param>
    /// <param name="stopOnNonSolidTile"></param>
    public static void GenerateBeams(Point start, Tile beamTile, ushort beamInterval, ushort beamsCount,
        ushort maxBeamSize = 50, bool stopOnNonSolidTile = false)
    {

        for (int i = 0; i < beamsCount; i++)
        {
            bool validBeamLocation = true;
            int y2 = 1; //put us 1 below the floor
            int x2 = start.X + (i * beamInterval);

            //if there's a tile there already, don't place a beam
            if (Terraria.WorldGen.SolidTile(x2, start.Y + y2)) continue;

            while (!Terraria.WorldGen.SolidTile(x2, start.Y + y2) &&
                   !(stopOnNonSolidTile && Main.tile[x2, start.Y + y2].HasTile))
            {
                if (y2 >= maxBeamSize)
                {
                    validBeamLocation = false;
                    break;
                }

                y2++;
            }

            if (validBeamLocation)
            {
                for (int j = 0; j < y2; j++)
                {
                    Tile tile = Main.tile[x2, start.Y + j];
                    tile.HasTile = beamTile.HasTile;
                    tile.Slope = SlopeType.Solid;
                    tile.IsHalfBlock = false;
                    tile.TileType = beamTile.TileType;
                    tile.TileColor = beamTile.TileColor;
                }

                //make the tile beneath the beam (and the one just above) a full block
                Tile bottomTerrainTile = Main.tile[x2, start.Y + y2];
                bottomTerrainTile.Slope = SlopeType.Solid;
                bottomTerrainTile.IsHalfBlock = false;
            }
        }

        // set the tile frames
        WorldUtils.Gen(new Point(start.X + ((beamInterval + 1) * beamsCount), start.Y + (maxBeamSize / 2)),
            new Shapes.Circle(radius: maxBeamSize), new Actions.SetFrames());
    }

    /// <summary>
    /// Generates a circle of a specific tile
    /// </summary>
    /// <param name="start"></param>
    /// <param name="tileID"></param>
    /// <param name="foundationRadius"></param>
    public static void GenerateFoundation(Point start, ushort tileID, int foundationRadius, bool useHalfCircle = false)
    {
        if (useHalfCircle)
        {
            WorldUtils.Gen(start, new Shapes.HalfCircle(foundationRadius), Actions.Chain(
                new Modifiers.Flip(false, true),
                new Actions.Custom((i, j, args) =>
                {
                    {
                        Tile tile = Main.tile[i, j];
                        tile.HasTile = true;
                        tile.TileType = tileID;
                        tile.Slope = SlopeType.Solid;
                        tile.IsHalfBlock = false;
                    }
                    return true;
                })
            ));
        }
        else
            WorldUtils.Gen(start, new Shapes.Circle(foundationRadius),
                new Actions.Custom((i, j, args) =>
                {
                    {
                        Tile tile = Main.tile[i, j];
                        tile.HasTile = true;
                        tile.TileType = tileID;
                        tile.Slope = SlopeType.Solid;
                        tile.IsHalfBlock = false;
                    }
                    return true;
                })
            );
    }

    /// <summary>
    /// digs a simple tunnel. used by the mineshaft
    /// </summary>
    /// <param name="start"></param>
    /// <param name="randomStepOffset"></param>
    /// <param name="steps"></param>
    public static void DigVerticalTunnel(Point start, int randomStepOffset, int steps)
    {
        int initialYOffset = 0;
        for (int i = 0; i < steps; i++)
        {
            int width = Terraria.WorldGen.genRand.Next(7, 17);
            double toleranceFactor = width / 16.0;
            int baseXOffset = i == 0
                ? 0
                : (int)(Terraria.WorldGen.genRand.Next(-randomStepOffset, randomStepOffset + 1) * toleranceFactor);
            double vectorXOffset = i == 0
                ? 0
                : (int)(Terraria.WorldGen.genRand.Next(-randomStepOffset, randomStepOffset + 1) * toleranceFactor *
                        0.65);
            if (i == 0 || i == steps - 1)
            {
                initialYOffset = (int)(width * 1.7);
                baseXOffset = 0;
                vectorXOffset = 0;
            }

            WorldUtils.Gen(new Point(start.X + baseXOffset, start.Y + initialYOffset + i * 15),
                new Shapes.Tail(width, new Vector2D(vectorXOffset, width * 1.7)),
                new ClearTileSafe(true)
            );

            WorldUtils.Gen(new Point(start.X + baseXOffset, start.Y + initialYOffset + 1 + i * 15),
                new Shapes.Tail(width, new Vector2D(vectorXOffset, width * -1.7)),
                new ClearTileSafe(true)
            );
        }
    }

    /// <summary>
    /// gets data about the average surface level, based on start/end, and the raycast's starting y
    /// </summary>
    /// <param name="x1"></param>
    /// <param name="x2"></param>
    /// <param name="y"></param>
    /// <param name="step">each time it measures, increase target x by this</param>
    /// <param name="maxCastDistance"></param>
    /// <returns></returns>
    public static (double average, double sd) GetSurfaceLevel(
        int x1, int x2, int y, byte step = 1, ushort maxCastDistance = 50)
    {
        List<double> surfaceLevels = [];
        for (int i = x1; i <= x2; i += step)
        for (int j = 0; j < maxCastDistance; j++)
            if (Terraria.WorldGen.SolidTile(i, y + j))
            {
                surfaceLevels.Add(y + j);
                break;
            }

        double average = surfaceLevels.Average();
        double sumOfSquaresOfDifferences = surfaceLevels.Select(val => (val - average) * (val - average)).Sum();
        double sd = Math.Sqrt(sumOfSquaresOfDifferences / surfaceLevels.Count);
        return (average, sd);
    }

    /// <summary>
    /// Gives distance to first solid tile, pointing downwards
    /// </summary>
    /// <param name="x"></param>
    /// <param name="y"></param>
    /// <param name="endPos"></param>
    /// <param name="maxCastDistance"></param>
    /// <returns></returns>
    /// <exception cref="Exception"></exception>
    public static (int distance, int yCoord) SurfaceRaycast(int x, int y, int maxCastDistance = 100)
    {
        for (int i = 0; i < maxCastDistance; i++)
            if (Terraria.WorldGen.SolidTile(x, y + i))
                return (i, y + i);
        throw new Exception("surface not found within " + maxCastDistance + " tiles");
    }

    /// <summary>
    /// Gives distance to first solid tile, pointing downwards, but only looks for solid tiles after it's no longer in a solid tile. Must be called from within a solid tile
    /// </summary>
    /// <param name="x"></param>
    /// <param name="y"></param>
    /// <param name="maxAirGapLength"></param>
    /// <param name="maxCastDistance"></param>
    /// <returns></returns>
    /// <exception cref="Exception"></exception>
    public static (bool found, int distance, int yCoord) SurfaceRaycastFromInsideTile(int x, int y,
        int maxCastDistance = 40)
    {
        bool inSolid = true;
        for (int i = 0; i < maxCastDistance; i++)
            if (Terraria.WorldGen.SolidTile(x, y + i))
            {
                if (!inSolid)
                    return (true, i, y + i);
            }
            else if (inSolid)
                inSolid = false;

        return (false, -1, -1);
    }


    /// <summary>
    /// Gives terrain a smooth slope from the start point to a raycasted point
    /// </summary>
    /// <param name="start"></param>
    /// <param name="blendDistance"></param>
    /// <param name="topTileID"></param>
    /// <param name="fillTileID">defaults to the topTileID, but has special cases when top tile is a grass variant</param>
    /// <param name="canUsePartialTiles">if true, the blending will use slopes and half-blocks to smooth the slope</param>
    /// <param name="removeWalls"></param>
    /// <param name="maxHeight"></param>
    /// <param name="blendLeftSide">if true, blending happens on the left side of start. otherwise, the right side</param>
    public static void Blend(Point start, ushort blendDistance, ushort topTileID, ushort fillTileID = 0,
        bool canUsePartialTiles = true,
        bool removeWalls = true, ushort maxHeight = 38, bool blendLeftSide = true)
    {
        int startX;
        int startY;

        int endX;
        int endY;

        if (!blendLeftSide)
        {
            endX = start.X;
            endY = start.Y;

            startX = endX + blendDistance;
            startY = SurfaceRaycast(startX, endY - maxHeight).yCoord;

            var possibleDeeperCast = SurfaceRaycastFromInsideTile(startX, startY, 20);
            if (possibleDeeperCast is { found: true, distance: < 20 })
                startY = possibleDeeperCast.yCoord;
        }
        else
        {
            startX = start.X;
            startY = start.Y;

            endX = startX - blendDistance;
            endY = SurfaceRaycast(endX, startY - maxHeight).yCoord;

            var possibleDeeperCast = SurfaceRaycastFromInsideTile(endX, endY, 20);
            if (possibleDeeperCast is { found: true, distance: < 20 })
                endY = possibleDeeperCast.yCoord;
        }

        if (fillTileID == 0)
        {
            if (topTileID == TileID.Grass)
                fillTileID = TileID.Dirt;

            else if (topTileID == TileID.JungleGrass)
                fillTileID = TileID.Mud;

            else
                fillTileID = topTileID;
        }

        double slope = (double)(endY - startY) / (endX - startX) * -1;

        // initialize the center tiles for when we call frametiles()
        int frameCenterX = 0;
        int frameCenterY = 0;

        // keep track of how far we filled the last tile down
        int lastTileVerticalFillLen = 1;

        for (int dX = blendDistance; dX >= 0; dX--)
        {
            // get the top tile of the final slope, change its values
            int topTileY = startY + (int)Math.Round(dX * slope);

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
                tile.WallType = WallID.None;

            // give the top tile a random slope if it's in the right spot
            int nextTileDx = 1;
            if (slope < 0) nextTileDx = -1;
            int nextTileY = startY + (int)Math.Round((dX + nextTileDx) * slope);

            if (canUsePartialTiles && topTileY != nextTileY)
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
            byte airCounter = 0;
            if (removeWalls)
            {
                while (airCounter < 10 || Main.tile[startX - dX, topTileY - dYUp].WallType != 0)
                {
                    Tile upperTile = Main.tile[startX - dX, topTileY - dYUp];
                    upperTile.HasTile = false;
                    upperTile.WallType = WallID.None;
                    dYUp++;

                    if (!Main.tile[startX - dX, topTileY - dYUp].HasTile)
                        airCounter++;
                }
            }
            else
            {
                while (airCounter < 10)
                {
                    Tile upperTile = Main.tile[startX - dX, topTileY - dYUp];
                    upperTile.HasTile = false;
                    dYUp++;

                    if (!Main.tile[startX - dX, topTileY - dYUp].HasTile)
                        airCounter++;
                }
            }


            // get/change the tiles beneath the top tiles. make sure we establish a min/max depth based on the last tile
            ushort dYDown = 1;
            ushort fillCount = 0;
            while (((!Terraria.WorldGen.SolidTile(startX - dX, topTileY + dYDown) &&
                     dYDown <= lastTileVerticalFillLen * 1.3 + 2) || dYDown <= lastTileVerticalFillLen * 0.6) &&
                   fillCount < 25)
            {
                Tile lowerTile = Main.tile[startX - dX, topTileY + dYDown];
                lowerTile.HasTile = true;
                lowerTile.BlockType = BlockType.Solid;
                lowerTile.TileType = fillTileID;

                dYDown++;
                fillCount++;
            }

            lastTileVerticalFillLen = dYDown;

            // make sure that the tile we found at the bottom is full
            // 'dyDown' will end up as the y-coord of the lowest tile
            Tile lowestTile = Main.tile[startX - dX, topTileY + dYDown];

            bool overwriteTileType = !lowestTile.HasTile;
            lowestTile.HasTile = true;
            lowestTile.BlockType = BlockType.Solid;
            if (overwriteTileType)
                lowestTile.TileType = fillTileID;
        }

        WorldUtils.Gen(new Point(frameCenterX, frameCenterY), new Shapes.Circle(radius: blendDistance * 6),
            new Actions.SetFrames());
    }

    public static void Blend(ConnectPoint start, ushort blendDistance, ushort topTileID, ushort fillTileID = 0,
        bool canUsePartialTiles = true,
        bool removeWalls = true, ushort maxHeight = 38, bool blendLeftSide = true)
    {
        Blend(new Point(start.X, start.Y), blendDistance, topTileID, fillTileID, canUsePartialTiles, removeWalls,
            maxHeight, blendLeftSide);
    }

    /// <summary>
    /// Gives the y-pos of all tiles from the point (inclusive), moving length - 1 tiles to the right. intended to follow rooftops/terrain
    /// </summary>
    /// <param name="start">starting point. expected to be on a tile, or 1 tile above</param>
    /// <param name="length"></param>
    /// <param name="requiresSolidTiles">If true, 'valid' tiles will have to pass both tile.HasTile and WorldGen.SolidTile(tile)</param>
    /// <returns>y-position and slope of every tile along length. Remember that a positive slope results in going down, not up</returns>
    public static (int[] pos, double[] slope) GetTopTilesPos(Point16 start, int length, bool requiresSolidTiles = false)
    {
        bool IsValidTile(Tile tile)
        {
            return tile.HasTile && (!requiresSolidTiles || Terraria.WorldGen.SolidTile(tile));
        }

        // get the pos
        int[] pos = new int[length];
        int lastY = start.Y;
        for (int i = 0; i < length; i++)
        {
            /*
            if cur target has a tile
                go up until we find air
                next tile is y + 1
            if cur target has no tile
                go up 1, if there is a tile, thats the one
                go down until tile is found
            */
            
            int jumpDist = 0;
            
            if (IsValidTile(Main.tile[start.X + i, lastY]))
            {
                do
                    jumpDist--;
                while (IsValidTile(Main.tile[start.X + i, lastY + jumpDist]));
                jumpDist++;
            }
            else
            {
                if (IsValidTile(Main.tile[start.X + i, lastY - 1]))
                    jumpDist--;
                else
                    do
                        jumpDist++;
                    while (!IsValidTile(Main.tile[start.X + i, lastY + jumpDist]));
            }

            lastY += jumpDist;
            pos[i] = lastY;
        }

        // get the slope
        double[] slope = new double[length];
        int lastChangeIndex = 0;
        for (int i = 0; i < length - 1; i++)
        {
            if (pos[i] == pos[i + 1])
                slope[i] = 0;
            else
            {
                // if we've had a flat part, not followed by a jump of > 1
                if (lastChangeIndex != i - 1 && Math.Abs(pos[i] - pos[lastChangeIndex]) > 1.05)
                {
                    double newSlope = (double)(pos[i] - pos[lastChangeIndex]) / (i - lastChangeIndex);
                    for (int j = lastChangeIndex + 1; j <= i; j++)
                        slope[j] = newSlope;
                }
                else
                    slope[i] = pos[i + 1] - pos[i];
        
                lastChangeIndex = i;
            }
        }
        slope[length - 1] = slope[length - 2];
        
        return (pos, slope);
    }

    /// <summary>
    /// Gives the lengths and starts of all flat spots, using tile y-coords. Made to take the input from GetTopTilesPos()
    /// </summary>
    /// <param name="tiles"></param>
    /// <returns></returns>
    public static (List<int> flatStartIndexes, List<int> flatLengths) GetFlatTiles((int[] pos, double[] slope) tiles)
    {
        List<int> flatLengths = [];
        List<int> flatStartIndexes = [];
        int currentFlatStartIndex = 0;
        for (int i = 1; i < tiles.pos.Length; i++)
        {
            if (tiles.pos[i] != tiles.pos[i - 1])
            {
                // remove flats with length of 1
                if (i - currentFlatStartIndex != 1)
                {
                    flatLengths.Add(i - currentFlatStartIndex);
                    flatStartIndexes.Add(currentFlatStartIndex);
                }

                currentFlatStartIndex = i;
            }
        }
        
        // ensure we got the last flat, if it exists
        if (tiles.pos.Length >= 2 && tiles.pos[^1] == tiles.pos[^2])
        {
            flatLengths.Add(tiles.pos.Length - currentFlatStartIndex);
            flatStartIndexes.Add(currentFlatStartIndex);
        }

        return (flatStartIndexes, flatLengths);
    }
    
    /// <summary>
    /// Gives the starts of all jumps AT or OVER a certain threshold, along with info from the highest tiles, using tile y-coords. 
    /// Made to take the input from GetTopTilesPos()
    /// </summary>
    /// <param name="tiles"></param>
    /// <param name="threshold"></param>
    /// <returns>indexes of the jumps, index of the start of the highest point, and it's length</returns>
    public static (List<int> jumpIndexes, int lowestStart, int lowestLength) GetJumpsInfo((int[] pos, double[] slope) tiles, int threshold = 4)
    {
        List<int> jumpIndexes = [];
        int lowestPos = tiles.pos[0];
        int highestStart = 0;
        int highestLength = 1;
        for (int i = 1; i < tiles.pos.Length; i++)
        {
            if (Math.Abs(tiles.pos[i] - tiles.pos[i - 1]) >= 4)
                jumpIndexes.Add(i);
            if (tiles.pos[i] < lowestPos)
            {
                lowestPos = tiles.pos[i];
                highestLength = 0;
                highestStart = i;
            }
            if (tiles.pos[i] == lowestPos)
                highestLength += 1;
        }
        return (jumpIndexes, highestStart, highestLength);
    }
}