using System;
using System.Collections.Generic;
using SpawnHouses.AdvStructures;
using SpawnHouses.Structures.AdvStructures;
using SpawnHouses.Structures.StructureParts;
using Terraria;
using Terraria.ModLoader;
using Range = SpawnHouses.Structures.Range;

namespace SpawnHouses.Helpers;

public static class RoomLayoutHelper
{
    public static bool IsValidSize(Shape volume, RoomLayoutParams roomLayoutParams)
    {
        return volume.Size.X >= roomLayoutParams.RoomWidth.Min &&
            volume.Size.X <= roomLayoutParams.RoomWidth.Max &&
            volume.Size.Y >= roomLayoutParams.RoomHeight.Min &&
            volume.Size.Y <= roomLayoutParams.RoomHeight.Max;
    }

    /// <summary>
    /// true if volume's dimensions are not smaller than min sizes
    /// </summary>
    /// <param name="volume"></param>
    /// <param name="roomLayoutParams"></param>
    /// <returns></returns>
    public static bool IsWithinMinSize(Shape volume, RoomLayoutParams roomLayoutParams)
    {
        return volume.Size.X >= roomLayoutParams.RoomWidth.Min &&
               volume.Size.Y >= roomLayoutParams.RoomHeight.Min;
    }

    /// <summary>
    /// true if volume's dimensions are not larger than max sizes
    /// </summary>
    /// <param name="volume"></param>
    /// <param name="roomLayoutParams"></param>
    /// <returns></returns>
    public static bool IsWithinMaxSize(Shape volume, RoomLayoutParams roomLayoutParams)
    {
        return volume.Size.X <= roomLayoutParams.RoomWidth.Max &&
               volume.Size.Y <= roomLayoutParams.RoomHeight.Max;
    }

    /// <summary>
    /// find all corners of a shape based on their x and y positions, useful for ensuring beams and such make sense visually
    /// </summary>
    /// <param name="shape"></param>
    /// <returns></returns>
    public static (List<int> xCoords, List<int> yCoords) GetCorners(Shape shape)
    {
        List<int> cornersX = [];
        List<int> cornersY = [];

        foreach (var point in shape.Points)
        {
            if (point.X != shape.BoundingBox.topLeft.X &&
                point.X != shape.BoundingBox.bottomRight.X && !cornersX.Contains(point.X))
            {
                cornersX.Add(point.X);
            }
            if (point.Y != shape.BoundingBox.topLeft.Y &&
                point.Y != shape.BoundingBox.bottomRight.Y && !cornersY.Contains(point.Y))
            {
                cornersY.Add(point.Y);
            }
        }
        return (cornersX, cornersY);
    }

