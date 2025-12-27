using System;
using System.Collections.Generic;
using System.Linq;
using SpawnHouses.AdvStructures.AdvStructureParts;
using Terraria;
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
    public static Floor CreateFloor(int y, int xStart, int xEnd, bool extendHigher, int width, bool isExternal = true) =>
        new(
            new Shape(
                true,
                new Point16(xStart, y),
                new Point16(xEnd, y + (extendHigher ? width - 1 : -width + 1))
            ),
            isExternal
        );

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
    public static Wall CreateWall(int x, int yStart, int yEnd, bool extendHigher, int width, bool isExternal = true) =>
        new(
            new Shape(
                true,
                new Point16(x, yStart),
                new Point16(x + (extendHigher ? width - 1 : -width + 1), yEnd)
            ),
            isExternal
        );

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
        bool thisRoofStartExtendable = true;
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
                if (lastPoint?.Y < thisPoint.Y) thisRoofStartExtendable = false;

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
                    Roof roof = new(new Path(roofPoints, thisRoofStartExtendable, nextPoint.Y <= thisPoint.Y));
                    roof.Line.Offset(new Point16(0, -floorWidth));
                    roofs.Add(roof);
                    roofPoints.Clear();
                    thisRoofStartExtendable = true;
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
            Roof roof = new(new Path(roofPoints, thisRoofStartExtendable));
            roof.Line.Offset(new Point16(0, -floorWidth));
            roofs.Add(roof);
        }

        return (floors, walls, roofs);
    }

    /// <summary>
    ///     creates a basic muti-segment roof, can create roof slopes and can handle different starting and ending Ys
    /// </summary>
    /// <param name="left">X must be less than <see cref="right" />'s X</param>
    /// <param name="right">X must be greater than <see cref="left" />'s X</param>
    /// <param name="floorThickness"></param>
    /// <param name="wallThickness"></param>
    /// <param name="forceFlat">if true, the roof will have no slope of any kind</param>
    /// <param name="splitRoof">if true, roof will attempt to be split</param>
    /// <returns></returns>
    public static (List<Floor> floors, List<Wall> walls, List<Roof> roofs) CreateBasicRoof(Point16 left, Point16 right, int floorThickness, int wallThickness,
        bool forceFlat, bool splitRoof) {
        double[] validSplitRoofSlopes = [0.67, 1, 1.5, 2];
        double[] validSingleRoofSlopes = [1, 1.33];
        int startX = left.X + 1 - wallThickness;
        int endX = right.X - 1 + wallThickness;
        int fullLength = right.X - left.X - 2 + 2 * wallThickness;
        bool leftRoofHigher = left.Y < right.Y;
        int upperRoofBottomY = int.Min(left.Y, right.Y);
        int lowerRoofBottomY = int.Max(left.Y, right.Y);
        int unevenRoofStartX = leftRoofHigher
            ? Terraria.WorldGen.genRand.Next(startX + (int)(fullLength * 0.5), endX - (int)(fullLength * 0.35))
            : Terraria.WorldGen.genRand.Next(startX + (int)(fullLength * 0.35), endX - (int)(fullLength * 0.5));
        bool hasSlopedSideRoof = !forceFlat && Terraria.WorldGen.genRand.NextBool(3, 4);
        bool hasRoofPeak = !forceFlat;
        double peakRoofSlope = Terraria.WorldGen.genRand.NextFromList(splitRoof ? validSplitRoofSlopes : validSingleRoofSlopes);
        double sideRoofSlope = double.Min(peakRoofSlope, 0.5);

        List<Point16> path;
        if (splitRoof) {
            int lowerRoofLength = leftRoofHigher
                ? endX - unevenRoofStartX
                : unevenRoofStartX - startX;
            if (hasSlopedSideRoof) {
                int lowerRoofOffset = (int)(sideRoofSlope * lowerRoofLength);
                path = [
                    new Point16(startX, leftRoofHigher ? upperRoofBottomY : lowerRoofBottomY),
                    new Point16(unevenRoofStartX, leftRoofHigher ? upperRoofBottomY : lowerRoofBottomY - lowerRoofOffset),
                    new Point16(unevenRoofStartX, !leftRoofHigher ? upperRoofBottomY : lowerRoofBottomY - lowerRoofOffset),
                    new Point16(endX, !leftRoofHigher ? upperRoofBottomY : lowerRoofBottomY)
                ];
            }
            else {
                path = [
                    new Point16(startX, leftRoofHigher ? upperRoofBottomY : lowerRoofBottomY),
                    new Point16(unevenRoofStartX, leftRoofHigher ? upperRoofBottomY : lowerRoofBottomY),
                    new Point16(unevenRoofStartX, !leftRoofHigher ? upperRoofBottomY : lowerRoofBottomY),
                    new Point16(endX, !leftRoofHigher ? upperRoofBottomY : lowerRoofBottomY)
                ];
            }

            // add roof peak if required
            if (hasRoofPeak) {
                int higherRoofLength = fullLength - lowerRoofLength;
                int roofPeakX = leftRoofHigher
                    ? (int)Math.Ceiling(startX + higherRoofLength * 0.5)
                    : (int)Math.Ceiling(unevenRoofStartX + higherRoofLength * 0.5);
                int roofPeakOffset = (int)(peakRoofSlope * 0.5 * higherRoofLength);
                path.Insert(leftRoofHigher ? 1 : 3, new Point16(roofPeakX, upperRoofBottomY - roofPeakOffset));
                if (higherRoofLength % 2 == 1) path.Insert(leftRoofHigher ? 1 : 3, new Point16(roofPeakX - 1, upperRoofBottomY - roofPeakOffset));
            }
        }
        else {
            if (hasRoofPeak) {
                path = [
                    new Point16(startX, upperRoofBottomY),
                    new Point16(startX + fullLength / 2, upperRoofBottomY - (int)(peakRoofSlope * fullLength * 0.5)),
                    new Point16(endX, upperRoofBottomY)
                ];
                if (fullLength % 2 == 1) path.Insert(2, new Point16(startX + 1 + fullLength / 2, upperRoofBottomY - (int)(peakRoofSlope * fullLength * 0.5)));
            }
            else {
                path = [
                    new Point16(startX, upperRoofBottomY),
                    new Point16(endX, upperRoofBottomY)
                ];
            }
        }

        var result = CreateTopFloorsWallsRoofs(
            path,
            floorThickness,
            true,
            wallThickness
        );
        foreach (Floor floor in result.floors) {
            floor.AddRequiredTag(ComponentTags.ApplySloping, SlopeHelper.SimpleSlopes);
            floor.AddRequiredTag(ComponentTags.SlopingModifier, SlopeModifier.GlobalOnlySloping);
        }

        foreach (Roof roof in result.roofs) {
            roof.AddRequiredTag(ComponentTags.ApplySloping, SlopeHelper.SmoothTop);
            roof.AddRequiredTag(ComponentTags.SlopingModifier, SlopeModifier.LocalSloping);
        }

        if (hasRoofPeak && Terraria.WorldGen.genRand.NextBool(3, 5)) {
            result.roofs[!leftRoofHigher && splitRoof ? 1 : 0].AddRequiredTag(ComponentTags.RoofTall);
            if (splitRoof && hasSlopedSideRoof && Terraria.WorldGen.genRand.NextBool(1, 2)) result.roofs[!leftRoofHigher ? 0 : 1].AddRequiredTag(ComponentTags.RoofTall);
        }

        return result;
    }

    /// <summary>
    /// </summary>
    /// <param name="components"></param>
    /// <typeparam name="T">must be a component type</typeparam>
    /// <returns></returns>
    public static int GetHighestBoundingPoint<T>(List<T> components)
        where T : Component {
        int topY = int.MaxValue;
        foreach (T component in components) {
            int pos = component.Generator.GetBoundingShape(component.Params).BoundingBox.topLeft.Y;
            if (pos < topY) topY = pos;
        }

        return topY;
    }
}