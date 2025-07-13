using System;
using System.Collections.Generic;
using System.Linq;
using SpawnHouses.AdvStructures.AdvStructureParts;
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
    /// </summary>
    /// <param name="x"></param>
    /// <param name="yStart"></param>
    /// <param name="yEnd"></param>
    /// <param name="extendHigher">
    ///     if true, will expand floor by (<paramref name="width" /> - 1) in the positive direction.
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
    ///     creates floor and walls as needed to fulfill the given path. intended to be used to top-off structures
    /// </summary>
    /// <param name="path"></param>
    /// <param name="floorWidth"></param>
    /// <param name="extendWallsHigher"></param>
    /// <param name="wallWidth"></param>
    /// <param name="isExternal"></param>
    /// <remarks>assumes that floors get priority over walls</remarks>
    /// <returns></returns>
    public static (List<Floor> floors, List<Wall> walls, List<Roof> roofs) CreateTopFloorsAndWalls(List<Point16> path, int floorWidth, bool extendWallsHigher, int wallWidth, bool isExternal = true) {
        List<Floor> floors = [];
        List<Wall> walls = [];
        List<Roof> roofs = [];

        List<Point16> roofPoints = [];
        bool lastComponentWasFloor = false;
        for (int pathIndex = 0; pathIndex < path.Count - 1; pathIndex++) {
            Point16 thisPoint = path[pathIndex];
            Point16 nextPoint = path[pathIndex + 1];

            if (thisPoint == nextPoint) {
                continue;
            }

            bool isFloor = thisPoint.X != nextPoint.X;
            bool nextComponentIsFloor = pathIndex == path.Count - 2 || nextPoint.X != path[pathIndex + 2].X;

            if (isFloor) {
                roofPoints.Add(thisPoint);

                if (thisPoint.Y == nextPoint.Y)
                {
                    floors.Add(CreateFloor(thisPoint.Y, thisPoint.X - (lastComponentWasFloor ? wallWidth + 1 : 0),
                        nextPoint.X + (!nextComponentIsFloor && extendWallsHigher ? wallWidth - 1 : 0), true, floorWidth, isExternal));
                }
                else {
                    floors.Add(new Floor(new Shape(
                        new Point16()
                    )));
                }


            }
            else {
                // create a roof out of the last non-wall segments
                if (roofPoints.Count != 0) {
                    roofPoints.Add(thisPoint);
                    for (int i = roofPoints.Count - 1; i >= 0; i--) {
                        roofPoints.Add(new Point16(
                            roofPoints[i].X,
                            roofPoints[i].Y - 3
                        ));
                    }
                    roofs.Add(new Roof(new Shape(roofPoints)));
                    roofPoints.Clear();
                }

                if (lastComponentWasFloor) {
                    walls.Add(CreateWall(thisPoint.X, nextPoint.Y > thisPoint.Y ? thisPoint.Y + 1 : thisPoint.Y - floorWidth,
                        nextPoint.Y > thisPoint.Y ? nextPoint.Y - floorWidth : nextPoint.Y + 1, extendWallsHigher, wallWidth, isExternal));
                }
                else {
                    walls.Add(CreateWall(thisPoint.X, thisPoint.Y,
                        nextPoint.Y > thisPoint.Y ? nextPoint.Y - floorWidth : nextPoint.Y + 1, extendWallsHigher, wallWidth, isExternal));
                }
            }

            lastComponentWasFloor = isFloor;
        }

        if (roofPoints.Count != 0) {
            roofPoints.Add(path[^1]);
            for (int i = roofPoints.Count - 1; i >= 0; i--) {
                roofPoints.Add(new Point16(
                    roofPoints[i].X,
                    roofPoints[i].Y - 3
                ));
            }

            roofs.Add(new Roof(new Shape(roofPoints)));
        }

        return (floors, walls, roofs);
    }
}