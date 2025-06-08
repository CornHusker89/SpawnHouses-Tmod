using SpawnHouses.Structures.Chains;
using Terraria;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;

namespace SpawnHouses.Items.Debug;

public class SpawnCaveTown1 : ModItem {
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
        Point16 point = (Main.MouseWorld / 16).ToPoint16();

        CaveTown1 chain = new((ushort)point.X, (ushort)point.Y);
        chain.CalculateChain();
        chain.Generate();
        return true;
    }
}