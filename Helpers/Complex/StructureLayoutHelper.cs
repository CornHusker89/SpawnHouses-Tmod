#nullable enable

using System;
using System.Collections.Generic;
using System.Linq;
using SpawnHouses.Common.Modules.Components;
using SpawnHouses.Common.Parameters;
using SpawnHouses.Common.Tagging;
using SpawnHouses.Common.Types;
using SpawnHouses.Common.Types.Geometry;
using SpawnHouses.Types;
using Terraria;
using Terraria.DataStructures;
using Terraria.ModLoader;
using Range = SpawnHouses.Structures.Range;

namespace SpawnHouses.Helpers.Complex;

public interface IStructureLayoutHelper {
    public static abstract HashSet<Tag> PossibleTags { get; }
}

public static class StructureLayoutHelper {
    /// <summary>
    ///     procedural BSP algorithm to split rooms
    /// </summary>
    public abstract class SubdivideRoom : IStructureLayoutHelper {
        public static HashSet<Tag> PossibleTags => [
            Tags.HasOnlyRectangleRooms,
            Tags.HasSomeRectangleRooms,
            Tags.HasLargeRoom
        ];

        /// <summary>
        ///     tells if the next cut should be along the x or the y-axis, if either
        /// </summary>
        /// <param name="param"></param>
        /// <param name="roomVolume"></param>
        /// <param name="floorWidth"></param>
        /// <param name="wallWidth"></param>
        /// <param name="xCutCount"></param>
        /// <param name="yCutCount"></param>
        /// <returns></returns>
        private static (bool success, bool cutOnX) EvaluateCutOnX(RoomLayoutParams param, Shape roomVolume, int floorWidth, int wallWidth, int xCutCount, int yCutCount) {
            int expandedArea = roomVolume.GetExpandedShape(1).GetArea(true);
            bool canSplitAlongX =
                roomVolume.Size.Y >= floorWidth + 2 * param.RoomHeight.Min // make sure volume is tall enough to be split
                && expandedArea > 90 // make sure area is large enough to be split (area is approximated and it's thoroughly checked later, so it's low balled)
                && roomVolume.Size.X < roomVolume.Size.Y * 1.3; // make sure width/height ratio of resulting rooms isn't stupid

            bool canSplitAlongY =
                roomVolume.Size.X >= wallWidth + 2 * param.RoomWidth.Min
                && expandedArea > 90
                && roomVolume.Size.Y < roomVolume.Size.X * 1.6;

            switch (canSplitAlongX) {
                case true when canSplitAlongY: {
                    if (roomVolume.Size.Y > param.RoomHeight.Max) return (true, true);
                    double xWeight = Math.Pow(1.0 / (xCutCount + 2), 3);
                    double yWeight = Math.Pow(1.0 / (yCutCount + 1), 3);
                    double totalWeight = xWeight + yWeight;
                    // xWeight divided by totalWeight represents chance of picking x cut
                    return (true, Terraria.WorldGen.genRand.NextDouble() < xWeight / totalWeight);
                }
                case false when canSplitAlongY:
                    return (true, false);
                case true:
                    return (true, true);
                default:
                    return (false, false);
            }
        }

        /// <summary>
        ///     removes splits that would make invalid rooms from the list of possible splits
        /// </summary>
        /// <param name="splitStarts"></param>
        /// <param name="shape"></param>
        /// <param name="splitAlongX"></param>
        /// <param name="iterationSplitWidth"></param>
        private static void PruneInvalidSplits(HashSet<int> splitStarts, Shape shape, bool splitAlongX, int iterationSplitWidth) {
            foreach (int splitStartPos in splitStarts) {
                Shape? cutShape = shape.CutOnce(splitAlongX, splitStartPos + iterationSplitWidth - 1, true, true);
                if (cutShape == null || !RoomHelper.IsValidHousingSize(cutShape))
                    splitStarts.Remove(splitStartPos);
            }
        }

