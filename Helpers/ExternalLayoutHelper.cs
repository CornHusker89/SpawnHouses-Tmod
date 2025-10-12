using System;
using System.Collections.Generic;
using System.Linq;
using SpawnHouses.AdvStructures.AdvStructureParts;
using SpawnHouses.Types;
using Terraria.DataStructures;

namespace SpawnHouses.Helpers;

public class ExternalLayoutHelper {
    /// <summary>
    ///     leaves original array intact
    /// </summary>
    /// <param name="sets"></param>
    /// <returns></returns>
    public static (Point16 start, Point16 end)[] SortClockwise((Point16 start, Point16 end)[] sets) {
        double centerX = (double)sets.Sum(set => set.start.X + set.end.X) / (sets.Length * 2);
        double centerY = (double)sets.Sum(set => set.start.Y + set.end.Y) / (sets.Length * 2);

        return sets.OrderBy(set =>
            // ReSharper disable PossibleLossOfFraction
            Math.Atan2(
                (set.start.X + set.end.X) / 2 - centerY,
                (set.start.Y + set.end.Y) / 2 - centerX)
        ).ToArray();
    }

    /// <summary>
    ///     helper function to simplify creating flat floors
    /// </summary>
    /// <param name="y"></param>
    /// <param name="xStart"></param>
    /// <param name="xEnd"></param>
    /// <param name="extendHigher">
    ///     if true, will expand floor by (<paramref name="width" /> - 1) in the positive direction.
    ///     otherwise in the negative direction
    /// </param>
    /// <param name="width"></param>
    /// <param name="isExternal"></param>
    /// <returns></returns>
    public static Floor CreateFloor(int y, int xStart, int xEnd, bool extendHigher, int width, bool isExternal = true) {
        return new Floor(
            new Shape(
                new Point16(xStart, y),
                new Point16(xEnd, y + (extendHigher ? width - 1 : -width + 1))
            ),
            isExternal
        );
    }

    /// <summary>
    ///     helper function to simplify creating flat walls
    /// </summary>
    /// <param name="x"></param>
    /// <param name="yStart"></param>
    /// <param name="yEnd"></param>
    /// <param name="extendHigher">
    ///     if true, will expand wall by (<paramref name="width" /> - 1) in the positive direction.
    ///     otherwise in the negative direction
    /// </param>
    /// <param name="width"></param>
    /// <param name="isExternal"></param>
    /// <returns></returns>
    public static Wall CreateWall(int x, int yStart, int yEnd, bool extendHigher, int width, bool isExternal = true) {
        return new Wall(
            new Shape(
                new Point16(x, yStart),
                new Point16(x + (extendHigher ? width - 1 : -width + 1), yEnd)
            ),
            isExternal
        );
    }

