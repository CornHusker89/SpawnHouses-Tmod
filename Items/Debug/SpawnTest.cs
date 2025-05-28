using System;
using SpawnHouses.Helpers;
using Terraria;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;

namespace SpawnHouses.Items.Debug;

public class SpawnTest : ModItem {
    public override void SetDefaults() {
        Item.useStyle = ItemUseStyleID.Swing;
        Item.useTime = 20;
        Item.useAnimation = 20;
        Item.rare = ItemRarityID.Blue;
    }

    public override bool AltFunctionUse(Player player) {
        return true;
    }

    public override bool? UseItem(Player player) {
        int x = (Main.MouseWorld / 16).ToPoint16().X;
        int y = (Main.MouseWorld / 16).ToPoint16().Y;

        Console.WriteLine($"item netmode is {Main.netMode} and position is - {x}, {y}");
        if (Main.netMode == NetmodeID.MultiplayerClient) {
            Console.WriteLine("updating from test item");
            //CompatabilityHelper.UpdateStorageNetwork(x, y);
            MagicStorage.NetHelper.SendSearchAndRefresh(x, y);
            //MagicStorage.Components.TEStorageComponent.SearchAndRefreshNetwork(new Point16(x, y));
        }

        return true;
    }
}