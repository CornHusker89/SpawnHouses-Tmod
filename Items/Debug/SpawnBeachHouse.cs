using SpawnHouses.Structures.Structures;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace SpawnHouses.Items.Debug;

public class SpawnBeachHouse : ModItem {
    public override void SetDefaults() {
        Item.useStyle = ItemUseStyleID.Swing;
        Item.useTime = 20;
        Item.useAnimation = 20;
        Item.rare = ItemRarityID.Blue;
    }

    public override void AddRecipes() {
    }

    public override bool AltFunctionUse(Player player) {
        return true;
    }


    public override bool? UseItem(Player player) {
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

        y = (ushort)(y - 29); //the structure spawning has an offset + we want it to be a little off the ground
        x = (ushort)(x - 18); //center the struct

        bool reverse = x > Main.LocalPlayer.position.X / 16;

        BeachHouse structure = new(x, y, 0, reverse);
        structure.Generate();
        //structure._GenerateStructure();

        return true;
    }
}