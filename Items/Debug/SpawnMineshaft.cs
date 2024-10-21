using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using SpawnHouses.Structures.Structures;


namespace SpawnHouses.Items.Debug
{
	public class SpawnMineshaft : ModItem
	{
		
		public override void SetDefaults()
		{
			Item.useStyle = ItemUseStyleID.Swing;
			Item.useTime = 20;
			Item.useAnimation = 20;
			Item.rare = ItemRarityID.Blue;
		}

		public override void AddRecipes() {}

		public override bool AltFunctionUse(Terraria.Player player)
		{
			return true;
		}

		public override bool? UseItem(Terraria.Player player)
		{
			bool foundLocation = false;
			ushort x = 0;
			ushort y = 0;
			while (!foundLocation)
			{
				x = (ushort)(Main.MouseWorld / 16).ToPoint16().X; ;
				y = 1;
				while (y < Main.worldSurface)
				{
					if (Terraria.WorldGen.SolidTile(x, y))
					{
						break;
					}
					y++;
				}
				foundLocation = true;
			}

			y = (ushort)(y - 13);
			x = (ushort)(x - 10);

			Mineshaft structure = new Mineshaft(x, y);
			structure.Generate();

			return true;
		}
	}
}