#if SPAWNHOUSES_DEBUG

#nullable enable
using System;
using SpawnHouses.Content.Types.Interfaces;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace SpawnHouses.Items.Debug;

public class SpawnTest2 : ModItem {
    public static IComponent? Component;
    
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
        
        return true;
    }
}

#endif