    /// <summary>
    ///     creates floor and walls as needed to fulfill the given path. intended to create structure roofs
    /// </summary>
    /// <param name="path"></param>
    /// <param name="floorWidth"></param>
    /// <param name="extendWallsHigher"></param>
    /// <param name="wallWidth"></param>
    /// <param name="isExternal"></param>
    /// <remarks>assumes that floors get priority over walls</remarks>
    /// <returns></returns>
    public static (List<Floor> floors, List<Wall> walls, List<Roof> roofs) CreateTopFloorsWallsRoofs(List<Point16> path, int floorWidth, bool extendWallsHigher, int wallWidth, bool isExternal = true) {
        List<Floor> floors = [];
        List<Wall> walls = [];
        List<Roof> roofs = [];

        List<Point16> roofPoints = [];
        bool lastComponentWasFloor = false;
        for (int pathIndex = 0; pathIndex < path.Count - 1; pathIndex++) {
            Point16 thisPoint = path[pathIndex];
            Point16 nextPoint = path[pathIndex + 1];
            Point16? lastPoint = pathIndex == 0 ? null : path[pathIndex - 1];

            if (thisPoint == nextPoint) continue;

            bool isFloor = thisPoint.X != nextPoint.X;
            bool nextComponentIsFloor = pathIndex == path.Count - 2 || nextPoint.X != path[pathIndex + 2].X;

            if (isFloor) {
                roofPoints.Add(
                    !lastComponentWasFloor && lastPoint != null && lastPoint.Value.Y < thisPoint.Y ? thisPoint + new Point16(1, 0) : thisPoint
                );

                if (thisPoint.Y == nextPoint.Y) {
                    floors.Add(CreateFloor(thisPoint.Y, thisPoint.X - (lastComponentWasFloor ? wallWidth + 1 : 0),
                        nextPoint.X + (!nextComponentIsFloor && extendWallsHigher ? wallWidth - 1 : 0), true, floorWidth, isExternal));
                }
                else {
                    List<Point16> floorPoints = [];

                    if (lastComponentWasFloor) {
                        floorPoints.Add(new Point16(thisPoint.X - wallWidth + 1, thisPoint.Y - floorWidth - 1)); // make floorWidth 2 higher by not doing +1 and instead -1
                        floorPoints.Add(new Point16(thisPoint.X - wallWidth + 1, thisPoint.Y));
                    }

                    floorPoints.Add(thisPoint);
                    floorPoints.Add(nextPoint);
                    floorPoints.Add(nextPoint + new Point16(0, -floorWidth - 1));

                    if (!nextComponentIsFloor && extendWallsHigher) {
                        floorPoints.Add(new Point16(thisPoint.X + wallWidth - 1, thisPoint.Y - floorWidth - 1));
                        floorPoints.Add(new Point16(thisPoint.X + wallWidth - 1, thisPoint.Y));
                    }

                    floorPoints.Add(thisPoint + new Point16(0, -floorWidth - 1));
                    floors.Add(new Floor(new Shape(floorPoints), isExternal));
                }
            }
            else {
                // create a roof out of the last non-wall segments
                if (roofPoints.Count != 0) {
                    if (lastComponentWasFloor) roofPoints.Add(path[pathIndex] + new Point16(nextPoint.Y < thisPoint.Y ? -1 : 0, 0));
                    Roof roof = new(new Path(roofPoints));
                    roof.Line.Offset(new Point16(0, -floorWidth));
                    roofs.Add(roof);
                    roofPoints.Clear();
                }

                if (lastComponentWasFloor)
                    walls.Add(CreateWall(thisPoint.X, nextPoint.Y > thisPoint.Y ? thisPoint.Y + 1 : thisPoint.Y - floorWidth,
                        nextPoint.Y > thisPoint.Y ? nextPoint.Y - floorWidth : nextPoint.Y + 1, extendWallsHigher, wallWidth, isExternal));
                else
                    walls.Add(CreateWall(thisPoint.X, thisPoint.Y,
                        nextPoint.Y > thisPoint.Y ? nextPoint.Y - floorWidth : nextPoint.Y + 1, extendWallsHigher, wallWidth, isExternal));
            }

            lastComponentWasFloor = isFloor;
        }

        if (roofPoints.Count != 0) {
            if (lastComponentWasFloor) roofPoints.Add(path[^1]);
            Roof roof = new(new Path(roofPoints));
            roof.Line.Offset(new Point16(0, -floorWidth));
            roofs.Add(roof);
        }

        return (floors, walls, roofs);
    }

