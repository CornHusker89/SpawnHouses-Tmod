#nullable enable
using System;
using SpawnHouses.AdvStructures.AdvStructureParts;
using SpawnHouses.Types;

namespace SpawnHouses.Helpers;

public static class ComponentFillHelper {
    /// <summary>
    ///     fills shape with a painted type
    /// </summary>
    /// <param name="shape"></param>
    /// <param name="param"></param>
    /// <param name="paintedType"></param>
    /// <param name="additionalAction"></param>
    /// <remarks>supports ApplySloping and SlopingModifier component tags</remarks>
    public static void FillShape(Shape shape, ComponentParams param, PaintedType paintedType, Action<int, int>? additionalAction = null) {
        SlopingAlgorithm? sloping = param.Component.GetTagDataSafe<SlopingAlgorithm?>(ComponentTag.ApplySloping);
        SlopeModifier slopeModifier = param.Component.GetTagDataSafe<SlopeModifier>(ComponentTag.SlopingModifier);

        if (sloping == null) {
            shape.ExecuteInArea((x, y) => {
                param.Tilemap.PlaceTile(x, y, paintedType);
                additionalAction?.Invoke(x, y);
            });
            return;
        }

        if (slopeModifier == SlopeModifier.LocalSloping)
            shape.ExecuteInArea((x, y, bt) => {
                param.Tilemap.PlaceTile(x, y, paintedType, bt);
                additionalAction?.Invoke(x, y);
            }, sloping);
        else
            shape.ExecuteInArea((x, y) => {
                param.Tilemap.PlaceTile(x, y, paintedType, sloping, slopeModifier);
                additionalAction?.Invoke(x, y);
            });
    }
}