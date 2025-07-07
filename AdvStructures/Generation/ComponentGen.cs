using System;
using System.Collections.Generic;
using System.Linq;
using SpawnHouses.Types;

namespace SpawnHouses.AdvStructures.Generation;

public static class ComponentGen {
    public static readonly (ComponentTag[] possibleTags, Func<ComponentParams, object> method)[] GenMethods = [
        // ===== floors =====


        // ===== walls =====


        // ===== decor =====


        // ===== stairways =====


        // ===== backgrounds =====


        // ===== roofs =====


        // ===== debug =====
    ];

    /// <summary>
    ///     Gets a random component method that aligns with the given <see cref="ComponentParams" />
    /// </summary>
    /// <param name="componentParams"></param>
    /// <returns></returns>
    /// <exception cref="Exception">When no components can be found for the given tags</exception>
    public static Func<ComponentParams, object> GetRandomMethod(ComponentParams componentParams) {
        List<(ComponentTag[] possibleTags, Func<ComponentParams, object> method)> methodTuples = [];
        foreach (var tuple in GenMethods) {
            var requiredTags = componentParams.TagsRequired.ToList();
            bool valid = true;
            foreach (ComponentTag possibleTag in tuple.possibleTags) {
                if (componentParams.TagsBlacklist.Contains(possibleTag)) {
                    valid = false;
                    break;
                }

                requiredTags.Remove(possibleTag);
            }

            if (valid && requiredTags.Count == 0)
                methodTuples.Add(tuple);
        }

        if (methodTuples.Count == 0)
            throw new Exception("No components found were compatible with given tags");
        return methodTuples[Terraria.WorldGen.genRand.Next(0, methodTuples.Count)].method;
    }

    #region Floor Methods

    #endregion


    #region Wall Methods

    #endregion


    #region Stairway Methods

    #endregion


    #region Background Methods

    #endregion


    #region Debug Methods

    #endregion
}