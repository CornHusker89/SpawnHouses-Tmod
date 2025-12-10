using System.Collections.Generic;
using System.Linq;
using SpawnHouses.AdvStructures.AdvStructureParts;
using SpawnHouses.Helpers;
using SpawnHouses.Types;

namespace SpawnHouses.AdvStructures.Generation.Components;

public static class WallGen {
    /// <summary>
    ///     Fills the volume with primary walls
    /// </summary>
    [ComponentGenerator(typeof(Wall))]
    public class WallGenerator1 : VolumeComponentGenerator {
        public new readonly HashSet<ComponentTag> PossibleTags = new HashSet<ComponentTag>(
            [
                ComponentTag.External
            ])
            .Concat(ComponentHelper.FillShapeTiles.PossibleTags)
            .ToHashSet();

        public override bool Generate(VolumeComponentParams param) {
            bool external = param.Component.TagsRequired.ContainsKey(ComponentTag.External);
            ComponentHelper.FillShapeTiles.Action(param.Component.Volume, param, (_, _) => (external ? param.Palette.ExternalWall : param.Palette.InternalWall).Primary);
            return true;
        }
    }

    /// <summary>
    ///     Fills a volume with random wall blocks, with special vertical blocks at the first and last x position of every other row
    /// </summary>
    [ComponentGenerator(typeof(Wall))]
    public class WallGenerator2 : VolumeComponentGenerator {
        public new readonly HashSet<ComponentTag> PossibleTags = new HashSet<ComponentTag>(
            [
                ComponentTag.External
            ])
            .Concat(ComponentHelper.FillShapeTiles.PossibleTags)
            .ToHashSet();

        public override bool Generate(VolumeComponentParams param) {
            bool external = param.Component.TagsRequired.ContainsKey(ComponentTag.External);
            int yStart = param.Component.Volume.BoundingBox.topLeft.Y;
            int yEnd = param.Component.Volume.BoundingBox.bottomRight.Y;
            int[] lowX = new int[param.Component.Volume.Size.Y];
            int[] highX = new int[param.Component.Volume.Size.Y];
            param.Component.Volume.ExecuteInArea((x, y) => {
                // replace default values
                if (lowX[y - yStart] == 0) lowX[y - yStart] = x;
                if (highX[y - yStart] == 0) highX[y - yStart] = x;

                // move values to true positions
                if (x < lowX[y - yStart]) lowX[y - yStart] = x;
                if (x > highX[y - yStart]) highX[y - yStart] = x;
            });

            ComponentHelper.FillShapeTiles.Action(param.Component.Volume, param, (x, y) =>
                // make sure that: not at very top or bottom, every other line, either highest x or lowest x
                y != yStart && y != yEnd && y % 2 == 0 && (x == lowX[y - yStart] || x == highX[y - yStart])
                    ? (external ? param.Palette.ExternalWall : param.Palette.InternalWall).VerticalDetail
                    : (external ? param.Palette.ExternalWall : param.Palette.InternalWall).Primary
            );

            return true;
        }
    }
}