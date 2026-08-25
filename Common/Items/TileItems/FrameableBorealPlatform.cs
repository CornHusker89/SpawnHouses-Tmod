using SpawnHouses.Common.Tiles;
using Terraria.ModLoader;

namespace SpawnHouses.Common.Items.TileItems;

public class FrameableBorealPlatform : ModItem {
    public override void SetDefaults() {
        Item.DefaultToPlaceableTile(ModContent.TileType<FrameableWoodPlatform>(), 19);
        Item.value = 150;
    }
}