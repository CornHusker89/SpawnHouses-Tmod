using Terraria;
using Terraria.ID;
using Terraria.ModLoader;


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
			
			//Console.WriteLine(WebClientInstance.WebClient.GetSpawnCount()["main_houses"]);
			
			//WebClientInstance.WebClient.AddSpawnCount(true);
			
			return true;
		}

	}
}