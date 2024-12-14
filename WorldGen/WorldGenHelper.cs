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
	private static byte _mainHouseOffsetDirection = Directions.None;
	
    public static void GenerateMainHouse()
    {
	    bool spawnUnderworld = Main.ActiveWorldFileData.SeedText.ToLower().Replace(" ", "").Replace("'", "") == "dontdigup" || 
	                           Main.ActiveWorldFileData.SeedText.ToLower().Replace(" ", "") == "getfixedboi";
		
		int initialX = 1;
		int initialY = 1;
		
		bool foundValidSpot = false;
		
		for (ushort counts = 0; counts < 10; counts++)
		{
			initialX = Terraria.WorldGen.genRand.Next(Main.spawnTileX - 25, Main.spawnTileX + 25);
			if (!spawnUnderworld)
				initialY = Main.spawnTileY - 40;
			else
				initialY = Main.spawnTileY - 20;
				
			//make sure we're not under the surface
			if (!spawnUnderworld)
				while (!Is40AboveTilesClear(initialX, initialY))
					initialY -= 30;

			bool Is40AboveTilesClear(int x, int y)
			{
				for (byte i = 1; i < 41; i++)
					if (Terraria.WorldGen.SolidTile(x, y - i))
						return false;
				
				return true;
			}
			
			// Move out of the way of the big spawn tree or just offset it
			try
			{
				if (ModHelper.IsRemnantsEnabled || ModContent.GetInstance<SpawnHousesConfig>().SpawnPointHouseOffset)
				{
					var leftSurface = StructureGenHelper.GetSurfaceLevel(initialX - 120 - 30, initialX - 120 + 30,
						initialY, maxCastDistance: 400);
					var rightSurface = StructureGenHelper.GetSurfaceLevel(initialX + 120 - 30, initialX + 120 + 30,
						initialY, maxCastDistance: 400);
					if (leftSurface.sd < rightSurface.sd)
					{
						initialX -= 120;
						initialY = (int)leftSurface.average;
						_mainHouseOffsetDirection = Directions.Left;
					}
					else
					{
						initialX += 120;
						initialY = (int)rightSurface.average;
						_mainHouseOffsetDirection = Directions.Right;
					}
				}
				else
				{
					var surface = StructureGenHelper.GetSurfaceLevel(initialX - 30, initialX + 30, initialY,
						maxCastDistance: 400);
					initialY = (int)surface.average;
				}
			}
			catch (Exception e)
			{
				ModContent.GetInstance<SpawnHouses>().Logger.Error($"Main house failed to generate:\n{e}");
				return;
			}
			
			// if we found a good spot, break search loop
			if (spawnUnderworld)
			{
				if (initialY >= Main.spawnTileY - 25)
				{
					foundValidSpot = true;
					break;
				}
			}
			else
			{
				foundValidSpot = true;
				break;
			}
		}
		
		// just in case something above got fucked up
		if (!foundValidSpot)
		{
			ModContent.GetInstance<SpawnHouses>().Logger.Error("Failed to generate SpawnPointHouse. Please report this world seed and your client.log to the mod's author");
			return;
		}

		try
		{
			MainHouse house = new MainHouse((ushort)(initialX - 31), (ushort)(initialY - 16), hasBasement: ModContent.GetInstance<SpawnHousesConfig>().EnableSpawnPointBasement, inUnderworld: spawnUnderworld);
			house.Generate();
			SpawnHousesSystem.MainHouse = house;
			
			// move the spawn point to the upper floor of the house
			if (ModContent.GetInstance<SpawnHousesConfig>().SpawnPointHouseSetsSpawn)
			{
				Main.spawnTileX = initialX + house.LeftSize - 1 - 31;
				Main.spawnTileY = initialY + 5 - 15;
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
		catch (Exception e)
		{
			ModContent.GetInstance<SpawnHouses>().Logger.Error($"Main house failed to generate:\n{e}");
			return;
		}
    }
	
	
	public static void GenerateMineshaft()
	{
		int x, y;
		try
		{
			bool FindValidLocation(bool left = true)
			{
				if (SpawnHousesSystem.MainHouse != null)
				{
					int centerHouse = SpawnHousesSystem.MainHouse.X + (SpawnHousesSystem.MainHouse.StructureXSize / 2);
					if (left)
						x = centerHouse - Terraria.WorldGen.genRand.Next(18, 38) - 35;
					else
						x = centerHouse + Terraria.WorldGen.genRand.Next(18, 38) + 35;
				}
				else
				{
					if (left)
						x = Main.spawnTileX - Terraria.WorldGen.genRand.Next(18, 34) - 35;
					else
						x = Main.spawnTileX + Terraria.WorldGen.genRand.Next(18, 34) + 35;
				}

				var surfaceLevel = StructureGenHelper.GetSurfaceLevel(x - 10, x + 11, Main.spawnTileY - 24);
				y = (int)surfaceLevel.average;
			
				return surfaceLevel.sd <= 2.8;
			}

			bool startLeftSide;
			if (_mainHouseOffsetDirection == Directions.None)
				startLeftSide = Terraria.WorldGen.genRand.NextBool();
			else
				startLeftSide = _mainHouseOffsetDirection == Directions.Left;

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
		ushort fillTileType = TileID.Sand;
		
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
					
				while (true) { // sample vertical segment
					y++;
					
					if (Terraria.WorldGen.SolidTile(x, y))
					{
						if (!Terraria.WorldGen.SolidTile(x, y + 20) || !Terraria.WorldGen.SolidTile(x, y + 28)) // if we're on an "island" keep going
							break;
						
						ushort type = Main.tile[x, y].TileType;
						if (type is TileID.Sand or TileID.ShellPile or TileID.Crimsand or TileID.Ebonsand or TileID.Pearlsand || force)
						{
							tileX = x;
							tileY = y;
							
							//sample a deeper tile
							fillTileType = Main.tile[x, y + 10].TileType;
							if (fillTileType == TileID.ShellPile)
								fillTileType = TileID.Sand;
							
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
				
				// replace all sand with filltype sand (for when the beaches are corrupt blocks)
				if (fillTileType is not TileID.Sand)
					WorldUtils.Gen(new Point(tileX, tileY), new Shapes.Circle(150, 100), Actions.Chain(
						new Actions.Custom((i, j, args) =>
						{
							if (Main.tile[i, j].HasTile && Main.tile[i, j].TileType == TileID.Sand)
							{
								Tile tile = Main.tile[i, j];
								tile.TileType = fillTileType;
							}
							return true;
						})
					));
			}
			catch (Exception e)
			{
				ModContent.GetInstance<SpawnHouses>().Logger.Error($"Beach house failed to generate:\n{e}");
				return;
			}
		}
	}
}