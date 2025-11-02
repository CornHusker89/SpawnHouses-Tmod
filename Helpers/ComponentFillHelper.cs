#nullable enable
using System;
using SpawnHouses.AdvStructures.AdvStructureParts;
using SpawnHouses.Types;

namespace SpawnHouses.Helpers;

public static class ComponentFillHelper {
    /// <summary>
    ///     fills shape with tiles of a painted type 
    /// </summary>
    /// <param name="shape"></param>
    /// <param name="param"></param>
    /// <param name="paintedType"></param>
    /// <param name="additionalAction">callback that if returns false, will prevent tile placement for that tile in the shape</param>
    /// <remarks>supports ApplySloping and SlopingModifier component tags</remarks>
    public static void FillShapeTiles(Shape shape, ComponentParams param, PaintedType paintedType, Func<int, int, bool>? additionalAction = null) {
        SlopingAlgorithm? sloping = param.Component.GetTagDataSafe<SlopingAlgorithm?>(ComponentTag.ApplySloping);
        SlopeModifier slopeModifier = param.Component.GetTagDataSafe<SlopeModifier>(ComponentTag.SlopingModifier);

        if (sloping == null) {
            shape.ExecuteInArea((x, y) => {
                if (additionalAction?.Invoke(x, y) != false)
                    param.Tilemap.PlaceTile(x, y, paintedType);
            });
            return;
        }

        if (slopeModifier == SlopeModifier.LocalSloping)
            shape.ExecuteInArea((x, y, bt) => {
                if (additionalAction?.Invoke(x, y) != false) {
                    param.Tilemap.PlaceTile(x, y, paintedType, bt);
                    param.Tilemap[x, y].SlopeModifier = SlopeModifier.LocalSloping;
                }
            }, sloping);
        else
            shape.ExecuteInArea((x, y) => {
                if (additionalAction?.Invoke(x, y) != false)
                    param.Tilemap.PlaceTile(x, y, paintedType, sloping, slopeModifier);
            });
    }

    /// <summary>
    ///     fills shape with tiles of a painted type
    /// </summary>
    /// <param name="shape"></param>
    /// <param name="param"></param>
    /// <param name="paintedType"></param>
    /// <param name="additionalAction">callback that if returns false, will prevent tile placement for that tile in the shape</param>
    public static void FillShapeWalls(Shape shape, ComponentParams param, PaintedType paintedType, Func<int, int, bool>? additionalAction = null) {
        shape.ExecuteInArea((x, y) => {
            if (additionalAction?.Invoke(x, y) != false)
                param.Tilemap.PlaceWall(x, y, paintedType);
        });
    }


}