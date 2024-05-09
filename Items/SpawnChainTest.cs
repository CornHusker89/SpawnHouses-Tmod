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
using SpawnHouses.Structures.Structures;
using Terraria.Utilities;


namespace SpawnHouses.Items
{
	public class SpawnChainTest : ModItem
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
			List<CustomStructure> costList = new List<CustomStructure>();
			costList.Add(new ChainTestStructure(cost: 15));
			List<Bridge> bridgeList = new List<Bridge>();
			bridgeList.Add(new ParabolaBridge("Structures/StructureFiles/WoodBridge", 2, -2, 6, 0.6));

			StructureChain chain = new StructureChain(100, costList, bridgeList, point, 7);
			
			
			
			
			return true;
		}

	}
}