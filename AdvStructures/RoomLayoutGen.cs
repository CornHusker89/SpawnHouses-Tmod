#nullable enable
using System;
using System.Collections.Generic;
using System.Linq;
using SpawnHouses.Helpers;
using SpawnHouses.Types;

namespace SpawnHouses.AdvStructures;

public class RoomLayoutGen {
    public static readonly (StructureTag[] possibleTags, Func<RoomLayoutParams, RoomLayout> method)[] GenMethods = [
        (
            [
                StructureTag.HasRoomLayout,
                StructureTag.ProceduralRoomLayout,
                StructureTag.HasLargeRoom,
                StructureTag.HasFlatFloors
            ],
            RoomLayout1
        )
    ];

    public static Func<RoomLayoutParams, RoomLayout> GetRandomMethod(RoomLayoutParams roomLayoutParams) {
        List<(StructureTag[] possibleTags, Func<RoomLayoutParams, RoomLayout> method)> methodTuples = [];
        foreach (var tuple in GenMethods) {
            var requiredTags = roomLayoutParams.TagsRequired.ToList();
            foreach (StructureTag possibleTag in tuple.possibleTags) {
                if (roomLayoutParams.TagsBlacklist.Contains(possibleTag))
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
    ///     procedural BSP algorithm to split rooms
    /// </summary>
    public static RoomLayout RoomLayout1(RoomLayoutParams roomLayoutParams) {
        var corners = RoomLayoutHelper.GetCorners(roomLayoutParams.MainVolume);
        RoomLayoutVolumes? pickedLayout = null;

        var possibleLayouts = new RoomLayoutVolumes[roomLayoutParams.Attempts];
        for (int attempt = 0; attempt < roomLayoutParams.Attempts; attempt++) {
            RoomLayoutVolumes? volumes = RoomLayoutHelper.SplitBsp(roomLayoutParams, corners.xCoords, corners.yCoords);
            if (volumes.RoomVolumes.Count == roomLayoutParams.Housing) {
                pickedLayout = volumes;
                break;
            }

            possibleLayouts[attempt] = volumes;
        }

        // find the layout with the closest housing to the requested amount
        if (pickedLayout is null) {
            int closetHousingCount = Math.Abs(possibleLayouts[0].RoomVolumes.Count - roomLayoutParams.Housing);
            for (int i = 0; i < possibleLayouts.Length; i++)
                if (Math.Abs(possibleLayouts[i].RoomVolumes.Count - roomLayoutParams.Housing) < closetHousingCount) {
                    closetHousingCount = Math.Abs(possibleLayouts[i].RoomVolumes.Count - roomLayoutParams.Housing);
                    pickedLayout = possibleLayouts[i];
                }
        }

        return RoomLayoutHelper.CreateGaps(pickedLayout);
    }
}