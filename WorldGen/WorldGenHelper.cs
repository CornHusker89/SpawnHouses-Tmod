using System;
using Microsoft.Xna.Framework;
using SpawnHouses.Structures;
using SpawnHouses.Structures.Chains;
using SpawnHouses.Structures.Structures;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.WorldBuilding;
using BoundingBox = SpawnHouses.Structures.StructureParts.BoundingBox;

namespace SpawnHouses.WorldGen;

public static class WorldGenHelper
{
    public static void GenerateMainHouse()
    {
		bool spawnUnderworld = Main.ActiveWorldFileData.SeedText.ToLower() == "dont dig up" || Main.ActiveWorldFileData.SeedText.ToLower() == "get fixed boi";
		
		int initialX = 1;
		int initialY = 1;
		
		bool foundValidSpot = false;

		for (ushort counts = 0; counts < 400; counts++)
		{
			int xVal = Terraria.WorldGen.genRand.Next(Main.spawnTileX - (counts / 8), Main.spawnTileX + (counts / 8));
			
			// Move out of the way of the big spawn tree
			if (ModHelper.IsRemnantsEnabled)
				xVal -= 70;
			
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
			ModContent.GetInstance<SpawnHouses>().Logger.Error("Failed to generate SpawnPointHouse. Please report this world seed and your client.log to the mod's author");
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
		MainHouse house = null;
		try
		{
			house = new MainHouse((ushort)(initialX - 31), (ushort)(initialY - 16), hasBasement: ModContent.GetInstance<SpawnHousesConfig>().EnableSpawnPointBasement, inUnderworld: spawnUnderworld);
			house.Generate();
			SpawnHousesSystem.MainHouse = house;
		}
		catch (Exception e)
		{
			ModContent.GetInstance<SpawnHouses>().Logger.Error($"Main house failed to generate:\n{e}");
			return;
		}
		
		// move the spawn point to the upper floor of the house
		Main.spawnTileX = initialX + house.LeftSize - 1 - 31;
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
	
	
	public static void GenerateMineshaft()
	{
		int x, y;
		try
		{
			bool FindValidLocation(bool left = true)
			{
				if (left)
					x = Main.spawnTileX - Terraria.WorldGen.genRand.Next(18, 34) - 35;
				else
					x = Main.spawnTileX + Terraria.WorldGen.genRand.Next(18, 34) + 35 ;

				var surfaceLevel = StructureGenHelper.GetSurfaceLevel(x - 10, x + 11, Main.spawnTileY - 24);
				y = (int)surfaceLevel.average;
			
				return surfaceLevel.sd <= 2.8;
			}

			bool startLeftSide = false;//Terraria.WorldGen.genRand.NextBool();

			if (FindValidLocation(startLeftSide) || FindValidLocation(!startLeftSide))
			{
				Mineshaft mineshaft = new Mineshaft((ushort)(x - 13), (ushort)(y - 13));
				mineshaft.Generate();
				SpawnHousesSystem.Mineshaft = mineshaft;
			}
		}
		catch (Exception e)
		{
			ModContent.GetInstance<SpawnHouses>().Logger.Error($"Mineshaft failed to generate:\n{e}");
			return;
		}
	}
	
	
	public static void GenerateMainBasement()
	{
		BoundingBox[] mineshaftBoundingBox = [];
		if (SpawnHousesSystem.Mineshaft is not null)
		{
			Mineshaft structure = SpawnHousesSystem.Mineshaft;
			BoundingBox structureBox = new BoundingBox(structure.X - 8, structure.Y,
				structure.X + structure.StructureXSize + 8, structure.Y + 200);
			mineshaftBoundingBox = [structureBox];
		}

		MainBasement chain;
		if (SpawnHousesSystem.MainHouse is not null)
		{
			   
			chain = new MainBasement((ushort)SpawnHousesSystem.MainHouse.BasementEntryPos.X, (ushort)SpawnHousesSystem.MainHouse.BasementEntryPos.Y, 
				startingBoundingBoxes: mineshaftBoundingBox);
		}
		else
		{
			chain = new MainBasement((ushort)Main.spawnTileX, (ushort)Main.spawnTileY,
				startingBoundingBoxes: mineshaftBoundingBox);
		}

		if (chain.SuccessfulGeneration)
		{
			try
			{
				chain.CalculateChain();
				chain.Generate();
				SpawnHousesSystem.MainBasement = chain;
			}
			catch (Exception e)
			{
				ModContent.GetInstance<SpawnHouses>().Logger.Error($"Main basement failed to generate:\n{e}");
				return;
			}
		}
	}
	
	
	public static void GenerateBeachHouse()
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
						
						if (Terraria.WorldGen.SolidTile(x, y))
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
				try
				{
					BeachHouse beachHouse = leftSide ? 
						new BeachHouse((ushort)(tileX - 9), (ushort)(tileY - 32)) : 
						new BeachHouse((ushort)(tileX - 23), (ushort)(tileY - 32), reverse: true);
					beachHouse.Generate();
					SpawnHousesSystem.BeachHouse = beachHouse;
				}
				catch (Exception e)
				{
					ModContent.GetInstance<SpawnHouses>().Logger.Error($"Beach house failed to generate:\n{e}");
					return;
				}
				
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
		
					Firepit structure = new Firepit(x, y);
					structure.Generate();
				}
			}
	}
}