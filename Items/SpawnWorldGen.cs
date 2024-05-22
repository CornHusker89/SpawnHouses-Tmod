using System;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.DataStructures;
using Microsoft.Xna.Framework;
using ReLogic.Utilities;
using Terraria.WorldBuilding;
using SpawnHouses.Structures.Structures;
using SpawnHouses.WorldGen;


namespace SpawnHouses.Items
{
	public class SpawnWorldGen : ModItem
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
			Point16 point = (Main.MouseWorld / 16).ToPoint16();
			int x = point.X;
			int y = point.Y;

			Terraria.WorldGen.digTunnel(x, y, 1.2, 0, 140, 4);
		
			return true;
		}

	}
}