using System;
using System.Collections.Generic;
using System.Linq;
using Terraria;
using Terraria.DataStructures;

namespace SpawnHouses.Helpers;

public static class RaycastHelper {
    /// <summary>
    ///     gets data about the average surface level, based on start/end, and the raycast's starting y
    /// </summary>
    /// <param name="x1"></param>
    /// <param name="x2"></param>
    /// <param name="y"></param>
    /// <param name="step">each time it measures, increase target x by this</param>
    /// <param name="maxCastDistance"></param>
    /// <returns></returns>
    public static (double average, double sd) GetSurfaceLevel(
        int x1, int x2, int y, byte step = 1, ushort maxCastDistance = 50) {
        List<double> surfaceLevels = [];
        for (int i = x1; i <= x2; i += step)
        for (int j = 0; j < maxCastDistance; j++)
            if (Terraria.WorldGen.SolidTile(i, y + j)) {
                surfaceLevels.Add(y + j);
                break;
            }

        double average = surfaceLevels.Average();
        double sumOfSquaresOfDifferences = surfaceLevels.Select(val => (val - average) * (val - average)).Sum();
        double sd = Math.Sqrt(sumOfSquaresOfDifferences / surfaceLevels.Count);
        return (average, sd);
    }

    /// <summary>
    ///     Gives distance to first solid tile, pointing downwards
    /// </summary>
    /// <param name="x"></param>
    /// <param name="y"></param>
    /// <param name="endPos"></param>
    /// <param name="maxCastDistance"></param>
    /// <returns></returns>
    /// <exception cref="Exception"></exception>
    public static (int distance, int yCoord) SurfaceRaycast(int x, int y, int maxCastDistance = 100) {
        for (int i = 0; i < maxCastDistance; i++)
            if (Terraria.WorldGen.SolidTile(x, y + i))
                return (i, y + i);
        throw new Exception("surface not found within " + maxCastDistance + " tiles");
    }

    /// <summary>
    ///     Gives distance to first solid tile, pointing downwards, but only looks for solid tiles after it's no longer in a
    ///     solid tile. Must be called from within a solid tile
    /// </summary>
    /// <param name="x"></param>
    /// <param name="y"></param>
    /// <param name="maxAirGapLength"></param>
    /// <param name="maxCastDistance"></param>
    /// <returns></returns>
    /// <exception cref="Exception"></exception>
    public static (bool found, int distance, int yCoord) SurfaceRaycastFromInsideTile(int x, int y,
        int maxCastDistance = 40) {
        bool inSolid = true;
        for (int i = 0; i < maxCastDistance; i++)
            if (Terraria.WorldGen.SolidTile(x, y + i)) {
                if (!inSolid)
                    return (true, i, y + i);
            }
            else if (inSolid) {
                inSolid = false;
            }

        return (false, -1, -1);
    }

    /// <summary>
    ///     Gives the y-pos of all tiles from the point (inclusive), moving length - 1 tiles to the right. intended to follow
    ///     rooftops/terrain
    /// </summary>
    /// <param name="start">starting point. expected to be on a tile, or 1 tile above</param>
    /// <param name="length"></param>
    /// <param name="requiresSolidTiles">
    ///     If true, 'valid' tiles will have to pass both tile.HasTile and
    ///     WorldGen.SolidTile(tile)
    /// </param>
    /// <returns>y-position and slope of every tile along length. Remember that a positive slope results in going down, not up</returns>
    public static (int[] pos, double[] slope)
        GetTopTilesPos(Point16 start, int length, bool requiresSolidTiles = false) {
        bool IsValidTile(Tile tile) {
            return tile.HasTile && (!requiresSolidTiles || Terraria.WorldGen.SolidTile(tile));
        }

        // get the pos
        int[] pos = new int[length];
        int lastY = start.Y;
        for (int i = 0; i < length; i++) {
            /*
            if cur target has a tile
                go up until we find air
                next tile is y + 1
            if cur target has no tile
                go up 1, if there is a tile, thats the one
                go down until tile is found
            */

            int jumpDist = 0;

            if (IsValidTile(Main.tile[start.X + i, lastY])) {
                do {
                    jumpDist--;
                } while (IsValidTile(Main.tile[start.X + i, lastY + jumpDist]));

                jumpDist++;
            }
            else {
                if (IsValidTile(Main.tile[start.X + i, lastY - 1]))
                    jumpDist--;
                else
                    do {
                        jumpDist++;
                    } while (!IsValidTile(Main.tile[start.X + i, lastY + jumpDist]));
            }

            lastY += jumpDist;
            pos[i] = lastY;
        }

        // get the slope
        double[] slope = new double[length];
        int lastChangeIndex = 0;
        for (int i = 0; i < length - 1; i++)
            if (pos[i] == pos[i + 1]) {
                slope[i] = 0;
            }
            else {
                // if we've had a flat part, not followed by a jump of > 1
                if (lastChangeIndex != i - 1 && Math.Abs(pos[i] - pos[lastChangeIndex]) > 1.05) {
                    double newSlope = (double)(pos[i] - pos[lastChangeIndex]) / (i - lastChangeIndex);
                    for (int j = lastChangeIndex + 1; j <= i; j++)
                        slope[j] = newSlope;
                }
                else {
                    slope[i] = pos[i + 1] - pos[i];
                }

                lastChangeIndex = i;
            }

        slope[length - 1] = slope[length - 2];

        return (pos, slope);
    }

    /// <summary>
    ///     Gives the lengths and starts of all flat spots, using tile y-coords. Made to take the input from GetTopTilesPos()
    /// </summary>
    /// <param name="tiles"></param>
    /// <returns></returns>
    public static (List<int> flatStartIndexes, List<int> flatLengths) GetFlatTiles((int[] pos, double[] slope) tiles) {
        List<int> flatLengths = [];
        List<int> flatStartIndexes = [];
        int currentFlatStartIndex = 0;
        for (int i = 1; i < tiles.pos.Length; i++)
            if (tiles.pos[i] != tiles.pos[i - 1]) {
                // remove flats with length of 1
                if (i - currentFlatStartIndex != 1) {
                    flatLengths.Add(i - currentFlatStartIndex);
                    flatStartIndexes.Add(currentFlatStartIndex);
                }

                currentFlatStartIndex = i;
            }

        // ensure we got the last flat, if it exists
        if (tiles.pos.Length >= 2 && tiles.pos[^1] == tiles.pos[^2]) {
            flatLengths.Add(tiles.pos.Length - currentFlatStartIndex);
            flatStartIndexes.Add(currentFlatStartIndex);
        }

        return (flatStartIndexes, flatLengths);
    }

    /// <summary>
    ///     Gives the starts of all jumps AT or OVER a certain threshold, along with info from the highest tiles, using tile
    ///     y-coords.
    ///     Made to take the input from GetTopTilesPos()
    /// </summary>
    /// <param name="tiles"></param>
    /// <param name="threshold"></param>
    /// <returns>indexes of the jumps, index of the start of the highest point, and it's length</returns>
    public static (List<int> jumpIndexes, int lowestStart, int lowestLength) GetJumpsInfo(
        (int[] pos, double[] slope) tiles, int threshold = 4) {
        List<int> jumpIndexes = [];
        int lowestPos = tiles.pos[0];
        int highestStart = 0;
        int highestLength = 1;
        for (int i = 1; i < tiles.pos.Length; i++) {
            if (Math.Abs(tiles.pos[i] - tiles.pos[i - 1]) >= 4)
                jumpIndexes.Add(i);
            if (tiles.pos[i] < lowestPos) {
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