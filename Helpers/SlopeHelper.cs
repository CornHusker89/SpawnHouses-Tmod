using System;
using Terraria.ID;

namespace SpawnHouses.Helpers;

public delegate BlockType SlopingAlgorithm(int x, int y, bool[,] tilemap);

public enum SlopeGrouping {
    /// applies sloping algorithm taking entire tilemap into consideration. default sloping modifier
    GlobalSloping = 0,

    /// applies sloping algorithm taking entire tilemap into consideration, but excludes anything that uses LocalSloping
    GlobalOnlySloping = 1,

    /// applies sloping algorithm only taking this shape into consideration
    LocalSloping = 2,

    /// module uses a sloping algorithm determined by itself
    CustomSloping = 3
}

public static class SlopeHelper {
    /// <summary>
    ///     normal-looking sloping, creates a smooth edge along entire shape using a combination of slopes and half-blocks
    /// </summary>
    /// <returns></returns>
    public static readonly SlopingAlgorithm SimpleSlopes = (x, y, tilemap) => {
        int value = 0;
        if (x != 0 && tilemap[x - 1, y]) value += 1;
        if (x != tilemap.GetUpperBound(0) && tilemap[x + 1, y]) value += 2;
        if (y != 0 && tilemap[x, y - 1]) value += 4;
        if (y != tilemap.GetUpperBound(1) && tilemap[x, y + 1]) value += 8;

        return value switch {
            0 or 1 or 2 or 3 or 4 => // none
                // left
                // right
                // right, left
                // up
                BlockType.Solid,
            5 => // up, left
                BlockType.SlopeUpLeft,
            6 => // up, right
                BlockType.SlopeUpRight,
            7 => // up, left, right
                BlockType.Solid,
            8 => // down
                BlockType.HalfBlock,
            9 => // down, left
                BlockType.SlopeDownLeft,
            10 => // down, right
                BlockType.SlopeDownRight,
            11 or 12 or 13 or 14 or 15 => // down, right, left
                // down, up
                // down, up, left
                // down, up, right
                // down, up, right, left
                BlockType.Solid,
            _ => throw new Exception("no slope condition was met when evaluating shape slopes")
        };
    };

    /// <summary>
    ///     same as <see cref="SimpleSlopes" /> but slightly different to create a contrast between a smooth top
    ///     surface and straight bottom
    /// </summary>
    /// <returns></returns>
    public static readonly SlopingAlgorithm SmoothTop = (x, y, tilemap) => {
        int value = 0;
        if (x != 0 && tilemap[x - 1, y]) value += 1;
        if (x != tilemap.GetUpperBound(0) && tilemap[x + 1, y]) value += 2;
        if (y != 0 && tilemap[x, y - 1]) value += 4;
        if (y != tilemap.GetUpperBound(1) && tilemap[x, y + 1]) value += 8;

        return value switch {
            0 or 1 or 2 or 3 => // none
                // left
                // right
                // right, left
                // up
                BlockType.Solid,
            4 => // up
                x == 0 ? BlockType.SlopeUpLeft :
                x == tilemap.GetLength(0) - 1 ? BlockType.SlopeUpRight :
                BlockType.Solid,
            5 => // up, left
                x == 0 || x == tilemap.GetLength(0) - 1 ? BlockType.Solid : BlockType.SlopeUpLeft,
            6 => // up, right
                x == 0 || x == tilemap.GetLength(0) - 1 ? BlockType.Solid : BlockType.SlopeUpRight,
            7 => // up, left, right
                BlockType.Solid,
            8 => // down
                BlockType.HalfBlock,
            9 => // down, left
                BlockType.SlopeDownLeft,
            10 => // down, right
                BlockType.SlopeDownRight,
            11 or 12 or 13 or 14 or 15 => // down, right, left
                // down, up
                // down, up, left
                // down, up, right
                // down, up, right, left
                BlockType.Solid,
            _ => throw new Exception("no slope condition was met when evaluating shape slopes")
        };
    };

