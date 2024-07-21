using System;
using Microsoft.Xna.Framework;
using SpawnHouses.Structures;
using SpawnHouses.Structures.Structures;
using Terraria.ModLoader;

namespace SpawnHouses;

public class SpawnHousesPlayer : ModPlayer
{
    private int frameCounter = 0;
    
    public override void PostUpdate()
    {
        frameCounter++;
        if (frameCounter >= 20)
        {
            frameCounter = 0;
            int x = (int)Player.Center.X / 16;
            int y = (int)Player.Center.Y / 16;
            
            if (SpawnHousesSystem.MainBasement.Status == StructureStatus.GeneratedButNotFound)
            {
                if (
                    x > SpawnHousesSystem.MainBasement.EntryPosX - 5
                    && x < SpawnHousesSystem.MainBasement.EntryPosX + 5
                    && y > SpawnHousesSystem.MainBasement.EntryPosY + 6
                    && y < SpawnHousesSystem.MainBasement.EntryPosY + 17
                )
                {
                    SpawnHousesSystem.MainBasement.OnFound();
                }
            }

            if (SpawnHousesSystem.BeachHouse.Status == StructureStatus.GeneratedButNotFound)
            {
                int houseCenterX = SpawnHousesSystem.BeachHouse.X + BeachHouseStructure._structureXSize / 2;
                int houseCenterY = SpawnHousesSystem.BeachHouse.Y + BeachHouseStructure._structureYSize / 2;

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