using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ReLogic.Content;
using SpawnHouses.Structures.Structures;
using Terraria;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;

namespace SpawnHouses.Items.StructureSpawns {
    public class SpawnMainHouse : ModItem {
        private static int _xSize;
        private static int _xSizePixels;
        private static int _ySize;
        private static bool _evaluatedHouseSize;
        private static Asset<Texture2D> _pixelTexture;

        public override void SetDefaults() {
            Item.useStyle = ItemUseStyleID.Swing;
            Item.useTime = 20;
            Item.useAnimation = 20;
            Item.rare = ItemRarityID.Blue;
            Item.consumable = true;
        }

        public override void Load()
        {
            On_Main.DrawPlayers_AfterProjectiles += DrawOutline;
            _pixelTexture = ModContent.Request<Texture2D>("SpawnHouses/Items/StructureSpawns/Pixel");
        }

        public override void AddRecipes() {
            Recipe recipe = Recipe.Create(ModContent.ItemType<SpawnMainHouse>());
            recipe.AddIngredient(ItemID.Silk, 5);
            recipe.AddIngredient(ItemID.IronBar, 10);
            recipe.AddIngredient(ItemID.Wood, 400);
            recipe.AddIngredient(ItemID.StoneBlock, 500);
            recipe.AddTile(TileID.WorkBenches);
            recipe.Register();
        }

        public override bool AltFunctionUse(Terraria.Player player) {
            return true;
        }

        public override bool? UseItem(Terraria.Player player) {
            Point16 mousePos = (Main.MouseWorld / 16).ToPoint16();
            int mouseX = mousePos.X;
            int mouseY = mousePos.Y;

            MainHouse house = new MainHouse((ushort)(mouseX - Math.Floor(_xSize / 2.0)), (ushort)(mouseY - Math.Floor(_ySize / 2.0) + 10));
            house.Generate(true);

            //NetMessage.SendTileSquare(-1, house.X, house.X, house.StructureXSize, house.StructureYSize);

            return true;
        }

        private void DrawOutline(On_Main.orig_DrawPlayers_AfterProjectiles orig, Main self)
        {
            orig(self);

            if (Main.LocalPlayer.HeldItem.type != ModContent.ItemType<SpawnMainHouse>()) {
                return;
            }

            if (!_evaluatedHouseSize) {
                MainHouse sampleHouse = new MainHouse(100, 100);
                _xSize = sampleHouse.StructureXSize - 1;
                _xSizePixels = _xSize * 16;
                _ySize = sampleHouse.StructureYSize;
                _evaluatedHouseSize = true;
            }

            Point16 pos = new Point16(Terraria.Player.tileTargetX, Terraria.Player.tileTargetY);
            Vector2 pos2 = pos.ToVector2() * 16 - Main.screenPosition;
            int x = (int)pos2.X;
            int y = (int)pos2.Y;

            int topOffset = -16 * (int)Math.Floor(_ySize / 2.0);
            int bottomOffset = 16 * (int)Math.Ceiling(_ySize / 2.0);
            int floorOffset = topOffset + 26 * 16;
            int aboveGroundHeightPixels = 16 * (_ySize - 10);
            int floorHeightPixels = 16 * (_ySize - 26);
            int leftOffset = -16 * (int)Math.Floor(_xSize / 2.0);

            Main.spriteBatch.Begin(default, default, default, default, default, default, Main.GameViewMatrix.TransformationMatrix);

            Main.spriteBatch.Draw(_pixelTexture.Value, new Rectangle(x + leftOffset, y + topOffset, _xSizePixels, 3), Color.Yellow);
            Main.spriteBatch.Draw(_pixelTexture.Value, new Rectangle(x + leftOffset + _xSizePixels, y + topOffset, 3, aboveGroundHeightPixels), Color.Yellow);
            Main.spriteBatch.Draw(_pixelTexture.Value, new Rectangle(x + leftOffset, y + topOffset, 3, aboveGroundHeightPixels), Color.Yellow);

            Main.spriteBatch.Draw(_pixelTexture.Value, new Rectangle(x + leftOffset, y + floorOffset, _xSizePixels, 3), Color.Cyan);
            Main.spriteBatch.Draw(_pixelTexture.Value, new Rectangle(x + leftOffset + _xSizePixels, y + floorOffset, 3, floorHeightPixels), Color.Cyan);
            Main.spriteBatch.Draw(_pixelTexture.Value, new Rectangle(x + leftOffset, y + floorOffset, 3, floorHeightPixels), Color.Cyan);
            Main.spriteBatch.Draw(_pixelTexture.Value, new Rectangle(x + leftOffset, y + bottomOffset, _xSizePixels, 3), Color.Cyan);

            Main.spriteBatch.End();
        }
    }
}