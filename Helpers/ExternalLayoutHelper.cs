using System;
using System.Linq;
using SpawnHouses.AdvStructures.AdvStructureParts;

namespace SpawnHouses.Helpers;

public class ExternalLayoutHelper {
    /// <summary>
    ///     leaves original array intact
    /// </summary>
    /// <param name="entryPoints"></param>
    /// <returns></returns>
    public static EntryPoint[] SortClockwise(EntryPoint[] entryPoints) {
        double centerX = (double)entryPoints.Sum(entryPoint => entryPoint.Start.X) / entryPoints.Length;
        double centerY = (double)entryPoints.Sum(entryPoint => entryPoint.Start.Y) / entryPoints.Length;
        return entryPoints.OrderBy(entryPoint => Math.Atan2(entryPoint.Start.Y - centerY, entryPoint.Start.X - centerX)).ToArray();
    }
}