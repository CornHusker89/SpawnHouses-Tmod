#nullable enable
using System;
using System.Collections.Generic;
using SpawnHouses.AdvStructures.AdvStructureParts;
using SpawnHouses.Common;
using SpawnHouses.Structures;
using SpawnHouses.Types.Palette;
using Terraria;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;

namespace SpawnHouses.Items.Debug;

public class SpawnTest : ModItem {
    public static AdvStructure Structure;

    public override void SetDefaults() {
        Item.useStyle = ItemUseStyleID.Swing;
        Item.useTime = 20;
        Item.useAnimation = 20;
        Item.rare = ItemRarityID.Blue;
    }

    public override bool AltFunctionUse(Player player) => true;

    public override bool? UseItem(Player player) {
        int x = (Main.MouseWorld / 16).ToPoint16().X;
        int y = (Main.MouseWorld / 16).ToPoint16().Y;

        Console.WriteLine(x + ", " + y);

        // full house
        // float scale = Terraria.WorldGen.genRand.NextFloat();
        // Structure = new AdvStructure(
        //     new StructureParams(
        //         new Dictionary<StructureTag, object?>([
        //             new KeyValuePair<StructureTag, object?>(StructureTag.HasRooms, new Range(5, 7).Evaluate(scale)),
        //             new KeyValuePair<StructureTag, object?>(StructureTag.HasHousing, new Range(5, 7).Evaluate(scale))
        //         ]),
        //         [],
        //         [
        //             new EntryPoint(
        //                 new Point16(x, y - 2),
        //                 3,
        //                 Directions.Right
        //             ),
        //             new EntryPoint(
        //                 new Point16(x + 25, y - 8),
        //                 3,
        //                 Directions.Left
        //             )
        //         ],
        //         TilePalette.Medieval,
        //         new Range(350, 500).Evaluate(scale),
        //         true
        //     )
        // );

        // 1-room house
        AdvStructure structure = new(
            new StructureParams(
                new StructureTagSet([
                    new KeyValuePair<StructureTag, object?>(StructureTag.HasRooms, 1),
                    new KeyValuePair<StructureTag, object?>(StructureTag.HasHousing, 1),
                    new KeyValuePair<StructureTag, object?>(StructureTag.HasOnlyRectangleRooms, null)
                ]),
                [
                    new EntryPoint(
                        new Point16(x, y - 2),
                        3,
                        Directions.Right
                    ),
                    new EntryPoint(
                        new Point16(x + 28, y - 2),
                        3,
                        Directions.Left
                    )
                ],
                200,
                false
            ),
            TilePalette.Medieval
        );

        return true;
    }
}