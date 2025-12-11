#nullable enable
using System.Collections.Generic;
using System.Linq;
using SpawnHouses.AdvStructures.AdvStructureParts;
using SpawnHouses.Helpers;
using SpawnHouses.Types;

namespace SpawnHouses.AdvStructures.Generation.Components;

public static class FloorGen {
    /// <summary>
    ///     Fills a volume floor blocks
    /// </summary>
    [ComponentGenerator(typeof(Floor))]
    public class FloorGenerator1 : VolumeComponentGenerator {
        public override HashSet<ComponentTag> PossibleTags { get; } = new HashSet<ComponentTag>(
            [
                ComponentTag.External
            ])
            .Concat(ComponentHelper.FillShapeTiles.PossibleTags)
            .ToHashSet();

        public override bool Generate(VolumeComponentParams param) {
            bool external = param.Component.TagsRequired.ContainsKey(ComponentTag.External);
            ComponentHelper.FillShapeTiles.Action(param.Component.Volume, param, (_, _) => (external ? param.Palette.ExternalFloor : param.Palette.InternalFloor).Primary);
            return true;
        }
    }

    // /// <summary>
    // ///     Fills a volume with random blocks, but the top block consistent
    // /// </summary>
    // [ComponentGenerator(typeof(Floor))]
    // public class FloorGenerator3 : VolumeComponentGenerator {
    //     public override HashSet<ComponentTag> GetPossibleTags() {
    //         return [
    //             ComponentTag.FloorSolid,
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
    //         int xStart = param.Component.Volume.BoundingBox.topLeft.X;
    //         int[] topY = new int[param.Component.Volume.Size.X];
    //
    //         ComponentHelper.FillShapeTiles.Action(param.Component.Volume, param, elevated ? param.Palette.FloorAlt : param.Palette.FloorAltElevated,
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
    [ComponentGenerator(typeof(Floor))]
    public class FloorGenerator4 : VolumeComponentGenerator {
        public override HashSet<ComponentTag> PossibleTags { get; } = [
            ComponentTag.FloorHollow
        ];

        public override bool CanGenerate(VolumeComponentParams componentParams) {
            return componentParams.Component.Volume.GetDetailedAxisSizes(false).average >= 3;
        }

        public override bool Generate(VolumeComponentParams param) {
            int xStart = param.Component.Volume.BoundingBox.topLeft.X;
            int[] topY = new int[param.Component.Volume.Size.X];
            int[] bottomY = new int[param.Component.Volume.Size.X];
            int supportInterval = Terraria.WorldGen.genRand.Next(3, 5);

            param.Component.Volume.ExecuteInArea((x, y) => {
                if ((x - xStart - 2) % supportInterval == 0 || x == xStart ||
                    x == param.Component.Volume.BoundingBox.bottomRight.X) {
                    param.Tilemap.PlaceTile(x, y, param.Palette.InternalFloor.Vertical);
                }
                else {
                    param.Tilemap.PlaceWall(x, y, param.Palette.InternalFloor.PrimaryBackground);
                    if (Terraria.WorldGen.genRand.Next(0, 3) == 0)
                        param.Tilemap.PlaceTile(x, y, param.Palette.Debris1X1);
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
                param.Tilemap.PlaceTile(xStart + index, topY[index], param.Palette.InternalFloor.Primary);
                param.Tilemap.PlaceTile(xStart + index, bottomY[index], param.Palette.InternalFloor.Primary);
            }

            return true;
        }
    }
}