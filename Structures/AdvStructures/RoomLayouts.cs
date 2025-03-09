#nullable enable
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using SpawnHouses.Structures.StructureParts;
using Terraria;
using Terraria.DataStructures;

namespace SpawnHouses.Structures.AdvStructures;

public class RoomLayouts
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
        List<Shape> floorVolumes = [];
        List<Shape> floorGapVolumes = [];
        List<Shape> wallVolumes = [];
        List<Shape> wallGapVolumes = [];

        List<Shape> extraRoomVolumes = [];

        List<int> potentialXSplits = [];
        List<int> potentialYSplits = [];

        Queue<Shape> roomQueue = new Queue<Shape>([roomLayoutParams.MainVolume]);

        foreach (var point in roomLayoutParams.MainVolume.Points)
        {
            if (point.X != roomLayoutParams.MainVolume.BoundingBox.topLeft.X &&
                point.X != roomLayoutParams.MainVolume.BoundingBox.bottomRight.X && !potentialXSplits.Contains(point.X))
            {
                potentialXSplits.Add(point.X);
            }
            if (point.Y != roomLayoutParams.MainVolume.BoundingBox.topLeft.Y &&
                point.Y != roomLayoutParams.MainVolume.BoundingBox.bottomRight.Y && !potentialYSplits.Contains(point.Y))
            {
                potentialYSplits.Add(point.Y);
            }
        }

        RoomLayout[] possibleLayouts = new RoomLayout[5];
        for (int attempt = 0; attempt < roomLayoutParams.Attempts; attempt++)
        {
            int extraCuts = 0;
            for (int curHousing = 0; curHousing < roomLayoutParams.Housing + extraCuts - 1; curHousing++)
            {
                Shape roomVolume;
                if (roomQueue.Count > 0)
                    roomVolume = roomQueue.Dequeue();
                else
                    break;

                bool canSplitAlongX = roomVolume.BoundingBox.bottomRight.Y - roomVolume.BoundingBox.topLeft.Y >= roomLayoutParams.FloorWidth + 2 * roomLayoutParams.RoomHeight.Min;
                bool canSplitAlongY = roomVolume.BoundingBox.bottomRight.X - roomVolume.BoundingBox.topLeft.X >= roomLayoutParams.WallWidth + roomLayoutParams.RoomMinWidth &&
                                      roomVolume.BoundingBox.bottomRight.Y - roomVolume.BoundingBox.topLeft.Y <= roomLayoutParams.RoomHeight.Max;
                bool splitAlongX;
                if (canSplitAlongX && canSplitAlongY)
                    splitAlongX = Terraria.WorldGen.genRand.NextBool();
                else if (!canSplitAlongX && canSplitAlongY)
                    splitAlongX = false;
                else if (canSplitAlongX)
                    splitAlongX = true;
                else
                {
                    // if can't split at all, don't add this room back to the queue
                    extraRoomVolumes.Add(roomVolume);
                    extraCuts++;
                    continue;
                }

                int outerBoundaryWidth = splitAlongX ? 4 : 7;
                Range validCutRange = new Range(
                    (splitAlongX ? roomVolume.BoundingBox.topLeft.Y : roomVolume.BoundingBox.topLeft.X) +
                    outerBoundaryWidth + 1,
                    (splitAlongX ? roomVolume.BoundingBox.bottomRight.Y : roomVolume.BoundingBox.bottomRight.X) -
                    outerBoundaryWidth - (splitAlongX ? roomLayoutParams.FloorWidth : roomLayoutParams.WallWidth)
                );

                List<int> predeterminedSplits = [];
                foreach (int predeterminedSplit in splitAlongX ? potentialXSplits : potentialYSplits)
                    if (validCutRange.InRange(predeterminedSplit))
                        predeterminedSplits.Add(predeterminedSplit);

                int splitStart = predeterminedSplits.Count == 0
                    ? Terraria.WorldGen.genRand.Next(validCutRange.Min, validCutRange.Max + 1)
                    : predeterminedSplits[Terraria.WorldGen.genRand.Next(predeterminedSplits.Count)];
                int splitEnd = splitAlongX
                    ? splitStart + roomLayoutParams.FloorWidth - 1
                    : splitStart + roomLayoutParams.WallWidth - 1;

                var roomSubsections = roomVolume.CutTwice(splitAlongX, splitStart, splitEnd);
                if (roomSubsections.lower?.GetArea() < roomLayoutParams.RoomMinVolume || roomSubsections.higher?.GetArea() < roomLayoutParams.RoomMinVolume)
                {
                    extraRoomVolumes.Add(roomVolume);
                    extraCuts++;
                    continue;
                }
                if (roomSubsections.lower is not null)
                    roomQueue.Enqueue(roomSubsections.lower!);

                if (roomSubsections.higher is not null)
                    roomQueue.Enqueue(roomSubsections.higher!);

                if (roomSubsections.middle is not null)
                {
                    if (splitAlongX)
                        floorVolumes.Add(roomSubsections.middle!);
                    else
                        wallVolumes.Add(roomSubsections.middle!);
                }
            }
            List<Shape> roomVolumes = roomQueue.ToList();
            roomVolumes.AddRange(extraRoomVolumes);

            possibleLayouts[attempt] = new RoomLayout(
                floorVolumes,
                null,
                wallVolumes,
                null,
                null,
                null,
                roomVolumes
            );

            if (roomVolumes.Count >= roomLayoutParams.Housing)
                break;
        }

        // find the layout with the closest housing
        int closetHousing = -1;
        int closetHousingIndex = 0;
        for (int i = 0; i < possibleLayouts.Length; i++)
        {
            var layout = possibleLayouts[i];
            if (layout.BackgroundVolumes.Count == roomLayoutParams.Housing)
                return layout;
            if (Math.Abs(layout.BackgroundVolumes.Count - roomLayoutParams.Housing) < closetHousing)
            {
                closetHousing = Math.Abs(layout.BackgroundVolumes.Count - roomLayoutParams.Housing);
                closetHousingIndex = i;
            }
        }

        return possibleLayouts[closetHousingIndex];
    }
}