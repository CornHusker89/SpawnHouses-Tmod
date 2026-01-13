using SpawnHouses.Common;
using SpawnHouses.Common.Modules.Components;
using SpawnHouses.Common.Types;
using SpawnHouses.Helpers.Complex;

namespace SpawnHouses.AdvStructures.Generation.Components;

public static class WallGen {
    /// <summary>
    ///     Fills the volume with primary walls
    /// </summary>
    [InstanceGenerator(typeof(Wall))]
    public class WallGenerator1 : VolumeComponentGenerator {
        public override HashSet<Tag> PossibleTags { get; } = ComponentTagSystem.NewPartialTagSet(
            [
                Tags.External
            ],
            ComponentHelper.FillShapeTiles.PossibleTags
        );

        public override bool Generate(VolumeComponentParams param) {
            bool external = param.Component.Required.ContainsKey(Tags.External);
            ComponentHelper.FillShapeTiles.Action(param.Component.Volume, param, (_, _) => (external ? param.Palette.ExternalWall : param.Palette.InternalWall).Primary);
            return true;
        }
    }

    /// <summary>
    ///     Fills a volume with random wall blocks, with special vertical blocks at the first and last x position of every other row
    /// </summary>
    [InstanceGenerator(typeof(Wall))]
    public class WallGenerator2 : VolumeComponentGenerator {
        public override HashSet<Tag> PossibleTags { get; } = ComponentTagSystem.NewPartialTagSet(
            [
                Tags.External
            ]
        );

        public override bool Generate(VolumeComponentParams param) {
            bool external = param.Component.Required.ContainsKey(Tags.External);
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