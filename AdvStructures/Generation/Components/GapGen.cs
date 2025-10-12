using System.Collections.Generic;
using SpawnHouses.Types;

namespace SpawnHouses.AdvStructures.Generation.Components;

public class GapGen {
    /// <summary>
    ///     A gap floor, places platforms
    /// </summary>
    public class FloorGapGenerator1 : VolumeComponentGenerator {
        public override HashSet<ComponentTag> GetPossibleTags() {
            return [
                ComponentTag.IsFloorGap,
                ComponentTag.Elevated,
                ComponentTag.GroundLevel,
                ComponentTag.UnderGround,
                ComponentTag.External
            ];
        }

        public override bool Generate(VolumeComponentParams param) {
            int xStart = param.Component.Volume.BoundingBox.topLeft.X;
            int[] topY = new int[param.Component.Volume.Size.X];
            int[] bottomY = new int[param.Component.Volume.Size.X];
            bool placeWalls = !param.Component.TagsRequired.Contains(ComponentTag.External);

            param.Component.Volume.ExecuteInArea((x, y) => {
                if (placeWalls)
                    param.Tilemap.PlaceWall(x, y, param.Palette.BackgroundFloorMain);
                StructureTile tile = param.Tilemap[x, y];
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
                param.Tilemap.PlaceTile(xStart + index, topY[index], param.Palette.Platform);
                param.Tilemap.PlaceTile(xStart + index, bottomY[index], param.Palette.Platform);
            }

            return true;
        }
    }

    /// <summary>
    ///     Fills a volume with random background walls
    /// </summary>
    public class WallGapGenerator1 : VolumeComponentGenerator {
        public override HashSet<ComponentTag> GetPossibleTags() {
            return [
                ComponentTag.IsWallGap,
                ComponentTag.Elevated,
                ComponentTag.GroundLevel,
                ComponentTag.UnderGround,
                ComponentTag.External
            ];
        }

        public override bool Generate(VolumeComponentParams param) {
            if (!param.Component.TagsRequired.Contains(ComponentTag.External))
                param.Component.Volume.ExecuteInArea((x, y) => { param.Tilemap.PlaceWall(x, y, PaintedType.PickRandom(param.Palette.BackgroundWallAlt)); });
            return true;
        }
    }
}