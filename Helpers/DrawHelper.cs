using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ReLogic.Content;
using SpawnHouses.Common.Modules;
using SpawnHouses.Common.Modules.Components;
using Terraria;
using Terraria.DataStructures;
using Terraria.ModLoader;

namespace SpawnHouses.Helpers;

public static class DrawHelper {
    private static readonly Asset<Texture2D> PixelTexture = ModContent.Request<Texture2D>("SpawnHouses/Common/Assets/Pixel");

    public static int DebugDrawWidth { get; private set; } = 3;

    public static void IncreaseDebugDrawWidth() {
        DebugDrawWidth++;
        if (DebugDrawWidth <= 0) DebugDrawWidth = 1;
    }

    public static void DecreaseDebugDrawWidth() {
        DebugDrawWidth--;
    }

    /// <summary>
    ///     array of general colors
    /// </summary>
    private static readonly Color[] AllColors = [
        Color.Black,
        Color.Red,
        Color.Green,
        Color.Orange,
        Color.DarkSalmon,
        Color.DarkBlue,
        Color.Purple,
        Color.Pink,
        Color.Brown,
        Color.Yellow,
        Color.White,
        Color.Gray,
        Color.SkyBlue
    ];

    /// <summary>
    ///     array of red-adjacent colors
    /// </summary>
    private static readonly Color[] FloorColors = [
        Color.Red,
        Color.RosyBrown,
        Color.HotPink,
        Color.Maroon,
        Color.DarkSalmon,
        Color.Crimson,
        Color.Pink,
        Color.LightSalmon,
        Color.DarkRed,
        Color.Coral
    ];

    /// <summary>
    ///     array of blue-adjacent colors
    /// </summary>
    private static readonly Color[] WallColors = [
        Color.LightBlue,
        Color.Azure,
        Color.DarkBlue,
        Color.Aquamarine,
        Color.SkyBlue,
        Color.BlueViolet,
        Color.RoyalBlue,
        Color.DarkCyan,
        Color.Indigo
    ];

    /// <summary>
    ///     array of yellow-adjacent colors
    /// </summary>
    private static readonly Color[] GapColors = [
        Color.Yellow,
        Color.YellowGreen,
        Color.Khaki,
        Color.GreenYellow,
        Color.Beige
    ];

    /// <summary>
    ///     array of greyscale colors
    /// </summary>
    private static readonly Color[] RoomColors = [
        Color.White,
        Color.Gray,
        Color.LightSlateGray,
        Color.DarkSlateGray,
        Color.Black
    ];

    /// <summary>
    ///     array of green-adjacent colors
    /// </summary>
    private static readonly Color[] StairwayColors = [
        Color.Green,
        Color.DarkGreen,
        Color.LightGreen,
        Color.Olive
    ];

    /// <summary>
    ///     array of
    /// </summary>
    private static readonly Color[] RoofColors = [
        Color.Brown,
        Color.DarkKhaki,
        Color.SaddleBrown
    ];

    /// <summary>
    ///     starts <see cref="Main.spriteBatch" /> in a world-relative state, useful for overlaying on tiles
    /// </summary>
    public static void BeginWorldSpriteBatch() => Main.spriteBatch.Begin(default, null, null, null, null, null, Main.GameViewMatrix.TransformationMatrix);

    /// <summary>
    ///     draws a path of points. path segments cannot be diagonal
    /// </summary>
    /// <param name="path">expected to be world coordinates (tile coords * 16)</param>
    /// <param name="color"></param>
    /// <param name="width"></param>
    /// <remarks>assumes world relative sprite batch has already begun</remarks>
    public static void DrawPath(Point16[] path, Color color, int width) {
        for (int i = 0; i < path.Length - 1; i++) {
            Point16 cur = path[i];
            Point16 next = path[i + 1];

            if (cur.X != next.X && cur.Y != next.Y)
                throw new Exception("path segments cannot be diagonal");

            Main.spriteBatch.Draw(
                PixelTexture.Value,
                cur.X != next.X ? new Rectangle(Math.Min(cur.X, next.X), cur.Y, Math.Abs(cur.X - next.X), width) : new Rectangle(Math.Min(cur.Y, next.Y), cur.X, Math.Abs(cur.Y - next.Y), width),
                color
            );
        }
    }

