using System.Collections.Generic;
using SpawnHouses.Types;

namespace SpawnHouses.AdvStructures.Generation.Components;

public static class WallGen {
    /// <summary>
    ///     Fills a volume with the same wall blocks, with special blocks at the first and last x position of each row
    /// </summary>
    public class WallGenerator1 : IVolumeComponentGenerator {
        public HashSet<ComponentTag> GetPossibleTags() {
            return [
                ComponentTag.External,
                ComponentTag.Elevated,
                ComponentTag.GroundLevel,
                ComponentTag.UnderGround
            ];
        }

        public bool Generate(VolumeComponentParams componentParams) {
            bool elevated = componentParams.Component.TagsRequired.Contains(ComponentTag.Elevated);

            componentParams.Component.Volume.ExecuteInArea((x, y) => {
                componentParams.Tilemap.PlaceTile(x, y,
                    elevated ? componentParams.Palette.WallMainElevated : componentParams.Palette.WallMain
                );
            });
            return true;
        }
    }

    /// <summary>
    ///     Fills a volume with random blocks
    /// </summary>
    public class WallGenerator2 : IVolumeComponentGenerator {
        public HashSet<ComponentTag> GetPossibleTags() {
            return [
                ComponentTag.External,
                ComponentTag.Elevated,
                ComponentTag.GroundLevel,
                ComponentTag.UnderGround
            ];
        }

        public bool Generate(VolumeComponentParams componentParams) {
            bool elevated = componentParams.Component.TagsRequired.Contains(ComponentTag.Elevated);

            componentParams.Component.Volume.ExecuteInArea((x, y) => {
                componentParams.Tilemap.PlaceTile(x, y,
                    PaintedType.PickRandom(elevated ? componentParams.Palette.WallAltElevated : componentParams.Palette.WallAlt)
                );
            });
            return true;
        }
    }

    /// <summary>
    ///     Fills a volume with the same wall blocks, with special blocks at the first and last x position of each row
    /// </summary>
    public class WallGenerator3 : IVolumeComponentGenerator {
        public HashSet<ComponentTag> GetPossibleTags() {
            return [
                ComponentTag.External,
                ComponentTag.Elevated,
                ComponentTag.GroundLevel,
                ComponentTag.UnderGround
            ];
        }

        public bool CanGenerate(VolumeComponentParams componentParams) {
            return componentParams.Component.Volume.GetTrueSize(true).average >= 4;
        }

        public bool Generate(VolumeComponentParams componentParams) {
            bool elevated = componentParams.Component.TagsRequired.Contains(ComponentTag.Elevated);
            int yStart = componentParams.Component.Volume.BoundingBox.topLeft.Y;
            int[] lowX = new int[componentParams.Component.Volume.Size.Y];
            int[] highX = new int[componentParams.Component.Volume.Size.Y];
            componentParams.Component.Volume.ExecuteInArea((x, y) => {
                componentParams.Tilemap.PlaceTile(x, y,
                    elevated ? componentParams.Palette.WallMainElevated : componentParams.Palette.WallMain);

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
                componentParams.Tilemap.PlaceTile(lowX[index], yStart + index, componentParams.Palette.WallSpecial);
                componentParams.Tilemap.PlaceTile(highX[index], yStart + index, componentParams.Palette.WallSpecial);
            }

            return true;
        }
    }

    /// <summary>
    ///     Fills a volume with random wall blocks, with special blocks at the first and last x position of each row
    /// </summary>
    public class WallGenerator4 : IVolumeComponentGenerator {
        public HashSet<ComponentTag> GetPossibleTags() {
            return [
                ComponentTag.External,
                ComponentTag.Elevated,
                ComponentTag.GroundLevel,
                ComponentTag.UnderGround
            ];
        }

        public bool CanGenerate(VolumeComponentParams componentParams) {
            return componentParams.Component.Volume.GetTrueSize(true).average >= 4;
        }

        public bool Generate(VolumeComponentParams componentParams) {
            bool elevated = componentParams.Component.TagsRequired.Contains(ComponentTag.Elevated);
            int yStart = componentParams.Component.Volume.BoundingBox.topLeft.Y;
            int[] lowX = new int[componentParams.Component.Volume.Size.Y];
            int[] highX = new int[componentParams.Component.Volume.Size.Y];
            componentParams.Component.Volume.ExecuteInArea((x, y) => {
                componentParams.Tilemap.PlaceTile(x, y, PaintedType.PickRandom(
                    elevated ? componentParams.Palette.WallAltElevated : componentParams.Palette.WallAlt));

                if (lowX[y - yStart] == 0) lowX[y - yStart] = x;
                if (highX[y - yStart] == 0) highX[y - yStart] = x;

                if (x < lowX[y - yStart]) lowX[y - yStart] = x;
                if (x > highX[y - yStart]) highX[y - yStart] = x;
            });

            for (int index = 0; index < lowX.Length; index++) {
                componentParams.Tilemap.PlaceTile(lowX[index], yStart + index, componentParams.Palette.WallSpecial);
                componentParams.Tilemap.PlaceTile(highX[index], yStart + index, componentParams.Palette.WallSpecial);
            }

            return true;
        }
    }

    /// <summary>
    ///     Fills a volume with random blocks, but the bottom block consistent
    /// </summary>
    public class WallGenerator5 : IVolumeComponentGenerator {
        public HashSet<ComponentTag> GetPossibleTags() {
            return [
                ComponentTag.Elevated,
                ComponentTag.GroundLevel,
                ComponentTag.UnderGround
            ];
        }

        public bool Generate(VolumeComponentParams componentParams) {
            bool elevated = componentParams.Component.TagsRequired.Contains(ComponentTag.Elevated);
            int xStart = componentParams.Component.Volume.BoundingBox.topLeft.X;
            int[] bottomY = new int[componentParams.Component.Volume.Size.X];

            componentParams.Component.Volume.ExecuteInArea((x, y) => {
                componentParams.Tilemap.PlaceTile(x, y, PaintedType.PickRandom(
                    elevated ? componentParams.Palette.WallAltElevated : componentParams.Palette.WallAlt));

                if (bottomY[x - xStart] == 0)
                    bottomY[x - xStart] = y;

                if (y > bottomY[x - xStart])
                    bottomY[x - xStart] = y;
            });

            for (int index = 0; index < bottomY.Length; index++)
                componentParams.Tilemap.PlaceTile(xStart + index, bottomY[index],
                    elevated ? componentParams.Palette.WallAccentElevated : componentParams.Palette.WallAccent);

            return true;
        }
    }
}