using SpawnHouses.Content.Tiles;
using Terraria.ModLoader;

namespace SpawnHouses.Content.Items.TileItems;

public class FrameableBorealPlatformNoCollide : ModItem {
    public override void SetDefaults() {
        Item.DefaultToPlaceableTile(ModContent.TileType<FrameableWoodPlatformNoCollide>(), 19);
        Item.value = 150;
    }
}