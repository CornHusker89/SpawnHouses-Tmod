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
    
    private Asset<Texture2D> _backgroundTexture;
    private Asset<Texture2D>[] _foregroundTextures;

    private double _frameInterval = 1000.0 / 9;

    public override void Load()
    {        
        _stopwatch = Stopwatch.StartNew();
                
        _backgroundTexture = ModContent.Request<Texture2D>("SpawnHouses/Assets/Menu/back0050");
        _foregroundTextures =
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
        _backgroundTexture.Dispose();
        foreach (var texture in _foregroundTextures)
            texture.Dispose();
        _backgroundTexture = null;
        _foregroundTextures = null;
    }

    public override bool PreDrawCloseBackground(SpriteBatch spriteBatch)
    {
        spriteBatch.Draw(
            _backgroundTexture.Value,
            new Rectangle(0, 0, Main.screenWidth, Main.screenHeight),
            Color.White
        );
        
        spriteBatch.Draw(
            _foregroundTextures[(int)Math.Round(_stopwatch.ElapsedMilliseconds / _frameInterval) % 6].Value,
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