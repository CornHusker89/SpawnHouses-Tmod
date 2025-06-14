using System;
using Microsoft.Xna.Framework;
using SpawnHouses.Helpers;
using SpawnHouses.Structures.Structures;
using Terraria;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;

namespace SpawnHouses.Items.StructureSpawns;

public class SpawnMainHouse : ModItem {
    private static int _xSize;
    private static int _xSizePixels;
    private static int _ySize;
    private static bool _evaluatedHouseSize;

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
        Recipe recipe = Recipe.Create(ModContent.ItemType<SpawnMainHouse>());
        recipe.AddIngredient(ItemID.IronBar, 10);
        recipe.AddIngredient(ItemID.Wood, 350);
        recipe.AddIngredient(ItemID.StoneBlock, 500);
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

            MainHouse house = new((ushort)(mouseX - Math.Floor(_xSize / 2.0)), (ushort)(mouseY - Math.Floor(_ySize / 2.0)));
            house.Generate(true);
            house.OnFound();

            NetMessage.SendTileSquare(-1, house.X, house.Y, house.StructureXSize, house.StructureYSize);
        }

        return true;
    }

    private void DrawOutline(On_Main.orig_DrawPlayers_AfterProjectiles orig, Main self) {
        orig(self);

        if (Main.LocalPlayer.HeldItem.type != ModContent.ItemType<SpawnMainHouse>()) return;

        if (!_evaluatedHouseSize) {
            MainHouse sampleHouse = new(100, 100);
            _xSize = sampleHouse.StructureXSize - 1;
            _xSizePixels = _xSize * 16;
            _ySize = sampleHouse.StructureYSize;
            _evaluatedHouseSize = true;
        }

        Point16 pos = new(Player.tileTargetX, Player.tileTargetY);
        Vector2 pos2 = pos.ToVector2() * 16 - Main.screenPosition;
        int x = (int)pos2.X;
        int y = (int)pos2.Y;

        int topOffset = -16 * (int)Math.Floor(_ySize / 2.0);
        int leftOffset = -16 * (int)Math.Floor(_xSize / 2.0);

        Rectangle[] rectangles = [
            new(x + leftOffset, y + topOffset, _xSizePixels, 16 * (_ySize - 10)),
            new(x + leftOffset, y + topOffset + 26 * 16, _xSizePixels, 16 * (_ySize - 26))
        ];
        Color[] colors = [
            Color.Yellow,
            Color.Cyan
        ];
        DrawHelper.DrawRectangles(rectangles, colors, 3);
    }
}