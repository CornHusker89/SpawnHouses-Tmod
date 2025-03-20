using System;
using System.Collections.Generic;
using System.Linq;
using SpawnHouses.AdvStructures;
using SpawnHouses.AdvStructures.AdvStructureParts;
using SpawnHouses.Structures;
using SpawnHouses.Structures.StructureParts;
using SpawnHouses.Types;
using Terraria;
using Terraria.DataStructures;
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
    public static RoomLayoutVolumes SplitBsp(RoomLayoutParams roomLayoutParams, List<int> priorityXSplits, List<int> priorityYSplits)
    {
        if (roomLayoutParams.RoomHeight.Max < roomLayoutParams.FloorWidth.Max + 2 * roomLayoutParams.RoomHeight.Min)
            ModContent.GetInstance<SpawnHouses>().Logger.Warn(
                $"a max room height of {roomLayoutParams.RoomHeight.Max} was given, but at least {roomLayoutParams.FloorWidth.Max + 2 * roomLayoutParams.RoomHeight.Min} is required");
        if (roomLayoutParams.RoomWidth.Max < roomLayoutParams.WallWidth.Max + 2 * roomLayoutParams.RoomWidth.Min)
            ModContent.GetInstance<SpawnHouses>().Logger.Warn(
                $"a max room height of {roomLayoutParams.RoomWidth.Max} was given, but at least {roomLayoutParams.WallWidth.Max + 2 * roomLayoutParams.RoomWidth.Min} is required");

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

            double inverseProgressFactor = double.Max(1 - (double)curHousing / roomLayoutParams.Housing, 0);
            int iterationFloorWidth = (int)Math.Round((roomLayoutParams.FloorWidth.Max - roomLayoutParams.FloorWidth.Min) * inverseProgressFactor) +
                roomLayoutParams.FloorWidth.Min;
            int iterationWallWidth = (int)Math.Round((roomLayoutParams.WallWidth.Max - roomLayoutParams.WallWidth.Min) * inverseProgressFactor) +
                roomLayoutParams.WallWidth.Min;

            bool canSplitAlongX = roomVolume.Size.Y >= iterationFloorWidth + 2 * roomLayoutParams.RoomHeight.Min;
            bool canSplitAlongY = roomVolume.Size.X >= iterationWallWidth + 2 * roomLayoutParams.RoomWidth.Min;

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
                outerBoundaryWidth - (splitAlongX ? iterationFloorWidth : iterationWallWidth)
            );

            List<int> predeterminedSplits = [];
            foreach (int predeterminedSplit in splitAlongX ? priorityXSplits : priorityYSplits)
                if (validCutRange.InRange(predeterminedSplit))
                    predeterminedSplits.Add(predeterminedSplit);

            int splitStart = predeterminedSplits.Count == 0
                ? Terraria.WorldGen.genRand.Next(validCutRange.Min, validCutRange.Max + 1)
                : predeterminedSplits[Terraria.WorldGen.genRand.Next(predeterminedSplits.Count)];
            int splitEnd = splitAlongX
                ? splitStart + iterationFloorWidth - 1
                : splitStart + iterationWallWidth - 1;

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
        return new RoomLayoutVolumes(floorVolumes, wallVolumes, finishedRoomVolumes);
    }

    /// <summary>
    /// gets closest room to the point
    /// </summary>
    /// <param name="roomLayout"></param>
    /// <param name="point"></param>
    /// <returns></returns>
    public static Room GetClosestRoom(RoomLayout roomLayout, Point16 point)
    {
        if (roomLayout.Rooms.Count == 0)
            throw new Exception("BackgroundVolumes in given room layout was empty");

        int closestIndex = -1;
        double closestDistance = double.MaxValue;

        for (int i = 0; i < roomLayout.Rooms.Count; i++)
        {
            double closestDistanceInShape = double.MaxValue;
            roomLayout.Rooms[i].Volume.ExecuteOnPerimeter((x, y, _) =>
            {
                double distance = Math.Sqrt(Math.Pow(point.X - x, 2) + Math.Pow(point.Y - y, 2));
                if (distance < closestDistanceInShape)
                    closestDistanceInShape = distance;
            });

            if (closestDistanceInShape < closestDistance)
            {
                closestDistance = closestDistanceInShape;
                closestIndex = i;
            }
        }
        return roomLayout.Rooms[closestIndex];
    }

    public static bool InFloor(RoomLayoutVolumes roomLayoutVolumes, Point16 point)
    {
        foreach (var floorVolume in roomLayoutVolumes.FloorVolumes)
            if (floorVolume.Contains(point))
                return true;
        return false;
    }

    public static bool InWall(RoomLayoutVolumes roomLayoutVolumes, Point16 point)
    {
        foreach (var floorVolume in roomLayoutVolumes.FloorVolumes)
            if (floorVolume.Contains(point))
                return true;
        return false;
    }

    /// <returns>null if no room found</returns>
    public static Room GetRoomFromPos(List<Room> rooms, Point16 point)
    {
        foreach (var room in rooms)
            if (room.Volume.Contains(point))
                return room;

        return null;
    }

    public static List<(Room otherRoom, Shape volume)> OptimizeGapVolumes(List<(Room otherRoom, List<Shape> volumes)> gapSections)
    {
        List<(Room otherRoom, Shape volume)> gaps = [];
        foreach(var incompleteGapSection in gapSections)
            gaps.Add((incompleteGapSection.otherRoom, Shape.Union(incompleteGapSection.volumes)));

        return gaps;
    }

    /// <summary>
    /// adds gaps between rooms in a <see cref="RoomLayoutVolumes"/> for doors, etc.
    /// </summary>
    /// <param name="roomLayoutVolumes"></param>
    /// <returns></returns>
    public static RoomLayout CreateGaps(RoomLayoutVolumes roomLayoutVolumes)
    {
        List<Room> rooms = (List<Room>)roomLayoutVolumes.RoomVolumes.Select(roomVolume => new Room(roomVolume, []));
        List<Gap> gaps = [];

        foreach(Room room in rooms)
        {
            List<(Room otherRoom, List<Shape> volumes)> gapSections = [];
            (Room otherRoom, List<Shape> volumes) curGapSection = (null, []);
            byte lastDirection = Directions.None;

            room.Volume.ExecuteOnPerimeter((x, y, direction) =>
            {
                Point16 pos = new Point16(x, y);
                Point16 step = new Point16(0, 0);
                switch (direction)
                {
                    case Directions.Up:
                        step = new Point16(0, -1);
                        break;
                    case Directions.Down:
                        step = new Point16(0, 1);
                        break;
                    case Directions.Left:
                        step = new Point16(-1, 0);
                        break;
                    case Directions.Right:
                        step = new Point16(1, 0);
                        break;
                }

                do
                    pos += step;
                while (direction is Directions.Up or Directions.Down? InFloor(roomLayoutVolumes, pos): InWall(roomLayoutVolumes, pos));
                Room foundRoom = GetRoomFromPos(rooms, pos);

                if (direction != lastDirection)
                {
                    if (curGapSection.volumes.Count != 0)
                    {
                        gapSections.Add(curGapSection);
                        curGapSection = (null, []);
                    }
                }

                if (foundRoom != null)
                {
                    if (curGapSection.volumes.Count == 0)
                        curGapSection.otherRoom = foundRoom;
                    curGapSection.volumes.Add(new Shape(pos - step, new Point16(x, y) + step));
                }
                else
                {
                    // reset curGapSection
                    if (curGapSection.volumes.Count != 0)
                    {
                        gapSections.Add(curGapSection);
                        curGapSection = (null, []);
                    }
                }
                lastDirection = direction;
            });

            // create list of gaps for the room
            var optimizedGapSections = OptimizeGapVolumes(gapSections);
            foreach(var optimizedGapSection in optimizedGapSections)
            {
                Room lowerRoom, higherRoom;
                if (room.Volume.Center.Max(optimizedGapSection.otherRoom.Volume.Center) == room.Volume.Center)
                {
                    lowerRoom = optimizedGapSection.otherRoom;
                    higherRoom = room;
                }
                else
                {
                    lowerRoom = room;
                    higherRoom = optimizedGapSection.otherRoom;
                }
                Gap gap = new Gap(optimizedGapSection.volume, lowerRoom, higherRoom);
                room.Gaps.Add(gap);
                gaps.Add(gap);
            }
        }
        return new RoomLayout(roomLayoutVolumes.FloorVolumes, roomLayoutVolumes.WallVolumes, rooms, gaps);
    }
}