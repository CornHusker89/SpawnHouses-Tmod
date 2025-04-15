using Microsoft.Xna.Framework;
using ReLogic.Utilities;
using Terraria;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.WorldBuilding;

namespace SpawnHouses.Items.Debug;

public class SpawnIceCave : ModItem {
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
        Point16 point = (Main.MouseWorld / 16).ToPoint16();
        int x = point.X;
        int y = point.Y;

        int w = 23;
        int h = 17;
        int pondDiff = 7;
        int tunnelHeight = 10;
        byte bleed = 10;

        bool facingLeft = Terraria.WorldGen.genRand.Next(0, 2) == 0;

        // cave floor
        WorldUtils.Gen(new Point((int)(x - w * 1.35), y), new Shapes.Rectangle((int)((w + 3) * 2.5), bleed),
            Actions.Chain(
                new Actions.SetTile(TileID.IceBlock, true),
                new Modifiers.Dither(0.65),
                new Actions.SetTile(TileID.SnowBlock, true)
            ));


        // central cave itself
        WorldUtils.Gen(new Point(x, y), new Shapes.Mound(w - pondDiff + bleed, h - pondDiff + bleed), Actions.Chain(
            new Modifiers.Flip(false, true),
            new Actions.SetTile(TileID.IceBlock, true),
            new Modifiers.Dither(0.22),
            new Actions.SetTile(TileID.SnowBlock, true)
        ));
        WorldUtils.Gen(new Point(x, y), new Shapes.Mound(w - pondDiff, h - pondDiff), Actions.Chain(
            new Modifiers.Flip(false, true),
            new Actions.ClearTile(),
            new Actions.SetLiquid(LiquidID.Water, 232),
            new Actions.SetFrames(true)
        ));
        WorldUtils.Gen(new Point(x, y - tunnelHeight), new Shapes.Mound(w + bleed, h + bleed), Actions.Chain(
            new Actions.SetTile(TileID.IceBlock, true),
            new Modifiers.Dither(0.65),
            new Actions.SetTile(TileID.SnowBlock, true)
        ));
        WorldUtils.Gen(new Point(x, y - tunnelHeight), new Shapes.Mound(w, h), Actions.Chain(
            new Actions.ClearTile(),
            new Actions.ClearWall(),
            new Actions.SetLiquid(0, 0),
            new Actions.SetFrames(true)
        ));


        // tunnel
        WorldUtils.Gen(new Point((int)(x - w * 1.35), y + 1 - tunnelHeight),
            new Shapes.Rectangle((int)((w + 3) * 2.5), bleed + 1), Actions.Chain(
                new Actions.ClearTile(),
                new Actions.ClearWall(),
                new Actions.SetLiquid(0, 0),
                new Actions.SetFrames(true)
            )
        );


        //hanging icicles
        int iterations = Terraria.WorldGen.genRand.Next(w / 20 + 2, w / 10 + 3);
        for (byte i = 0; i < iterations; i++) {
            double tailXOffset = w * 1.5 / iterations * i + Terraria.WorldGen.genRand.Next(-3, 3) - w / 2;
            double pointXOffset = Terraria.WorldGen.genRand.Next(-(w / 15) - 1, w / 15 + 2);

            WorldUtils.Gen(new Point(x + (int)tailXOffset, y - h - tunnelHeight), new Shapes.Tail(w / 8 + 5,
                    new Vector2D(pointXOffset, h / 3.5 + Terraria.WorldGen.genRand.Next(0, (int)(h / 1.5)))),
                Actions.Chain(
                    new Actions.SetTile(TileID.IceBlock, true),
                    new Modifiers.Dither(0.75),
                    new Actions.SetTile(TileID.SnowBlock, true)
                )
            );
        }


        // entry cave
        byte caveNum = (byte)Terraria.WorldGen.genRand.Next(1, 3);
        if (facingLeft)
            for (byte i = 0; i < caveNum; i++)
                Terraria.WorldGen.digTunnel(x - w, y - tunnelHeight / 2, -1.2, 0, 140,
                    Terraria.WorldGen.genRand.Next(4, 7));
        else
            for (byte i = 0; i < caveNum; i++)
                Terraria.WorldGen.digTunnel(x + w, y - tunnelHeight / 2, 1.2, 0, 140,
                    Terraria.WorldGen.genRand.Next(4, 7));


        return true;
    }
}