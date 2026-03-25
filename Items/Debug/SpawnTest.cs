#nullable enable
using System;
using SpawnHouses.Common;
using SpawnHouses.Common.Palette;
using SpawnHouses.Common.Parameters;
using SpawnHouses.Common.Tagging;
using SpawnHouses.Common.Types;
using SpawnHouses.Structures;
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

    public override bool AltFunctionUse(Player player) => true;

    public override bool? UseItem(Player player) {
        int x = (Main.MouseWorld / 16).ToPoint16().X;
        int y = (Main.MouseWorld / 16).ToPoint16().Y;

        Console.WriteLine(x + ", " + y);

        // full house
        float scale = WorldGen.genRand.NextFloat();
        TagMap requiredTags = new();
        requiredTags.Add(Tags.HasRooms, new Range(5, 7).Evaluate(scale));
        requiredTags.Add(Tags.HasHousing, new Range(5, 7).Evaluate(scale));
        Structure = new AdvStructure(
            "Test",
            new StructureLayoutParams(
                requiredTags,
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
                new Range(350, 500).Evaluate(scale),
                false
            ),
            PalettePresets.Medieval,
            generate: true
        );

        // Point16 left = new(x, y - 15);
        // Point16 right = new(x + 25, y - 15);
        // var points = StructureLayoutHelper.CreateRoof.WavyPeak(Structure, left, right);
        // Path path = new(points);
        // Shape shape = path.ToShape(1);
        // shape.ExecuteInArea((x2, y2) => {
        //     Tile tile = Main.tile[x2, y2];
        //     tile.HasTile = true;
        //     tile.TileType = TileID.AmberGemspark;
        // });
        // foreach (Point16 point in points) {
        //     Tile tile = Main.tile[point.X, point.Y];
        //     tile.HasTile = true;
        //     tile.TileType = TileID.EmeraldGemspark;
        // }

        // 1-room house
        // TagMap requiredTags = new();
        // requiredTags.Add(Tags.HasRooms, 1);
        // requiredTags.Add(Tags.HasHousing, 1);
        // requiredTags.Add(Tags.HasOnlyRectangleRooms);
        //
        // AdvStructure structure = new(
        //     new StructureLayoutParams(
        //         requiredTags,
        //         [
        //             new EntryPoint(
        //                 new Point16(x, y - 2),
        //                 3,
        //                 Directions.Right
        //             ),
        //             new EntryPoint(
        //                 new Point16(x + 28, y - 2),
        //                 3,
        //                 Directions.Left
        //             )
        //         ],
        //         200,
        //         false
        //     ),
        //     TilePalette.Medieval
        // );

        return true;
    }
}