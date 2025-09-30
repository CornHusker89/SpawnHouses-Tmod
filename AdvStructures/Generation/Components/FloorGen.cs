using System;
using System.Collections.Generic;
using SpawnHouses.Helpers;
using SpawnHouses.Types;
using Terraria.ID;

namespace SpawnHouses.AdvStructures.Generation.Components;

public static class FloorGen {
    /// <summary>
    ///     Fills a volume with the same floor blocks
    /// </summary>
    public class FloorGenerator1 : IVolumeComponentGenerator {
        public HashSet<ComponentTag> GetPossibleTags() {
            return [
                ComponentTag.External,
                ComponentTag.GroundLevel,
                ComponentTag.UnderGround,
                ComponentTag.UseSimpleSloping,
                ComponentTag.UseHalfSloping
            ];
        }

        public bool Generate(VolumeComponentParams componentParams) {
            Func<int, int, bool[,], BlockType> sloping = componentParams.Component.TagsRequired.Contains(ComponentTag.UseSimpleSloping)
                ? SlopeHelper.SimpleSlopes
                : componentParams.Component.TagsRequired.Contains(ComponentTag.UseSimpleSloping)
                    ? SlopeHelper.HalfSlopes
                    : null;
            componentParams.Component.Volume.ExecuteInArea((x, y, bt) => { componentParams.Tilemap.PlaceTile(x, y, componentParams.Palette.FloorMain, bt); }, sloping);
            return true;
        }
    }

    /// <summary>
    ///     Fills a volume with random floor blocks
    /// </summary>
    public class FloorGenerator2 : IVolumeComponentGenerator {
        public HashSet<ComponentTag> GetPossibleTags() {
            return [
                ComponentTag.FloorSolid,
                ComponentTag.External,
                ComponentTag.Elevated,
                ComponentTag.GroundLevel,
                ComponentTag.UnderGround,
                ComponentTag.UseSimpleSloping,
                ComponentTag.UseHalfSloping
            ];
        }

        public bool Generate(VolumeComponentParams componentParams) {
            Func<int, int, bool[,], BlockType> sloping = componentParams.Component.TagsRequired.Contains(ComponentTag.UseSimpleSloping)
                ? SlopeHelper.SimpleSlopes
                : componentParams.Component.TagsRequired.Contains(ComponentTag.UseSimpleSloping)
                    ? SlopeHelper.HalfSlopes
                    : null;
            componentParams.Component.Volume.ExecuteInArea((x, y, bt) => { componentParams.Tilemap.PlaceTile(x, y, PaintedType.PickRandom(componentParams.Palette.FloorAlt), bt); }, sloping);
            return true;
        }
    }

    /// <summary>
    ///     Fills a volume with random blocks, but the top block consistent
    /// </summary>
    public class FloorGenerator3 : IVolumeComponentGenerator {
        public HashSet<ComponentTag> GetPossibleTags() {
            return [
                ComponentTag.FloorSolid,
                ComponentTag.External,
                ComponentTag.Elevated,
                ComponentTag.GroundLevel,
                ComponentTag.UnderGround,
                ComponentTag.UseSimpleSloping,
                ComponentTag.UseHalfSloping
            ];
        }

        public bool CanGenerate(VolumeComponentParams componentParams) {
            return componentParams.Component.Volume.Size.Y >= 2;
        }

        public bool Generate(VolumeComponentParams componentParams) {
            Func<int, int, bool[,], BlockType> sloping = componentParams.Component.TagsRequired.Contains(ComponentTag.UseSimpleSloping)
                ? SlopeHelper.SimpleSlopes
                : componentParams.Component.TagsRequired.Contains(ComponentTag.UseSimpleSloping)
                    ? SlopeHelper.HalfSlopes
                    : null;
            bool elevated = componentParams.Component.TagsRequired.Contains(ComponentTag.Elevated);
            int xStart = componentParams.Component.Volume.BoundingBox.topLeft.X;
            int[] topY = new int[componentParams.Component.Volume.Size.X];

            componentParams.Component.Volume.ExecuteInArea((x, y, bt) => {
                componentParams.Tilemap.PlaceTile(
                    x,
                    y,
                    PaintedType.PickRandom(elevated
                        ? componentParams.Palette.FloorAlt
                        : componentParams.Palette.FloorAltElevated),
                    bt
                );

                if (topY[x - xStart] == 0)
                    topY[x - xStart] = y;

                if (y < topY[x - xStart])
                    topY[x - xStart] = y;
            }, sloping);

            for (int index = 0; index < topY.Length; index++)
                componentParams.Tilemap.SoftPlaceTile(
                    xStart + index, topY[index],
                    elevated ? componentParams.Palette.FloorMainElevated : componentParams.Palette.FloorMain
                );
            return true;
        }
    }

    /// <summary>
    ///     Fills top and bottom of volume, adds support struts in the middle
    /// </summary>
    public class FloorGenerator4 : IVolumeComponentGenerator {
        public HashSet<ComponentTag> GetPossibleTags() {
            return [
                ComponentTag.FloorHollow,
                ComponentTag.Elevated
            ];
        }

        public bool CanGenerate(VolumeComponentParams componentParams) {
            return componentParams.Component.Volume.GetTrueSize(false).average >= 3;
        }

        public bool Generate(VolumeComponentParams componentParams) {
            bool elevated = componentParams.Component.TagsRequired.Contains(ComponentTag.Elevated);
            int xStart = componentParams.Component.Volume.BoundingBox.topLeft.X;
            int[] topY = new int[componentParams.Component.Volume.Size.X];
            int[] bottomY = new int[componentParams.Component.Volume.Size.X];
            int supportInterval = Terraria.WorldGen.genRand.Next(3, 5);

            componentParams.Component.Volume.ExecuteInArea((x, y) => {
                if ((x - xStart - 2) % supportInterval == 0 || x == xStart ||
                    x == componentParams.Component.Volume.BoundingBox.bottomRight.X) {
                    componentParams.Tilemap.PlaceTile(x, y,
                        elevated ? componentParams.Palette.FloorMainElevated : componentParams.Palette.FloorMain);
                }
                else {
                    componentParams.Tilemap.PlaceWall(x, y, componentParams.Palette.BackgroundFloorMain);
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
                componentParams.Tilemap.PlaceTile(xStart + index, topY[index],
                    elevated ? componentParams.Palette.FloorMainElevated : componentParams.Palette.FloorMain);
                componentParams.Tilemap.PlaceTile(xStart + index, bottomY[index],
                    elevated ? componentParams.Palette.FloorMainElevated : componentParams.Palette.FloorMain);
            }

            return true;
        }
    }
}