    /// <summary>
    ///     creates a basic 1 or 2-segment roof, can create roof slopes and can handle different starting and ending Ys
    /// </summary>
    /// <param name="left">X must be less than <see cref="right" />'s X</param>
    /// <param name="right">X must be greater than <see cref="left" />'s X</param>
    /// <param name="p"></param>
    /// <param name="floorThickness"></param>
    /// <param name="wallThickness"></param>
    /// <returns></returns>
    public static (List<Floor> floors, List<Wall> walls, List<Roof> roofs) CreateBasicRoof(Point16 left, Point16 right, StructureParams p, int floorThickness, int wallThickness) {
        double[] validRoofSlopes = [0.5, 1, 1.5, 2];
        bool hasHigherSide = left.Y != right.Y;
        bool leftRoofHigher = left.Y < right.Y;
        int upperRoofBottomY = int.Min(left.Y, right.Y);
        int lowerRoofBottomY = int.Max(left.Y, right.Y);
        int unevenRoofStartX = Terraria.WorldGen.genRand.Next(p.StartEntryPointX + (int)(p.Length * 0.4), p.StartEntryPointX + (int)(p.Length * 0.6))
                               + (int)(p.Length * 0.14) * (leftRoofHigher ? 1 : -1);
        bool hasSlopedSideRoof = Terraria.WorldGen.genRand.NextDouble() < 0.75;
        bool hasRoofPeak = Terraria.WorldGen.genRand.NextDouble() < 0.9;
        double peakRoofSlope = validRoofSlopes[Terraria.WorldGen.genRand.Next(validRoofSlopes.Length)];
        double sideRoofSlope = double.Min(peakRoofSlope, 0.5);

        List<Point16> path;
        if (hasHigherSide) {
            int lowerRoofLength = leftRoofHigher
                ? p.EndEntryPointX - 1 + wallThickness - unevenRoofStartX
                : unevenRoofStartX - (p.StartEntryPointX + 1 - wallThickness);
            if (hasSlopedSideRoof) {
                int lowerRoofOffset = (int)(sideRoofSlope * lowerRoofLength);
                path = [
                    new Point16(p.StartEntryPointX + 1 - wallThickness, leftRoofHigher ? upperRoofBottomY : lowerRoofBottomY),
                    new Point16(unevenRoofStartX, leftRoofHigher ? upperRoofBottomY : lowerRoofBottomY - lowerRoofOffset),
                    new Point16(unevenRoofStartX, !leftRoofHigher ? upperRoofBottomY : lowerRoofBottomY - lowerRoofOffset),
                    new Point16(p.EndEntryPointX - 1 + wallThickness, !leftRoofHigher ? upperRoofBottomY : lowerRoofBottomY)
                ];
            }
            else {
                path = [
                    new Point16(p.StartEntryPointX + 1 - wallThickness, leftRoofHigher ? upperRoofBottomY : lowerRoofBottomY),
                    new Point16(unevenRoofStartX, leftRoofHigher ? upperRoofBottomY : lowerRoofBottomY),
                    new Point16(unevenRoofStartX, !leftRoofHigher ? upperRoofBottomY : lowerRoofBottomY),
                    new Point16(p.EndEntryPointX - 1 + wallThickness, !leftRoofHigher ? upperRoofBottomY : lowerRoofBottomY)
                ];
            }

            // add roof peak if required
            if (hasRoofPeak) {
                int higherRoofLength = p.Length - lowerRoofLength;
                int roofPeakX = leftRoofHigher
                    ? (int)(p.StartEntryPointX + 1 - wallThickness + higherRoofLength * 0.5)
                    : (int)(unevenRoofStartX + higherRoofLength * 0.5);
                int roofPeakOffset = (int)(peakRoofSlope * 0.5 * higherRoofLength);
                path.Insert(leftRoofHigher ? 1 : 3, new Point16(roofPeakX, upperRoofBottomY - roofPeakOffset));
            }
        }
        else {
            int roofLength = p.EndEntryPointX - p.StartEntryPointX;
            path = [
                new Point16(p.StartEntryPointX + 1 - wallThickness, upperRoofBottomY),
                new Point16(p.StartEntryPointX + roofLength / 2, upperRoofBottomY - (int)(peakRoofSlope * roofLength * 0.5)),
                new Point16(p.EndEntryPointX - 1 + wallThickness, upperRoofBottomY)
            ];
        }

        var result = CreateTopFloorsWallsRoofs(
            path,
            floorThickness,
            true,
            wallThickness
        );
        foreach (Floor floor in result.floors) {
            floor.TagsRequired.Add(ComponentTag.UseSimpleSloping);
        }

        if (hasRoofPeak)
            foreach (Roof roof in result.roofs)
                roof.TagsRequired.Add(ComponentTag.RoofHasLargeOverhang);

        return result;
    }
}