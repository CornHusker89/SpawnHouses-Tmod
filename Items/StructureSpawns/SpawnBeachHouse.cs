using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ReLogic.Content;
using SpawnHouses.Helpers;
using SpawnHouses.Structures.Structures;
using Terraria;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.WorldBuilding;

namespace SpawnHouses.Items.StructureSpawns;

public class SpawnBeachHouse : ModItem {

    public override void SetDefaults() {
        Item.useStyle = ItemUseStyleID.Swing;
        Item.useTime = 20;
        Item.useAnimation = 20;
        Item.rare = ItemRarityID.Blue;
        Item.consumable = true;
    }

    public override void Load() {
        On_Main.DrawPlayers_AfterProjectiles += DrawOutline;
    }

    public override void AddRecipes() {
        Recipe recipe = Recipe.Create(ModContent.ItemType<SpawnBeachHouse>());
        recipe.AddIngredient(ItemID.IronBar, 6);
        recipe.AddIngredient(ItemID.Wood, 300);
        recipe.AddTile(TileID.WorkBenches);
        recipe.Register();
    }

    public override bool AltFunctionUse(Player player) {
        return true;
    }

    public override bool? UseItem(Player player) {
        if (player.whoAmI == Main.myPlayer) {
            Point16 mousePos = (Main.MouseWorld / 16).ToPoint16();
            int mouseX = mousePos.X;
            int mouseY = mousePos.Y;

            BeachHouse house = new((ushort)(mouseX - Math.Floor(24 / 2.0)), (ushort)(mouseY - Math.Floor(34 / 2.0)));
            house.FilePath = "Assets/StructureFiles/beachHouse/beachHouse_v2_altfoundation.shstruct";
            house.Generate(true);

            NetMessage.SendTileSquare(-1, house.X, house.Y, house.StructureXSize, house.StructureYSize);
        }
        return true;
    }

    private void DrawOutline(On_Main.orig_DrawPlayers_AfterProjectiles orig, Main self) {
        orig(self);

        if (Main.LocalPlayer.HeldItem.type != ModContent.ItemType<SpawnBeachHouse>()) return;

        Point16 pos = new(Player.tileTargetX, Player.tileTargetY);
        Vector2 pos2 = pos.ToVector2() * 16 - Main.screenPosition;
        int x = (int)pos2.X;
        int y = (int)pos2.Y;

        Rectangle[] rectangles = [
            new Rectangle(x + -16 * 12, y + 16 * -17, 16 * 24, 16 * 31),
            new Rectangle(x + -16 * 12, y + 16 * 14, 16 * 24, 16 * 3)
        ];
        Color[] colors = [
            Color.Yellow,
            Color.Cyan
        ];
        DrawHelper.DrawRectangles(rectangles, colors, 3);
    }
}