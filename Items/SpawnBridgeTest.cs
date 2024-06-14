using System;
using System.Threading;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.DataStructures;
using Microsoft.Xna.Framework;
using SpawnHouses.Structures;
using SpawnHouses.Structures.Bridges;
using Terraria.Chat;
using Terraria.WorldBuilding;
using SpawnHouses.Structures.Structures;
using Terraria.Utilities;


namespace SpawnHouses.Items
{
	public class SpawnBridgeTest : ModItem
	{
		
		public override void SetDefaults()
		{
			Item.useStyle = ItemUseStyleID.Swing;
			Item.useTime = 20;
			Item.useAnimation = 20;
			Item.rare = ItemRarityID.Blue;
		}

		public override void AddRecipes()
		{
		}

		public override bool AltFunctionUse(Player player)
		{
			return true;
		}
		

		public override bool? UseItem(Player player)
		{
			bool foundLocation = false;
			ushort x = 0;
			ushort y = 0;
			while (!foundLocation)
			{
				x = (ushort)(Main.MouseWorld / 16).ToPoint16().X;;
				y = 1;
				while (y < Main.worldSurface) {
					if (Terraria.WorldGen.SolidTile(x, y)) {
						break;
					}
					y++;
				}
				foundLocation = true;
			}

			y = (ushort)(y - 9); //the structure spawning has an offset + we want it to be a little off the ground
			x = (ushort)(x - 4); //center the struct
			
			BridgeTestStructure structure1 = new BridgeTestStructure(x, y);
			structure1._GenerateStructure();


			// make the 2nd structure

			short xOffset = (short)34; //Terraria.WorldGen.genRand.Next(25, 35);
			foundLocation = false;
			x = 0;
			y = 0;
			while (!foundLocation)
			{
				x = (ushort)((Main.MouseWorld / 16).ToPoint16().X + xOffset);
				y = 1;
				while (y < Main.worldSurface) {
					if (Terraria.WorldGen.SolidTile(x, y)) {
						break;
					}
					y++;
				}
				foundLocation = true;
			}
			
			
			y = (ushort)(y - 9); //the structure spawning has an offset + we want it to be a little off the ground
			x = (ushort)(x - 4); //center the struct
			
			BridgeTestStructure structure2 = new BridgeTestStructure(x, y);
			structure2._GenerateStructure();


			ParabolaBridge bridge = ParabolaBridge.TestBridge.Clone();
			bridge.SetPoints(structure1.ConnectPoints[3][0], structure2.ConnectPoints[2][0]);
			bridge.Generate();
			
			return true;
		}

	}
}