using System;
using System.Collections.Generic;
using System.Diagnostics;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ReLogic.Content;
using SpawnHouses.Structures;
using Terraria;
using Terraria.ModLoader;
using Terraria.ID;

namespace SpawnHouses;

public class SpawnHousesModMenu : ModMenu
{
    public override bool IsAvailable => true;

    public override string DisplayName => "Generated Housing";
    //public override int Music => MusicLoader.GetMusicSlot(Mod, "Sounds/Music/YourMenuTheme"); // Optional custom music

    public override ModSurfaceBackgroundStyle MenuBackgroundStyle => ModContent.GetInstance<CustomMenuBackgroundStyle>();
}


public class CustomMenuBackgroundStyle : ModSurfaceBackgroundStyle
{
    private Stopwatch _stopwatch;

    private Asset<Texture2D> _skyBackTexture;
    private Asset<Texture2D>[] _skyDetailTextures = new Asset<Texture2D>[55];
    private Asset<Texture2D> _backTexture;
    private Asset<Texture2D>[] _frontTextures;

    private readonly double _frontFrameInterval = 1000.0 / 9;
    private readonly double _skyDetailFrameInterval = 1000.0 / 5;

    public override void Load()
    {
        _stopwatch = Stopwatch.StartNew();

        _skyBackTexture = ModContent.Request<Texture2D>("SpawnHouses/Assets/Menu/sky_back0050");
        for (int i = 0; i < _skyDetailTextures.Length; i++)
        {
            string name = i + "";
            if (name.Length == 1)
                name = "0" + name;
            _skyDetailTextures[i] = ModContent.Request<Texture2D>($"SpawnHouses/Assets/Menu/sky_detail00{name}");
        }

        _backTexture = ModContent.Request<Texture2D>("SpawnHouses/Assets/Menu/back0050");
        _frontTextures =
        [
            ModContent.Request<Texture2D>("SpawnHouses/Assets/Menu/front0050"),
            ModContent.Request<Texture2D>("SpawnHouses/Assets/Menu/front0051"),
            ModContent.Request<Texture2D>("SpawnHouses/Assets/Menu/front0052"),
            ModContent.Request<Texture2D>("SpawnHouses/Assets/Menu/front0053"),
            ModContent.Request<Texture2D>("SpawnHouses/Assets/Menu/front0054"),
            ModContent.Request<Texture2D>("SpawnHouses/Assets/Menu/front0055")
        ];
    }

    public override void Unload()
    {
        // force this to main thread because sometimes unloading is done on separate thread
        Main.RunOnMainThread(() =>
        {
            _skyBackTexture?.Dispose();
            foreach (var texture in _skyDetailTextures)
                texture?.Dispose();
            _backTexture?.Dispose();
            foreach (var texture in _frontTextures)
                texture?.Dispose();
        });
    }

    public override bool PreDrawCloseBackground(SpriteBatch spriteBatch)
    {

        spriteBatch.Draw(
            _skyBackTexture.Value,
            new Rectangle(0, 0, Main.screenWidth, Main.screenHeight),
            Color.White
        );

        spriteBatch.Draw(
            _skyDetailTextures[(int)Math.Round(_stopwatch.ElapsedMilliseconds / _skyDetailFrameInterval) % 55].Value,
            new Rectangle(0, 0, Main.screenWidth, Main.screenHeight),
            Color.White
        );

        spriteBatch.Draw(
            _backTexture.Value,
            new Rectangle(0, 0, Main.screenWidth, Main.screenHeight),
            Color.White
        );

        spriteBatch.Draw(
            _frontTextures[(int)Math.Round(_stopwatch.ElapsedMilliseconds / _frontFrameInterval) % 6].Value,
            new Rectangle(0, 0, Main.screenWidth, Main.screenHeight),
            Color.White
        );

        return false;
    }

    public override void ModifyFarFades(float[] fades, float transitionSpeed)
    {
        for (int i = 0; i < fades.Length; i++) {
            if (i == Slot) {
                fades[i] += transitionSpeed;
                if (fades[i] > 1f) {
                    fades[i] = 1f;
                }
            }
            else {
                fades[i] -= transitionSpeed;
                if (fades[i] < 0f) {
                    fades[i] = 0f;
                }
            }
        }
    }
}