using SpawnHouses.Structures.Chains;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace SpawnHouses.Items.Debug;

public class SpawnChainTest : ModItem {
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
        var point = (Main.MouseWorld / 16).ToPoint16();

        var chain = new TestChain((ushort)point.X, (ushort)point.Y);
        chain.CalculateChain();
        chain.Generate();
        return true;
    }
}