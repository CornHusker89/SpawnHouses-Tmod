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
using SpawnHouses.Structures.Structures;


namespace SpawnHouses.WorldGen
{
    public class CustomStructureGen : ModSystem
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
            
            if (sunflowersIndex != -1) 
                // 6. We register our world generation pass by passing in an instance of our custom GenPass class below. The GenPass class will execute our world generation code.
                tasks.Insert(sunflowersIndex + 1, new CustomHousesPass("Generate Custom Houses Pass", 100f));
            else
            {
	            tasks.Insert(tasks.Count - 2, new CustomHousesPass("Generate Custom Houses Pass", 100f));
            }


            tasks.Insert(tasks.Count - 2, item: new CustomBeachHousePass("Custom Beach House Pass", 100f));
        }
    }
    
	// 7. Make sure to inherit from the GenPass class.
	public class CustomHousesPass : GenPass
	{
		public CustomHousesPass(string name, float loadWeight) : base(name, loadWeight) {
		}

		// 8. The ApplyPass method is where the actual world generation code is placed.
		protected override void ApplyPass(GenerationProgress progress, GameConfiguration configuration) {
			
			// 9. Finally, we do the actual world generation code.

			if (ModContent.GetInstance<SpawnHousesConfig>().EnableSpawnPointHouse)
			{
				ushort initialX = 1;
				ushort initialY = 1;
				ushort counts = 500;

				short yModifier = 0; //used when the generation is much taller than we thought

				while (Terraria.WorldGen.SolidTile(
					    (Terraria.WorldGen.genRand.Next(
						    Main.spawnTileX - (counts / 10), Main.spawnTileX + (counts / 10))),
					    (ushort)(Main.worldSurface * 2 / 3 - yModifier)))
				{
					yModifier -= 85;
				}

				while ((!Main.tile[initialX, initialY].HasTile || Main.tile[initialX, initialY].TileType != TileID.Grass) && counts < 700)
				{
					counts++;
					initialX = (ushort)( Terraria.WorldGen.genRand.Next(Main.spawnTileX - (counts / 10), Main.spawnTileX + (counts / 10)) );
					initialY = (ushort)(Main.worldSurface * 2 / 3 - yModifier);
					while (initialY < Main.worldSurface + 20)
					{
						if (Terraria.WorldGen.SolidTile(initialX, initialY)) {
							break;
						}
						initialY++;
					}
				}
				
				// just in case something above got fucked up
				if (!Main.tile[initialX, initialY].HasTile) return;
			
				int sum = 0;
				for (int i = -3; i <= 3; i++)
				{
					int x = (i * 10) + (initialX);
					int y = (ushort)(Main.worldSurface * 2 / 3 - yModifier);
				
					
					while (!Terraria.WorldGen.SolidTile(x, y))
					{
						y++;
					}

					sum += y;
				}

				// set initialY to the average y pos of the raycasts
				initialY = (ushort) Math.Round(sum / 7.0);
			
				MainHouseStructure houseStructure = new MainHouseStructure(Convert.ToUInt16(initialX - 31), Convert.ToUInt16(initialY - 24));
				houseStructure.Generate();
			}
			
		}
	}
	
	public class CustomBeachHousePass : GenPass
	{
		public CustomBeachHousePass(string name, float loadWeight) : base(name, loadWeight) {
		}

		// 8. The ApplyPass method is where the actual world generation code is placed.
		protected override void ApplyPass(GenerationProgress progress, GameConfiguration configuration) {
			
			// 9. Finally, we do the actual world generation code.

			if (ModContent.GetInstance<SpawnHousesConfig>().EnableBeachHouse)
			{
				ushort tileX = 0, tileY = 0;
				short yModifier = 0;

				bool FindLeft(bool reverse = false, bool force = false) {
					ushort x, y;
					if (!reverse)
						x = 70;	
					else
						x = (ushort)(Main.maxTilesX - 70);
					
					while (true) {
						if (!reverse)
							x++;
						else
							x--;

						y = (ushort)(Main.worldSurface * 2 / 5 - yModifier);
						if (Terraria.WorldGen.SolidTile(x, y))
							yModifier -= 70;
							
						while (true) {
							y++;
							
							if (Main.tile[x, y].HasTile)
							{
								if ((Main.tile[x, y].TileType == TileID.Sand || Main.tile[x, y].TileType == TileID.ShellPile) || force)
								{
									tileX = x;
									tileY = y;
									return true;
								}
								return false;
							}
							
							if (Main.tile[x, y].LiquidAmount != 0) 
								break;
						}
					}
				}

				bool FindRight(bool force = false) {
					return FindLeft(true, force);
				}

				bool leftSide = Terraria.WorldGen.genRand.Next(0, 2) == 0;
				if (leftSide)
				{
					if (!FindLeft())
					{
						leftSide = false;
						FindRight(force: true);
					}
				}
				else
				{
					if (!FindRight())
					{
						leftSide = true;
						FindLeft(force: true);
					}
				}

				if (tileX != 0 && tileY != 0)
				{
					BeachHouseStructure houseStructure = leftSide ? 
						new BeachHouseStructure(Convert.ToUInt16(tileX - 9), Convert.ToUInt16(tileY - 32)) : 
						new BeachHouseStructure(Convert.ToUInt16(tileX - 23), Convert.ToUInt16(tileY - 32), true);
					houseStructure.Generate();
					
					// firepit generation
					if (Terraria.WorldGen.genRand.Next(0, 3) != 0) // 2/3 chance
					{
						bool foundLocation = false;
						ushort x;

						if (leftSide)
							x = (ushort)(tileX - 9 + 35 + Terraria.WorldGen.genRand.Next(8, 12));
						else
							x = (ushort)(tileX - 23 - Terraria.WorldGen.genRand.Next(8, 12));
					
						ushort y = 10;
						while (!foundLocation)
						{
							y = tileY;
							while (y < Main.worldSurface) {
								if (Terraria.WorldGen.SolidTile(x, y)) {
									break;
								}
								y++;
							}
							foundLocation = true;
						}

						y = (ushort)(y - 2);
						x = (ushort)(x - 3);
			
						FirepitStructure structure = new FirepitStructure(x, y);
						structure.Generate();
					}
				}
			}
		}
	}
}