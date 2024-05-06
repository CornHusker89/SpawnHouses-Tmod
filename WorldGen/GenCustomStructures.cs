using System;
using Terraria;
using Terraria.ModLoader;
using Terraria.ID;
using Terraria.GameContent.Generation;
using System.Collections.Generic;
using Terraria.WorldBuilding;
using Terraria.IO;
using Terraria.Localization;
using Terraria.DataStructures;
using Microsoft.Xna.Framework;
using SpawnHouses.Structures;

namespace SpawnHouses.WorldGen;

public static class GenCustomStructures
{
    public static void GenerateMainHouse()
    {
        ushort initialX = 1;
        ushort initialY = 1;
        ushort counts = 500;
        // do while the tile is not grass
        while (!Main.tile[initialX, initialY].HasTile || Main.tile[initialX, initialY].TileType != TileID.Grass)
        {
            counts++;
            initialX = Convert.ToUInt16( Terraria.WorldGen.genRand.Next(Main.spawnTileX - (counts / 10), Main.spawnTileX + (counts / 10)) );
            initialY = Convert.ToUInt16(Main.spawnTileY - 80);
            while (initialY < Main.worldSurface) {
                if (Terraria.WorldGen.SolidTile(initialX, initialY)) {
                    break;
                }
                initialY++;
            }
        }
			
        int sum = 0;
        for (int i = -3; i <= 3; i++)
        {
            int x = (i * 10) + (initialX);
            int y = Main.spawnTileY - 80;
				
            while (!Terraria.WorldGen.SolidTile(x, y))
            {
                y++;
            }

            sum += y;
        }

        // set initialY to the average y pos of the raycasts
        initialY = (ushort) Math.Round(sum / 7.0);
			
        MainHouseStructure houseStructure = new MainHouseStructure(Convert.ToUInt16(initialX - 31), Convert.ToUInt16(initialY - 27));
    }
}