using System.Diagnostics.Contracts;
using Microsoft.Xna.Framework;
using SpawnHouses.Common.DataStructures;

namespace SpawnHouses.Helpers;

public static class Utils {
    /// <summary>
    ///     note that width and height will be different by +1
    /// </summary>
    /// <param name="rect"></param>
    /// <returns></returns>
    public static TileBox ToTileBox(this Rectangle rect) => new(rect.X, rect.Y, rect.Width + 1, rect.Height + 1);

    /// <summary>
    ///     scales both rectangle size and preexisting translation
    /// </summary>
    /// <param name="r"></param>
    /// <param name="scale"></param>
    /// <returns></returns>
    [Pure]
    public static Rectangle Scale(this Rectangle r, int scale) => new(r.X * scale, r.Y * scale, r.Width * scale, r.Height * scale);

    /// <inheritdoc cref="Scale(Rectangle, int)" />
    [Pure]
    public static Rectangle Scale(this Rectangle r, float scale) => new((int)(r.X * scale), (int)(r.Y * scale), (int)(r.Width * scale), (int)(r.Height * scale));

    /// <summary>
    ///     scales rectangle size and translates such that center is not changed
    /// </summary>
    /// <param name="r"></param>
    /// <param name="scale"></param>
    /// <returns></returns>
    [Pure]
    public static Rectangle ScaleLocal(this Rectangle r, int scale) => new(r.X - r.Width * scale / 2, r.Y - r.Height * scale / 2, r.Width * scale, r.Width * scale);

    /// <inheritdoc cref="ScaleLocal(Rectangle, int)" />
    [Pure]
    public static Rectangle ScaleLocal(this Rectangle r, float scale) => new((int)(r.X - r.Width * scale / 2), (int)(r.Y - r.Height * scale / 2), (int)(r.Width * scale), (int)(r.Width * scale));

    /// <summary>
    ///     scales only rectangle size, does not modify top left point
    /// </summary>
    /// <param name="r"></param>
    /// <param name="scale"></param>
    /// <returns></returns>
    [Pure]
    public static Rectangle AnchoredScaleLocal(this Rectangle r, int scale) => new(r.X, r.Y, r.Height * scale, r.Width * scale);

    /// <inheritdoc cref="AnchoredScaleLocal(Rectangle, float)" />
    [Pure]
    public static Rectangle AnchoredScaleLocal(this Rectangle r, float scale) => new(r.X, r.Y, (int)(r.Height * scale), (int)(r.Width * scale));
}