using System;
using System.Collections.Generic;
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
using SpawnHouses.Structures.ChainStructures;
using SpawnHouses.Structures.StructureChains;
using SpawnHouses.Structures.Structures;
using Terraria.Utilities;


namespace SpawnHouses.Items.Debug
{
	public class SpawnMainBasement : ModItem
	{
		
		public override void SetDefaults()
		{
			Item.useStyle = ItemUseStyleID.Swing;
			Item.useTime = 20;
			Item.useAnimation = 20;
			Item.rare = ItemRarityID.Blue;
		}

		public override void AddRecipes() {}

		public override bool AltFunctionUse(Player player)
		{
			return true;
		}

		public override bool? UseItem(Player player)
		{
			Point16 point = (Main.MouseWorld / 16).ToPoint16();

			MainBasement chain = new MainBasementChain((ushort)point.X, (ushort)point.Y);
			chain.Generate();
			return true;
		}
	}
}