        /// <summary>
        ///     gets all possible splits along cut range
        /// </summary>
        /// <param name="roomVolume"></param>
        /// <param name="prioritySplits"></param>
        /// <param name="splitAlongX"></param>
        /// <param name="splitWidth"></param>
        /// <param name="verticalGapXs"></param>
        /// <param name="horizontalGapYs"></param>
        /// <param name="validCutRange"></param>
        /// <returns></returns>
        private static HashSet<int> GetValidSplits(Shape roomVolume, PriorityCollection<PartialPoint16> prioritySplits, bool splitAlongX, int splitWidth,
            HashSet<int> verticalGapXs, HashSet<int> horizontalGapYs, Range validCutRange) {
            // ensure that, taking blocklisted coordinates into account, there is valid places for the split
            HashSet<int> validSplitStarts = [];
            foreach (var tuple in prioritySplits.ToSortedHashSetArray()) {
                foreach (PartialPoint16 split in tuple.set) {
                    if ((splitAlongX && split.HasY) || (!splitAlongX && split.HasX)) // ignore invalid coordinates
                        continue;

                    int splitPos = splitAlongX ? split.Y : split.X;
                    bool valid = true;
                    for (int offset = 0; offset <= splitWidth - 1; offset++)
                        if (splitAlongX ? horizontalGapYs.Contains(splitPos + offset) : verticalGapXs.Contains(splitPos + offset)) {
                            valid = false;
                            break;
                        }

                    if (!valid) continue;
                    validSplitStarts.Add(splitAlongX ? split.Y : split.X);
                }

                if (validSplitStarts.Count != 0) PruneInvalidSplits(validSplitStarts, roomVolume, splitAlongX, splitWidth);

                if (validSplitStarts.Count != 0) // if we find anything at a given priority level, stop there
                    break;
            }

            // if priority splits didn't get anything, then try using normal splits
            if (validSplitStarts.Count == 0)
                for (int pos = validCutRange.Min; pos <= validCutRange.Max; pos++) {
                    // for gaps check along entire split
                    bool valid = true;
                    for (int offset = 0; offset <= splitWidth - 1; offset++)
                        if (splitAlongX ? horizontalGapYs.Contains(pos + offset) : verticalGapXs.Contains(pos + offset)) {
                            valid = false;
                            break;
                        }

                    if (!valid) continue;
                    validSplitStarts.Add(pos);
                }

            PruneInvalidSplits(validSplitStarts, roomVolume, splitAlongX, splitWidth);
            return validSplitStarts;
        }

