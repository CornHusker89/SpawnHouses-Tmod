#nullable enable
using SpawnHouses.Common.Types.Geometry;
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

    public override bool AltFunctionUse(Player player) => true;

    public override bool? UseItem(Player player) {
        int x = (Main.MouseWorld / 16).ToPoint16().X;
        int y = (Main.MouseWorld / 16).ToPoint16().Y;

        Path original = new(
            false,
            false,
            new Point16(x, y),
            new Point16(x + 6, y),
            new Point16(x + 12, y - 6),
            new Point16(x + 21, y + 3),
            new Point16(x + 25, y + 3)
        );

        Shape originalShape = original.ToShape(2);
        Path upper = original.GetOffsetEvenPath(-3);
        Shape upperShape = upper.ToShape(2);

        originalShape.ExecuteInArea((x2, y2) => {
            Tile tile = Main.tile[x2, y2];
            tile.HasTile = true;
            tile.TileType = TileID.AmberGemspark;
        });

        upperShape.ExecuteInArea((x2, y2) => {
            Tile tile = Main.tile[x2, y2];
            tile.HasTile = true;
            tile.TileType = TileID.EmeraldGemspark;
        });

        
        

        return true;
    }
}