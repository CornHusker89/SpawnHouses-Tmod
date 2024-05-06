
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

namespace SpawnHouses.WorldGen
{
    public class CustomHouseGen : ModSystem
    {
        // 3. These lines setup the localization for the message shown during world generation. Update your localization files after building and reloading the mod to provide values for this.
        public static LocalizedText WorldGenCustomHousesPassMessage { get; private set; }

        public override void SetStaticDefaults() {
	        WorldGenCustomHousesPassMessage = Language.GetOrRegister(Mod.GetLocalizationKey($"WorldGen.{nameof(WorldGenCustomHousesPassMessage)}"));
        }
        
        // 4. We use the ModifyWorldGenTasks method to tell the game the order that our world generation code should run
        public override void ModifyWorldGenTasks(List<GenPass> tasks, ref double totalWeight) {
            // 5. We use FindIndex to locate the index of the vanilla world generation task called "Sunflowers". This ensures our code runs at the correct step.
            int sunflowersIndex = tasks.FindIndex(genpass => genpass.Name.Equals("Sunflowers"));
            if (sunflowersIndex != -1) {
                // 6. We register our world generation pass by passing in an instance of our custom GenPass class below. The GenPass class will execute our world generation code.
                tasks.Insert(sunflowersIndex + 1, new WorldGenCustomHousesPass("World Gen Custom Houses", 100f));
            }
        }
    }
    
	// 7. Make sure to inherit from the GenPass class.
	public class WorldGenCustomHousesPass : GenPass
	{
		public WorldGenCustomHousesPass(string name, float loadWeight) : base(name, loadWeight) {
		}

		// 8. The ApplyPass method is where the actual world generation code is placed.
		protected override void ApplyPass(GenerationProgress progress, GameConfiguration configuration) {
			
			// 9. Finally, we do the actual world generation code.
			
			
			int initialX = 1;
			int initialY = 1;
			ushort counts = 1000;
			// do while the tile is not grass
			while (!Main.tile[initialX, initialY].HasTile || Main.tile[initialX, initialY].TileType != TileID.Grass)
			{
				counts++;
				initialX = Terraria.WorldGen.genRand.Next(Main.spawnTileX - (counts / 10), Main.spawnTileX + (counts / 10));
				initialY = Main.spawnTileY - 80;
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
			initialY = (int) Math.Round(sum / 7.0);
			
			MainHouseStructure houseStructure = new MainHouseStructure(initialX - 31, initialY - 27);
		}
	}
}