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
	public class SpawnHouse : ModItem
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
			Recipe recipe = CreateRecipe();
			recipe.AddIngredient(ItemID.DirtBlock, 10);
			recipe.AddTile(TileID.WorkBenches);
			recipe.Register();
		}

		public override bool AltFunctionUse(Player player)
		{
			return true;
		}
		

		public override bool? UseItem(Player player)
		{
			SurfaceHouseStructure structure = new SurfaceHouseStructure();
			
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

			y -= 16; //the structure spawning has an offset + we want it to be a little off the ground
			
			bool beamResult = structure.GenerateBeams(x: x, y: y);
			bool structResult = structure.GenerateStructure(x: x, y: y);
			return structResult;
		}

	}
}