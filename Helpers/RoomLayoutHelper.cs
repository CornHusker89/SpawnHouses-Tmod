using System;
using System.Collections.Generic;
using System.Linq;
using SpawnHouses.AdvStructures.AdvStructureParts;
using SpawnHouses.Structures;
using SpawnHouses.Types;
using Terraria.DataStructures;
using Terraria.ModLoader;
using Range = SpawnHouses.Structures.Range;

namespace SpawnHouses.Helpers;

public static class RoomLayoutHelper {
    public static bool IsValidSize(Shape volume, RoomLayoutParams roomLayoutParams) {
        return volume.Size.X >= roomLayoutParams.RoomWidth.Min &&
               volume.Size.X <= roomLayoutParams.RoomWidth.Max &&
               volume.Size.Y >= roomLayoutParams.RoomHeight.Min &&
               volume.Size.Y <= roomLayoutParams.RoomHeight.Max;
    }

    /// <summary>
    ///     find all corners of a shape based on their x and y positions, useful for ensuring beams and such make sense
    ///     visually
    /// </summary>
    /// <param name="shape"></param>
    /// <returns></returns>
    public static (List<int> xCoords, List<int> yCoords) GetCorners(Shape shape) {
        List<int> cornersX = [];
        List<int> cornersY = [];

        foreach (var point in shape.Points) {
            if (point.X != shape.BoundingBox.topLeft.X &&
                point.X != shape.BoundingBox.bottomRight.X && !cornersX.Contains(point.X))
                cornersX.Add(point.X);
            if (point.Y != shape.BoundingBox.topLeft.Y &&
                point.Y != shape.BoundingBox.bottomRight.Y && !cornersY.Contains(point.Y))
                cornersY.Add(point.Y);
        }

        return (cornersX, cornersY);
    }

