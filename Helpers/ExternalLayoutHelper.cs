using System;
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
    public static Floor CreateFloor(int y, int xStart, int xEnd, bool extendHigher, int width, bool isExternal) {
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
    public static Wall CreateWall(int x, int yStart, int yEnd, bool extendHigher, int width, bool isExternal) {
        return new Wall(
            new Shape(
                new Point16(x, yStart),
                new Point16(x + (extendHigher ? width - 1 : -width + 1), yEnd)
            ),
            isExternal
        );
    }
}