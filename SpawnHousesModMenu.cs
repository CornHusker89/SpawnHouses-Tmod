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
    private readonly Vector2[] _mousePosHistory = new Vector2[11];
    private int _mousePosHistoryIndex;

    private Asset<Texture2D> _forestFrontTexture;
    private Asset<Texture2D>[] _forestBackTextures;
    private Asset<Texture2D> _mountainFrontTexture;
    private Asset<Texture2D> _mountainBackTexture;
    private readonly Asset<Texture2D>[] _cloudsTextures = new Asset<Texture2D>[55];
    private Asset<Texture2D> _skyTexture;

    private readonly float _skyDetailFrameInterval = 1000f / 5;
    private readonly float _frontFrameInterval = 1000f / 7.5f;
    private readonly float _parallaxScale = -4.1f;

    private float[] _layerParallaxFactor;
    private float[] _layerFrameZoom;

    private Asset<Texture2D>[] GetLayerTextures() => [
        _skyTexture,
        _cloudsTextures[(int)Math.Round(_stopwatch.ElapsedMilliseconds / _skyDetailFrameInterval) % 55],
        _mountainBackTexture,
        _mountainFrontTexture,
        _forestBackTextures[(int)Math.Round(_stopwatch.ElapsedMilliseconds / _frontFrameInterval) % 6],
        _forestFrontTexture
    ];

    public override void Load() {
        _layerParallaxFactor = [
            0f * _parallaxScale, // sky
            0.24f * _parallaxScale, // clouds
            0.43f * _parallaxScale, // mountain back
            0.82f * _parallaxScale, // mountain front
            0.82f * _parallaxScale, // forest back
            1.65f * _parallaxScale // forest front
        ];

        _layerFrameZoom = [
            1.0f, // sky
            1.035f, // clouds
            1.08f, // mountain back
            1.1f, // mountain front
            1.1f, // forest back
            1.1f // forest front
        ];
        
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
        float mousePosX = float.Min(float.Max(Main.MouseScreen.X, 0), Main.LastLoadedResolution.X) - Main.LastLoadedResolution.X;
        float mousePosY = float.Min(float.Max(Main.MouseScreen.Y, 0), Main.LastLoadedResolution.Y) - Main.LastLoadedResolution.Y;
        Vector2 thisMousePos = new Vector2(mousePosX, mousePosY) / 2f / 100f;
        _mousePosHistory[_mousePosHistoryIndex] = thisMousePos;
        float mousePosXRunningTotal = 0, mousePosYRunningTotal = 0;
        foreach (Vector2 pos in _mousePosHistory) {
            mousePosXRunningTotal += pos.X;
            mousePosYRunningTotal += pos.Y;
        }

        mousePosXRunningTotal /= _mousePosHistory.Length;
        mousePosYRunningTotal /= _mousePosHistory.Length;
        Vector2 averageMousePos = new(mousePosXRunningTotal, mousePosYRunningTotal);


        float transparency = (float)Math.Floor(Main.bgAlphaFrontLayer[Slot]);
        var layerTextures = GetLayerTextures();

        for (int layerIndex = 0; layerIndex < _layerParallaxFactor.Length; layerIndex++) {
            int frameOffsetX = (int)(Main.screenWidth * (1 - _layerFrameZoom[layerIndex]) / 2);
            int frameOffsetY = (int)(Main.screenHeight * (1 - _layerFrameZoom[layerIndex]) / 2);
            int frameWidth = (int)(Main.screenWidth * _layerFrameZoom[layerIndex]);
            int frameHeight = (int)(Main.screenHeight * _layerFrameZoom[layerIndex]);

            spriteBatch.Draw(
                layerTextures[layerIndex].Value,
                new Rectangle(frameOffsetX + (int)(averageMousePos.X * _layerParallaxFactor[layerIndex]),
                    frameOffsetY + (int)(averageMousePos.Y * _layerParallaxFactor[layerIndex]),
                    frameWidth,
                    frameHeight),
                Color.White * transparency * (layerIndex == 1 ? 0.85f : 1f)
            );
        }
        return true;
    }

    public override void ModifyFarFades(float[] fades, float transitionSpeed) {
        // Console.WriteLine(transitionSpeed);
        
        for (int i = 0; i < fades.Length; i++) {
            if (i == Slot) {
                // Console.WriteLine(fades[i]);
                fades[i] += transitionSpeed * 0.1f;
            }
            else {
                fades[i] -= transitionSpeed * 0.1f;
            }
        }
    }
}