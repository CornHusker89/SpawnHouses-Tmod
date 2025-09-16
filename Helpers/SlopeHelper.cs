using System;
using System.Drawing;
using Microsoft.Xna.Framework;
using Terraria.ID;

namespace SpawnHouses.Helpers;

public class SlopeHelper {
    
    /// <summary>
    ///     normal-looking sloping, but uses exclusively half-blocks and full blocks to create slopes on the top edge
    /// </summary>
    /// <returns></returns>
    public static BlockType SimpleSlopes(int x, int y, bool[,] tilemap) { 
        int value = 0;
        if (x != 0 && tilemap[x - 1, y]) {
            value |= 1;
        }
        if (x != tilemap.GetUpperBound(0) && tilemap[x + 1, y]) {
            value |= 2;
        }
        if (y != 0 && tilemap[x, y - 1]) {
            value |= 4;
        }
        if (y != tilemap.GetUpperBound(1) && tilemap[x, y + 1]) {
            value |= 8;
        }

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
            7 or 8 => // up, left, right
                // down
                BlockType.Solid,
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
    }
    
    /// <summary>
    ///     normal-looking sloping, but uses a combination of half-blocks and slopes on top edges to create a spiky look
    /// </summary>
    /// <returns></returns>
    public static BlockType GothicSlopes(int x, int y, bool[,] tilemap) { 
        int value = 0;
        if (x != 0 && tilemap[x - 1, y]) {
            value |= 1;
        }
        if (x != tilemap.GetUpperBound(0) && tilemap[x + 1, y]) {
            value |= 2;
        }
        if (y != 0 && tilemap[x, y - 1]) {
            value |= 4;
        }
        if (y != tilemap.GetUpperBound(1) && tilemap[x, y + 1]) {
            value |= 8;
        }

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
    }
    
    /// <summary>
    ///     normal-looking sloping, but uses exclusively half-blocks and full blocks to create slopes on the top edge
    /// </summary>
    /// <returns></returns>
    public static BlockType HalfSlopes(int x, int y, bool[,] tilemap) {

        throw new NotImplementedException();
        
        int value = 0;
        if (x != 0 && tilemap[x - 1, y]) {
            value |= 1;
        }
        if (x != tilemap.GetUpperBound(0) && tilemap[x + 1, y]) {
            value |= 2;
        }
        if (y != 0 && tilemap[x, y - 1]) {
            value |= 4;
        }
        if (y != tilemap.GetUpperBound(1) && tilemap[x, y + 1]) {
            value |= 8;
        }

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
    }
}