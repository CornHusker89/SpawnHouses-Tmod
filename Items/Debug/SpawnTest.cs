using System;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.WorldBuilding;

namespace SpawnHouses.Items.Debug;

public class SpawnTest : ModItem {
    public override void SetDefaults() {
        Item.useStyle = ItemUseStyleID.Swing;
        Item.useTime = 20;
        Item.useAnimation = 20;
        Item.rare = ItemRarityID.Blue;
    }

    public override bool AltFunctionUse(Player player) => true;

    public override bool? UseItem(Player player) {
        int x = (Main.MouseWorld / 16).ToPoint16().X;
        int y = (Main.MouseWorld / 16).ToPoint16().Y;
        
        Console.WriteLine(x + ", " + y);
        Console.WriteLine($"is {GenVars.rightBeachStart}");

        return true;
    }
}