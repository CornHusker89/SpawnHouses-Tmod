using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.DataStructures;
using Microsoft.Xna.Framework;
using Terraria.Chat;
using Terraria.WorldBuilding;
using SpawnHouses.Structures;


namespace SpawnHouses.Items
{
	public class SpawnMainHouse : ModItem
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
			MainHouseStructure structure = new MainHouseStructure();
			
			bool foundLocation = false;
			int x = 0;
			int y = 0;
			while (!foundLocation)
			{
				x = (Main.MouseWorld / 16).ToPoint16().X;;
				y = 1;
				while (y < Main.worldSurface) {
					if (Terraria.WorldGen.SolidTile(x, y)) {
						break;
					}
					y++;
				}
				foundLocation = true;
			}

			structure.Y = y - 27; //the structure spawning has an offset + we want it to be a little off the ground
			structure.X = x - 31; //center the struct

			bool foundationResult = structure.GenerateFoundation();
			bool structResult = structure.GenerateStructure();
			bool blendLeftResult = structure.BlendLeft();
			bool blendRightResult = structure.BlendLeft(true);
			return structResult;
		}

	}
}