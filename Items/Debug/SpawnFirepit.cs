using SpawnHouses.Structures.Structures;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace SpawnHouses.Items.Debug;

public class SpawnFirepit : ModItem {
    public override void SetDefaults() {
        Item.useStyle = ItemUseStyleID.Swing;
        Item.useTime = 20;
        Item.useAnimation = 20;
        Item.rare = ItemRarityID.Blue;
    }

    public override void AddRecipes() {
    }

    public override bool AltFunctionUse(Terraria.Player player) {
        return true;
    }


    public override bool? UseItem(Terraria.Player player) {
        bool foundLocation = false;
        ushort x = 0;
        ushort y = 0;
        while (!foundLocation) {
            x = (ushort)(Main.MouseWorld / 16).ToPoint16().X;
            ;
            y = 1;
            while (y < Main.worldSurface) {
                if (Terraria.WorldGen.SolidTile(x, y)) break;
                y++;
            }

            foundLocation = true;
        }

        y = (ushort)(y - 2);
        x = (ushort)(x - 3);

        Firepit structure = new(x, y);
        structure.Generate();

        return true;
    }
}