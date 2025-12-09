#nullable enable
using SpawnHouses.AdvStructures.AdvStructureParts;
using SpawnHouses.Types;
using SpawnHouses.Types.Palette;

namespace SpawnHouses.Helpers;

public delegate bool FillCondition(int x, int y);

public static class ComponentFillHelper {
    /// <summary>
    ///     fills shape with tiles of a painted type 
    /// </summary>
    /// <param name="shape"></param>
    /// <param name="param"></param>
    /// <param name="paintedType"></param>
    /// <param name="fillCondition">callback that if returns false, will prevent tile placement for that tile in the shape.
    /// can also be used to execute arbitrary callback on each tile</param>
    /// <remarks>supports ApplySloping and SlopingModifier component tags</remarks>
    public static void FillShapeTiles(Shape shape, ComponentParams param, PaintedType paintedType, FillCondition? fillCondition = null) {
        SlopingAlgorithm? sloping = param.Component.GetTagDataSafe<SlopingAlgorithm?>(ComponentTag.ApplySloping);
        SlopeModifier slopeModifier = param.Component.GetTagDataSafe<SlopeModifier>(ComponentTag.SlopingModifier);

        if (sloping == null) {
            shape.ExecuteInArea((x, y) => {
                if (fillCondition?.Invoke(x, y) != false)
                    param.Tilemap.PlaceTile(x, y, paintedType);
            });
            return;
        }

        if (slopeModifier == SlopeModifier.LocalSloping)
            shape.ExecuteInArea((x, y, bt) => {
                if (fillCondition?.Invoke(x, y) != false) {
                    param.Tilemap.PlaceTile(x, y, paintedType, bt);
                    param.Tilemap[x, y].SlopeModifier = SlopeModifier.LocalSloping;
                }
            }, sloping);
        else
            shape.ExecuteInArea((x, y) => {
                if (fillCondition?.Invoke(x, y) != false)
                    param.Tilemap.PlaceTile(x, y, paintedType, sloping, slopeModifier);
            });
    }

    /// <summary>
    ///     fills shape with tiles of a painted type
    /// </summary>
    /// <param name="shape"></param>
    /// <param name="param"></param>
    /// <param name="paintedTypes">will pick a random one for each tile</param>
    /// <param name="fillCondition">
    ///     callback that if returns false, will prevent tile placement for that tile in the shape.
    ///     can also be used to execute arbitrary callback on each tile
    /// </param>
    /// <remarks>supports ApplySloping and SlopingModifier component tags</remarks>
    public static void FillShapeTiles(Shape shape, ComponentParams param, PaintedType[] paintedTypes, FillCondition? fillCondition = null) {
        FillShapeTiles(shape, param, PaintedType.PickRandom(paintedTypes), fillCondition);
    }

    /// <summary>
    ///     fills shape with tiles of a painted type
    /// </summary>
    /// <param name="shape"></param>
    /// <param name="param"></param>
    /// <param name="paintedType"></param>
    /// <param name="fillCondition">callback that if returns false, will prevent tile placement for that tile in the shape.
    /// can also be used to execute arbitrary callback on each tile</param>
    public static void FillShapeWalls(Shape shape, ComponentParams param, PaintedType paintedType, FillCondition? fillCondition = null) {
        shape.ExecuteInArea((x, y) => {
            if (fillCondition?.Invoke(x, y) != false)
                param.Tilemap.PlaceWall(x, y, paintedType);
        });
    }

    /// <summary>
    ///     fills shape with tiles of a painted type
    /// </summary>
    /// <param name="shape"></param>
    /// <param name="param"></param>
    /// <param name="paintedTypes">will pick a random one for each tile</param>
    /// <param name="fillCondition">
    ///     callback that if returns false, will prevent tile placement for that tile in the shape.
    ///     can also be used to execute arbitrary callback on each tile
    /// </param>
    public static void FillShapeWalls(Shape shape, ComponentParams param, PaintedType[] paintedTypes, FillCondition? fillCondition = null) {
        FillShapeWalls(shape, param, PaintedType.PickRandom(paintedTypes), fillCondition);
    }
}