using System;
using System.Diagnostics;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ReLogic.Content;
using Terraria;
using Terraria.ModLoader;

namespace SpawnHouses;

public class SpawnHousesModMenu : ModMenu {
    public override bool IsAvailable => true;
    public override string DisplayName => "Nighttime Forest (Generated Housing)";
    public override ModSurfaceBackgroundStyle MenuBackgroundStyle => ModContent.GetInstance<CustomMenuBackgroundStyle>();

    public override bool PreDrawLogo(SpriteBatch spriteBatch, ref Vector2 logoDrawCenter, ref float logoRotation, ref float logoScale, ref Color drawColor) {
        logoScale = 0.85f;
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


    private const float SkyDetailFrameInterval = 1000f / 6;
    private const float FrontFrameInterval = 1000f / 9;

    private readonly float ParalaxScale = -8;
    private readonly float ForestFrontParallaxFactor = 1.1f;
    private readonly float ForestBackParallaxFactor = 0.7f;
    private readonly float MountainFrontParallaxFactor = 0.6f;
    private readonly float MountainBackParallaxFactor = 0.32f;
    private readonly float CloudsParallaxFactor = 0.2f;

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
        Vector2 mousePos = (Main.MouseScreen - Main.ScreenSize.ToVector2() / 0.65f) / 100f;
        int frameOffsetX = (int)(Main.screenWidth * -0.1f);
        int frameOffsetY = (int)(Main.screenHeight * -0.1f);
        int frameWidth = (int)(Main.screenWidth * 1.1f);
        int frameHeight = (int)(Main.screenHeight * 1.1f);
        
        spriteBatch.Draw(
            _skyTexture.Value,
            new Rectangle(0, 0, Main.screenWidth, Main.screenHeight),
            Color.White
        );

        spriteBatch.Draw(
            _cloudsTextures[(int)Math.Round(_stopwatch.ElapsedMilliseconds / SkyDetailFrameInterval) % 55].Value,
            new Rectangle(frameOffsetX + (int)(mousePos.X * CloudsParallaxFactor * ParalaxScale), frameOffsetY + (int)(mousePos.Y * CloudsParallaxFactor * ParalaxScale), frameWidth, frameHeight),
            Color.White
        );

        spriteBatch.Draw(
            _mountainBackTexture.Value,
            new Rectangle(frameOffsetX + (int)(mousePos.X * MountainBackParallaxFactor * ParalaxScale), frameOffsetY + (int)(mousePos.Y * MountainBackParallaxFactor * ParalaxScale), frameWidth, frameHeight),
            Color.White
        );
        
        spriteBatch.Draw(
            _mountainFrontTexture.Value,
            new Rectangle(frameOffsetX + (int)(mousePos.X * MountainFrontParallaxFactor * ParalaxScale), frameOffsetY + (int)(mousePos.Y * MountainFrontParallaxFactor * ParalaxScale), frameWidth, frameHeight),
            Color.White
        );

        spriteBatch.Draw(
            _forestBackTextures[(int)Math.Round(_stopwatch.ElapsedMilliseconds / FrontFrameInterval) % 6].Value,
            new Rectangle(frameOffsetX + (int)(mousePos.X * ForestBackParallaxFactor * ParalaxScale), frameOffsetY + (int)(mousePos.Y * ForestBackParallaxFactor * ParalaxScale), frameWidth, frameHeight),
            Color.White
        );

        spriteBatch.Draw(
            _forestFrontTexture.Value,
            new Rectangle(frameOffsetX + (int)(mousePos.X * ForestFrontParallaxFactor * ParalaxScale), frameOffsetY + (int)(mousePos.Y * ForestFrontParallaxFactor * ParalaxScale), frameWidth, frameHeight),
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