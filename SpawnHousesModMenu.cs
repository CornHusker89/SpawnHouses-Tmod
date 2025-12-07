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
    public override ModSurfaceBackgroundStyle MenuBackgroundStyle => ModContent.GetInstance<NighttimeForestBackgroundStyle>();

    public override bool PreDrawLogo(SpriteBatch spriteBatch, ref Vector2 logoDrawCenter, ref float logoRotation,
        ref float logoScale, ref Color drawColor) {
        logoScale = 0.93f;
        return true;
    }

    // public override void OnDeselected() {
    //     base.OnDeselected();
    // }
}

public class NighttimeForestBackgroundStyle : ModSurfaceBackgroundStyle {
    private Stopwatch _stopwatch;
    private readonly Vector2[] _mousePosHistory = new Vector2[12];
    private int _mousePosHistoryIndex;

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
    private readonly float _forestFrontParallaxFactor = 1.4f;
    private readonly float _forestBackParallaxFactor = 0.7f;
    private readonly float _mountainFrontParallaxFactor = 0.7f;
    private readonly float _mountainBackParallaxFactor = 0.4f;
    private readonly float _cloudsParallaxFactor = 0.25f;

    private float _transparency;

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

    // public override int ChooseCloseTexture(ref float scale, ref double parallax, ref float a, ref float b) {
    //     return base.ChooseCloseTexture(ref scale, ref parallax, ref a, ref b);
    // }

    public override bool PreDrawCloseBackground(SpriteBatch spriteBatch) {
        // get the average mouse position over <_mousePosHistory.Length> frames
        _mousePosHistoryIndex++;
        if (_mousePosHistoryIndex == _mousePosHistory.Length)
            _mousePosHistoryIndex = 0;
        Vector2 thisMousePos = (Main.MouseScreen - Main.LastLoadedResolution.ToVector2() / 2f) / 100f;
        _mousePosHistory[_mousePosHistoryIndex] = thisMousePos;
        float mousePosXRunningTotal = 0, mousePosYRunningTotal = 0;
        foreach (Vector2 pos in _mousePosHistory) {
            mousePosXRunningTotal += pos.X;
            mousePosYRunningTotal += pos.Y;
        }

        mousePosXRunningTotal /= _mousePosHistory.Length;
        mousePosYRunningTotal /= _mousePosHistory.Length;
        Vector2 averageMousePos = new(mousePosXRunningTotal, mousePosYRunningTotal);
            
        
        int frameOffsetX = (int)(Main.screenWidth * (1 - _frameZoom) / 2);
        int frameOffsetY = (int)(Main.screenHeight * (1 - _frameZoom) / 2);
        int frameWidth = (int)(Main.screenWidth * _frameZoom);
        int frameHeight = (int)(Main.screenHeight * _frameZoom);

        _transparency = (float)Math.Floor(Main.bgAlphaFrontLayer[Slot]);
        
        spriteBatch.Draw(
            _skyTexture.Value,
            new Rectangle(0, 0, Main.screenWidth, Main.screenHeight),
            Color.White * _transparency
        );

        spriteBatch.Draw(
            _cloudsTextures[(int)Math.Round(_stopwatch.ElapsedMilliseconds / _skyDetailFrameInterval) % 55].Value,
            new Rectangle(frameOffsetX + (int)(averageMousePos.X * _cloudsParallaxFactor * _parallaxScale),
                frameOffsetY + (int)(averageMousePos.Y * _cloudsParallaxFactor * _parallaxScale),
                frameWidth,
                frameHeight),
            Color.White * _transparency
        );

        spriteBatch.Draw(
            _mountainBackTexture.Value,
            new Rectangle(frameOffsetX + (int)(averageMousePos.X * _mountainBackParallaxFactor * _parallaxScale),
                frameOffsetY + (int)(averageMousePos.Y * _mountainBackParallaxFactor * _parallaxScale),
                frameWidth,
                frameHeight),
            Color.White * _transparency
        );
        
        spriteBatch.Draw(
            _mountainFrontTexture.Value,
            new Rectangle(frameOffsetX + (int)(averageMousePos.X * _mountainFrontParallaxFactor * _parallaxScale),
                frameOffsetY + (int)(averageMousePos.Y * _mountainFrontParallaxFactor * _parallaxScale),
                frameWidth,
                frameHeight),
            Color.White * _transparency
        );

        spriteBatch.Draw(
            _forestBackTextures[(int)Math.Round(_stopwatch.ElapsedMilliseconds / _frontFrameInterval) % 6].Value,
            new Rectangle(frameOffsetX + (int)(averageMousePos.X * _forestBackParallaxFactor * _parallaxScale),
                frameOffsetY + (int)(averageMousePos.Y * _forestBackParallaxFactor * _parallaxScale),
                frameWidth,
                frameHeight),
            Color.White * _transparency
        );

        spriteBatch.Draw(
            _forestFrontTexture.Value,
            new Rectangle(frameOffsetX + (int)(averageMousePos.X * _forestFrontParallaxFactor * _parallaxScale),
                frameOffsetY + (int)(averageMousePos.Y * _forestFrontParallaxFactor * _parallaxScale),
                frameWidth,
                frameHeight),
            Color.White * _transparency
        );

        return true;
    }

    public override void ModifyFarFades(float[] fades, float transitionSpeed) {
        // Console.WriteLine(transitionSpeed);
        
        for (int i = 0; i < fades.Length; i++) {
            if (i == Slot) {
                fades[i] = 1;
            }
            else {
                fades[i] = 0;
            }
        }
    }
}