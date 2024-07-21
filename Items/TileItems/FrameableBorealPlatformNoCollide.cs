using Terraria.ModLoader;

namespace SpawnHouses.Items.TileItems;

public class FrameableBorealPlatformNoCollide : ModItem
{
    public override void SetDefaults() {
        Item.DefaultToPlaceableTile(ModContent.TileType<Tiles.FrameableWoodPlatformNoCollide>(), 19);
        Item.value = 150;
        
    }
}