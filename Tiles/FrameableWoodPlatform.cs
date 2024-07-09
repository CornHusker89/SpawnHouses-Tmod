using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.Audio;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.ObjectData;

namespace SpawnHouses.Tiles;

public class FrameableWoodPlatform : ModTile
{
    public override void SetStaticDefaults()
    {
        Main.tileLighted[Type] = true;
        Main.tileFrameImportant[Type] = true;
        Main.tileSolidTop[Type] = true;
        Main.tileSolid[Type] = true;
        Main.tileNoAttach[Type] = true;
        Main.tileTable[Type] = true;
        Main.tileLavaDeath[Type] = true;
        Main.tileNoSunLight[Type] = true;
        
        TileID.Sets.CanBeSloped[Type] = true;
        
        
        DustType = DustID.BorealWood_Small;
        AddMapEntry(new Color(200, 200, 200));
        // Set other values here
        
        TileObjectData.newTile.FullCopyFrom(TileID.Platforms);
        TileObjectData.addTile(Type);
    }
    
    public override void PlaceInWorld(int i, int j, Item item) {
        int style = Main.LocalPlayer.HeldItem.placeStyle;
        Tile tile = Main.tile[i, j];
        tile.TileFrameY = (short)(style * 18);
        if (Main.netMode == NetmodeID.MultiplayerClient) {
            NetMessage.SendTileSquare(-1, Player.tileTargetX, Player.tileTargetY, 1, TileChangeType.None);
        }
    }
    
    public override IEnumerable<Item> GetItemDrops(int i, int j) {
        Tile t = Main.tile[i, j];
        int style = t.TileFrameY / 18;
        int dropItem = TileLoader.GetItemDropFromTypeAndStyle(TileID.Platforms, style);
        yield return new Item(dropItem);
    }
    
    public override bool Slope(int i, int j) {
        Tile tile = Framing.GetTileSafely(i, j);
        tile.TileFrameX = (short)((tile.TileFrameX + 18) % 486); //27 * 18
        if (Main.netMode == NetmodeID.MultiplayerClient) {
            NetMessage.SendTileSquare(-1, Player.tileTargetX, Player.tileTargetY, 1, TileChangeType.None);
        }
        SoundEngine.PlaySound(SoundID.MenuTick);
        return false;
    }
    
    public override bool TileFrame(int i, int j, ref bool resetFrame, ref bool noBreak) {
        return false;
    }
}
