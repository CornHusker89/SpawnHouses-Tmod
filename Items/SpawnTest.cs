using System;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.DataStructures;
using Microsoft.Xna.Framework;
using Terraria.Chat;
using Terraria.WorldBuilding;
using SpawnHouses.Structures.Structures;
using SpawnHouses;


namespace SpawnHouses.Items
{
	public class SpawnTest : ModItem
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
			int x = (Main.MouseWorld / 16).ToPoint16().X;
			int y = (Main.MouseWorld / 16).ToPoint16().Y;
			
			int signIndex = Sign.ReadSign(x, y, true);
			Console.WriteLine(signIndex);
			if (signIndex != -1)
			    Sign.TextSign(signIndex, "aaa");
			
			return true;
		}

	}
}