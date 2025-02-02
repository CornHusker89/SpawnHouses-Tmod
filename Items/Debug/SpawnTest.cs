using System;
using Microsoft.Xna.Framework;
using SpawnHouses.Structures;
using SpawnHouses.Structures.AdvStructures;
using SpawnHouses.Structures.StructureParts;
using Terraria;
using Terraria.DataStructures;
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

			int length = 30;
			
			// ComponentGen.Roof2(new Data.ComponentParams([], [], null, new Point16(x, y), length));
			// WorldUtils.Gen(new Point(x + length / 2, y + length / 2), new Shapes.Circle((length + 50) / 2), Actions.Chain(
			// 	new Actions.SetFrames(),
			// 	new Actions.Custom((i, j, args) =>
			// 	{
			// 		Framing.WallFrame(i, j);
			// 		return true;
			// 	})
			// ));

			
			AdvStructureGen.Layout1(new Data.StructureParams([], [], [], [], [], 
				new Point16(x, y),
				new Point16(x + length, y), 
				900, 1000));
			
			return true;
		}

	}
}