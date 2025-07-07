using SpawnHouses.Types;

namespace SpawnHouses.AdvStructures.Generation.Components;

public class GapGen {
    /// <summary>
    ///     A gap floor, places platforms
    /// </summary>
    public class FloorGapGenerator1 : IComponentGenerator {
        public ComponentTag[] GetPossibleTags() {
            return [
                ComponentTag.IsFloorGap,
                ComponentTag.FloorGroundLevel,
                ComponentTag.Elevated
            ];
        }

        public bool Generate(ComponentParams componentParams) {
            int xStart = componentParams.Volume.BoundingBox.topLeft.X;
            int[] topY = new int[componentParams.Volume.Size.X];
            int[] bottomY = new int[componentParams.Volume.Size.X];

            componentParams.Volume.ExecuteInArea((x, y) => {
                PaintedType.PlaceWall(x, y, componentParams.TilePalette.BackgroundFloorMain, componentParams.Tilemap);
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
                PaintedType.PlaceTile(xStart + index, topY[index], componentParams.TilePalette.Platform, componentParams.Tilemap);
                PaintedType.PlaceTile(xStart + index, bottomY[index], componentParams.TilePalette.Platform, componentParams.Tilemap);
            }

            return true;
        }
    }

    /// <summary>
    ///     Fills a volume with random background walls
    /// </summary>
    public class WallGapGenerator1 : IComponentGenerator {
        public ComponentTag[] GetPossibleTags() {
            return [
                ComponentTag.IsWallGap,
                ComponentTag.WallGroundLevel,
                ComponentTag.Elevated
            ];
        }

        public bool Generate(ComponentParams componentParams) {
            componentParams.Volume.ExecuteInArea((x, y) => { PaintedType.PlaceWall(x, y, PaintedType.PickRandom(componentParams.TilePalette.BackgroundWallAlt), componentParams.Tilemap); });
            return true;
        }
    }
}