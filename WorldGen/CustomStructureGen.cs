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
				bool spawnUnderworld = Main.ActiveWorldFileData.SeedText.ToLower() == "dont dig up" || Main.ActiveWorldFileData.ForTheWorthy;
				
				int initialX = 1;
				int initialY = 1;
				ushort counts = 500;

				short yModifier = 0; //used when the generation is much taller than we thought

				while (Terraria.WorldGen.SolidTile(
					       (Terraria.WorldGen.genRand.Next(Main.spawnTileX - (counts / 10), Main.spawnTileX + (counts / 10))),
					       (int)(Main.worldSurface * 2 / 3 - yModifier)) && !spawnUnderworld)
					yModifier -= 85;

				while ((!Main.tile[initialX, initialY].HasTile || Main.tile[initialX, initialY].TileType != TileID.Grass) && counts < 700)
				{
					counts++;
					initialX = Terraria.WorldGen.genRand.Next(Main.spawnTileX - (counts / 10), Main.spawnTileX + (counts / 10));

					if (!spawnUnderworld)
						initialY = (int)(Main.worldSurface * 2 / 3 - yModifier);
					else
						initialY = Main.spawnTileY - 15;
					
					while (initialY < Main.worldSurface + 20)
					{
						if (Terraria.WorldGen.SolidTile(initialX, initialY))
							break;
						
						initialY++;
					}
				}
				
				// just in case something above got fucked up
				if (!Main.tile[initialX, initialY].HasTile && !spawnUnderworld) return;
			
				int sum = 0;
				for (int i = -3; i <= 3; i++)
				{
					int x = i * 10 + initialX;
					int y;

					if (!spawnUnderworld)
						y = (int)(Main.worldSurface * 2 / 3 - yModifier);
					else
						y = initialY - 14;
				
					
					while (!Terraria.WorldGen.SolidTile(x, y))
						y++;
					
					sum += y;
				}

				// set initialY to the average y pos of the raycasts
				initialY = (int) Math.Round(sum / 7.0);
				
				if (ModContent.GetInstance<SpawnHousesConfig>().EnableSpawnPointBasement)
				{
					MainHouseBStructure houseStructure = new MainHouseBStructure(Convert.ToUInt16(initialX - 31), Convert.ToUInt16(initialY - 24));
					houseStructure.Generate();
				
					StructureChain.MainBasementChain chain = new StructureChain.MainBasementChain(new Point16(initialX - 31 + 42, initialY - 24 + 35));
				}
				else
				{
					MainHouseStructure houseStructure = new MainHouseStructure(Convert.ToUInt16(initialX - 31), Convert.ToUInt16(initialY - 24));
					houseStructure.Generate();
				}
			
				// replace all dirt with ash if we're in the underworld
				if (spawnUnderworld)
					WorldUtils.Gen(new Point(initialX, initialY), new Shapes.Circle(150, 100), Actions.Chain(
						new Actions.Custom((i, j, args) =>
						{
							if (Main.tile[i, j].HasTile && Main.tile[i, j].TileType == TileID.Dirt)
							{
								Tile tile = Main.tile[i, j];
								tile.TileType = TileID.Ash;
							}
							if (Main.tile[i, j].HasTile && Main.tile[i, j].TileType == TileID.Grass)
							{
								Tile tile = Main.tile[i, j];
								tile.TileType = TileID.AshGrass;
							}
							return true;
						})
					));
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

				bool dungeonIsLeftSide = Main.dungeonX < Main.maxTilesX / 2;

				bool spawnDungeonSide = Terraria.WorldGen.genRand.Next(0, 4) == 0; // 1 out of 4

				// initally set it to the same side, then swap it if we aren't spawning on the DungeonSide
				bool leftSide = dungeonIsLeftSide;
				if (!spawnDungeonSide)
					leftSide = !leftSide;
				
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