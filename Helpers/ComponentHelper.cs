#nullable enable
using System;
using System.Collections.Generic;
using SpawnHouses.Common;
using SpawnHouses.Common.Types.Geometry;
using SpawnHouses.Types.Palette;

namespace SpawnHouses.Helpers;

public delegate TilePaintedType? TilePaletteCondition(int x, int y);

public delegate WallPaintedType? WallPaletteCondition(int x, int y);

public interface IComponentHelper {
    public static abstract ComponentTagPartialSet PossibleTags { get; }
}

/// <summary>
///     high-level static helpers for generating components. each helper is a dedicated subclass with its own tag output
/// </summary>
public static class ComponentHelper {
    /// <summary>
    ///     fills shape with tiles of a painted type
    /// </summary>
    public abstract class FillShapeTiles : IComponentHelper {
        public static ComponentTagPartialSet PossibleTags => [
            ComponentTags.ApplySloping,
            ComponentTags.SlopingModifier
        ];

        /// <summary>
        ///     fills shape with tiles of a painted type
        /// </summary>
        /// <param name="shape"></param>
        /// <param name="structure"></param>
        /// <param name="param"></param>
        /// <param name="paletteCondition">
        ///     callback that returns the palette entry, if null prevents tile placement for that tile in the shape.
        ///     can also be used to execute arbitrary callback on each tile
        /// </param>
        /// <remarks>supports ApplySloping and SlopingModifier component tags</remarks>
        public static void Action(Shape shape, AdvStructure structure, ComponentParams param, TilePaletteCondition paletteCondition) {
            SlopingAlgorithm? sloping = param.Component.GetTagRequiredDataSafe<SlopingAlgorithm?>(ComponentTags.ApplySloping);
            SlopeModifier slopeModifier = param.Component.GetTagRequiredDataSafe<SlopeModifier>(ComponentTags.SlopingModifier);

            if (sloping == null) {
                shape.ExecuteInArea((x, y) => {
                    TilePaintedType? type = paletteCondition.Invoke(x, y);
                    if (type != null)
                        structure.Tilemap.PlaceTile(x, y, type);
                });
                return;
            }

            if (slopeModifier == SlopeModifier.LocalSloping)
                shape.ExecuteInArea((x, y, bt) => {
                    TilePaintedType? type = paletteCondition.Invoke(x, y);
                    if (type != null) {
                        structure.Tilemap.PlaceTile(x, y, type, bt);
                        structure.Tilemap[x, y].SlopeModifier = SlopeModifier.LocalSloping;
                    }
                }, sloping);
            else
                shape.ExecuteInArea((x, y) => {
                    TilePaintedType? type = paletteCondition.Invoke(x, y);
                    if (type != null)
                        structure.Tilemap.PlaceTile(x, y, type, sloping, slopeModifier);
                });
        }
    }

    /// <summary>
    ///     fills shape with tiles of a painted type
    /// </summary>
    public abstract class FillShapeWalls : IComponentHelper {
        public static ComponentTagPartialSet PossibleTags => [];

        /// <summary>
        ///     fills shape with tiles of a painted type
        /// </summary>
        /// <param name="shape"></param>
        /// <param name="structure"></param>
        /// <param name="fillCondition">
        ///     callback that returns the palette entry, if null prevents wall placement for that wall in the shape.
        ///     can also be used to execute arbitrary callback on each wall
        /// </param>
        public static void Action(Shape shape, AdvStructure structure, WallPaletteCondition? fillCondition) {
            shape.ExecuteInArea((x, y) => {
                WallPaintedType? type = fillCondition?.Invoke(x, y);
                if (type != null)
                    structure.Tilemap.PlaceWall(x, y, type);
            });
        }
    }

    /// <summary>
    ///     create beams at a regular interval of every 4, but ensure symmetry is kept throughout the shape
    /// </summary>
    public abstract class CreateBeams : IComponentHelper {
        public static ComponentTagPartialSet PossibleTags => [
            ComponentTags.RoomHasArbitraryBeams,
            ComponentTags.RoomHasSpecificBeams
        ];

        /// <summary>
        ///     create beams at a regular interval of every 4, but ensure symmetry is kept throughout the shape
        /// </summary>
        /// <param name="shape"></param>
        /// <param name="param"></param>
        /// <returns>tilemap x-positions of each beam</returns>
        public static int[] Action(Shape shape, ComponentParams param) {
            int[]? possibleBeams = param.Component.GetTagRequiredDataSafe<int[]?>(ComponentTags.RoomHasSpecificBeams);
            if (possibleBeams != null) return possibleBeams;

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