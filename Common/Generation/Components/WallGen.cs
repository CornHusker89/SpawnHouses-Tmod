using System.Collections.Generic;
using SpawnHouses.Common.Modules;
using SpawnHouses.Common.Modules.Components;
using SpawnHouses.Common.Palette;
using SpawnHouses.Common.Parameters;
using SpawnHouses.Common.Tagging;
using SpawnHouses.Common.Tiles;
using SpawnHouses.Common.Types;
using SpawnHouses.Helpers.Complex;
using Terraria.Utilities;

namespace SpawnHouses.Common.Generation.Components;

public static class WallGen {
    /// <summary>
    ///     Fills the volume with primary walls
    /// </summary>
    [ModuleGenerator(typeof(Wall))]
    public class WallGenerator1 : VolumeComponentGenerator {
        public override HashSet<Tag> PossibleTags { get; } = TagMap.NewTagSet(
            [
                Tags.External
            ],
            ComponentHelper.FillShapeTiles.PossibleTags,
            ComponentHelper.FillShapeWalls.PossibleTags
        );

        public override bool Generate(VolumeComponent component, VolumeComponentParams param, UnifiedRandom random, TilePalette palette, StructureTilemap tilemap) {
            bool external = param.TagsRequired.HasTag(Tags.External);
            if (external)
                component.TagsCurrent.Add(Tags.External);
            ComponentHelper.FillShapeTiles.Action(component, component.Geometry, (_, _) => (external ? palette.ExternalWall : palette.InternalWall).Primary);
            ComponentHelper.FillShapeRedundantWalls.Action(component.Geometry, param.Structure, palette.InternalWall.PrimaryBackground);
            return true;
        }
    }

    /// <summary>
    ///     Fills a volume with random wall blocks, with special vertical blocks at the first and last x position of every other row
    /// </summary>
    [ModuleGenerator(typeof(Wall))]
    public class WallGenerator2 : VolumeComponentGenerator {
        public override HashSet<Tag> PossibleTags { get; } = TagMap.NewTagSet(
            [
                Tags.External
            ],
            ComponentHelper.FillShapeTiles.PossibleTags,
            ComponentHelper.FillShapeWalls.PossibleTags
        );

        public override bool Generate(VolumeComponent component, VolumeComponentParams param, UnifiedRandom random, TilePalette palette, StructureTilemap tilemap) {
            bool external = param.TagsRequired.HasTag(Tags.External);
            int yStart = component.Geometry.BoundingBox.Top;
            int yEnd = component.Geometry.BoundingBox.Bottom;
            int[] lowX = new int[component.Geometry.BoundingBox.Height];
            int[] highX = new int[component.Geometry.BoundingBox.Height];
            component.Geometry.ExecuteInArea((x, y) => {
                // replace default values
                if (lowX[y - yStart] == 0) lowX[y - yStart] = x;
                if (highX[y - yStart] == 0) highX[y - yStart] = x;

                // move values to true positions
                if (x < lowX[y - yStart]) lowX[y - yStart] = x;
                if (x > highX[y - yStart]) highX[y - yStart] = x;
            });

            if (external)
                component.TagsCurrent.Add(Tags.External);
            
            ComponentHelper.FillShapeTiles.Action(component, component.Geometry, (x, y) =>
                // make sure that: not at very top or bottom, every other line, either highest x or lowest x
                y != yStart && y != yEnd && y % 2 == 0 && (x == lowX[y - yStart] || x == highX[y - yStart])
                    ? (external ? palette.ExternalWall : palette.InternalWall).VerticalDetail
                    : (external ? palette.ExternalWall : palette.InternalWall).Primary
            );

            ComponentHelper.FillShapeRedundantWalls.Action(component.Geometry, param.Structure, palette.InternalWall.PrimaryBackground);
            return true;
        }
    }
}