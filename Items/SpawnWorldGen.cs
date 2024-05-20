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
		
			int w = 33;
			int h = 20;
			int pondDiff = 8;
			int tunnelHeight = 12;
			byte bleed = 10;
			
			// cave floor
			WorldUtils.Gen(new Point((int)(x - w * 1.35), y), new Shapes.Rectangle((int)((w + 3) * 2.5), bleed), Actions.Chain(
				new Actions.SetTile(TileID.IceBlock, true),
				new Modifiers.Dither(0.65),
				new Actions.SetTile(TileID.SnowBlock, true)
			));
			
			
			// central cave itself
			WorldUtils.Gen(new Point(x, y), new CustomShapes.ReverseMound(w - pondDiff + bleed, h - pondDiff + bleed), Actions.Chain(
				new Actions.SetTile(TileID.IceBlock, true),
				new Modifiers.Dither(0.3),
				new Actions.SetTile(TileID.SnowBlock, true)
			));
			WorldUtils.Gen(new Point(x, y), new CustomShapes.ReverseMound(w - pondDiff, h - pondDiff), Actions.Chain(
				new Actions.ClearTile(),
				new Actions.SetLiquid(LiquidID.Water, 232),
				new Actions.SetFrames(true)
			));
			WorldUtils.Gen(new Point(x, y - tunnelHeight), new Shapes.Mound(w + bleed, h + bleed), Actions.Chain(
				new Actions.SetTile(TileID.IceBlock, true),
				new Modifiers.Dither(0.65),
				new Actions.SetTile(TileID.SnowBlock, true)
			));
			WorldUtils.Gen(new Point(x, y - tunnelHeight), new Shapes.Mound(w, h), Actions.Chain(
				new Actions.ClearTile(),
				new Actions.ClearWall(),
				new Actions.SetLiquid(0, 0),
				new Actions.SetFrames(true)
			));
			
			
			// tunnel
			WorldUtils.Gen(new Point((int)(x - w * 1.35), y + 1 - tunnelHeight),
				new Shapes.Rectangle((int)((w + 3) * 2.5), bleed + 1), Actions.Chain(
					new Actions.ClearTile(),
					new Actions.ClearWall(),
					new Actions.SetLiquid(0, 0),
					new Actions.SetFrames(true)
				)
			);
			
			//hanging icecicles
			int iterations = Terraria.WorldGen.genRand.Next(w / 20 + 1, w / 10 + 2);
			for (byte i = 0; i < iterations; i++)
			{
				double tailXOffset = (w * 1.5) / iterations * i + Terraria.WorldGen.genRand.Next(-3, 3) - w / 2;
				double pointXOffset = 0; //Terraria.WorldGen.genRand.Next( -(w / 5 + 5), w / 5 + 5 );
				
				WorldUtils.Gen(new Point( x + (int)tailXOffset, y - h - tunnelHeight), new Shapes.Tail((w / 8) + 5,
					new Vector2D(pointXOffset, (h / 4) + Terraria.WorldGen.genRand.Next(0, (int)(h / 1.5) ))),
					Actions.Chain(
						new Actions.SetTile(TileID.IceBlock, true),
						new Modifiers.Dither(0.75),
						new Actions.SetTile(TileID.SnowBlock, true)
					));
			}
			
			
		
			return true;
		}

	}
}