    /// <summary>
    ///     draws rectangles relative to the player camera
    /// </summary>
    /// <param name="rectangle">expected to be world coordinates (tile coords * 16)</param>
    /// <param name="color"></param>
    /// <param name="width"></param>
    /// <remarks>assumes world relative sprite batch has already begun</remarks>
    public static void DrawRectangle(Rectangle rectangle, Color color, int width) {
        Main.spriteBatch.Draw(PixelTexture.Value, new Rectangle(rectangle.Left, rectangle.Top, rectangle.Width, width), color);
        Main.spriteBatch.Draw(PixelTexture.Value, new Rectangle(rectangle.Right, rectangle.Top, width, rectangle.Height), color);
        Main.spriteBatch.Draw(PixelTexture.Value, new Rectangle(rectangle.Left, rectangle.Bottom, rectangle.Width, width), color);
        Main.spriteBatch.Draw(PixelTexture.Value, new Rectangle(rectangle.Left, rectangle.Top, width, rectangle.Height), color);
    }

    /// <summary>
    ///     draws rectangles relative to the player camera. Main.spriteBatch must be ended before this is called
    /// </summary>
    /// <param name="rectangles">expected to be world coordinates (tile coords * 16)</param>
    /// <param name="color"></param>
    /// <param name="width"></param>
    /// <remarks>assumes world relative sprite batch has already begun</remarks>
    public static void DrawRectangles(Rectangle[] rectangles, Color color, int width) {
        foreach (Rectangle rectangle in rectangles)
            DrawRectangle(rectangle, color, width);
    }

    /// <summary>
    ///     draws rectangles relative to the player camera. Main.spriteBatch must be ended before this is called
    /// </summary>
    /// <param name="rectangles">expected to be world coordinates (tile coords * 16)</param>
    /// <param name="colors">must have a color for every rectangle</param>
    /// <param name="width"></param>
    /// <remarks>assumes world relative sprite batch has already begun</remarks>
    public static void DrawRectangles(Rectangle[] rectangles, Color[] colors, int width) {
        for (int index = 0; index < rectangles.Length; index++) {
            Rectangle rect = rectangles[index];
            Color color = colors[index];
            DrawRectangle(rect, color, width);
        }
    }

    /// <summary>
    /// </summary>
    /// <param name="point">middle of the point, in world coordinates (not tile)</param>
    /// <param name="color"></param>
    /// <param name="width"></param>
    /// <remarks>assumes world relative sprite batch has already begun</remarks>
    public static void DrawPoint(Point16 point, Color color, int width) =>
        Main.spriteBatch.Draw(PixelTexture.Value, new Rectangle(point.X - width / 2, point.Y - width / 2, width, width), color);

    /// <summary>
    /// </summary>
    /// <param name="points">middle of the points, in world coordinates (not tile)</param>
    /// <param name="color"></param>
    /// <param name="width"></param>
    /// <remarks>assumes world relative sprite batch has already begun</remarks>
    public static void DrawPoints(Point16[] points, Color color, int width) {
        foreach (Point16 point in points)
            DrawPoint(point, color, width);
    }

    /// <summary>
    /// </summary>
    /// <param name="text"></param>
    /// <param name="position"></param>
    /// <param name="color"></param>
    /// <remarks>assumes world relative sprite batch has already begun</remarks>
    public static void DrawText(string text, Point16 position, Color color) {
        //Main.spriteBatch.DrawString();
        Utils.DrawBorderString(Main.spriteBatch, text, position.ToVector2(), color);
    }

    public static Color GetColor(ushort id) => AllColors[id % AllColors.Length];

    public static Color GetColor(IComponent component) {
        return component switch {
            Floor => FloorColors[component.Id % FloorColors.Length],
            Wall => WallColors[component.Id % WallColors.Length],
            Gap => GapColors[component.Id % GapColors.Length],
            Room => RoomColors[component.Id % RoomColors.Length],
            Stairway => StairwayColors[component.Id % StairwayColors.Length],
            Roof => RoofColors[component.Id % RoofColors.Length],
            _ => AllColors[component.Id % AllColors.Length]
        };
    }
}