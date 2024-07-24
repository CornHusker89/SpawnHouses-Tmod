using System;
using System.Linq;
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
	public class SpawnTest : ModItem	
	{
		
		public override void SetDefaults()
		{
			Item.useStyle = ItemUseStyleID.Swing;
			Item.useTime = 20;
			Item.useAnimation = 20;
			Item.rare = ItemRarityID.Blue;
		}
		
		public override bool AltFunctionUse(Player player)
		{
			return true;
		}
		
		public override bool? UseItem(Player player)
		{
			int x = (Main.MouseWorld / 16).ToPoint16().X;
			int y = (Main.MouseWorld / 16).ToPoint16().Y;

			Terraria.WorldGen.PlaceTile(x, y, SpawnHousesModHelper.RemoteAccessTileID);
			TileEntity.PlaceEntityNet(x - 1, y - 1, SpawnHousesModHelper.RemoteAccessTileEntityID);
			//
			// Console.WriteLine(x + ", " + y);
			// Console.WriteLine(SpawnHousesSystem.MainHouse.StorageHeartPos);
			
			Point16 remotePos = new Point16(x - 1, y - 1);
			Point16 heartPos = SpawnHousesSystem.MainHouse.StorageHeartPos;
			
			Console.WriteLine(SpawnHousesModHelper.LinkRemoteStorage(remotePos, heartPos));
			
			
			
			return true;

			
		}

	}
}