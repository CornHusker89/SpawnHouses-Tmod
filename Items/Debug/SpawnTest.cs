using System;
using SpawnHouses.AdvStructures;
using SpawnHouses.Types;
using Terraria;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;
using Range = SpawnHouses.Structures.Range;

namespace SpawnHouses.Items.Debug;

public class SpawnTest : ModItem {
    public override void SetDefaults() {
        Item.useStyle = ItemUseStyleID.Swing;
        Item.useTime = 20;
        Item.useAnimation = 20;
        Item.rare = ItemRarityID.Blue;
    }

    public override bool AltFunctionUse(Terraria.Player player) {
        return true;
    }

    // var roomLayoutParams = new RoomLayoutParams([], [], new Shape(new Point16(x, y), new Point16(x + length, y - length)),
    // 	palette, 11, new Range(4, 18), new Range(7, 24), new Range(1, 2), new Range(1, 2), 0.3f, 1);

    // WorldUtils.Gen(new Point(x + length / 2, y - length), new Shapes.Circle((length + 50) / 2), Actions.Chain(
    //     new Actions.SetFrames(),
    //     new Actions.Custom((i, j, args) => {
    //         Framing.WallFrame(i, j);
    //         return true;
    //     })
    // ));

    public override bool? UseItem(Terraria.Player player) {
        int x = (Main.MouseWorld / 16).ToPoint16().X;
        int y = (Main.MouseWorld / 16).ToPoint16().Y;

        Console.WriteLine(x + ", " + y);

        StructureParams structureLayoutParams = new StructureParams(
            TilePalette.Palette1,
            [StructureTag.IsStructure, StructureTag.HasHousing],
            [],
            new Point16(x, y),
            new Point16(x + 25, y),
            new Range(600, 1000),
            new Range(9, 15)
        );

        AdvStructure structure = new AdvStructure(structureLayoutParams);
        structure.ApplyLayoutMethod();
        structure.FillComponents();

        return true;
    }
}