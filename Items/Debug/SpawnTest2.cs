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
            new Point16(x - 15, y + 5),
            new Point16(x - 10, y + 10),
            new Point16(x, y - 10),
            new Point16(x + 8, y + 8),
            new Point16(x + 12, y + 8),
            new Point16(x + 20, y + 8),
            new Point16(x + 30, y - 12)
        );

        Shape originalShape = original.ToShape(2);
        Path upper = original.GetOffsetEvenPath(-3, true, true);
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