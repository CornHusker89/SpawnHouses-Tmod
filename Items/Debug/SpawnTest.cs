#nullable enable
using System;
using System.Collections.Generic;
using SpawnHouses.AdvStructures;
using SpawnHouses.AdvStructures.AdvStructureParts;
using SpawnHouses.Structures;
using SpawnHouses.Types;
using SpawnHouses.Types.Palette;
using Terraria;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;
using Range = SpawnHouses.Structures.Range;

namespace SpawnHouses.Items.Debug;

public class SpawnTest : ModItem {
    public static AdvStructure Structure;

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

        Console.WriteLine(x + ", " + y);

        float scale = Terraria.WorldGen.genRand.NextFloat();
        Structure = new AdvStructure(
            new StructureParams(
                new Dictionary<StructureTag, object?>([
                    new KeyValuePair<StructureTag, object?>(StructureTag.HasRooms, new Range(5, 7).Evaluate(scale)),
                    new KeyValuePair<StructureTag, object?>(StructureTag.HasHousing, new Range(5, 7).Evaluate(scale))
                ]),
                [],
                [
                    new EntryPoint(
                        new Point16(x, y - 2),
                        3,
                        Directions.Right
                    ),
                    new EntryPoint(
                        new Point16(x + 25, y - 8),
                        3,
                        Directions.Left
                    )
                ],
                TilePalette.Palette1,
                new Range(350, 500).Evaluate(scale),
                true
            )
        );

        return true;
    }
}