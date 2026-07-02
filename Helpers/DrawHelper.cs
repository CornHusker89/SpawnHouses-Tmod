using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ReLogic.Content;
using Terraria;
using Terraria.ModLoader;

namespace SpawnHouses.Helpers;

public static class DrawHelper {
    private static readonly Asset<Texture2D> PixelTexture = ModContent.Request<Texture2D>("SpawnHouses/Items/StructureSpawns/Pixel");

    /// <summary>
    ///     Draws rectangles relative to the player camera. Main.spriteBatch must be ended before this is called
    /// </summary>
    /// <param name="rectangles">Expected to be world coordinates (tile coords * 16)</param>
    /// <param name="colors">Must have a color for every rectangle</param>
    /// <param name="width"></param>
    public static void DrawRectangles(Rectangle[] rectangles, Color[] colors, int width) {
        Main.spriteBatch.Begin(default, default, default, default, default, default, Main.GameViewMatrix.TransformationMatrix);

        for (int index = 0; index < rectangles.Length; index++) {
            Rectangle rect = rectangles[index];
            Color color = colors[index];
            Main.spriteBatch.Draw(PixelTexture.Value, new Rectangle(rect.Left, rect.Top, rect.Width, width), color);
            Main.spriteBatch.Draw(PixelTexture.Value, new Rectangle(rect.Right, rect.Top, width, rect.Height), color);
            Main.spriteBatch.Draw(PixelTexture.Value, new Rectangle(rect.Left, rect.Bottom, rect.Width, width), color);
            Main.spriteBatch.Draw(PixelTexture.Value, new Rectangle(rect.Left, rect.Top, width, rect.Height), color);
        }

        Main.spriteBatch.End();
    }
}