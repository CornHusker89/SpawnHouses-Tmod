using System;
using SpawnHouses.AdvStructures;
using SpawnHouses.AdvStructures.AdvStructureParts;
using SpawnHouses.Structures;
using SpawnHouses.Types;
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

    public override bool AltFunctionUse(Terraria.Player player) {
        return true;
    }

    public override bool? UseItem(Terraria.Player player) {
        int x = (Main.MouseWorld / 16).ToPoint16().X;
        int y = (Main.MouseWorld / 16).ToPoint16().Y;

        // Point16 point = SpawnTest.Structure.Tilemap.ConvertToRelative(x, y);
        // Console.WriteLine(point);
        // if (SpawnTest.Structure.Tilemap.InBounds(point))
        //     Console.WriteLine(SpawnTest.Structure.Tilemap[point].IsInside);

        Shape s = new Shape(
            new Point16(x, y),
            new Point16(x, y),
            new Point16(x, y + 8),
            new Point16(x + 10, y + 8),
            new Point16(x + 10, y + 15),
            new Point16(x, y + 15)
        );

        Console.WriteLine(s);

        s.ExecuteInArea((x, y) => Terraria.WorldGen.PlaceTile(x, y, TileID.EmeraldGemspark));


        return true;
    }
}