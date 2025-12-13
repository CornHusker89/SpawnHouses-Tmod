#nullable enable
using System;
using System.Collections.Generic;
using SpawnHouses.AdvStructures.AdvStructureParts;
using SpawnHouses.Types;
using SpawnHouses.Types.Palette;

namespace SpawnHouses.Helpers;

public delegate TilePaintedType? TilePaletteCondition(int x, int y);

public delegate WallPaintedType? WallPaletteCondition(int x, int y);

/// <summary>
///     high-level static helpers for generating components. each helper is a dedicated subclass with its own tag output
/// </summary>
public static class ComponentHelper {
    /// <summary>
    ///     fills shape with tiles of a painted type
    /// </summary>
    public static class FillShapeTiles {
        public static readonly HashSet<ComponentTag> PossibleTags = [
            ComponentTag.ApplySloping,
            ComponentTag.SlopingModifier
        ];

        /// <summary>
        ///     fills shape with tiles of a painted type
        /// </summary>
        /// <param name="shape"></param>
        /// <param name="param"></param>
        /// <param name="paletteCondition">
        ///     callback that returns the palette entry, if null prevents tile placement for that tile in the shape.
        ///     can also be used to execute arbitrary callback on each tile
        /// </param>
        /// <remarks>supports ApplySloping and SlopingModifier component tags</remarks>
        public static void Action(Shape shape, ComponentParams param, TilePaletteCondition paletteCondition) {
            SlopingAlgorithm? sloping = param.Component.GetTagRequiredDataSafe<SlopingAlgorithm?>(ComponentTag.ApplySloping);
            SlopeModifier slopeModifier = param.Component.GetTagRequiredDataSafe<SlopeModifier>(ComponentTag.SlopingModifier);

            if (sloping == null) {
                shape.ExecuteInArea((x, y) => {
                    TilePaintedType? type = paletteCondition.Invoke(x, y);
                    if (type != null)
                        param.Tilemap.PlaceTile(x, y, type);
                });
                return;
            }

            if (slopeModifier == SlopeModifier.LocalSloping)
                shape.ExecuteInArea((x, y, bt) => {
                    TilePaintedType? type = paletteCondition.Invoke(x, y);
                    if (type != null) {
                        param.Tilemap.PlaceTile(x, y, type, bt);
                        param.Tilemap[x, y].SlopeModifier = SlopeModifier.LocalSloping;
                    }
                }, sloping);
            else
                shape.ExecuteInArea((x, y) => {
                    TilePaintedType? type = paletteCondition.Invoke(x, y);
                    if (type != null)
                        param.Tilemap.PlaceTile(x, y, type, sloping, slopeModifier);
                });
        }
    }

    /// <summary>
    ///     fills shape with tiles of a painted type
    /// </summary>
    public static class FillShapeWalls {
        public static readonly HashSet<ComponentTag> PossibleTags = [];

        /// <summary>
        ///     fills shape with tiles of a painted type
        /// </summary>
        /// <param name="shape"></param>
        /// <param name="param"></param>
        /// <param name="fillCondition">
        ///     callback that returns the palette entry, if null prevents wall placement for that wall in the shape.
        ///     can also be used to execute arbitrary callback on each wall
        /// </param>
        public static void Action(Shape shape, ComponentParams param, WallPaletteCondition? fillCondition) {
            shape.ExecuteInArea((x, y) => {
                WallPaintedType? type = fillCondition?.Invoke(x, y);
                if (type != null)
                    param.Tilemap.PlaceWall(x, y, type);
            });
        }
    }

    public static class CreateBeams {
        public static readonly HashSet<ComponentTag> PossibleTags = [
            ComponentTag.RoomHasArbitraryBeams,
            ComponentTag.RoomHasSpecificBeams
        ];

        public static int[] Action(Shape shape, ComponentParams param) {
            if (param.Component.TagsRequired.ContainsKey(ComponentTag.RoomHasSpecificBeams)) return param.Component.GetTagRequiredData<int[]>(ComponentTag.RoomHasSpecificBeams);

            if (shape.Size.X <= 10) {
                if (shape.Size.X <= 8) {
                    if (shape.Size.X <= 4) return [];
                    return [shape.BoundingBox.topLeft.X + (int)Math.Round(shape.Size.X / 2.0)];
                }

                return [shape.BoundingBox.topLeft.X + 2, shape.BoundingBox.topLeft.X + shape.Size.X - 1 - 2];
            }

            // add by pairs on each side
            List<int> leftBeams = [shape.BoundingBox.topLeft.X + 3];
            List<int> rightBeams = [shape.BoundingBox.topLeft.X + shape.Size.X - 1 - 3];

            // engage in one-off shenanigans to make things symmetrical
            while (rightBeams[^1] - leftBeams[^1] >= 6) {
                if (rightBeams[^1] - leftBeams[^1] < 8) {
                    if (rightBeams[^1] - leftBeams[^1] >= 7) {
                        leftBeams[^1] += 1;
                        rightBeams[^1] -= 1;
                    }
                    else if (rightBeams[^1] - leftBeams[^1] == 5) {
                        leftBeams.Add(leftBeams[^1] + 3);
                    }

                    break;
                }

                leftBeams.Add(leftBeams[^1] + 4);
                rightBeams.Add(rightBeams[^1] - 4);

                if (rightBeams[^1] - leftBeams[^1] <= 2) {
                    leftBeams[^2] -= 1;
                    leftBeams[^1] -= 1;
                    rightBeams[^1] += 1;
                    rightBeams[^2] += 1;
                }
            }

            rightBeams.Reverse();
            leftBeams.AddRange(rightBeams);
            return leftBeams.ToArray();
        }
    }

}