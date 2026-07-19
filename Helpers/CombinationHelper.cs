using System.Collections.Generic;

namespace SpawnHouses.Helpers;

public static class CombinationHelper {
    /// <summary>
    ///     creates all possible combinations of something?? idk lmaoo
    /// </summary>
    /// <param name="sequences"></param>
    /// <typeparam name="T"></typeparam>
    /// <returns></returns>
    public static IEnumerable<T[]> CartesianProduct<T>(IReadOnlyList<T[]> sequences) {
        if (sequences.Count == 0) {
            yield return [];
            yield break;
        }

        int[] indices = new int[sequences.Count];
        while (true) {
            var combo = new T[sequences.Count];
            for (int i = 0; i < sequences.Count; i++)
                combo[i] = sequences[i][indices[i]];
            yield return combo;

            int pos = sequences.Count - 1;
            while (pos >= 0) {
                if (++indices[pos] < sequences[pos].Length) break;
                indices[pos] = 0;
                pos--;
            }

            if (pos < 0) yield break;
        }
    }
}