using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.DataStructures;
using SpawnHouses.Structures.Chains;


namespace SpawnHouses.Items.Debug
{
	public class SpawnCaveTown1 : ModItem
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
			Point16 point = (Main.MouseWorld / 16).ToPoint16();

			CaveTown1 chain = new CaveTown1((ushort)point.X, (ushort)point.Y);
			chain.Generate();
			return true;
		}
	}
}