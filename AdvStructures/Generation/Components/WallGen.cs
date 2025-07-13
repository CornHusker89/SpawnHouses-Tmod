using System.Linq;
using SpawnHouses.Types;

namespace SpawnHouses.AdvStructures.Generation.Components;

public static class WallGen {
    /// <summary>
    ///     Fills a volume with the same wall blocks, with special blocks at regular intervals
    /// </summary>
    public class WallGenerator1 : IComponentGenerator {
        public ComponentTag[] GetPossibleTags() {
            return [
                ComponentTag.IsWall,
                ComponentTag.External,
                ComponentTag.Elevated,
                ComponentTag.GroundLevel,
                ComponentTag.UnderGround
            ];
        }

        public bool Generate(ComponentParams componentParams) {
            bool elevated = componentParams.TagsRequired.Contains(ComponentTag.Elevated);
            int yStart = componentParams.Volume.BoundingBox.topLeft.Y;
            int[] lowX = new int[componentParams.Volume.Size.Y];
            int[] highX = new int[componentParams.Volume.Size.Y];
            componentParams.Volume.ExecuteInArea((x, y) => {
                PaintedType.PlaceTile(x, y,
                    elevated ? componentParams.TilePalette.WallMainElevated : componentParams.TilePalette.WallMain,
                    componentParams.Tilemap);

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
                PaintedType.PlaceTile(lowX[index], yStart + index, componentParams.TilePalette.WallSpecial, componentParams.Tilemap);
                PaintedType.PlaceTile(highX[index], yStart + index, componentParams.TilePalette.WallSpecial, componentParams.Tilemap);
            }

            return true;
        }
    }

    /// <summary>
    ///     Fills a volume with random wall blocks, with special blocks at regular intervals
    /// </summary>
    public class WallGenerator2 : IComponentGenerator {
        public ComponentTag[] GetPossibleTags() {
            return [
                ComponentTag.IsWall,
                ComponentTag.External,
                ComponentTag.Elevated,
                ComponentTag.GroundLevel,
                ComponentTag.UnderGround
            ];
        }

        public bool Generate(ComponentParams componentParams) {
            bool elevated = componentParams.TagsRequired.Contains(ComponentTag.Elevated);
            int yStart = componentParams.Volume.BoundingBox.topLeft.Y;
            int[] lowX = new int[componentParams.Volume.Size.Y];
            int[] highX = new int[componentParams.Volume.Size.Y];
            componentParams.Volume.ExecuteInArea((x, y) => {
                PaintedType.PlaceTile(x, y, PaintedType.PickRandom(
                        elevated ? componentParams.TilePalette.WallAltElevated : componentParams.TilePalette.WallAlt),
                    componentParams.Tilemap);

                if (lowX[y - yStart] == 0) lowX[y - yStart] = x;
                if (highX[y - yStart] == 0) highX[y - yStart] = x;

                if (x < lowX[y - yStart]) lowX[y - yStart] = x;
                if (x > highX[y - yStart]) highX[y - yStart] = x;
            });

            for (int index = 0; index < lowX.Length; index++) {
                PaintedType.PlaceTile(lowX[index], yStart + index, componentParams.TilePalette.WallSpecial, componentParams.Tilemap);
                PaintedType.PlaceTile(highX[index], yStart + index, componentParams.TilePalette.WallSpecial, componentParams.Tilemap);
            }

            return true;
        }
    }

    /// <summary>
    ///     Fills a volume with random blocks, but the bottom block consistent
    /// </summary>
    public class WallGenerator3 : IComponentGenerator {
        public ComponentTag[] GetPossibleTags() {
            return [
                ComponentTag.IsWall,
                ComponentTag.Elevated,
                ComponentTag.GroundLevel,
                ComponentTag.UnderGround
            ];
        }

        public bool Generate(ComponentParams componentParams) {
            bool elevated = componentParams.TagsRequired.Contains(ComponentTag.Elevated);
            int xStart = componentParams.Volume.BoundingBox.topLeft.X;
            int[] bottomY = new int[componentParams.Volume.Size.X];

            componentParams.Volume.ExecuteInArea((x, y) => {
                PaintedType.PlaceTile(x, y, PaintedType.PickRandom(
                        elevated ? componentParams.TilePalette.WallAltElevated : componentParams.TilePalette.WallAlt),
                    componentParams.Tilemap);

                if (bottomY[x - xStart] == 0)
                    bottomY[x - xStart] = y;

                if (y > bottomY[x - xStart])
                    bottomY[x - xStart] = y;
            });

            for (int index = 0; index < bottomY.Length; index++)
                PaintedType.PlaceTile(xStart + index, bottomY[index],
                    elevated ? componentParams.TilePalette.WallAccentElevated : componentParams.TilePalette.WallAccent,
                    componentParams.Tilemap);

            return true;
        }
    }
}