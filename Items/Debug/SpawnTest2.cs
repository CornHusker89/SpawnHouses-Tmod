using System;
using SpawnHouses.AdvStructures.AdvStructureParts;
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

    public override bool AltFunctionUse(Player player) {
        return true;
    }

    public override bool? UseItem(Player player) {
        int x = (Main.MouseWorld / 16).ToPoint16().X;
        int y = (Main.MouseWorld / 16).ToPoint16().Y;

        Shape s = new(
            new Point16(x, y),
            new Point16(x + 20, y),
            new Point16(x + 40, y + 20),
            new Point16(x + 38, y + 20),
            new Point16(x, y + 17)
        );

        Console.WriteLine("shape points: ");
        Console.WriteLine(s);

        s.ExecuteInArea((xNew, yNew) => { Terraria.WorldGen.PlaceTile(xNew, yNew, TileID.AmberGemspark); });

        foreach (Point16 p in s.Points) Terraria.WorldGen.PlaceTile(p.X, p.Y, TileID.EmeraldGemspark);

        var lst = s.GetCorners();
        Console.WriteLine("final corners:");
        foreach (PartialPoint16 a in lst) Console.WriteLine(a);

        return true;
    }
}