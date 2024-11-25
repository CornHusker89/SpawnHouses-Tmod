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
    public override bool IsAvailable => false;
    
    public override string DisplayName => "Generated Housing";
    //public override int Music => MusicLoader.GetMusicSlot(Mod, "Sounds/Music/YourMenuTheme"); // Optional custom music

    public override ModSurfaceBackgroundStyle MenuBackgroundStyle => ModContent.GetInstance<CustomMenuBackgroundStyle>();
    
    public void test()
    {
        // EntryPoint=CustomMenu in build.txt?
    }
}


public class CustomMenuBackgroundStyle : ModSurfaceBackgroundStyle
{
    private Stopwatch _stopwatch;
    
    private Asset<Texture2D> _backgroundTexture;
    private Asset<Texture2D>[] _foregroundTextures;

    private double _frameInterval = 1000.0 / 7;
    private int _totalFrameNum;

    public override void Load()
    {        
        _stopwatch = Stopwatch.StartNew();
        
        _backgroundTexture = ModContent.Request<Texture2D>("SpawnHouses/Assets/Menu/back0001");

        _foregroundTextures =
        [
            ModContent.Request<Texture2D>("SpawnHouses/Assets/Menu/front0001"),
            ModContent.Request<Texture2D>("SpawnHouses/Assets/Menu/front0002"),
            ModContent.Request<Texture2D>("SpawnHouses/Assets/Menu/front0003"),
            ModContent.Request<Texture2D>("SpawnHouses/Assets/Menu/front0004"),
            ModContent.Request<Texture2D>("SpawnHouses/Assets/Menu/front0005"),
            ModContent.Request<Texture2D>("SpawnHouses/Assets/Menu/front0006")
        ];
    }

    public override bool PreDrawCloseBackground(SpriteBatch spriteBatch)
    {
        spriteBatch.Draw(
            _backgroundTexture.Value,
            new Rectangle(0, 0, Main.screenWidth, Main.screenHeight),
            Color.White
        );
        
        if (Math.Round(_stopwatch.Elapsed.Milliseconds / _frameInterval) >= 1)
        {
            _stopwatch.Restart();
            _totalFrameNum++;
        }
        
        spriteBatch.Draw(
            _foregroundTextures[_totalFrameNum % 6].Value,
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