using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using Microsoft.Xna.Framework;
using Terraria.DataStructures;

namespace SpawnHouses.StructureCommon.Types.DataStructures;

/// <summary>
///     very similar to <see cref="Microsoft.Xna.Framework.Rectangle" />, but all corners are inclusive
/// </summary>
/// <remarks>uses signed 16-bit integers for internal data, intended for use with tile coordinates.
/// will overflow if used for world coordinates</remarks>
public readonly struct TileBox {
    /// <summary>
    ///     left-edge position
    /// </summary>
    public readonly short X;

    /// <summary>
    ///     top-edge position
    /// </summary>
    public readonly short Y;

    public readonly short Width;
    public readonly short Height;

    public short Left => X;
    public short Top => Y;
    public int Right => X + Width - 1;
    public int Bottom => Y + Height - 1;

    public TileBox(int x, int y, int width, int height) {
        X = (short)x;
        Y = (short)y;
        Width = (short)width;
        Height = (short)height;

        // check for overflow; it shouldn't ever be intentional
        if (x != X || y != Y || width != Width || height != Height) throw new ArgumentException("overflow was detected when creating TileBox");
    }

    public TileBox(short x, short y, short width, short height) {
        X = x;
        Y = y;
        Width = width;
        Height = height;
    }

    public TileBox(Point16 topLeft, Point16 bottomRight) {
        X = topLeft.X;
        Y = topLeft.Y;
        Width = (short)(bottomRight.X - topLeft.X + 1);
        Height = (short)(bottomRight.Y - topLeft.Y + 1);
    }

    public Point16 TopLeftPoint16 => new(X, Y);
    public Point16 TopRightPoint16 => new(X + Width - 1, Y);
    public Point16 BottomLeftPoint16 => new(X, Y + Height - 1);
    public Point16 BottomRightPoint16 => new(X + Width - 1, Y + Height - 1);
    public Point16 CenterPoint16 => new(X + (Width - 1) / 2, Y + (Height - 1) / 2);
    public Point16 SizePoint16 => new(Width, Height);

    public Point TopLeftPoint => new(X, Y);
    public Point CenterPoint => new(X + Width - 1, Y + Height - 1);


    public bool Contains(Point16 point) => !(Left > point.X || Right < point.X || Top > point.Y || Bottom < point.Y);

    /// <summary>
    ///     true if any in the first set collide with any in the second set
    /// </summary>
    /// <returns></returns>
    public static bool AnyColliding(TileBox[] first, List<TileBox> second) {
        foreach (TileBox tileBox in first)
        foreach (TileBox otherTileBox in second)
            if (tileBox.Intersects(otherTileBox))
                return true;

        return false;
    }

    /// <summary>
    ///     true if any in the first set collide with any in the second set
    /// </summary>
    /// <returns></returns>
    public static bool AnyColliding(TileBox[] first, TileBox[] second) {
        foreach (TileBox tileBox in first)
        foreach (TileBox otherTileBox in second)
            if (tileBox.Intersects(otherTileBox))
                return true;

        return false;
    }

    /// <summary>
    ///     note that the width and height values will be different by -1
    /// </summary>
    /// <returns></returns>
    [Pure]
    public Rectangle ToRectangle() => new(X, Y, Width, Height);

    /// <summary>
    ///     scales both rectangle size and preexisting translation
    /// </summary>
    /// <param name="r"></param>
    /// <param name="scale"></param>
    /// <returns></returns>
    [Pure]
    public Rectangle Scale(int scale) => new(X * scale, Y * scale, (Width + 1) * scale, (Height + 1) * scale);

    /// <inheritdoc cref="Scale(int)" />
    [Pure]
    public Rectangle Scale(float scale) => new((int)(X * scale), (int)(Y * scale), (int)(Width * scale), (int)(Height * scale));

    /// <summary>
    ///     scales rectangle size and translates such that center is not changed
    /// </summary>
    /// <param name="r"></param>
    /// <param name="scale"></param>
    /// <returns></returns>
    [Pure]
    public Rectangle ScaleLocal(int scale) => new(X - Width * scale / 2, Y - Height * scale / 2, Width * scale, Width * scale);

    /// <inheritdoc cref="ScaleLocal(int)" />
    [Pure]
    public Rectangle ScaleLocal(float scale) => new((int)(X - Width * scale / 2), (int)(Y - Height * scale / 2), (int)(Width * scale), (int)(Width * scale));

    /// <summary>
    ///     scales only rectangle size, does not modify top left point
    /// </summary>
    /// <param name="r"></param>
    /// <param name="scale"></param>
    /// <returns></returns>
    [Pure]
    public Rectangle AnchoredScaleLocal(int scale) => new(X, Y, Height * scale, Width * scale);

    /// <inheritdoc cref="AnchoredScaleLocal(float)" />
    [Pure]
    public Rectangle AnchoredScaleLocal(float scale) => new(X, Y, (int)(Height * scale), (int)(Width * scale));

    /// <summary>
    ///     ADDs the offset to the current position. for setting an absolute position, see <see cref="Move"/>
    /// </summary>
    /// <param name="offsetX"></param>
    /// <param name="offsetY"></param>
    /// <returns></returns>
    [Pure]
    public TileBox Offset(int offsetX, int offsetY) => new(X + offsetX, Y + offsetY, Width, Height);

    /// <summary>
    ///     ADDs the offset to the current position. for setting an absolute position, see <see cref="Move"/>
    /// </summary>
    /// <param name="offset"></param>
    /// <returns></returns>
    [Pure]
    public TileBox Offset(Point16 offset) => new(X + offset.X, Y + offset.Y, Width, Height);

    /// <summary>
    ///     creates a new TileBox with the same size, at a different set position. for an offset, see <see cref="Offset(int,int)" />
    /// </summary>
    /// <param name="x"></param>
    /// <param name="y"></param>
    /// <returns></returns>
    [Pure]
    public TileBox Move(int x, int y) => new(X, Y, Width, Height);

    [Pure]
    public bool Intersects(TileBox value) =>
        value.Left < Right &&
        Left < value.Right &&
        value.Top < Bottom &&
        Top < value.Bottom;
}