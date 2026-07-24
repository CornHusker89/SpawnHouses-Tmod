#nullable enable
using System.Collections.Generic;
using SpawnHouses.Content.Modules.Components;
using SpawnHouses.Content.Palette;
using SpawnHouses.Content.Parameters;
using SpawnHouses.Content.Tagging;
using SpawnHouses.Content.Tiles;
using SpawnHouses.Content.Types;
using SpawnHouses.Content.Types.Attributes;
using SpawnHouses.Content.Types.Interfaces;
using SpawnHouses.Helpers.Complex;
using Terraria.Utilities;

namespace SpawnHouses.Content.Generation.Components;

public static class FloorGen {
    /// <summary>
    ///     Fills a volume floor blocks
    /// </summary>
    [AdvGeneratorLoadable(typeof(Floor), false)]
    public class FloorGenerator1 : VolumeComponentAdvGenerator {
        public override HashSet<Tag> PossibleTags { get; } = TagMap.NewTagSet(
            [
                Tags.External
            ],
            ComponentHelper.FillShapeTiles.PossibleTags
        );

        public override bool Generate(VolumeComponent component, VolumeComponentParams param, UnifiedRandom random, TilePalette palette, StructureTilemap tilemap) {
            bool external = param.TagsRequired.HasTag(Tags.External);
            ComponentHelper.FillShapeTiles.Action(component, component.Geometry, (_, _) =>
                (external ? palette.ExternalFloor : palette.InternalFloor).Primary);

            if (external)
                component.TagsCurrent.Add(Tags.External);
            ComponentHelper.FillShapeRedundantWalls.Action(component.Geometry, param.Structure, palette.InternalFloor.PrimaryBackground);
            return true;
        }
    }

    // /// <summary>
    // ///     Fills a volume with random blocks, but the top block consistent
    // /// </summary>
    // [ComponentGenerator(typeof(Floor))]
    // public class FloorGenerator3 : VolumeComponentAdvGenerator {
    //     public override HashSet<ComponentTag> GetPossibleTags() {
    //         return [
    //             ComponentTag.Floor_Solid,
    //             ComponentTag.External,
    //             ComponentTag.Elevated,
    //             ComponentTag.GroundLevel,
    //             ComponentTag.UnderGround,
    //
    //             // from FillShape
    //             ComponentTag.ApplySloping,
    //             ComponentTag.SlopingModifier
    //         ];
    //     }
    //
    //     public override bool CanGenerate(VolumeComponentParams componentParams) {
    //         return componentParams.Component.Volume.Size.Y >= 2;
    //     }
    //
    //     public override bool Generate(VolumeComponentParams param) {
    //         bool elevated = param.Component.TagsRequired.ContainsKey(ComponentTag.Elevated);
    //         int xStart = component.Geometry.BoundingBox.topLeft.X;
    //         int[] topY = new int[component.Geometry.Size.X];
    //
    //         ComponentHelper.FillShapeTiles.Action(component.Geometry, param, elevated ? param.Palette.FloorAlt : param.Palette.FloorAltElevated,
    //             (x, y) => {
    //                 if (topY[x - xStart] == 0)
    //                     topY[x - xStart] = y;
    //
    //                 if (y < topY[x - xStart])
    //                     topY[x - xStart] = y;
    //                 return true;
    //             });
    //
    //         for (int index = 0; index < topY.Length; index++)
    //             param.Tilemap.SoftPlaceTile(
    //                 xStart + index, topY[index],
    //                 elevated ? param.Palette.FloorMainElevated : param.Palette.FloorMain
    //             );
    //         return true;
    //     }
    // }

    /// <summary>
    ///     Fills top and bottom of volume, adds support struts in the middle
    /// </summary>
    [AdvGeneratorLoadable(typeof(Floor), false)]
    public class FloorGenerator4 : VolumeComponentAdvGenerator {
        public override HashSet<Tag> PossibleTags { get; } = TagMap.NewTagSet(
            [
                Tags.Floor_Hollow
            ]
        );

        public override bool CanGenerate(VolumeComponent component, VolumeComponentParams param, UnifiedRandom random) => component.Geometry.GetDetailedAxisSizes(false).average >= 3 && param.TagsRequired.HasTag(Tags.Floor_Hollow);

        public override bool Generate(VolumeComponent component, VolumeComponentParams param, UnifiedRandom random, TilePalette palette, StructureTilemap tilemap) {
            int xStart = component.Geometry.BoundingBox.Left;
            int[] topY = new int[component.Geometry.BoundingBox.Width];
            int[] bottomY = new int[component.Geometry.BoundingBox.Width];
            int supportInterval = random.Next(3, 5);

            component.Geometry.ExecuteInArea((x, y) => {
                if ((x - xStart - 2) % supportInterval == 0 || x == xStart ||
                    x == component.Geometry.BoundingBox.Right) {
                    tilemap.PlaceTile(x, y, palette.InternalFloor.Vertical);
                }
                else {
                    tilemap.PlaceWall(x, y, palette.InternalFloor.PrimaryBackground);
                    if (random.Next(0, 3) == 0)
                        tilemap.PlaceTile(x, y, palette.Debris1X1);
                }

                if (topY[x - xStart] == 0)
                    topY[x - xStart] = y;
                if (bottomY[x - xStart] == 0)
                    bottomY[x - xStart] = y;

                if (y < topY[x - xStart])
                    topY[x - xStart] = y;
                if (y > bottomY[x - xStart])
                    bottomY[x - xStart] = y;
            });

            for (int index = 0; index < topY.Length; index++) {
                tilemap.PlaceTile(xStart + index, topY[index], palette.InternalFloor.Primary);
                tilemap.PlaceTile(xStart + index, bottomY[index], palette.InternalFloor.Primary);
            }

            component.TagsCurrent.Add(Tags.Floor_Hollow);
            
            ComponentHelper.FillShapeRedundantWalls.Action(component.Geometry, param.Structure, palette.InternalFloor.PrimaryBackground);

            return true;
        }
    }
}