#if SPAWNHOUSES_DEBUG

#nullable enable
using System;
using System.IO;
using SpawnHouses.Core.Interfaces;
using SpawnHouses.Core.RootStructureTypes;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace SpawnHouses.Content.Items.Debug;

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

        Main.QueueMainThreadAction(() => {
            foreach (FileStructure s in StructureManager.AllFileStructureVariations) {
                using FileStream stream = new($@"C:/Users/keega/tMod/StructureThumbnails/{s.TemplateName}_{s.Name.GetHashCode()}.png", FileMode.Create);
                s.Tilemap.Preview.Texture?.SaveAsPng(stream, s.Tilemap.Preview.Texture.Width, s.Tilemap.Preview.Texture.Height);
            }
        });
        
        return true;
    }
}

#endif