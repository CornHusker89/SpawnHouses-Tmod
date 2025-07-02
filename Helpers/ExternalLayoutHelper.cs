using System;
using System.Linq;
using SpawnHouses.AdvStructures.AdvStructureParts;
using Terraria.DataStructures;


namespace SpawnHouses.Helpers;

public class ExternalLayoutHelper {
    /// <summary>
    ///     leaves original array intact
    /// </summary>
    /// <param name="sets"></param>
    /// <returns></returns>
    public static (Point16 start, Point16 end)[] SortClockwise((Point16 start, Point16 end)[] sets) {
        double centerX = (double)sets.Sum(set => set.start.X + set.end.X) / (sets.Length * 2);
        double centerY = (double)sets.Sum(set => set.start.Y + set.end.Y) / (sets.Length * 2);
        // ReSharper disable PossibleLossOfFraction
        return sets.OrderBy(set =>
            Math.Atan2(
                (set.start.X + set.end.X) / 2 - centerY,
                (set.start.Y + set.end.Y) / 2 - centerX)
            ).ToArray();
    }
}