    /// <summary>
    /// uses a binary space partitioning algorithm to procedurally split a shape into a <see cref="RoomLayout"/>
    /// </summary>
    /// <param name="roomLayoutParams"></param>
    /// <param name="priorityXSplits"></param>
    /// <param name="priorityYSplits"></param>
    /// <returns></returns>
    public static RoomLayout SplitBsp(RoomLayoutParams roomLayoutParams, List<int> priorityXSplits, List<int> priorityYSplits)
    {
        if (roomLayoutParams.RoomHeight.Max < roomLayoutParams.FloorWidth + 2 * roomLayoutParams.RoomHeight.Min)
            ModContent.GetInstance<SpawnHouses>().Logger.Warn(
                $"a max room height of {roomLayoutParams.RoomHeight.Max} was given, but at least {roomLayoutParams.FloorWidth + 2 * roomLayoutParams.RoomHeight.Min} is required");

        List<Shape> floorVolumes = [];
        List<Shape> floorGapVolumes = [];
        List<Shape> wallVolumes = [];
        List<Shape> wallGapVolumes = [];
        Queue<Shape> roomQueue = new Queue<Shape>([roomLayoutParams.MainVolume]);
        List<Shape> finishedRoomVolumes = [];
        int extraCuts = 0;
        int largeRoomCount = 0;
        int xCutCount = 0;
        int yCutCount = 0;
        int maxLargeRooms = (int)Math.Ceiling(roomLayoutParams.LargeRoomChance * roomLayoutParams.Housing);
        for (int curHousing = 0; curHousing < roomLayoutParams.Housing + extraCuts - 1; curHousing++)
        {
            Shape roomVolume;
            if (roomQueue.Count > 0)
                roomVolume = roomQueue.Dequeue();
            else
                break;

            bool canSplitAlongX = roomVolume.Size.Y >= roomLayoutParams.FloorWidth + 2 * roomLayoutParams.RoomHeight.Min;
            bool canSplitAlongY = roomVolume.Size.X >= roomLayoutParams.WallWidth + 2 * roomLayoutParams.RoomWidth.Min;

            bool splitAlongX;

            if (canSplitAlongX && canSplitAlongY)
                if (roomVolume.Size.Y > roomLayoutParams.RoomHeight.Max)
                    splitAlongX = true;
                else
                {
                    double xWeight = Math.Pow(1.0 / (xCutCount + 2), 3);
                    double yWeight = Math.Pow(1.0 / (yCutCount + 1), 3);
                    double totalWeight = xWeight + yWeight;
                    splitAlongX = Terraria.WorldGen.genRand.NextDouble() < xWeight / totalWeight; //xWeight / totalWeight represents chance of picking x cut
                }
            else if (!canSplitAlongX && canSplitAlongY)
                splitAlongX = false;
            else if (canSplitAlongX)
                splitAlongX = true;
            else
            {
                // if can't split at all, don't add this room back to the queue
                finishedRoomVolumes.Add(roomVolume);
                extraCuts++;
                continue;
            }

            if (splitAlongX)
                xCutCount++;
            else
                yCutCount++;

            int outerBoundaryWidth = splitAlongX ? roomLayoutParams.RoomHeight.Min : roomLayoutParams.RoomWidth.Min;
            Range validCutRange = new Range(
                (splitAlongX ? roomVolume.BoundingBox.topLeft.Y : roomVolume.BoundingBox.topLeft.X) + outerBoundaryWidth,
                (splitAlongX ? roomVolume.BoundingBox.bottomRight.Y : roomVolume.BoundingBox.bottomRight.X) -
                outerBoundaryWidth - (splitAlongX ? roomLayoutParams.FloorWidth : roomLayoutParams.WallWidth)
            );

            List<int> predeterminedSplits = [];
            foreach (int predeterminedSplit in splitAlongX ? priorityXSplits : priorityYSplits)
                if (validCutRange.InRange(predeterminedSplit))
                    predeterminedSplits.Add(predeterminedSplit);

            int splitStart = predeterminedSplits.Count == 0
                ? Terraria.WorldGen.genRand.Next(validCutRange.Min, validCutRange.Max + 1)
                : predeterminedSplits[Terraria.WorldGen.genRand.Next(predeterminedSplits.Count)];
            int splitEnd = splitAlongX
                ? splitStart + roomLayoutParams.FloorWidth - 1
                : splitStart + roomLayoutParams.WallWidth - 1;

            var roomSubsections = roomVolume.CutTwice(splitAlongX, splitStart, splitEnd);
            if (roomSubsections.lower is not null)
                if (IsWithinMaxSize(roomSubsections.lower, roomLayoutParams) &&
                    Terraria.WorldGen.genRand.NextDouble() < (1 - Math.Pow(1 - roomLayoutParams.LargeRoomChance, roomLayoutParams.Attempts)) * 0.35 &&
                    largeRoomCount < maxLargeRooms)
                {
                    largeRoomCount++;
                    finishedRoomVolumes.Add(roomVolume);
                    extraCuts++;
                }
                else
                {
                    roomQueue.Enqueue(roomSubsections.lower);
                }

            if (roomSubsections.higher is not null)
                if (IsWithinMaxSize(roomSubsections.higher, roomLayoutParams) &&
                    Terraria.WorldGen.genRand.NextDouble() < (1 - Math.Pow(1 - roomLayoutParams.LargeRoomChance, roomLayoutParams.Attempts)) * 0.35 &&
                    largeRoomCount < maxLargeRooms)
                {
                    largeRoomCount++;
                    finishedRoomVolumes.Add(roomVolume);
                    extraCuts++;
                }
                else
                {
                    roomQueue.Enqueue(roomSubsections.higher);
                }

            if (roomSubsections.middle is not null)
            {
                if (splitAlongX)
                    floorVolumes.Add(roomSubsections.middle!);
                else
                    wallVolumes.Add(roomSubsections.middle!);
            }
        }
        finishedRoomVolumes.AddRange(roomQueue);

        return new RoomLayout(
            floorVolumes,
            null,
            wallVolumes,
            null,
            null,
            null,
            finishedRoomVolumes,
            largeRoomCount
        );
    }
}