    /// <summary>
    /// </summary>
    /// <returns></returns>
    public static readonly SlopingAlgorithm GothicSlopes = (x, y, tilemap) => {
        int value = 0;
        if (x != 0 && tilemap[x - 1, y]) value += 1;
        if (x != tilemap.GetUpperBound(0) && tilemap[x + 1, y]) value += 2;
        if (y != 0 && tilemap[x, y - 1]) value += 4;
        if (y != tilemap.GetUpperBound(1) && tilemap[x, y + 1]) value += 8;

        return value switch {
            0 or 1 or 2 or 3 or 4 => // none
                // left
                // right
                // right, left
                // up
                BlockType.Solid,
            5 => // up, left
                BlockType.SlopeUpLeft,
            6 => // up, right
                BlockType.SlopeUpRight,
            7 => // up, left, right
                BlockType.Solid,
            8 => // down
                BlockType.HalfBlock,
            9 => // down, left
                BlockType.SlopeDownLeft,
            10 => // down, right
                BlockType.SlopeDownRight,
            11 => // down, right, left
                BlockType.HalfBlock,
            12 or 13 or 14 or 15 => // down, up
                // down, up, left
                // down, up, right
                // down, up, right, left
                BlockType.Solid,
            _ => throw new Exception("no slope condition was met when evaluating shape slopes")
        };
    };

    /// <summary>
    /// </summary>
    /// <returns></returns>
    public static readonly SlopingAlgorithm HalfSlopes = (x, y, tilemap) => {
        int value = 0;
        if (x != 0 && tilemap[x - 1, y]) value += 1;
        if (x != tilemap.GetUpperBound(0) && tilemap[x + 1, y]) value += 2;
        if (y != 0 && tilemap[x, y - 1]) value += 4;
        if (y != tilemap.GetUpperBound(1) && tilemap[x, y + 1]) value += 8;

        return value switch {
            0 or 1 or 2 or 3 or 4 => // none
                // left
                // right
                // right, left
                // up
                BlockType.Solid,
            5 => // up, left
                BlockType.SlopeUpLeft,
            6 => // up, right
                BlockType.SlopeUpRight,
            7 => // up, left, right
                BlockType.Solid,
            8 => // down
                BlockType.HalfBlock,
            9 => // down, left
                BlockType.SlopeDownLeft,
            10 => // down, right
                BlockType.SlopeDownRight,
            11 => // down, right, left
                BlockType.HalfBlock,
            12 or 13 or 14 or 15 => // down, up
                // down, up, left
                // down, up, right
                // down, up, right, left
                BlockType.Solid,
            _ => throw new Exception("no slope condition was met when evaluating shape slopes")
        };
    };

    public static readonly SlopingAlgorithm SharpTopCorners = (x, y, tilemap) => {
        int value = 0;
        if (x != 0 && tilemap[x - 1, y]) value += 1;
        if (x != tilemap.GetUpperBound(0) && tilemap[x + 1, y]) value += 2;
        if (y != 0 && tilemap[x, y - 1]) value += 4;
        if (y != tilemap.GetUpperBound(1) && tilemap[x, y + 1]) value += 8;

        return value switch {
            0 or 1 or 2 or 3 or 4 => // none
                // left
                // right
                // right, left
                // up
                BlockType.Solid,
            5 => // up, left
                BlockType.SlopeUpLeft,
            6 => // up, right
                BlockType.SlopeUpRight,
            7 => // up, left, right
                BlockType.Solid,
            8 => // down
                BlockType.HalfBlock,
            9 => // down, left
                x == 0 || x == tilemap.GetLength(0) - 1 ? BlockType.Solid : BlockType.SlopeDownLeft,
            10 => // down, right
                x == 0 || x == tilemap.GetLength(0) - 1 ? BlockType.Solid : BlockType.SlopeDownRight,
            11 or 12 or 13 or 14 or 15 => // down, right, left
                // down, up
                // down, up, left
                // down, up, right
                // down, up, right, left
                BlockType.Solid,
            _ => throw new Exception("no slope condition was met when evaluating shape slopes")
        };
    };
}