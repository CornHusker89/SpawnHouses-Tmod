using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;
using SpawnHouses.Content.Types.Enums;
using Terraria.DataStructures;

namespace SpawnHouses.Helpers;

public static class DirectionUtils {
    private static readonly Dictionary<Direction, Point16> _directionPoints = new([
        // basic directions
        new KeyValuePair<Direction, Point16>(Direction.Up, new Point16(0, -1)),
        new KeyValuePair<Direction, Point16>(Direction.UpRight, new Point16(1, -1)),
        new KeyValuePair<Direction, Point16>(Direction.Right, new Point16(1, 0)),
        new KeyValuePair<Direction, Point16>(Direction.DownRight, new Point16(1, 1)),
        new KeyValuePair<Direction, Point16>(Direction.Down, new Point16(0, 1)),
        new KeyValuePair<Direction, Point16>(Direction.DownLeft, new Point16(-1, 1)),
        new KeyValuePair<Direction, Point16>(Direction.Left, new Point16(-1, 0)),
        new KeyValuePair<Direction, Point16>(Direction.UpLeft, new Point16(-1, -1)),
        new KeyValuePair<Direction, Point16>(Direction.None, new Point16(0, 0))
    ]);

    /// <summary>
    ///     will return false for <see cref="Direction.None" />
    /// </summary>
    /// <param name="direction"></param>
    /// <returns></returns>
    public static bool isDiagonal(Direction direction) => direction != Direction.None && direction != Direction.Invalid && (sbyte)direction % 2 == 1;

    /// <summary>
    ///     returns direction turned to the right by either 90 or 45 deg, depending on <see cref="allowDiagonals" />
    /// </summary>
    /// <param name="direction"></param>
    /// <param name="allowDiagonals">note: if true, will only ever turn by 45 deg</param>
    /// <returns></returns>
    public static Direction TurnRight(Direction direction, bool allowDiagonals = false) {
        if (direction is Direction.None or Direction.Invalid)
            throw new Exception("cannot turn given direction");
        return (Direction)(((sbyte)direction + (!allowDiagonals && !isDiagonal(direction) ? 2 : 1)) % 8);
    }

    /// <summary>
    ///     returns direction turned to the right by either 90 or 45 deg, depending on <see cref="allowDiagonals" />
    /// </summary>
    /// <param name="direction"></param>
    /// <param name="allowDiagonals">note: if true, will only ever turn by 45 deg</param>
    /// <returns></returns>
    public static Direction TurnLeft(Direction direction, bool allowDiagonals = false) {
        if (direction is Direction.None or Direction.Invalid)
            throw new Exception("cannot turn given direction");
        return (Direction)(((sbyte)direction - (!allowDiagonals && !isDiagonal(direction) ? 2 : 1)) % 8);
    }

    /// <summary>
    ///     puts that john in reverse
    /// </summary>
    /// <param name="direction"></param>
    /// <returns></returns>
    /// <exception cref="Exception"></exception>
    public static Direction Flip(Direction direction) {
        if (direction is Direction.None or Direction.Invalid)
            throw new Exception("cannot flip given direction");
        return (Direction)(((sbyte)direction + 4) % 8);
    }

    /// <summary>
    ///     gets the direction required to go from the start to the end points
    /// </summary>
    /// <param name="start"></param>
    /// <param name="end"></param>
    /// <returns></returns>
    /// <exception cref="ArgumentException"></exception>
    public static Direction GetDirectionFromPoints(Point16 start, Point16 end) {
        Point16 diff = end - start;
        Direction result = _directionPoints.FirstOrDefault(pair => pair.Value == diff, new KeyValuePair<Direction, Point16>(Direction.Invalid, new Point16(0))).Key;
        if (result == Direction.Invalid)
            throw new ArgumentException("points must be exactly one apart, allowing diagonals");
        return result;
    }

    public static Point16 GetPoint16FromDirection(Direction direction) => _directionPoints[direction];

    [Pure]
    public static Point16 Offset(Point16 point, Direction direction) => point + GetPoint16FromDirection(direction);
}