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

        public bool Generate(VolumeComponentParams param) {
            Func<int, int, bool[,], BlockType> sloping = param.Component.TagsRequired.Contains(ComponentTag.UseSimpleSloping)
                ? SlopeHelper.SimpleSlopes
                : param.Component.TagsRequired.Contains(ComponentTag.UseSimpleSloping)
                    ? SlopeHelper.HalfSlopes
                    : null;
            param.Component.Volume.ExecuteInArea((x, y, bt) => { param.Tilemap.PlaceTile(x, y, param.Palette.FloorMain, bt); }, sloping);
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

        public bool Generate(VolumeComponentParams param) {
            Func<int, int, bool[,], BlockType> sloping = param.Component.TagsRequired.Contains(ComponentTag.UseSimpleSloping)
                ? SlopeHelper.SimpleSlopes
                : param.Component.TagsRequired.Contains(ComponentTag.UseSimpleSloping)
                    ? SlopeHelper.HalfSlopes
                    : null;
            param.Component.Volume.ExecuteInArea((x, y, bt) => { param.Tilemap.PlaceTile(x, y, PaintedType.PickRandom(param.Palette.FloorAlt), bt); }, sloping);
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

        public bool Generate(VolumeComponentParams param) {
            Func<int, int, bool[,], BlockType> sloping = param.Component.TagsRequired.Contains(ComponentTag.UseSimpleSloping)
                ? SlopeHelper.SimpleSlopes
                : param.Component.TagsRequired.Contains(ComponentTag.UseSimpleSloping)
                    ? SlopeHelper.HalfSlopes
                    : null;
            bool elevated = param.Component.TagsRequired.Contains(ComponentTag.Elevated);
            int xStart = param.Component.Volume.BoundingBox.topLeft.X;
            int[] topY = new int[param.Component.Volume.Size.X];

            param.Component.Volume.ExecuteInArea((x, y, bt) => {
                param.Tilemap.PlaceTile(
                    x,
                    y,
                    PaintedType.PickRandom(elevated
                        ? param.Palette.FloorAlt
                        : param.Palette.FloorAltElevated),
                    bt
                );

                if (topY[x - xStart] == 0)
                    topY[x - xStart] = y;

                if (y < topY[x - xStart])
                    topY[x - xStart] = y;
            }, sloping);

            for (int index = 0; index < topY.Length; index++)
                param.Tilemap.SoftPlaceTile(
                    xStart + index, topY[index],
                    elevated ? param.Palette.FloorMainElevated : param.Palette.FloorMain
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

        public bool Generate(VolumeComponentParams param) {
            bool elevated = param.Component.TagsRequired.Contains(ComponentTag.Elevated);
            int xStart = param.Component.Volume.BoundingBox.topLeft.X;
            int[] topY = new int[param.Component.Volume.Size.X];
            int[] bottomY = new int[param.Component.Volume.Size.X];
            int supportInterval = Terraria.WorldGen.genRand.Next(3, 5);

            param.Component.Volume.ExecuteInArea((x, y) => {
                if ((x - xStart - 2) % supportInterval == 0 || x == xStart ||
                    x == param.Component.Volume.BoundingBox.bottomRight.X) {
                    param.Tilemap.PlaceTile(x, y,
                        elevated ? param.Palette.FloorMainElevated : param.Palette.FloorMain);
                }
                else {
                    param.Tilemap.PlaceWall(x, y, param.Palette.BackgroundFloorMain);
                    StructureTile tile = param.Tilemap[x, y];
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
                param.Tilemap.PlaceTile(xStart + index, topY[index],
                    elevated ? param.Palette.FloorMainElevated : param.Palette.FloorMain);
                param.Tilemap.PlaceTile(xStart + index, bottomY[index],
                    elevated ? param.Palette.FloorMainElevated : param.Palette.FloorMain);
            }

            return true;
        }
    }
}