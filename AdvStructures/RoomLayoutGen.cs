#nullable enable
using System;
using System.Collections.Generic;
using System.Linq;
using SpawnHouses.Helpers;

namespace SpawnHouses.AdvStructures;

public class RoomLayoutGen
{
    public static readonly (StructureTag[] possibleTags, Func<RoomLayoutParams, RoomLayout> method)[] GenMethods =
    [
        (
            [
                StructureTag.HasRoom,
                StructureTag.ProceduralRoom
            ],
            RoomLayout1
        )
    ];

    public static Func<RoomLayoutParams, RoomLayout> GetRandomMethod(RoomLayoutParams componentLayoutParams)
    {
        List<(StructureTag[] possibleTags, Func<RoomLayoutParams, RoomLayout> method)> methodTuples = [];
        foreach (var tuple in GenMethods)
        {
            List<StructureTag> requiredTags = componentLayoutParams.TagsRequired.ToList();
            foreach (var possibleTag in tuple.possibleTags)
            {
                if (componentLayoutParams.TagsBlacklist.Contains(possibleTag))
                    break;
                requiredTags.Remove(possibleTag);
            }

            if (requiredTags.Count == 0)
                methodTuples.Add(tuple);
        }

        if (methodTuples.Count == 0)
            throw new Exception("No components found were compatible with given tags");
        return methodTuples[Terraria.WorldGen.genRand.Next(0, methodTuples.Count)].method;
    }


    /// <summary>
    /// procedural BSP algorithm to split rooms
    /// </summary>
    public static RoomLayout RoomLayout1(RoomLayoutParams roomLayoutParams)
    {
        var corners = RoomLayoutHelper.GetCorners(roomLayoutParams.MainVolume);

        RoomLayout[] possibleLayouts = new RoomLayout[roomLayoutParams.Attempts];
        for (int attempt = 0; attempt < roomLayoutParams.Attempts; attempt++)
        {
            RoomLayout layout = RoomLayoutHelper.SplitBsp(roomLayoutParams, corners.xCoords, corners.yCoords);
            if (layout.BackgroundVolumes.Count == roomLayoutParams.Housing)
                return layout;
            possibleLayouts[attempt] = layout;
        }

        // find the layout with the closest housing to the requested amount
        int closetHousingCount = Math.Abs(possibleLayouts[0].BackgroundVolumes.Count - roomLayoutParams.Housing);
        int closetHousingIndex = 0;
        for (int i = 0; i < possibleLayouts.Length; i++)
        {
            if (Math.Abs(possibleLayouts[i].BackgroundVolumes.Count - roomLayoutParams.Housing) < closetHousingCount)
            {
                closetHousingCount = Math.Abs(possibleLayouts[i].BackgroundVolumes.Count - roomLayoutParams.Housing);
                closetHousingIndex = i;
            }
        }

        return possibleLayouts[closetHousingIndex];
    }
}