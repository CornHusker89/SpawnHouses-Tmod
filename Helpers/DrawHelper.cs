using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ReLogic.Content;
using SpawnHouses.Core.AdvGeneratables.Components;
using SpawnHouses.Core.Debug;
using SpawnHouses.Core.Interfaces;
using Terraria;
using Terraria.ModLoader;

namespace SpawnHouses.Helpers;

public static class DrawHelper {
    private static readonly Asset<Texture2D> PixelTexture = ModContent.Request<Texture2D>("SpawnHouses/Core/Assets/Pixel");
    private static readonly Dictionary<(Texture2D, byte), Texture2D> PaintCache = new();
    
    public static int DebugDrawWidth { get; private set; } = 3;

    public static void IncreaseDebugDrawWidth() {
        DebugDrawWidth++;
    }

    public static void DecreaseDebugDrawWidth() {
        DebugDrawWidth--;
        if (DebugDrawWidth <= 0) DebugDrawWidth = 1;
    }

    /// <summary>
    ///     array of general colors
    /// </summary>
    private static readonly Color[] AllColors = [
        Color.Black,
        Color.Red,
        Color.Green,
        Color.Orange,
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
        Color.Crimson,
        Color.Pink,
        Color.LightSalmon,
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
        Color.DarkSlateGray
    ];

