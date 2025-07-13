using System.Linq;
using SpawnHouses.Types;
using Terraria.ID;

namespace SpawnHouses.AdvStructures.Generation.Components;

public static class FloorGen {
    /// <summary>
    ///     Fills a volume with the same floor blocks
    /// </summary>
    public class FloorGenerator1 : IComponentGenerator {
        public ComponentTag[] GetPossibleTags() {
            return [
                ComponentTag.IsFloor,
                ComponentTag.External,
                ComponentTag.GroundLevel,
                ComponentTag.UnderGround
            ];
        }

        public bool Generate(ComponentParams componentParams) {
            componentParams.Volume.ExecuteInArea((x, y) => { PaintedType.PlaceTile(x, y, componentParams.TilePalette.FloorMain, componentParams.Tilemap); });
            return true;
        }
    }

    /// <summary>
    ///     Fills a volume with random floor blocks
    /// </summary>
    public class FloorGenerator2 : IComponentGenerator {
        public ComponentTag[] GetPossibleTags() {
            return [
                ComponentTag.IsFloor,
                ComponentTag.FloorSolid,
                ComponentTag.External,
                ComponentTag.Elevated,
                ComponentTag.GroundLevel,
                ComponentTag.UnderGround
            ];
        }

        public bool Generate(ComponentParams componentParams) {
            componentParams.Volume.ExecuteInArea((x, y) => { PaintedType.PlaceTile(x, y, PaintedType.PickRandom(componentParams.TilePalette.FloorAlt), componentParams.Tilemap); });
            return true;
        }
    }

    /// <summary>
    ///     Fills a volume with random blocks, but the top block consistent
    /// </summary>
    public class FloorGenerator3 : IComponentGenerator {
        public ComponentTag[] GetPossibleTags() {
            return [
                ComponentTag.IsFloor,
                ComponentTag.FloorSolid,
                ComponentTag.External,
                ComponentTag.Elevated,
                ComponentTag.GroundLevel,
                ComponentTag.UnderGround
            ];
        }

        public bool CanGenerate(ComponentParams componentParams) {
            return componentParams.Volume.Size.Y >= 2;
        }

        public bool Generate(ComponentParams componentParams) {
            bool elevated = componentParams.TagsRequired.Contains(ComponentTag.Elevated);
            int xStart = componentParams.Volume.BoundingBox.topLeft.X;
            int[] topY = new int[componentParams.Volume.Size.X];

            componentParams.Volume.ExecuteInArea((x, y) => {
                PaintedType.PlaceTile(x, y,
                    PaintedType.PickRandom(elevated
                        ? componentParams.TilePalette.FloorAlt
                        : componentParams.TilePalette.FloorAltElevated),
                    componentParams.Tilemap);

                if (topY[x - xStart] == 0)
                    topY[x - xStart] = y;

                if (y < topY[x - xStart])
                    topY[x - xStart] = y;
            });

            for (int index = 0; index < topY.Length; index++)
                PaintedType.PlaceTile(xStart + index, topY[index],
                    elevated ? componentParams.TilePalette.FloorMainElevated : componentParams.TilePalette.FloorMain,
                    componentParams.Tilemap);

            return true;
        }
    }

    /// <summary>
    ///     Fills top and bottom of volume, adds support struts in the middle
    /// </summary>
    public class FloorGenerator4 : IComponentGenerator {
        public ComponentTag[] GetPossibleTags() {
            return [
                ComponentTag.IsFloor,
                ComponentTag.FloorHollow,
                ComponentTag.Elevated
            ];
        }

        public bool CanGenerate(ComponentParams componentParams) {
            return componentParams.Volume.Size.Y >= 3;
        }

        public bool Generate(ComponentParams componentParams) {
            bool elevated = componentParams.TagsRequired.Contains(ComponentTag.Elevated);
            int xStart = componentParams.Volume.BoundingBox.topLeft.X;
            int[] topY = new int[componentParams.Volume.Size.X];
            int[] bottomY = new int[componentParams.Volume.Size.X];
            int supportInterval = Terraria.WorldGen.genRand.Next(3, 5);

            componentParams.Volume.ExecuteInArea((x, y) => {
                if ((x - xStart - 2) % supportInterval == 0 || x == xStart ||
                    x == componentParams.Volume.BoundingBox.bottomRight.X) {
                    PaintedType.PlaceTile(x, y,
                        elevated ? componentParams.TilePalette.FloorMainElevated : componentParams.TilePalette.FloorMain,
                        componentParams.Tilemap);
                }
                else {
                    PaintedType.PlaceWall(x, y, componentParams.TilePalette.BackgroundFloorMain, componentParams.Tilemap);
                    StructureTile tile = componentParams.Tilemap[x, y];
                    if (Terraria.WorldGen.genRand.Next(0, 3) == 0) {
                        tile.HasTile = true;
                        tile.TileType = TileID.Cobweb;
                    }
                    else {
                        tile.HasTile = false;
                    }
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
                PaintedType.PlaceTile(xStart + index, topY[index],
                    elevated ? componentParams.TilePalette.FloorMainElevated : componentParams.TilePalette.FloorMain,
                    componentParams.Tilemap);
                PaintedType.PlaceTile(xStart + index, bottomY[index],
                    elevated ? componentParams.TilePalette.FloorMainElevated : componentParams.TilePalette.FloorMain,
                    componentParams.Tilemap);
            }

            return true;
        }
    }
}