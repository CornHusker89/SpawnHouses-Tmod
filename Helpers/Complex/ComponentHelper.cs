#nullable enable
using System;
using System.Collections.Generic;
using System.Linq;
using SpawnHouses.Common;
using SpawnHouses.Common.Modules;
using SpawnHouses.Common.Palette;
using SpawnHouses.Common.Tagging;
using SpawnHouses.Common.Tiles;
using SpawnHouses.Common.Types.Geometry;

namespace SpawnHouses.Helpers.Complex;

public delegate TilePaintedType? TilePaletteCondition(int x, int y);

public delegate WallPaintedType? WallPaletteCondition(int x, int y);

public delegate bool PlacementCondition(int x, int y);

public interface IComponentHelper {
    public static abstract HashSet<Tag> PossibleTags { get; }
}

/// <summary>
///     high-level static helpers for generating components. each helper is a dedicated subclass with its own tag output
/// </summary>
public static class ComponentHelper {
    /// <summary>
    ///     fills shape with tiles of a painted type
    /// </summary>
    public abstract class FillShapeTiles : IComponentHelper {
        public static HashSet<Tag> PossibleTags => [
            Tags.SlopingAlgorithm,
            Tags.SlopeGrouping
        ];

        /// <summary>
        ///     fills shape with tiles of a painted type
        /// </summary>
        /// <param name="component"></param>
        /// <param name="shape"></param>
        /// <param name="paletteCondition">
        ///     callback that returns the palette entry, if null prevents tile placement for that tile in the shape.
        ///     can also be used to execute arbitrary callback on each tile
        /// </param>
        /// <remarks>supports ApplySloping and SlopingModifier component tags</remarks>
        public static void Action(IComponent component, Shape shape, TilePaletteCondition paletteCondition) {
            StructureTilemap t = component.Params.Structure.Tilemap;
            bool hasSloping = component.Params.TagsRequired.GetValueSafe(Tags.SlopingAlgorithm, out SlopingAlgorithm slopingAlgorithm);
            component.Params.TagsRequired.GetValueSafe(Tags.SlopeGrouping, out SlopeGrouping slopeModifier);

            if (!hasSloping) {
                shape.ExecuteInArea((x, y) => {
                    TilePaintedType? type = paletteCondition.Invoke(x, y);
                    if (type != null)
                        t.PlaceTile(x, y, type);
                });
                return;
            }

            if (slopeModifier == SlopeGrouping.LocalSloping) {
                shape.ExecuteInArea((x, y, bt) => {
                    TilePaintedType? type = paletteCondition.Invoke(x, y);
                    if (type != null) {
                        t.PlaceTile(x, y, type, bt);
                        t[x, y].SlopeGrouping = SlopeGrouping.LocalSloping;
                    }
                }, slopingAlgorithm);
            }
            else {
                shape.ExecuteInArea((x, y) => {
                    TilePaintedType? type = paletteCondition.Invoke(x, y);
                    if (type != null)
                        t.PlaceTile(x, y, type, slopingAlgorithm, slopeModifier);
                });
            }
        }
    }

    /// <summary>
    ///     fills shape with walls of a painted type
    /// </summary>
    public abstract class FillShapeWalls : IComponentHelper {
        public static HashSet<Tag> PossibleTags => [];

        /// <summary>
        ///     fills shape with walls of a painted type
        /// </summary>
        /// <param name="shape"></param>
        /// <param name="structure"></param>
        /// <param name="fillCondition">
        ///     callback that returns the palette entry, if null prevents wall placement for that wall in the shape.
        ///     can also be used to execute arbitrary callback on each wall
        /// </param>
        public static void Action(Shape shape, AdvStructure structure, WallPaletteCondition fillCondition) {
            shape.ExecuteInArea((x, y) => {
                WallPaintedType? type = fillCondition.Invoke(x, y);
                if (type != null)
                    structure.Tilemap.PlaceWall(x, y, type);
            });
        }
    }

    /// <summary>
    ///     fills shape with walls, but only where they won't be seen externally. intended to be called on wall and floor components
    /// </summary>
    public abstract class FillShapeRedundantWalls : IComponentHelper {
        public static HashSet<Tag> PossibleTags => FillShapeWalls.PossibleTags;

