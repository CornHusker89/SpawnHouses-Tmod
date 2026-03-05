using SpawnHouses.Helpers.Complex;
using Terraria.DataStructures;

namespace SpawnHouses.Helpers;

public static class GeometryHelper {
    /// <summary>
    ///     converts 2x2 grid cell into a marching square index
    /// </summary>
    /// <param name="isInside"></param>
    /// <param name="x"></param>
    /// <param name="y"></param>
    /// <returns></returns>
    public static int GetMarchingSquareIndex(Condition2D isInside, int x, int y) {
        int value = 0;
        // bottom-left
        if (isInside.Invoke(x, y))
            value |= 1;

        // bottom-right
        if (isInside.Invoke(x + 1, y))
            value |= 2;

        // top-right
        if (isInside.Invoke(x + 1, y - 1))
            value |= 4;

        // top-left
        if (isInside.Invoke(x, y - 1))
            value |= 8;

        return value;
    }

    /// <summary>
    ///     assumes clockwise direction
    /// </summary>
    /// <param name="index"></param>
    /// <param name="lastDirection"></param>
    /// <returns></returns>
    public static Point16 GetDirectionFromSquareIndex(int index, Point16 lastDirection = default) {
        return index switch {
            1 => new Point16(0, 1), // BL only: down
            2 => new Point16(1, 0), // BR only: right
            3 => new Point16(1, 0), // BL + BR: right
            4 => new Point16(0, -1), // TR only: up
            5 => lastDirection.X == -1 ? new Point16(0, -1) : new Point16(0, 1), // BL + TR: up if we were going left, otherwise down
            6 => new Point16(0, -1), // BR + TR: up
            7 => new Point16(0, -1), // BL + BR + TR: up
            8 => new Point16(-1, 0), // TL only: left
            9 => new Point16(0, 1), // BL + TL: down
            10 => lastDirection.Y == -1 ? new Point16(0, 1) : new Point16(0, -1), // BR + TL: down if we were going left, otherwise up
            11 => new Point16(1, 0), // BL + BR + TL: right
            12 => new Point16(-1, 0), // TR + TL: left
            13 => new Point16(0, 1), // BL + TR + TL: down
            14 => new Point16(-1, 0), // BR + TR + TL: left
            _ => new Point16(0, 0) // 0 or 15
        };
    }
}