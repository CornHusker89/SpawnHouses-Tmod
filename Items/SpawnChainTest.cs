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
using SpawnHouses.Structures.StructureChains;
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
			List<CustomChainStructure> costList = new List<CustomChainStructure>();
			Bridge bridge = new ParabolaBridge("Structures/StructureFiles/WoodBridge", 2, -2, 6, 0.4);
			costList.Add(new TestChainStructure(10, bridge));
			

			StructureChain chain = new StructureChain(100, costList, point, 7);
			
			
			
			
			return true;
		}

	}
}