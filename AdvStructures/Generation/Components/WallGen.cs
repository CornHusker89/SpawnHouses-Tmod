using System.Linq;
using SpawnHouses.Types;

namespace SpawnHouses.AdvStructures.Generation.Components;

public static class WallGen {
    /// <summary>
    ///     Fills a volume with the same wall blocks, with special blocks at the first and last x position of each row
    /// </summary>
    public class WallGenerator1 : IComponentGenerator {
        public ComponentTag[] GetPossibleTags() {
            return [
                ComponentTag.External,
                ComponentTag.Elevated,
                ComponentTag.GroundLevel,
                ComponentTag.UnderGround
            ];
        }

        public bool Generate(ComponentParams componentParams) {
            bool elevated = componentParams.Component.TagsRequired.Contains(ComponentTag.Elevated);

            componentParams.Component.Volume.ExecuteInArea((x, y) => {
                componentParams.Tilemap.PlaceTile(x, y,
                    elevated ? componentParams.TilePalette.WallMainElevated : componentParams.TilePalette.WallMain
                );
            });
            return true;
        }
    }

    /// <summary>
    ///     Fills a volume with random blocks
    /// </summary>
    public class WallGenerator2 : IComponentGenerator {
        public ComponentTag[] GetPossibleTags() {
            return [
                ComponentTag.External,
                ComponentTag.Elevated,
                ComponentTag.GroundLevel,
                ComponentTag.UnderGround
            ];
        }

        public bool Generate(ComponentParams componentParams) {
            bool elevated = componentParams.Component.TagsRequired.Contains(ComponentTag.Elevated);

            componentParams.Component.Volume.ExecuteInArea((x, y) => {
                componentParams.Tilemap.PlaceTile(x, y,
                    PaintedType.PickRandom(elevated ? componentParams.TilePalette.WallAltElevated : componentParams.TilePalette.WallAlt)
                );
            });
            return true;
        }
    }

    /// <summary>
    ///     Fills a volume with the same wall blocks, with special blocks at the first and last x position of each row
    /// </summary>
    public class WallGenerator3 : IComponentGenerator {
        public ComponentTag[] GetPossibleTags() {
            return [
                ComponentTag.External,
                ComponentTag.Elevated,
                ComponentTag.GroundLevel,
                ComponentTag.UnderGround
            ];
        }

        public bool CanGenerate(ComponentParams componentParams) {
            return componentParams.Component.Volume.GetTrueSize(true).average >= 4;
        }

        public bool Generate(ComponentParams componentParams) {
            bool elevated = componentParams.Component.TagsRequired.Contains(ComponentTag.Elevated);
            int yStart = componentParams.Component.Volume.BoundingBox.topLeft.Y;
            int[] lowX = new int[componentParams.Component.Volume.Size.Y];
            int[] highX = new int[componentParams.Component.Volume.Size.Y];
            componentParams.Component.Volume.ExecuteInArea((x, y) => {
                componentParams.Tilemap.PlaceTile(x, y,
                    elevated ? componentParams.TilePalette.WallMainElevated : componentParams.TilePalette.WallMain);

                if (lowX[y - yStart] == 0)
                    lowX[y - yStart] = x;
                if (highX[y - yStart] == 0)
                    highX[y - yStart] = x;

                if (x < lowX[y - yStart])
                    lowX[y - yStart] = x;
                if (x > highX[y - yStart])
                    highX[y - yStart] = x;
            });

            for (int index = 0; index < lowX.Length; index++) {
                componentParams.Tilemap.PlaceTile(lowX[index], yStart + index, componentParams.TilePalette.WallSpecial);
                componentParams.Tilemap.PlaceTile(highX[index], yStart + index, componentParams.TilePalette.WallSpecial);
            }

            return true;
        }
    }

    /// <summary>
    ///     Fills a volume with random wall blocks, with special blocks at the first and last x position of each row
    /// </summary>
    public class WallGenerator4 : IComponentGenerator {
        public ComponentTag[] GetPossibleTags() {
            return [
                ComponentTag.External,
                ComponentTag.Elevated,
                ComponentTag.GroundLevel,
                ComponentTag.UnderGround
            ];
        }

        public bool CanGenerate(ComponentParams componentParams) {
            return componentParams.Component.Volume.GetTrueSize(true).average >= 4;
        }

        public bool Generate(ComponentParams componentParams) {
            bool elevated = componentParams.Component.TagsRequired.Contains(ComponentTag.Elevated);
            int yStart = componentParams.Component.Volume.BoundingBox.topLeft.Y;
            int[] lowX = new int[componentParams.Component.Volume.Size.Y];
            int[] highX = new int[componentParams.Component.Volume.Size.Y];
            componentParams.Component.Volume.ExecuteInArea((x, y) => {
                componentParams.Tilemap.PlaceTile(x, y, PaintedType.PickRandom(
                    elevated ? componentParams.TilePalette.WallAltElevated : componentParams.TilePalette.WallAlt));

                if (lowX[y - yStart] == 0) lowX[y - yStart] = x;
                if (highX[y - yStart] == 0) highX[y - yStart] = x;

                if (x < lowX[y - yStart]) lowX[y - yStart] = x;
                if (x > highX[y - yStart]) highX[y - yStart] = x;
            });

            for (int index = 0; index < lowX.Length; index++) {
                componentParams.Tilemap.PlaceTile(lowX[index], yStart + index, componentParams.TilePalette.WallSpecial);
                componentParams.Tilemap.PlaceTile(highX[index], yStart + index, componentParams.TilePalette.WallSpecial);
            }

            return true;
        }
    }

    /// <summary>
    ///     Fills a volume with random blocks, but the bottom block consistent
    /// </summary>
    public class WallGenerator5 : IComponentGenerator {
        public ComponentTag[] GetPossibleTags() {
            return [
                ComponentTag.Elevated,
                ComponentTag.GroundLevel,
                ComponentTag.UnderGround
            ];
        }

        public bool Generate(ComponentParams componentParams) {
            bool elevated = componentParams.Component.TagsRequired.Contains(ComponentTag.Elevated);
            int xStart = componentParams.Component.Volume.BoundingBox.topLeft.X;
            int[] bottomY = new int[componentParams.Component.Volume.Size.X];

            componentParams.Component.Volume.ExecuteInArea((x, y) => {
                componentParams.Tilemap.PlaceTile(x, y, PaintedType.PickRandom(
                    elevated ? componentParams.TilePalette.WallAltElevated : componentParams.TilePalette.WallAlt));

                if (bottomY[x - xStart] == 0)
                    bottomY[x - xStart] = y;

                if (y > bottomY[x - xStart])
                    bottomY[x - xStart] = y;
            });

            for (int index = 0; index < bottomY.Length; index++)
                componentParams.Tilemap.PlaceTile(xStart + index, bottomY[index],
                    elevated ? componentParams.TilePalette.WallAccentElevated : componentParams.TilePalette.WallAccent);

            return true;
        }
    }
}