#nullable enable
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
            SlopingAlgorithm? sloping = param.Component.GetTagDataSafe<SlopingAlgorithm?>(ComponentTag.ApplySloping);
            SlopeModifier slopeModifier = param.Component.GetTagDataSafe<SlopeModifier>(ComponentTag.SlopingModifier);

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
        public static HashSet<ComponentTag> PossibleTags = [];

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


}