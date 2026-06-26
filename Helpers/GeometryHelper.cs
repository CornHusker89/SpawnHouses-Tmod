using SpawnHouses.Helpers.Complex;
using Terraria.DataStructures;

namespace SpawnHouses.Helpers;

public static class GeometryHelper {
    /// <summary>
    ///     converts 2x2 grid cell into a marching square index. assumes that x and y are 0 indexed, does NOT perform out-of-bounds checks
    /// </summary>
    /// <param name="isInside"></param>
    /// <param name="x"></param>
    /// <param name="y"></param>
    /// <returns></returns>
    public static int GetMarchingSquareIndex(Condition2D isInside, int x, int y) {
        int value = 0;

        // A (top-left)
        if (isInside(x - 1, y - 1)) value |= 8;

        // B (top-right)
        if (isInside(x, y - 1)) value |= 4;

        // C (bottom-right)
        if (isInside(x, y)) value |= 2;

        // D (bottom-left)
        if (isInside(x - 1, y)) value |= 1;

        return value;
    }

    /// <summary>
    ///     assumes clockwise direction
    /// </summary>
    /// <param name="index"></param>
    /// <param name="prevDirection"></param>
    /// <returns></returns>
    public static Point16 GetDirectionFromSquareIndex(int index, Point16 prevDirection) {
        return index switch {
            1 => new Point16(0, -1), // BL only: up
            2 => new Point16(1, 0), // BR only: right
            3 => new Point16(1, 0), // BL + BR: right
            4 => new Point16(0, -1), // TR only: up
            /*5 => new Point16(0, -1), // BL + TR: up*/
            5 => prevDirection.Y == 1
                ? new Point16(1, 0) // arrived from above → exit right
                : new Point16(0, -1), // arrived from left  → exit up
            6 => new Point16(0, -1), // BR + TR: up
            7 => new Point16(0, -1), // BL + BR + TR: up
            8 => new Point16(-1, 0), // TL only: left
            9 => new Point16(0, 1), // BL + TL: down
            // 10 => new Point16(0, 1), // BR + TL: down
            10 => prevDirection.X == 1
                ? new Point16(0, 1) // arrived from left  → exit down
                : new Point16(-1, 0), // arrived from above → exit left
            11 => new Point16(1, 0), // BL + BR + TL: right, from up
            12 => new Point16(-1, 0), // TR + TL: left
            13 => new Point16(0, 1), // BL + TR + TL: down
            14 => new Point16(-1, 0), // BR + TR + TL: left
            _ => new Point16(0, 0) // 0 or 15
        };
    }

    public static bool InBoundingBox(Point16 point, (Point16 topLeft, Point16 bottomRight) boundingBox) => point.X >= boundingBox.topLeft.X && point.X <= boundingBox.bottomRight.X && point.Y >= boundingBox.topLeft.Y && point.Y <= boundingBox.bottomRight.Y;
}