    /// <summary>
    ///     uses a binary space partitioning algorithm to procedurally split a shape into a <see cref="RoomLayout" />
    /// </summary>
    /// <param name="roomLayoutParams"></param>
    /// <param name="priorityXSplits"></param>
    /// <param name="priorityYSplits"></param>
    /// <returns></returns>
    public static RoomLayoutVolumes SplitBsp(RoomLayoutParams roomLayoutParams, List<int> priorityXSplits,
        List<int> priorityYSplits) {
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
        var roomQueue = new Queue<Shape>([roomLayoutParams.MainVolume]);
        List<Shape> finishedRoomVolumes = [];
        var extraCuts = 0;
        var largeRoomCount = 0;
        var xCutCount = 0;
        var yCutCount = 0;
        var maxLargeRooms = (int)Math.Ceiling(roomLayoutParams.LargeRoomChance * roomLayoutParams.Housing);
        for (var curHousing = 0; curHousing < roomLayoutParams.Housing + extraCuts - 1; curHousing++) {
            Shape roomVolume;
            if (roomQueue.Count > 0)
                roomVolume = roomQueue.Dequeue();
            else
                break;

            var inverseProgressFactor = double.Max(1 - (double)curHousing / roomLayoutParams.Housing, 0);
            var iterationFloorWidth =
                (int)Math.Round((roomLayoutParams.FloorWidth.Max - roomLayoutParams.FloorWidth.Min) *
                                inverseProgressFactor) +
                roomLayoutParams.FloorWidth.Min;
            var iterationWallWidth = (int)Math.Round((roomLayoutParams.WallWidth.Max - roomLayoutParams.WallWidth.Min) *
                                                     inverseProgressFactor) +
                                     roomLayoutParams.WallWidth.Min;

            var canSplitAlongX = roomVolume.Size.Y >= iterationFloorWidth + 2 * roomLayoutParams.RoomHeight.Min;
            var canSplitAlongY = roomVolume.Size.X >= iterationWallWidth + 2 * roomLayoutParams.RoomWidth.Min;

            bool splitAlongX;

            if (canSplitAlongX && canSplitAlongY) {
                if (roomVolume.Size.Y > roomLayoutParams.RoomHeight.Max) {
                    splitAlongX = true;
                }
                else {
                    var xWeight = Math.Pow(1.0 / (xCutCount + 2), 3);
                    var yWeight = Math.Pow(1.0 / (yCutCount + 1), 3);
                    var totalWeight = xWeight + yWeight;
                    splitAlongX =
                        Terraria.WorldGen.genRand.NextDouble() <
                        xWeight / totalWeight; //xWeight / totalWeight represents chance of picking x cut
                }
            }
            else if (!canSplitAlongX && canSplitAlongY) {
                splitAlongX = false;
            }
            else if (canSplitAlongX) {
                splitAlongX = true;
            }
            else {
                // if can't split at all, don't add this room back to the queue
                finishedRoomVolumes.Add(roomVolume);
                extraCuts++;
                continue;
            }

            if (splitAlongX)
                xCutCount++;
            else
                yCutCount++;

            var outerBoundaryWidth = splitAlongX ? roomLayoutParams.RoomHeight.Min : roomLayoutParams.RoomWidth.Min;
            var validCutRange = new Range(
                (splitAlongX ? roomVolume.BoundingBox.topLeft.Y : roomVolume.BoundingBox.topLeft.X) +
                outerBoundaryWidth,
                (splitAlongX ? roomVolume.BoundingBox.bottomRight.Y : roomVolume.BoundingBox.bottomRight.X) -
                outerBoundaryWidth - (splitAlongX ? iterationFloorWidth : iterationWallWidth)
            );

            List<int> predeterminedSplits = [];
            foreach (var predeterminedSplit in splitAlongX ? priorityXSplits : priorityYSplits)
                if (validCutRange.InRange(predeterminedSplit))
                    predeterminedSplits.Add(predeterminedSplit);

            var splitStart = predeterminedSplits.Count == 0
                ? Terraria.WorldGen.genRand.Next(validCutRange.Min, validCutRange.Max + 1)
                : predeterminedSplits[Terraria.WorldGen.genRand.Next(predeterminedSplits.Count)];
            var splitEnd = splitAlongX
                ? splitStart + iterationFloorWidth - 1
                : splitStart + iterationWallWidth - 1;

            var roomSubsections = roomVolume.CutTwice(splitAlongX, splitStart, splitEnd);
            if (roomSubsections.lower is not null)
                if (roomLayoutParams.IsWithinMaxSize(roomSubsections.lower) &&
                    Terraria.WorldGen.genRand.NextDouble() <
                    (1 - Math.Pow(1 - roomLayoutParams.LargeRoomChance, roomLayoutParams.Attempts)) * 0.35 &&
                    largeRoomCount < maxLargeRooms) {
                    largeRoomCount++;
                    finishedRoomVolumes.Add(roomSubsections.lower);
                    extraCuts++;
                }
                else {
                    roomQueue.Enqueue(roomSubsections.lower);
                }

            if (roomSubsections.higher is not null)
                if (roomLayoutParams.IsWithinMaxSize(roomSubsections.higher) &&
                    Terraria.WorldGen.genRand.NextDouble() <
                    (1 - Math.Pow(1 - roomLayoutParams.LargeRoomChance, roomLayoutParams.Attempts)) * 0.35 &&
                    largeRoomCount < maxLargeRooms) {
                    largeRoomCount++;
                    finishedRoomVolumes.Add(roomSubsections.higher);
                    extraCuts++;
                }
                else {
                    roomQueue.Enqueue(roomSubsections.higher);
                }

            if (roomSubsections.middle is not null) {
                if (splitAlongX)
                    floorVolumes.Add(roomSubsections.middle);
                else
                    wallVolumes.Add(roomSubsections.middle);
            }
        }

        finishedRoomVolumes.AddRange(roomQueue);
        return new RoomLayoutVolumes(floorVolumes, wallVolumes, finishedRoomVolumes);
    }

    /// <returns>null if no room found</returns>
    public static Room GetRoomFromPos(List<Room> rooms, Point16 point) {
        foreach (var room in rooms)
            if (room.Volume.Contains(point))
                return room;

        return null;
    }

