using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.Audio;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.ObjectData;

namespace SpawnHouses.Tiles;

public class FrameableWoodPlatform : ModTile {
    public override void SetStaticDefaults() {
        Main.tileFrameImportant[Type] = true;
        Main.tileSolidTop[Type] = true;
        Main.tileSolid[Type] = true;
        Main.tileNoAttach[Type] = true;
        Main.tileTable[Type] = true;
        Main.tileLavaDeath[Type] = true;
        Main.tileNoSunLight[Type] = true;

        TileID.Sets.CanBeSloped[Type] = true;
        TileID.Sets.Platforms[Type] = true;

        DustType = DustID.BorealWood_Small;
        AddMapEntry(new Color(191, 142, 111));

        TileObjectData.newTile.FullCopyFrom(TileID.Platforms);
        TileObjectData.newTile.StyleMultiplier = 35;
        TileObjectData.newTile.StyleWrapLimit = 35;

        TileObjectData.addTile(Type);
    }

    public override void PlaceInWorld(int i, int j, Item item) {
        int style = Main.LocalPlayer.HeldItem.placeStyle;
        Tile tile = Main.tile[i, j];
        tile.TileFrameY = (short)(style * 18);
        tile.TileFrameX = 0;

        if (Main.netMode == NetmodeID.MultiplayerClient)
            NetMessage.SendTileSquare(-1, Terraria.Player.tileTargetX, Terraria.Player.tileTargetY, 1);
    }

    public override IEnumerable<Item> GetItemDrops(int i, int j) {
        Tile t = Main.tile[i, j];
        int style = t.TileFrameY / 18;
        int dropItem = TileLoader.GetItemDropFromTypeAndStyle(TileID.Platforms, style);
        yield return new Item(dropItem);
    }

    public override bool Slope(int i, int j) {
        Tile tile = Framing.GetTileSafely(i, j);
        tile.TileFrameX = (short)((tile.TileFrameX + 18) % 630); //33 * 18
        tile.IsHalfBlock = tile.TileFrameX is 540 or 522 or 504 or 486;
        if (tile.TileFrameX is 144 or 342 or 378 or 414 or 450)
            tile.Slope = SlopeType.SlopeDownRight;
        else if (tile.TileFrameX is 180 or 360 or 396 or 432 or 468)
            tile.Slope = SlopeType.SlopeDownLeft;
        else
            tile.Slope = SlopeType.Solid;

        if (Main.netMode == NetmodeID.MultiplayerClient)
            NetMessage.SendTileSquare(-1, Terraria.Player.tileTargetX, Terraria.Player.tileTargetY, 1);

        SoundEngine.PlaySound(SoundID.MenuTick);
        return false;
    }

    public override bool TileFrame(int i, int j, ref bool resetFrame, ref bool noBreak) {
        return false;
    }
}