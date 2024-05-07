using System;
using System.Threading;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.DataStructures;
using Microsoft.Xna.Framework;
using Terraria.Chat;
using Terraria.WorldBuilding;
using SpawnHouses.Structures;
using Terraria.Utilities;
using Timer = System.Timers.Timer;


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


			string filepath = "Structures/PrebuiltStructures/bridgeWoodStructure";
			structure1.ConnectPoints[1].GenerateBridge(structure2.ConnectPoints[0], filepath, 2, -2, 0.6);
			
			return true;
		}

	}
}