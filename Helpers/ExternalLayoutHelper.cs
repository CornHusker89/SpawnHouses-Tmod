using System;
using System.Collections.Generic;
using System.Linq;
using SpawnHouses.Common;
using SpawnHouses.Common.Modules.Components;
using SpawnHouses.Common.Tagging;
using SpawnHouses.Common.Types.Geometry;
using Terraria.DataStructures;

namespace SpawnHouses.Helpers;

public static class ExternalLayoutHelper {
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
    /// <param name="structure"></param>
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
    public static Floor CreateFloor(AdvStructure structure, int y, int xStart, int xEnd, bool extendHigher, int width, bool isExternal = true) {
        Floor floor = new(
            structure,
            new Shape(
                true,
                new Point16(xStart, y),
                new Point16(xEnd, y + (extendHigher ? width - 1 : -width + 1))
            )
        );
        if (isExternal)
            floor.Params.TagsRequired.Add(Tags.External);
        return floor;
    }

    /// <summary>
    ///     helper function to simplify creating flat walls
    /// </summary>
    /// <param name="structure"></param>
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
    public static Wall CreateWall(AdvStructure structure, int x, int yStart, int yEnd, bool extendHigher, int width, bool isExternal = true) {
        Wall wall = new(
            structure,
            new Shape(
                true,
                new Point16(x, yStart),
                new Point16(x + (extendHigher ? width - 1 : -width + 1), yEnd)
            )
        );
        if (isExternal)
            wall.Params.TagsRequired.Add(Tags.External);
        return wall;
    }

    /// <summary>
    ///     creates floor and walls as needed to fulfill the given path. intended to create structure roofs
    /// </summary>
    /// <param name="structure"></param>
    /// <param name="path"></param>
    /// <param name="floorWidth"></param>
    /// <param name="extendWallsHigher"></param>
    /// <param name="wallWidth"></param>
    /// <param name="isExternal"></param>
    /// <remarks>assumes that floors get priority over walls</remarks>
    /// <returns></returns>
    public static (List<Floor> floors, List<Wall> walls, List<Roof> roofs) CreateTopFloorsWallsRoofs(AdvStructure structure, List<Point16> path, int floorWidth, bool extendWallsHigher, int wallWidth, bool isExternal = true) {
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
                    floors.Add(CreateFloor(structure, thisPoint.Y, thisPoint.X, nextPoint.X, false, floorWidth, isExternal));
                }
                else {
                    List<Point16> floorPoints = [];

                    floorPoints.Add(thisPoint + new Point16(0, -floorWidth));
                    
                    if (lastComponentWasFloor) {
                        floorPoints.Add(new Point16(thisPoint.X - wallWidth + 1, thisPoint.Y - floorWidth));
                        floorPoints.Add(new Point16(thisPoint.X - wallWidth + 1, thisPoint.Y));
                    }
                    
                    floorPoints.Add(thisPoint);
                    floorPoints.Add(nextPoint);

                    if (!nextComponentIsFloor && extendWallsHigher) {
                        floorPoints.Add(new Point16(thisPoint.X + wallWidth - 1, thisPoint.Y - floorWidth));
                        floorPoints.Add(new Point16(thisPoint.X + wallWidth - 1, thisPoint.Y));
                    }

                    floorPoints.Add(nextPoint + new Point16(0, -floorWidth));
                    
                    Floor floor = new(structure, new Shape(floorPoints));
                    if (isExternal)
                        floor.Params.TagsRequired.Add(Tags.External);
                    floors.Add(floor);
                }
            }
            else {
                // create a roof out of the last non-wall segments
                if (roofPoints.Count != 0) {
                    if (lastComponentWasFloor) roofPoints.Add(path[pathIndex] + new Point16(nextPoint.Y < thisPoint.Y ? -1 : 0, 0));
                    Roof roof = new(structure, new Path(roofPoints), thisRoofStartExtendable, nextPoint.Y <= thisPoint.Y);
                    roof.Geometry.Move(new Point16(0, -floorWidth));
                    roofs.Add(roof);
                    roofPoints.Clear();
                    thisRoofStartExtendable = true;
                }

                if (lastComponentWasFloor)
                    walls.Add(CreateWall(structure, thisPoint.X, nextPoint.Y > thisPoint.Y ? thisPoint.Y + 1 : thisPoint.Y - floorWidth,
                        nextPoint.Y > thisPoint.Y ? nextPoint.Y - floorWidth : nextPoint.Y + 1, extendWallsHigher, wallWidth, isExternal));
                else if (roofPoints.Count == 0)
                    walls.Add(CreateWall(structure, thisPoint.X, thisPoint.Y,
                        nextPoint.Y > thisPoint.Y ? nextPoint.Y : nextPoint.Y + 1, extendWallsHigher, wallWidth, isExternal));
                else
                    walls.Add(CreateWall(structure, thisPoint.X, thisPoint.Y,
                        nextPoint.Y > thisPoint.Y ? nextPoint.Y - floorWidth : nextPoint.Y + 1, extendWallsHigher, wallWidth, isExternal));
            }

            lastComponentWasFloor = isFloor;
        }

        if (roofPoints.Count != 0) {
            if (lastComponentWasFloor) roofPoints.Add(path[^1]);
            Roof roof = new(structure, new Path(roofPoints), thisRoofStartExtendable, true);
            roof.Geometry.Move(new Point16(0, -floorWidth));
            roofs.Add(roof);
        }

        return (floors, walls, roofs);
    }

    /// <summary>
    /// </summary>
    /// <param name="roofs"></param>
    /// <returns></returns>
    public static int GetHighestRoofPoint(IEnumerable<Roof> roofs) {
        int topY = int.MaxValue;
        foreach (Roof roof in roofs) {
            int pos = roof.GetBoundingShape().BoundingBox.topLeft.Y;
            if (pos < topY) topY = pos;
        }

        return topY;
    }
}