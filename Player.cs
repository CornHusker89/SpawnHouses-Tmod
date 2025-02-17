using System;
using Microsoft.Xna.Framework;
using SpawnHouses.Structures;
using SpawnHouses.Structures.Structures;
using Terraria;
using Terraria.ModLoader;

namespace SpawnHouses;

public class Player : ModPlayer
{
    public override void OnEnterWorld()
    {
        if (ModHelper.ErrorLoadingMS)
            Main.NewText("Generated Houses had an issue loading Magic Storage content, so Magic Storage features in Generated Houses are disabled. Please contact the author about this issue!", Color.Red);
    }


    private int _frameCounter = 0;
    
    public override void PostUpdate()
    {
        _frameCounter++;
        if (_frameCounter >= 16)
        {
            _frameCounter = 0;
            int x = (int)Player.Center.X / 16;
            int y = (int)Player.Center.Y / 16;
            
            if (SpawnHousesSystem.MainBasement is not null && SpawnHousesSystem.MainBasement.Status == StructureStatus.GeneratedButNotFound)
            {
                if (
                    x > SpawnHousesSystem.MainBasement.EntryPosX - 7
                    && x < SpawnHousesSystem.MainBasement.EntryPosX + 7
                    && y > SpawnHousesSystem.MainBasement.EntryPosY + 6
                    && y < SpawnHousesSystem.MainBasement.EntryPosY + 20
                )
                {
                    SpawnHousesSystem.MainBasement.OnFound();
                }
            }

            if (SpawnHousesSystem.BeachHouse is not null && SpawnHousesSystem.BeachHouse.Status == StructureStatus.GeneratedButNotFound)
            {
                int houseCenterX = SpawnHousesSystem.BeachHouse.X + BeachHouse._structureXSize / 2;
                int houseCenterY = SpawnHousesSystem.BeachHouse.Y + BeachHouse._structureYSize / 2;

                if (
                    x > houseCenterX - 70
                    && x < houseCenterX + 70
                    && y > houseCenterY - 44
                    && y < houseCenterY + 44
                )
                {
                    SpawnHousesSystem.BeachHouse.OnFound();
                }
            }
        }
    }
}