        /// <summary>
        ///     uses a binary space partitioning algorithm to procedurally split a room into a <see cref="RoomLayout" />
        /// </summary>
        /// <param name="p"></param>
        /// <param name="prioritySplits"></param>
        /// <param name="room"></param>
        /// <param name="prioritizeSplitsOnGapFloors"></param>
        /// <param name="targetRoomCount"></param>
        /// <returns>RoomLayout is not in component mode</returns>
        /// <remarks>fully clears blocklist before returning</remarks>
        private static RoomLayout SplitBsp(RoomLayoutParams p, Room room, PriorityCollection<PartialPoint16> prioritySplits, bool prioritizeSplitsOnGapFloors, int targetRoomCount) {
            if (p.RoomHeight.Max < p.FloorWidth.Max + 2 * p.RoomHeight.Min)
                ModContent.GetInstance<SpawnHouses>().Logger.Warn(
                    $"a max room height of {p.RoomHeight.Max} was given, but at least {p.FloorWidth.Max + 2 * p.RoomHeight.Min} is required");
            if (p.RoomWidth.Max < p.WallWidth.Max + 2 * p.RoomWidth.Min)
                ModContent.GetInstance<SpawnHouses>().Logger.Warn(
                    $"a max room height of {p.RoomWidth.Max} was given, but at least {p.WallWidth.Max + 2 * p.RoomWidth.Min} is required");

            List<Shape> floorVolumes = [], wallVolumes = [];
            var roomQueue = new Queue<Shape>([room.Geometry]);
            List<Shape> finishedRoomVolumes = [];
            int extraCuts = 0, largeRoomCount = 0, xCutCount = 0, yCutCount = 0;
            int maxLargeRooms = (int)Math.Ceiling(p.LargeRoomChance * targetRoomCount);
            for (int curHousing = 1; curHousing < targetRoomCount + extraCuts; curHousing++) {
                Shape roomVolume;
                if (roomQueue.Count > 0)
                    roomVolume = roomQueue.Dequeue();
                else
                    break;

                // find the priority spots and blocklist spots for the current iteration's shape
                var iterationGaps = RoomHelper.GetAdjacentGaps(roomVolume, room.Gaps);
                HashSet<int> iterationVerticalGapXs = [], iterationHorizontalGapYs = [];
                prioritySplits.RemoveHashSet(0); // clear any gap floors from previous iterations
                foreach (Gap gap in iterationGaps)
                    if (gap.IsHorizontal)
                        for (int y = gap.Geometry.BoundingBox.topLeft.Y; y <= gap.Geometry.BoundingBox.bottomRight.Y; y++) {
                            iterationHorizontalGapYs.Add(y);

                            if (prioritizeSplitsOnGapFloors && gap.Geometry.BoundingBox.bottomRight.Y != room.Geometry.BoundingBox.bottomRight.Y)
                                prioritySplits.AddItem(new PartialPoint16(0, gap.Geometry.BoundingBox.bottomRight.Y + 1, false), 0);
                        }
                    else
                        for (int x = gap.Geometry.BoundingBox.topLeft.X; x <= gap.Geometry.BoundingBox.bottomRight.X; x++)
                            iterationVerticalGapXs.Add(x);

                // set this iteration's parameters
                double inverseProgressFactor = double.Max(1 - (double)curHousing / targetRoomCount, 0);
                int iterationFloorWidth = (int)Math.Round((p.FloorWidth.Max - p.FloorWidth.Min) * inverseProgressFactor) + p.FloorWidth.Min;
                int iterationWallWidth = (int)Math.Round((p.WallWidth.Max - p.WallWidth.Min) * inverseProgressFactor) + p.WallWidth.Min;

                (bool valid, bool splitAlongX) = EvaluateCutOnX(p, roomVolume, iterationFloorWidth, iterationWallWidth, xCutCount, yCutCount);

                if (!valid) {
                    // if the room can't be split at all, don't add it back to the queue
                    finishedRoomVolumes.Add(roomVolume);
                    extraCuts++;
                    continue;
                }

                int iterationSplitWidth = splitAlongX ? iterationFloorWidth : iterationWallWidth;
                int outerBoundaryWidth = splitAlongX ? p.RoomHeight.Min : p.RoomWidth.Min;
                Range validCutRange = new((splitAlongX ? roomVolume.BoundingBox.topLeft.Y : roomVolume.BoundingBox.topLeft.X) + outerBoundaryWidth,
                    (splitAlongX ? roomVolume.BoundingBox.bottomRight.Y : roomVolume.BoundingBox.bottomRight.X) - outerBoundaryWidth - iterationSplitWidth
                );

                var validSplitStarts = GetValidSplits(roomVolume, prioritySplits, splitAlongX, iterationSplitWidth, iterationVerticalGapXs, iterationHorizontalGapYs, validCutRange);

                if (validSplitStarts.Count == 0) {
                    finishedRoomVolumes.Add(roomVolume);
                    extraCuts++;
                    continue;
                }

                int splitStart = Terraria.WorldGen.genRand.NextFromCollection(validSplitStarts.ToList());
                int splitEnd = splitStart + iterationSplitWidth - 1;

                (Shape? lower, Shape? middle, Shape? higher) roomSubsections = roomVolume.CutTwice(splitAlongX, splitStart, splitEnd);

                if (splitAlongX)
                    xCutCount++;
                else
                    yCutCount++;

                prioritySplits.AddToBlocklist(new PartialPoint16(splitStart, splitStart, !splitAlongX, splitAlongX));

                if (roomSubsections.lower is not null) {
                    if (p.IsWithinMaxSize(roomSubsections.lower) && Terraria.WorldGen.genRand.NextDouble() < (1 - Math.Pow(1 - p.LargeRoomChance, p.Attempts)) * 0.35 &&
                        largeRoomCount < maxLargeRooms && inverseProgressFactor < 0.92) {
                        largeRoomCount++;
                        finishedRoomVolumes.Add(roomSubsections.lower);
                        extraCuts++;
                    }
                    else {
                        roomQueue.Enqueue(roomSubsections.lower);
                    }
                }

                if (roomSubsections.higher is not null) {
                    if (p.IsWithinMaxSize(roomSubsections.higher) && Terraria.WorldGen.genRand.NextDouble() < (1 - Math.Pow(1 - p.LargeRoomChance, p.Attempts)) * 0.35 &&
                        largeRoomCount < maxLargeRooms && inverseProgressFactor < 0.92) {
                        largeRoomCount++;
                        finishedRoomVolumes.Add(roomSubsections.higher);
                        extraCuts++;
                    }
                    else {
                        roomQueue.Enqueue(roomSubsections.higher);
                    }
                }

                if (roomSubsections.middle is not null) {
                    if (splitAlongX)
                        floorVolumes.Add(roomSubsections.middle);
                    else
                        wallVolumes.Add(roomSubsections.middle);
                }
            }

            finishedRoomVolumes.AddRange(roomQueue);
            prioritySplits.ClearBlocklist();
            return new RoomLayout(floorVolumes, wallVolumes, finishedRoomVolumes);
        }

