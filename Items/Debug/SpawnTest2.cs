using SpawnHouses.AdvStructures.AdvStructureParts;
using SpawnHouses.Types;
using Terraria;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;

namespace SpawnHouses.Items.Debug;

public class SpawnTest2 : ModItem {
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

        Path path = new([
            new Point16(x, y),
            new Point16(x + 10, y - 10)
        ]);

        Shape fillShape = path.FillFromBoundingBox(new PartialPoint16(0, 1), new Point16(-3, 7));

        fillShape.ExecuteInArea((x, y) => { Terraria.WorldGen.PlaceTile(x, y, TileID.AmberGemspark); });

        return true;
    }
}