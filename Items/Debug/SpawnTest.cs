using System;
using Microsoft.Xna.Framework;
using SpawnHouses.Structures;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.WorldBuilding;


namespace SpawnHouses.Items.Debug
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
		
		public override bool AltFunctionUse(Terraria.Player player)
		{
			return true;
		}
		
		public override bool? UseItem(Terraria.Player player)
		{
			int x = (Main.MouseWorld / 16).ToPoint16().X;
			int y = (Main.MouseWorld / 16).ToPoint16().Y;			
			
			Console.WriteLine(x + ", " + y);

			// var s = StructureGenHelper.GetSurfaceLevel(x - 30, x + 30, y);
			// Console.WriteLine($"ave: {s.average}, sd: {s.sd}");
			
			// WorldUtils.Gen(new Point(x, y), new Shapes.HalfCircle(20), Actions.Chain(
			// 	new Modifiers.Flip(false, true),
			// 	new Actions.PlaceTile(0)
			// ));
			
			return true;
		}

	}
}