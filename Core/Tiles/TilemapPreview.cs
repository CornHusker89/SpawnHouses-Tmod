using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ReLogic.Content;
using SpawnHouses.Helpers;
using Terraria;
using Terraria.GameContent;
using Terraria.ModLoader;

namespace SpawnHouses.Core.Tiles;

#nullable enable

public class TilemapPreview {
    public StructureTilemap Tilemap;

    public RenderTarget2D? Texture;

    public TilemapPreview(StructureTilemap tilemap) {
        Tilemap = tilemap;
    }

    /// <summary>
    ///     creates a render of the tilemap. tilemap must be loaded
    /// </summary>
    // shoutout to structurehelper - I didn't write a single line of this myself
    public RenderTarget2D GeneratePreview() {
        if (!Tilemap.IsAllTilesLoaded) throw new Exception("tilemap must be fully loaded before rendering tilemap");

        var oldTargets = Main.graphics.GraphicsDevice.GetRenderTargets();

        RenderTarget2D newTexture = new(Main.graphics.GraphicsDevice, Tilemap.Width * 16, Tilemap.Height * 16, false, default, default, default, RenderTargetUsage.PreserveContents);

        Main.graphics.GraphicsDevice.SetRenderTarget(newTexture);
        Main.graphics.GraphicsDevice.Clear(Color.Transparent);
        Main.spriteBatch.Begin();

        for (int x = 0; x < Tilemap.Width; x++)
        for (int y = 0; y < Tilemap.Height; y++) {
            StructureTile tile = Tilemap[x, y];
            if (tile.WallType != 0 && tile.WallType != StructureHelper.StructureHelper.NULL_IDENTIFIER && tile.WallType < TextureAssets.Wall.Length) {
                Asset<Texture2D> tex = TextureAssets.Wall[tile.WallType];
                if (!tex.IsLoaded) Main.instance.LoadWall(tile.WallType);
                Color tint = Color.White;
                if (tile.WallColor > 0)
                    tint = WorldGen.paintColor(tile.WallColor);
                Texture2D paintedTex = DrawHelper.GetPaintedTexture(tex.Value, tile.WallColor);
                Main.spriteBatch.Draw(paintedTex, new Rectangle(x * 16, y * 16, 16, 16), new Rectangle(8, 8, 16, 16), tint);
            }

            if (tile.HasTile && tile.TileType != StructureHelper.StructureHelper.NULL_IDENTIFIER && tile.TileType < TextureAssets.Tile.Length) {
                Asset<Texture2D> tex = TextureAssets.Tile[tile.TileType];
                if (!tex.IsLoaded) Main.instance.LoadTiles(tile.TileType);
                Color tint = Color.White;
                if (tile.TileColor > 0)
                    tint = WorldGen.paintColor(tile.TileColor);
                Texture2D paintedTex = DrawHelper.GetPaintedTexture(tex.Value, tile.TileColor);
                Main.spriteBatch.Draw(paintedTex, new Rectangle(x * 16, y * 16, 16, 16), new Rectangle(tile.TileFrameX, tile.TileFrameY, 16, 16), tint);
            }
        }

        Main.spriteBatch.End();

        Main.graphics.GraphicsDevice.SetRenderTargets(null);
        Main.graphics.GraphicsDevice.SetRenderTargets(oldTargets);
        return newTexture;
    }

    public void Dispose() {
        Texture?.Dispose();
    }
}

public class TilemapPreviewRendering : ILoadable {
    /// <summary>
    ///     queue of previews to render textures for when the next opportunity arises
    /// </summary>
    public static readonly List<TilemapPreview> Queue = [];

    public void Load(Mod mod) {
        On_Main.CheckMonoliths += DrawQueuedPreview;
    }

    public void Unload() {
        On_Main.CheckMonoliths -= DrawQueuedPreview;
    }

    /// <summary>
    ///     when the opportunity in the rendering cycle arises, render out the queued previews
    /// </summary>
    /// <param name="orig"></param>
    private void DrawQueuedPreview(On_Main.orig_CheckMonoliths orig) {
        // items might get added to the queue mid-render, keep track of what we're doing this cycle
        int len = Queue.Count;
        for (int i = 0; i < len; i++) Queue[i].Texture = Queue[i].GeneratePreview();

        Queue.RemoveRange(0, len);

        orig();
    }
}