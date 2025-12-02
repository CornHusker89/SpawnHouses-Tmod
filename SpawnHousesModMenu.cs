using System;
using System.Diagnostics;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ReLogic.Content;
using SpawnHouses.Structures;
using Terraria;
using Terraria.ModLoader;

namespace SpawnHouses;

public class SpawnHousesModMenu : ModMenu {
    public override bool IsAvailable => true;
    public override string DisplayName => $"Nighttime Forest ({ModInstance.Mod.DisplayName})"; 
    public override ModSurfaceBackgroundStyle MenuBackgroundStyle => ModContent.GetInstance<CustomMenuBackgroundStyle>();

    public override bool PreDrawLogo(SpriteBatch spriteBatch, ref Vector2 logoDrawCenter, ref float logoRotation, ref float logoScale, ref Color drawColor) {
        logoScale = 0.9f;
        return true;
    }
}

public class CustomMenuBackgroundStyle : ModSurfaceBackgroundStyle {
    private Stopwatch _stopwatch;

    private Asset<Texture2D> _forestFrontTexture;
    private Asset<Texture2D>[] _forestBackTextures;
    private Asset<Texture2D> _mountainFrontTexture;
    private Asset<Texture2D> _mountainBackTexture;
    private readonly Asset<Texture2D>[] _cloudsTextures = new Asset<Texture2D>[55];
    private Asset<Texture2D> _skyTexture;

    private readonly float _skyDetailFrameInterval = 1000f / 5;
    private readonly float _frontFrameInterval = 1000f / 7.5f;

    private readonly float _frameZoom = 1.1f;

    private readonly float _parallaxScale = -3.6f;
    private readonly float _forestFrontParallaxFactor = 1.25f;
    private readonly float _forestBackParallaxFactor = 0.7f;
    private readonly float _mountainFrontParallaxFactor = 0.7f;
    private readonly float _mountainBackParallaxFactor = 0.4f;
    private readonly float _cloudsParallaxFactor = 0.25f;

    private readonly int _mountainFrontYOffset = (int)(Main.screenWidth * 0f);
    private readonly int _mountainBackYOffset = (int)(Main.screenWidth * 0.02f);
    private readonly int _skyYOffset = (int)(Main.screenHeight * 0f);

    public override void Load() {
        _stopwatch = Stopwatch.StartNew();

        _forestFrontTexture = ModContent.Request<Texture2D>("SpawnHouses/Assets/Menu/forest_front0050");
        _forestBackTextures = [
            ModContent.Request<Texture2D>("SpawnHouses/Assets/Menu/forest_back0050"),
            ModContent.Request<Texture2D>("SpawnHouses/Assets/Menu/forest_back0051"),
            ModContent.Request<Texture2D>("SpawnHouses/Assets/Menu/forest_back0052"),
            ModContent.Request<Texture2D>("SpawnHouses/Assets/Menu/forest_back0053"),
            ModContent.Request<Texture2D>("SpawnHouses/Assets/Menu/forest_back0054"),
            ModContent.Request<Texture2D>("SpawnHouses/Assets/Menu/forest_back0055")
        ];
        _mountainFrontTexture = ModContent.Request<Texture2D>("SpawnHouses/Assets/Menu/mountain_front0050");
        _mountainBackTexture = ModContent.Request<Texture2D>("SpawnHouses/Assets/Menu/mountain_back0050");
        for (int i = 0; i < _cloudsTextures.Length; i++) {
            string name = i + "";
            if (name.Length == 1) {
                name = "0" + name;
            }

            _cloudsTextures[i] = ModContent.Request<Texture2D>($"SpawnHouses/Assets/Menu/clouds00{name}");
        }

        _skyTexture = ModContent.Request<Texture2D>("SpawnHouses/Assets/Menu/sky0050");

    }

    public override void Unload() {
        // force this to main thread because sometimes unloading is done on separate thread
        Main.RunOnMainThread(() => {
            _forestFrontTexture?.Dispose();
            foreach (var texture in _forestBackTextures) {
                texture?.Dispose();
            }

            _mountainFrontTexture?.Dispose();
            _mountainBackTexture?.Dispose();
            foreach (var texture in _cloudsTextures) {
                texture?.Dispose();
            }

            _skyTexture?.Dispose();
        });
    }

    public override bool PreDrawCloseBackground(SpriteBatch spriteBatch) {
        Vector2 mousePos = (Main.MouseScreen - Main.LastLoadedResolution.ToVector2() / 2f) / 100f;
        int frameOffsetX = (int)(Main.screenWidth * (1 - _frameZoom) / 2);
        int frameOffsetY = (int)(Main.screenHeight * (1 - _frameZoom) / 2);
        int frameWidth = (int)(Main.screenWidth * _frameZoom);
        int frameHeight = (int)(Main.screenHeight * _frameZoom);
        
        spriteBatch.Draw(
            _skyTexture.Value,
            new Rectangle(0, 0, Main.screenWidth, Main.screenHeight),
            Color.White
        );

        spriteBatch.Draw(
            _cloudsTextures[(int)Math.Round(_stopwatch.ElapsedMilliseconds / _skyDetailFrameInterval) % 55].Value,
            new Rectangle(frameOffsetX + (int)(mousePos.X * _cloudsParallaxFactor * _parallaxScale), frameOffsetY + _skyYOffset + (int)(mousePos.Y * _cloudsParallaxFactor * _parallaxScale), frameWidth, frameHeight),
            Color.White
        );

        spriteBatch.Draw(
            _mountainBackTexture.Value,
            new Rectangle(frameOffsetX + (int)(mousePos.X * _mountainBackParallaxFactor * _parallaxScale), frameOffsetY + _mountainBackYOffset + (int)(mousePos.Y * _mountainBackParallaxFactor * _parallaxScale), frameWidth, frameHeight),
            Color.White
        );
        
        spriteBatch.Draw(
            _mountainFrontTexture.Value,
            new Rectangle(frameOffsetX + (int)(mousePos.X * _mountainFrontParallaxFactor * _parallaxScale), frameOffsetY + _mountainFrontYOffset + (int)(mousePos.Y * _mountainFrontParallaxFactor * _parallaxScale), frameWidth, frameHeight),
            Color.White
        );

        spriteBatch.Draw(
            _forestBackTextures[(int)Math.Round(_stopwatch.ElapsedMilliseconds / _frontFrameInterval) % 6].Value,
            new Rectangle(frameOffsetX + (int)(mousePos.X * _forestBackParallaxFactor * _parallaxScale), frameOffsetY + (int)(mousePos.Y * _forestBackParallaxFactor * _parallaxScale), frameWidth, frameHeight),
            Color.White
        );

        spriteBatch.Draw(
            _forestFrontTexture.Value,
            new Rectangle(frameOffsetX + (int)(mousePos.X * _forestFrontParallaxFactor * _parallaxScale), frameOffsetY + (int)(mousePos.Y * _forestFrontParallaxFactor * _parallaxScale), frameWidth, frameHeight),
            Color.White
        );

        return false;
    }

    public override void ModifyFarFades(float[] fades, float transitionSpeed) {
        for (int i = 0; i < fades.Length; i++) {
            if (i == Slot) {
                fades[i] += transitionSpeed * 2;
                if (fades[i] > 1f) {
                    fades[i] = 1f;
                }
            }
            else {
                fades[i] -= transitionSpeed * 2;
                if (fades[i] < 0f) {
                    fades[i] = 0f;
                }
            }
        }
    }
}