        /// <summary>
        ///     fills shape with walls, but only where they won't be seen externally. intended to be called on wall and floor components
        /// </summary>
        /// <param name="shape"></param>
        /// <param name="structure"></param>
        /// <param name="wallType"></param>
        public static void Action(Shape shape, AdvStructure structure, WallPaintedType wallType) {
            FillShapeWalls.Action(shape, structure, (x, y) => {
                bool isTouchingExternal = false;
                for (int dx = -1; dx <= 1; dx++)
                for (int dy = -1; dy <= 1; dy++)
                    if (structure.Tilemap[x + dx, y + dy].IsOutside) {
                        isTouchingExternal = true;
                        break;
                    }

                return isTouchingExternal ? null : wallType;
            });
        }
    }

    /// <summary>
    ///     create beams at a regular interval of every 4, but ensure symmetry is kept throughout the shape
    /// </summary>
    public abstract class PlaceBeams : IComponentHelper {
        public static HashSet<Tag> PossibleTags => TagMap.NewTagSet(
            [
                Tags.RoomHasBeams,
                Tags.RoomHasSpecificBeams,
                Tags.RoomBeamsAreTiles,
                Tags.RoomBeamsAreWalls
            ],
            FillShapeWalls.PossibleTags
        );

        /// <summary>
        ///     
        /// </summary>
        /// <param name="shape"></param>
        /// <param name="component"></param>
        /// <param name="targetDistance">the target distance between each beam</param>
        /// <param name="roomPaletteSet"></param>
        /// <param name="fillCondition"></param>
        public static void Action(Shape shape, IComponent component, int targetDistance, PaintedTypeRoomSet roomPaletteSet, PlacementCondition? fillCondition = null) {
            bool placeTiles = component.Params.TagsRequired.HasTag(Tags.RoomBeamsAreTiles);
            bool placeWalls = component.Params.TagsRequired.HasTag(Tags.RoomBeamsAreWalls);
            if (!placeTiles && !placeWalls)
                throw new Exception("PlaceBeams was called but tags don't contain either \"RoomBeamsAreTiles\" or \"RoomBeamsAreWalls\"");

            StructureTilemap tilemap = component.Params.Structure.Tilemap;

            HashSet<int> beams;
            if (component.Params.TagsRequired.GetValueSafe(Tags.RoomHasSpecificBeams, out int[] requiredBeams))
                beams = requiredBeams.ToHashSet();
            else
                beams = shape.GetEvenSplits(true, targetDistance, 1);

            shape.ExecuteInArea((x, y) => {
                if (beams.Contains(x) && (fillCondition == null || fillCondition.Invoke(x, y))) {
                    if (placeTiles)
                        tilemap.PlaceTile(x, y, roomPaletteSet.BeamTile, actuated: roomPaletteSet.BeamTileActuation);
                    if (placeWalls)
                        tilemap.PlaceWall(x, y, roomPaletteSet.VerticalBeamBackground);
                }
            });
        }
    }

    /// <summary>
    ///     executes a <see cref="WallPaletteCondition" /> on every tile where a window should be
    /// </summary>
    public abstract class PlaceWindowAreas : IComponentHelper {
        public static HashSet<Tag> PossibleTags => [
            Tags.RoomHasWindows,
            Tags.RoomHasSpecificWindows
        ];

        public static void Action(Shape shape, IComponent component, int windowLength, int windowSpacing, int roomEdgeSpacing, WallPaletteCondition fillCondition) {
            List<Shape> windowVolumes;
            if (component.Params.TagsRequired.GetValueSafe(Tags.RoomHasSpecificWindows, out var requiredWindowVolumes))
                windowVolumes = requiredWindowVolumes.ToList();
            else {
                Shape baseWindowShape = shape.GetExpandedShape(-roomEdgeSpacing);
                var splits = baseWindowShape.GetEvenSplits(true, windowLength, windowSpacing);
                windowVolumes = [];

                foreach (int split in splits) {
                    Shape? thisWindowSubsection = baseWindowShape.SplitOnce(false, split, true, false);
                    Shape? remainingWindowVolume = baseWindowShape.SplitOnce(false, split + windowLength - 1, false, false);
                    if (thisWindowSubsection == null || remainingWindowVolume == null)
                        throw new Exception("splitting for window resulted in a nonexistent shape");
                    windowVolumes.Add(thisWindowSubsection);
                    baseWindowShape = remainingWindowVolume;
                }
            }

            foreach (Shape windowVolume in windowVolumes) {
                windowVolume.ExecuteInArea((x, y) => {
                    WallPaintedType? type = fillCondition.Invoke(x, y);
                    if (type != null)
                        component.Params.Structure.Tilemap.PlaceWall(x, y, type);
                });
            }
        }
    }
}