        /// <summary>
        ///     procedural BSP algorithm to split rooms
        /// </summary>
        /// <param name="roomLayout"></param>
        /// <param name="room">room to split, must be in the given roomLayout</param>
        /// <param name="p"></param>
        /// <param name="prioritizeSplitsOnGapFloors"></param>
        /// <returns>RoomLayout is in component mode</returns>
        public static RoomLayout Action(RoomLayout roomLayout, Room room, RoomLayoutParams p, bool prioritizeSplitsOnGapFloors = true) {
            if (roomLayout.ComponentMode) throw new Exception("given RoomLayout already has components set");
            if (room.Geometry.GetArea(true) < 92) return new RoomLayout([], [], [], [room]);
            if (!roomLayout.Rooms.Remove(room)) throw new Exception("room doesn't exist in the given RoomLayout");

            RoomLayout? pickedLayout = null;

            var possibleLayouts = new RoomLayout[p.Attempts];
            PriorityCollection<PartialPoint16> prioritySplits = new((thisObj, otherObj) => (thisObj.X == otherObj.X || !thisObj.HasX) && (thisObj.Y == otherObj.Y || thisObj.HasY));
            foreach (PartialPoint16 corner in room.Geometry.GetCorners())
                prioritySplits.AddItem(corner, 1);

            int targetRoomCount = p.TagsRequired.GetValue(Tags.HasRooms);

            for (int attempt = 0; attempt < p.Attempts; attempt++) {
                RoomLayout volumes = SplitBsp(p, room, prioritySplits, prioritizeSplitsOnGapFloors, targetRoomCount);
                if (volumes.RoomVolumes.Count == targetRoomCount) {
                    pickedLayout = volumes;
                    break;
                }

                possibleLayouts[attempt] = volumes;
            }

            // find the layout with the closest housing to the requested amount
            if (pickedLayout is null) {
                int closetHousingCount = Math.Abs(possibleLayouts[0].RoomVolumes.Count - targetRoomCount);
                pickedLayout = possibleLayouts[0]; // default to the first
                for (int i = 1; i < possibleLayouts.Length; i++)
                    if (Math.Abs(possibleLayouts[i].RoomVolumes.Count - targetRoomCount) < closetHousingCount) {
                        closetHousingCount = Math.Abs(possibleLayouts[i].RoomVolumes.Count - targetRoomCount);
                        pickedLayout = possibleLayouts[i];
                    }
            }

            // find the rooms that are connected with the original room's gaps
            pickedLayout.ConvertToComponents();
            foreach (Gap gap in room.Gaps) {
                bool isLowerRoom = gap.LowerRoom == room;
                Room roomToConnect = RoomLayoutHelper.GetClosestRoom(pickedLayout.Rooms, gap.Volume.Center);
                if (isLowerRoom) {
                    gap.LowerRoom = roomToConnect;
                }
                else {
                    // extra connection checking
                    if (gap.HigherRoom != room) throw new Exception("gap should've had this room before subdivision, but it didn't");

                    gap.HigherRoom = roomToConnect;
                }
            }

            roomLayout.Combine(pickedLayout);
            return pickedLayout;
        }
    }
    
    /// <summary>
    ///     creates all necessary stairways in a room
    /// </summary>
    public abstract class CreateStairways : IStructureLayoutHelper {
        public static HashSet<Tag> PossibleTags => [
        ];

        /// <summary>
        ///     creates a stairway to a point
        /// </summary>
        /// <param name="param"></param>
        /// <param name="gapBottom">the bottom-est point in the gap volume</param>
        /// <returns>shape that encloses the affected tiles</returns>
        private static Stairway CreateStairwayToGap(StructureLayoutParams param, Point16 gapBottom) {
        }

        /// <summary>
        ///     creates all necessary stairways in a room
        /// </summary>
        /// <param name="param"></param>
        /// <param name="room"></param>
        public static List<Stairway> Action(StructureLayoutParams param, Room room) {
            List<Stairway> stairways = [];
            foreach (Gap gap in room.Gaps)
                if (gap.IsHorizontal) {
                    Point16 gapBottom = gap.Geometry.BoundingBox.bottomRight;
                    if (room.Geometry.Contains(gapBottom + new Point16(1, 1)) || room.Geometry.Contains(gapBottom + new Point16(-1, 1))) {
                        CreateStairwayToGap(param, gapBottom);
                        somehow make sure that the stair ways dont collide and take up too much space and stuff
                    }
                }

            room.Stairways = stairways;
            return stairways;
        }
    }
}