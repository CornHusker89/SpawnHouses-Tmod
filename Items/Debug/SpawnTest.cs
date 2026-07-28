#if SPAWNHOUSES_DEBUG

using System;
using System.Collections.Generic;
using SpawnHouses.Content.Types.RootStructureTypes;
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

    public override bool AltFunctionUse(Player player) => true;

    public override bool? UseItem(Player player) {
        int x = (Main.MouseWorld / 16).ToPoint16().X;
        int y = (Main.MouseWorld / 16).ToPoint16().Y;
        
        Console.WriteLine(x + ", " + y);

        FileStructure thing = new(
            new Point16(x, y),
            "WoodHouse1",
            new Dictionary<string, string>([
                new KeyValuePair<string, string>("Main", "RightLarge")
            ]),
            generate: true
        );
        
        return true;
    }
}

#endif  