    /// <summary>
    ///     array of green-adjacent colors
    /// </summary>
    private static readonly Color[] StairwayColors = [
        Color.Green,
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
    ///     draws a path of non-diagonal points. path segments cannot be diagonal
    /// </summary>
    /// <param name="path">expected to be world coordinates (tile coords * 16), with no additional offsets</param>
    /// <param name="color"></param>
    /// <param name="width"></param>
    /// <remarks>assumes world relative sprite batch has already begun</remarks>
    public static void DrawWorldBasedRectangularPath(Point[] path, Color color, int width) {
        Point screenPos = Main.screenPosition.ToPoint();
        Point drawWidthOffset2D = new(-DebugDrawWidth / 2, -DebugDrawWidth / 2);
        for (int i = 0; i < path.Length; i++) {
            Point cur = path[i] + drawWidthOffset2D;
            Point next = path[(i + 1) % path.Length] + drawWidthOffset2D;

            if (cur.X != next.X && cur.Y != next.Y)
                throw new Exception("path segments cannot be diagonal");

            Main.spriteBatch.Draw(
                PixelTexture.Value,
                cur.X != next.X ? new Rectangle(Math.Min(cur.X, next.X) - screenPos.X, cur.Y - screenPos.Y, Math.Abs(cur.X - next.X), width) : new Rectangle(cur.X - screenPos.X, Math.Min(cur.Y, next.Y) - screenPos.Y, width, Math.Abs(cur.Y - next.Y)),
                color
            );
        }
    }

    /// <summary>
    ///     draws border-like rectangles relative to the world
    /// </summary>
    /// <param name="rectangle">expected to be world coordinates (tile coords * 16), with no additional offsets</param>
    /// <param name="color"></param>
    /// <param name="width"></param>
    /// <remarks>assumes world relative sprite batch has already begun</remarks>
    public static void DrawWorldBasedBorder(Rectangle rectangle, Color color, int width) {
        Point screenPos = Main.screenPosition.ToPoint();
        int drawWidthOffset = -width / 2;
        Point start = new(rectangle.X - screenPos.X + drawWidthOffset, rectangle.Y - screenPos.Y + drawWidthOffset);
        Rectangle offsetRectangle = new(start.X, start.Y, rectangle.Width, rectangle.Height);
        //Terraria.Utils.DrawRectangle(Main.spriteBatch, start, start + new Vector2(rectangle.Width, rectangle.Height), color, color, width);
        Main.spriteBatch.Draw(PixelTexture.Value, new Rectangle(offsetRectangle.Left, offsetRectangle.Top, offsetRectangle.Width, width), color);
        Main.spriteBatch.Draw(PixelTexture.Value, new Rectangle(offsetRectangle.Right, offsetRectangle.Top, width, offsetRectangle.Height), color);
        Main.spriteBatch.Draw(PixelTexture.Value, new Rectangle(offsetRectangle.Left, offsetRectangle.Bottom, offsetRectangle.Width, width), color);
        Main.spriteBatch.Draw(PixelTexture.Value, new Rectangle(offsetRectangle.Left, offsetRectangle.Top, width, offsetRectangle.Height), color);
    }

    /// <summary>
    ///     draws border-like relative to the world
    /// </summary>
    /// <param name="rectangles">expected to be world coordinates (tile coords * 16), with no additional offsets</param>
    /// <param name="color"></param>
    /// <param name="width"></param>
    /// <remarks>assumes world relative sprite batch has already begun</remarks>
    public static void DrawWorldBasedBorders(Rectangle[] rectangles, Color color, int width) {
        foreach (Rectangle rectangle in rectangles)
            DrawWorldBasedBorder(rectangle, color, width);
    }

    /// <summary>
    ///     draws border-like relative to the world
    /// </summary>
    /// <param name="rectangles">expected to be world coordinates (tile coords * 16), with no additional offsets</param>
    /// <param name="colors">must have a color for every rectangle</param>
    /// <param name="width"></param>
    /// <remarks>assumes world relative sprite batch has already begun</remarks>
    public static void DrawWorldBasedBorders(Rectangle[] rectangles, Color[] colors, int width) {
        for (int index = 0; index < rectangles.Length; index++) {
            Rectangle rect = rectangles[index];
            Color color = colors[index];
            DrawWorldBasedBorder(rect, color, width);
        }
    }

    /// <summary>
    /// </summary>
    /// <param name="point">middle of the point, in world coordinates (not tile), with no additional offsets</param>
    /// <param name="color"></param>
    /// <param name="width"></param>
    /// <remarks>assumes world relative sprite batch has already begun</remarks>
    public static void DrawWorldBasedPoint(Point point, Color color, int width) {
        Point offsetPoint = point - Main.screenPosition.ToPoint();
        Main.spriteBatch.Draw(PixelTexture.Value, new Rectangle(offsetPoint.X - width / 2, offsetPoint.Y - width / 2, width, width), color);
    }

    /// <summary>
    /// </summary>
    /// <param name="points">middle of the points, in world coordinates (not tile), with no additional offsets</param>
    /// <param name="color"></param>
    /// <param name="width"></param>
    /// <remarks>assumes world relative sprite batch has already begun</remarks>
    public static void DrawWorldBasedPoints(Point[] points, Color color, int width) {
        foreach (Point point in points)
            DrawWorldBasedPoint(point, color, width);
    }

    /// <summary>
    /// </summary>
    /// <param name="text"></param>
    /// <param name="position">in world coordinates, with no additional offsets</param>
    /// <param name="color"></param>
    /// <remarks>assumes world relative sprite batch has already begun</remarks>
    public static void DrawWorldBasedText(string text, Point position, Color color) {
        Point offsetPosition = position - Main.screenPosition.ToPoint();
        // uses font FontAssets.MouseText
        Utils.DrawBorderString(Main.spriteBatch, text, offsetPosition.ToVector2(), color);
    }

    /// <summary>
    ///     handles upda
    /// </summary>
    /// <param name="label"></param>
    /// <param name="position">in world coordinates, with no additional offsets</param>
    /// <param name="drawWidth"></param>
    /// <param name="color"></param>
    /// <remarks>assumes world relative sprite batch has already begun</remarks>
    public static void DrawDebugLabel(DebugLabel label, Point position, int drawWidth, Color color) {
        Point textDimensions = label.TextDimensions.ToPoint();

        // draw containing box
        DrawWorldBasedBorder(
            new Rectangle(position.X, position.Y, textDimensions.X + drawWidth * 7, textDimensions.Y),
            color,
            drawWidth
        );

        // draw text
        DrawWorldBasedText(label.GetDisplayString(), position + new Point(drawWidth * 2, drawWidth * 1), color);

        // draw line to root
        Point offsetLabelRoot = label.Root.ToPoint() * new Point(16, 16);
        Vector2 lineStartOffset;
        if (offsetLabelRoot.X < position.X + textDimensions.X / 2) {
            if (offsetLabelRoot.Y < position.Y + textDimensions.Y / 2) // from top left
                lineStartOffset = new Vector2(0, 0);
            else // from bottom left
                lineStartOffset = new Vector2(0, textDimensions.Y);
        }
        else {
            if (offsetLabelRoot.Y < position.Y + textDimensions.Y / 2) // from top right
                lineStartOffset = new Vector2(textDimensions.X + drawWidth * 7, 0);
            else // from bottom right
                lineStartOffset = new Vector2(textDimensions.X + drawWidth * 7, textDimensions.Y);
        }

        Utils.DrawLine(Main.spriteBatch, position.ToVector2() + lineStartOffset, offsetLabelRoot.ToVector2(), color, color, DebugDrawWidth);
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

    private static (float h, float s, float l) RgbToHsl(Color c) {
        float r = c.R / 255f, g = c.G / 255f, b = c.B / 255f;
        float max = Math.Max(r, Math.Max(g, b)), min = Math.Min(r, Math.Min(g, b));
        float l = (max + min) / 2f;
        if (Math.Abs(max - min) < 0.03) return (0f, 0f, l); // achromatic
        float d = max - min;
        float s = l > 0.5f ? d / (2f - max - min) : d / (max + min);
        float h;
        if (Math.Abs(max - r) < 0.03) h = (g - b) / d + (g < b ? 6f : 0f);
        else if (Math.Abs(max - g) < 0.03) h = (b - r) / d + 2f;
        else h = (r - g) / d + 4f;
        return (h / 6f, s, l);
    }

    private static Color HslToRgb(float h, float s, float l, byte a) {
        if (s == 0f) {
            byte v = (byte)(l * 255f);
            return new Color(v, v, v, a);
        }

        float q = l < 0.5f ? l * (1f + s) : l + s - l * s;
        float p = 2f * l - q;

        float Hue2Rgb(float t) {
            if (t < 0f) t += 1f;
            if (t > 1f) t -= 1f;
            if (t < 1f / 6f) return p + (q - p) * 6f * t;
            if (t < 1f / 2f) return q;
            if (t < 2f / 3f) return p + (q - p) * (2f / 3f - t) * 6f;
            return p;
        }

        return new Color((byte)(Hue2Rgb(h + 1f / 3f) * 255f), (byte)(Hue2Rgb(h) * 255f), (byte)(Hue2Rgb(h - 1f / 3f) * 255f), a);
    }

    private static float GetPaintHue(byte paintId) {
        // ids 1-12: red..Pink, evenly spaced. ids 13-24: Deep versions, same hue as (id-12).
        int baseId = paintId is >= 13 and <= 24 ? paintId - 12 : paintId;
        return (baseId - 1) / 12f; // 0f-1f hue, matches HslToRgb's h parameter
    }

    private static bool IsDeepPaint(byte paintId) => paintId is >= 13 and <= 24;

    private const float RegularPaintSaturationScale = 0.7f; // regular (1-12): scale existing saturation down
    private const float RegularPaintSaturationCap = 0.75f; // ...and cap so already-vivid pixels don't stay punchy
    private const float DeepPaintSaturationFloor = 0.55f; // deep (13-24): push saturation UP toward vivid
    private const float BrownSaturationScale = 0.15f;
    private const float BrownSaturationCap = 0.35f;
    private const float BrownLightnessScale = 1.3f;
    private const float BlackSaturationScale = 0.3f;
    private const float BlackLightnessScale = 0.65f;
    private const float BlackLightnessCap = 0.4f;
    private const float WhiteSaturationScale = 0.3f;
    private const float WhiteLightnessFloor = 0.6f; // pixels blend upward from this floor
    private const float WhiteLightnessRange = 0.4f; // ...scaled by this much of original lightness
    private const float GraySaturationScale = 0.15f;

    public static Texture2D GetPaintedTexture(Texture2D baseTex, byte paintId) {
        if (paintId == 0) return baseTex;
        (Texture2D baseTex, byte paintId) key = (baseTex, paintId);
        if (PaintCache.TryGetValue(key, out Texture2D cached)) return cached;

        var pixels = new Color[baseTex.Width * baseTex.Height];
        baseTex.GetData(pixels);

        switch (paintId) {
            case 25: // Black — scale saturation down, scale lightness down. Preserves relative shading.
                for (int i = 0; i < pixels.Length; i++) {
                    if (pixels[i].A == 0) continue;
                    (float h, float s, float l) = RgbToHsl(pixels[i]);
                    float newL = MathF.Min(l * BlackLightnessScale, BlackLightnessCap);
                    pixels[i] = HslToRgb(h, s * BlackSaturationScale, newL, pixels[i].A);
                }

                break;

            case 26: // White — scale saturation down, lift lightness toward a floor (still scaled by original).
                for (int i = 0; i < pixels.Length; i++) {
                    if (pixels[i].A == 0) continue;
                    (float h, float s, float l) = RgbToHsl(pixels[i]);
                    float newL = WhiteLightnessFloor + l * WhiteLightnessRange;
                    pixels[i] = HslToRgb(h, s * WhiteSaturationScale, newL, pixels[i].A);
                }

                break;

            case 27: // Gray — scale saturation down only, lightness untouched.
                for (int i = 0; i < pixels.Length; i++) {
                    if (pixels[i].A == 0) continue;
                    (float h, float s, float l) = RgbToHsl(pixels[i]);
                    pixels[i] = HslToRgb(h, s * GraySaturationScale, l, pixels[i].A);
                }

                break;

            case 28: // Brown — hue-lock to orange, scale (not floor) saturation, mild darken.
                for (int i = 0; i < pixels.Length; i++) {
                    if (pixels[i].A == 0) continue;
                    (float h, float s, float l) = RgbToHsl(pixels[i]);
                    float newS = MathF.Min(s * BrownSaturationScale, BrownSaturationCap);
                    float newL = MathF.Min(l * BrownLightnessScale, 1f);
                    pixels[i] = HslToRgb(30f / 360f, newS, newL, pixels[i].A);
                }

                break;

            case 30: // Negative — true RGB invert, no HSL involved.
                for (int i = 0; i < pixels.Length; i++)
                    pixels[i] = new Color(255 - pixels[i].R, 255 - pixels[i].G, 255 - pixels[i].B, pixels[i].A);
                break;

            default: // Regular (1-12): hue replace, scale+cap saturation down.
                // Deep (13-24): hue replace, floor saturation up.
                float hue = GetPaintHue(paintId);
                bool deep = IsDeepPaint(paintId);
                for (int i = 0; i < pixels.Length; i++) {
                    if (pixels[i].A == 0) continue;
                    (float h, float s, float l) = RgbToHsl(pixels[i]);
                    float newS = deep
                        ? MathF.Max(s, DeepPaintSaturationFloor)
                        : MathF.Min(s * RegularPaintSaturationScale, RegularPaintSaturationCap);
                    pixels[i] = HslToRgb(hue, newS, l, pixels[i].A);
                }

                break;
        }

        Texture2D result = new(Main.graphics.GraphicsDevice, baseTex.Width, baseTex.Height);
        result.SetData(pixels);
        PaintCache[key] = result;
        return result;
    }
}
