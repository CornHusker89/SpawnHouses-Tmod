using System.Collections.Generic;
using SpawnHouses.Types;

namespace SpawnHouses.AdvStructures.Generation.Components;

public class GapGen {
    /// <summary>
    ///     A gap floor, places platforms
    /// </summary>
    public class FloorGapGenerator1 : IVolumeComponentGenerator {
        public HashSet<ComponentTag> GetPossibleTags() {
            return [
                ComponentTag.IsFloorGap,
                ComponentTag.Elevated,
                ComponentTag.GroundLevel,
                ComponentTag.UnderGround,
                ComponentTag.External
            ];
        }

        public bool Generate(VolumeComponentParams componentParams) {
            int xStart = componentParams.Component.Volume.BoundingBox.topLeft.X;
            int[] topY = new int[componentParams.Component.Volume.Size.X];
            int[] bottomY = new int[componentParams.Component.Volume.Size.X];
            bool placeWalls = !componentParams.Component.TagsRequired.Contains(ComponentTag.External);

            componentParams.Component.Volume.ExecuteInArea((x, y) => {
                if (placeWalls)
                    componentParams.Tilemap.PlaceWall(x, y, componentParams.Palette.BackgroundFloorMain);
                StructureTile tile = componentParams.Tilemap[x, y];
                tile.HasTile = false;

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
                componentParams.Tilemap.PlaceTile(xStart + index, topY[index], componentParams.Palette.Platform);
                componentParams.Tilemap.PlaceTile(xStart + index, bottomY[index], componentParams.Palette.Platform);
            }

            return true;
        }
    }

    /// <summary>
    ///     Fills a volume with random background walls
    /// </summary>
    public class WallGapGenerator1 : IVolumeComponentGenerator {
        public HashSet<ComponentTag> GetPossibleTags() {
            return [
                ComponentTag.IsWallGap,
                ComponentTag.Elevated,
                ComponentTag.GroundLevel,
                ComponentTag.UnderGround,
                ComponentTag.External
            ];
        }

        public bool Generate(VolumeComponentParams componentParams) {
            if (!componentParams.Component.TagsRequired.Contains(ComponentTag.External))
                componentParams.Component.Volume.ExecuteInArea((x, y) => { componentParams.Tilemap.PlaceWall(x, y, PaintedType.PickRandom(componentParams.Palette.BackgroundWallAlt)); });
            return true;
        }
    }
}