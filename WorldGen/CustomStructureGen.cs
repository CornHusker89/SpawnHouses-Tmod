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
using SpawnHouses.Structures.StructureChains;
using SpawnHouses.Structures.Structures;
using Terraria.ModLoader.IO;


namespace SpawnHouses.WorldGen;

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
            tasks.Insert(tasks.Count - 8, new CustomHousesPass("Generate Custom Houses Pass", 100f));
        }
        
        int iceIndex = tasks.FindIndex(genpass => genpass.Name.Equals("Ice"));
        if (iceIndex != -1) 
	        // 6. We register our world generation pass by passing in an instance of our custom GenPass class below. The GenPass class will execute our world generation code.
	        tasks.Insert(iceIndex + 1, new ClearSpawnPointPass("Generate Custom Houses Pass", 100f));
        else
        {
	        tasks.Insert(tasks.Count - 40, new ClearSpawnPointPass("Generate Custom Houses Pass", 100f));
        }


        tasks.Insert(tasks.Count - 2, item: new CustomBeachHousePass("Custom Beach House Pass", 100f));
    }

    public override void PostWorldGen()
    {
	    // move guide into the main house (if it's there)
	    if (ModContent.GetInstance<SpawnHousesConfig>().EnableSpawnPointHouse)
	    {
		    foreach (var npc in Main.npc)
		    {
			    if (npc.type == NPCID.Guide)
			    {
				    npc.position.X = (SpawnHousesSystem.MainHouse.X + SpawnHousesSystem.MainHouse.LeftSize - 1 + Terraria.WorldGen.genRand.Next(-8, 9)) * 16; // tiles to pixels
				    npc.position.Y = (SpawnHousesSystem.MainHouse.Y + 13) * 16;
			    }
			    if (npc.type == 688) // magic storage's automation
			    {
				    npc.position.X = (SpawnHousesSystem.MainHouse.X + SpawnHousesSystem.MainHouse.LeftSize - 1 + Terraria.WorldGen.genRand.Next(-8, 9)) * 16; // tiles to pixels
				    npc.position.Y = (SpawnHousesSystem.MainHouse.Y + 13) * 16;

			    }
		    }
	    }
	    
	    if (ModContent.GetInstance<SpawnHousesConfig>().EnableSpawnPointBasement)
	    {
		    if (ModContent.GetInstance<SpawnHousesConfig>().EnableSpawnPointHouse)
		    {
			    MainBasementChain chain = new MainBasementChain((ushort)SpawnHousesSystem.MainHouse.BasementEntryPos.X, (ushort)SpawnHousesSystem.MainHouse.BasementEntryPos.Y);
			    chain.Generate();
				
			    SpawnHousesSystem.MainBasement = chain;
		    }
		    else
		    {
			    MainBasementChain chain = new MainBasementChain((ushort)Main.spawnTileX, (ushort)Main.spawnTileY);
			    chain.Generate();
				
			    SpawnHousesSystem.MainBasement = chain;
		    }
	    }
    }
}


public class ClearSpawnPointPass : GenPass
{
	public ClearSpawnPointPass(string name, float loadWeight) : base(name, loadWeight) {
	}

	// 8. The ApplyPass method is where the actual world generation code is placed.
	protected override void ApplyPass(GenerationProgress progress, GameConfiguration configuration) {
		
		// 9. Finally, we do the actual world generation code.

		if (ModContent.GetInstance<SpawnHousesConfig>().EnableSpawnPointBasement)
		{
			int x = Main.maxTilesX / 2;
			int y = (int)(Main.worldSurface / 2);
				
			//make sure we're not under the surface
			while (!Is40AboveTilesClear(x, y))
				y -= 30;

			bool Is40AboveTilesClear(int startX, int startY)
			{
				for (byte i = 1; i < 41; i++)
					if (Terraria.WorldGen.SolidTile(startX, startY - i))
						return false;
				
				return true;
			}
			
			// move down to the surface
			while (y < Main.worldSurface + 50)
			{
				if (Terraria.WorldGen.SolidTile(x, y))
					break;
				
				y++;
			}
			GenVars.structures.AddProtectedStructure(new Rectangle(x - 50, y - 50, 100, 100));
		}
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
			bool spawnUnderworld = Main.ActiveWorldFileData.SeedText.ToLower() == "dont dig up" || Main.ActiveWorldFileData.SeedText.ToLower() == "get fixed boi";
			
			int initialX = 1;
			int initialY = 1;
			
			bool foundValidSpot = false;

			for (ushort counts = 0; counts < 400; counts++)
			{
				int xVal = Terraria.WorldGen.genRand.Next(Main.spawnTileX - (counts / 8), Main.spawnTileX + (counts / 8));
				if (!spawnUnderworld)
					initialY = (int)(Main.worldSurface / 2);
				else
					initialY = Main.spawnTileY - 15;
					
				//make sure we're not under the surface
				while (!Is40AboveTilesClear(xVal, initialY) && !spawnUnderworld)
					initialY -= 30;

				bool Is40AboveTilesClear(int x, int y)
				{
					for (byte i = 1; i < 41; i++)
						if (Terraria.WorldGen.SolidTile(x, y - i))
							return false;
					
					return true;
				}
				
				initialX = xVal;
				
				// move down to the surface
				while (initialY < Main.worldSurface + 50)
				{
					if (Terraria.WorldGen.SolidTile(initialX, initialY))
						break;
					
					initialY++;
				}

				// if we found a good spot, break search loop
				if (!spawnUnderworld)
				{
					if (Terraria.WorldGen.SolidTile(initialX, initialY) && Main.tile[initialX, initialY].TileType == TileID.Grass)
					{
						foundValidSpot = true;
						break;
					}
				}
				else
				{
					if (initialY >= Main.spawnTileY - 50)
					{
						foundValidSpot = true;
						break;
					}
				}
			}
			
			// just in case something above got fucked up
			if (!foundValidSpot)
			{
				ModContent.GetInstance<SpawnHouses>().Logger.Error("Failed to generate SpawnPointHouse. Please report this world seed to the mod's author");
				return;
			}
			
			int sum = 0;
			for (int i = -3; i <= 3; i++)
			{
				int x = i * 10 + initialX;
				int y;

				if (!spawnUnderworld)
					y = initialY;
				else
					y = initialY - 14;
			
				
				while (!Terraria.WorldGen.SolidTile(x, y))
					y++;
				
				sum += y;
			}

			// set initialY to the average y pos of the raycasts
			initialY = (int) Math.Round(sum / 7.0);
			
			MainHouseStructure houseStructure = new MainHouseStructure((ushort)(initialX - 31), (ushort)(initialY - 16), hasBasement: ModContent.GetInstance<SpawnHousesConfig>().EnableSpawnPointBasement, inUnderworld: spawnUnderworld);
			houseStructure.Generate();
			
			SpawnHousesSystem.MainHouse = houseStructure;
			
			
			// move the spawn point to the upper floor of the house
			Main.spawnTileX = initialX + houseStructure.LeftSize - 1 - 31;
			Main.spawnTileY = initialY + 5 - 15;
		
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
				BeachHouseStructure beachHouseStructure = leftSide ? 
					new BeachHouseStructure((ushort)(tileX - 9), (ushort)(tileY - 32)) : 
					new BeachHouseStructure((ushort)(tileX - 23), (ushort)(tileY - 32), reverse: true);
				beachHouseStructure.Generate();
				
				SpawnHousesSystem.BeachHouse = beachHouseStructure;
				
				// firepit generation
				if (Terraria.WorldGen.genRand.Next(0, 2) == 0) // 1/2 chance
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
