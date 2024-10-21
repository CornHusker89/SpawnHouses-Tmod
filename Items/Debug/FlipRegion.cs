using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.DataStructures;
using Microsoft.Xna.Framework;
using Terraria.Chat;
using Terraria.WorldBuilding;
using SpawnHouses.Structures.Structures;
using SpawnHouses;


namespace SpawnHouses.Items.Debug
{
	public class FlipRegion : ModItem
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

		public override bool AltFunctionUse(Terraria.Player player)
		{
			return true;
		}
		

		public override bool? UseItem(Terraria.Player player)
		{

			SlopeType flipDir(SlopeType originalDir)
			{
				if (originalDir == SlopeType.SlopeDownLeft)
					return SlopeType.SlopeDownRight;
				if (originalDir == SlopeType.SlopeUpLeft)
					return SlopeType.SlopeUpRight;
				if (originalDir == SlopeType.SlopeDownRight)
					return SlopeType.SlopeDownLeft;
				if (originalDir == SlopeType.SlopeUpRight)
					return SlopeType.SlopeUpLeft;
				return originalDir;
			}
			
			
			ushort width = 100;
			ushort height = 100;
			
			ushort startX = (ushort)(Main.MouseWorld / 16).ToPoint16().X;
			ushort startY = (ushort)(Main.MouseWorld / 16).ToPoint16().Y;
			
			for (int x = 0; x < width / 2; x++)
			{
				for (int y = 0; y < height; y++)
				{
					// Calculate opposite x position
					int oppositeX = width - 1 - x;
                
					// Swap tiles
					Tile tempTile = Main.tile[startX + x, startY + y];
					
					Main.tile[startX + x, startY + y].CopyFrom(Main.tile[startX + oppositeX, startY + y]);
					Tile tile = Main.tile[startX + x, startY + y];
					tile.Slope = flipDir(tile.Slope);
					
					Main.tile[startX + oppositeX, startY + y].CopyFrom(tempTile);
					tile = Main.tile[startX + oppositeX, startY + y];
					tile.Slope = flipDir(tile.Slope);
					
					WorldUtils.Gen(new Point(x + width / 2, y + height / 2), new Shapes.Circle(100 ), new Actions.SetFrames());
				}
			}

			return true;
		}

	}
}