    public static List<Gap> CreateGapsFromVolumes(
        List<(Room room1, Room room2, List<Shape> volumes, bool isHorizontal)> gapSections) {
        List<Gap> gaps = [];
        foreach (var incompleteGapSection in gapSections)
            gaps.Add(new Gap(
                Shape.Union(incompleteGapSection.volumes),
                incompleteGapSection.room1, incompleteGapSection.room2, incompleteGapSection.isHorizontal
            ));

        return gaps;
    }

    /// <summary>
    ///     adds gaps between rooms in a <see cref="RoomLayoutVolumes" /> for doors, etc.
    /// </summary>
    /// <param name="roomLayoutVolumes"></param>
    /// <returns></returns>
    public static RoomLayout CreateGaps(RoomLayoutVolumes roomLayoutVolumes) {
        var rooms = roomLayoutVolumes.RoomVolumes.Select(roomVolume => new Room(roomVolume, [])).ToList();
        List<Gap> allGaps = [];

        foreach (var room in rooms) {
            List<Gap> roomGaps = [];
            Room lastRoom = null;
            List<Shape> curGapVolumes = [];
            var isHorizontal = false;
            var lastDirection = Directions.None;

            room.Volume.ExecuteOnPerimeter((x, y, direction) => {
                var pos = new Point16(x, y);
                var step = new Point16(0, 0);
                if (lastDirection == Directions.None)
                    lastDirection = direction;
                switch (direction) {
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

                pos += step;
                while (direction is Directions.Up or Directions.Down
                           ? roomLayoutVolumes.InFloor(pos)
                           : roomLayoutVolumes.InWall(pos))
                    pos += step;

                var foundRoom = GetRoomFromPos(rooms, pos);
                if (foundRoom == room)
                    foundRoom = null; // invalidate casts that find its own room

                if (direction != lastDirection && curGapVolumes.Count != 0) {
                    roomGaps.Add(new Gap(Shape.Union(curGapVolumes), room, lastRoom, isHorizontal));
                    curGapVolumes = [];
                }

                if (foundRoom != null) {
                    if (curGapVolumes.Count == 0) {
                        isHorizontal = direction is Directions.Left or Directions.Right;
                        lastRoom = foundRoom;
                    }

                    curGapVolumes.Add(new Shape(pos - step, new Point16(x, y) + step));
                }
                else {
                    // reset curGapSection
                    if (curGapVolumes.Count != 0) {
                        roomGaps.Add(new Gap(Shape.Union(curGapVolumes), room, lastRoom, isHorizontal));
                        curGapVolumes = [];
                    }
                }

                lastDirection = direction;
            });

            // prune gaps the player can't fit through
            for (var i = 0; i < roomGaps.Count; i++)
                if ((roomGaps[i].Volume.Size.Y < 3 && roomGaps[i].IsHorizontal) ||
                    (roomGaps[i].Volume.Size.X < 2 && !roomGaps[i].IsHorizontal)) {
                    roomGaps.RemoveAt(i);
                    i--;
                }

            room.Gaps = roomGaps;
            allGaps.AddRange(roomGaps);
        }

        // optimize gaps
        for (var i = allGaps.Count - 1; i >= 0; i--)
        for (var j = i - 1; j >= 0; j--) {
            var gap = allGaps[i];
            var otherGap = allGaps[j];

            if (gap.RepresentsSimilarGap(otherGap)) {
                otherGap.LowerRoom.Gaps.Add(gap);
                otherGap.LowerRoom.Gaps.Remove(otherGap);
                otherGap.HigherRoom!.Gaps.Add(gap);
                otherGap.HigherRoom.Gaps.Remove(otherGap);
                allGaps.RemoveAt(j);
                break;
            }
        }

        List<Gap> verticalGaps = [];
        for (var i = allGaps.Count - 1; i >= 0; i--)
            if (!allGaps[i].IsHorizontal) {
                verticalGaps.Add(allGaps[i]);
                allGaps.RemoveAt(i);
            }

        return new RoomLayout(roomLayoutVolumes.FloorVolumes, allGaps, roomLayoutVolumes.WallVolumes, verticalGaps,
            rooms);
    }
}