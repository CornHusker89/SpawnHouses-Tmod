#nullable enable
using System;
using System.Collections.Generic;
using System.Linq;
using SpawnHouses.AdvStructures.AdvStructureParts;
using SpawnHouses.Structures;
using SpawnHouses.Types;
using Terraria;
using Terraria.DataStructures;
using Terraria.ModLoader;
using Range = SpawnHouses.Structures.Range;

namespace SpawnHouses.Helpers;

public static class RoomLayoutHelper {
    /// <summary>
    ///     based on the size, so if room is not uniform square results might not be accurate
    /// </summary>
    /// <param name="volume"></param>
    /// <returns></returns>
    public static bool IsValidHousingSize(Shape volume) {
        return (volume.Size.X + 2) * (volume.Size.Y + 2) >= 60;
    }

    /// <summary>
    ///     find all corners of a shape based on their x and y positions, useful for ensuring beams and such make sense visually
    /// </summary>
    /// <param name="shape"></param>
    /// <returns></returns>
    public static (List<int> xCoords, List<int> yCoords) GetCorners(Shape shape) {
        List<int> cornersX = [];
        List<int> cornersY = [];

        foreach (Point16 point in shape.Points) {
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
    public static RoomLayoutVolumes SplitBsp(RoomLayoutParams roomLayoutParams, List<int> priorityXSplits, List<int> priorityYSplits) {
        if (roomLayoutParams.RoomHeight.Max < roomLayoutParams.FloorWidth.Max + 2 * roomLayoutParams.RoomHeight.Min)
            ModContent.GetInstance<SpawnHouses>().Logger.Warn(
                $"a max room height of {roomLayoutParams.RoomHeight.Max} was given, but at least {roomLayoutParams.FloorWidth.Max + 2 * roomLayoutParams.RoomHeight.Min} is required");
        if (roomLayoutParams.RoomWidth.Max < roomLayoutParams.WallWidth.Max + 2 * roomLayoutParams.RoomWidth.Min)
            ModContent.GetInstance<SpawnHouses>().Logger.Warn(
                $"a max room height of {roomLayoutParams.RoomWidth.Max} was given, but at least {roomLayoutParams.WallWidth.Max + 2 * roomLayoutParams.RoomWidth.Min} is required");

        List<Shape> floorVolumes = [];
        List<Shape> wallVolumes = [];
        var roomQueue = new Queue<Shape>([roomLayoutParams.MainVolume]);
        List<Shape> finishedRoomVolumes = [];
        int extraCuts = 0;
        int largeRoomCount = 0;
        int xCutCount = 0;
        int yCutCount = 0;
        int maxLargeRooms = (int)Math.Ceiling(roomLayoutParams.LargeRoomChance * roomLayoutParams.Housing);
        for (int curHousing = 0; curHousing < roomLayoutParams.Housing + extraCuts - 1; curHousing++) {
            Shape roomVolume;
            if (roomQueue.Count > 0)
                roomVolume = roomQueue.Dequeue();
            else
                break;

            double inverseProgressFactor = double.Max(1 - (double)curHousing / roomLayoutParams.Housing, 0);
            int iterationFloorWidth = (int)Math.Round((roomLayoutParams.FloorWidth.Max - roomLayoutParams.FloorWidth.Min) * inverseProgressFactor) + roomLayoutParams.FloorWidth.Min;
            int iterationWallWidth = (int)Math.Round((roomLayoutParams.WallWidth.Max - roomLayoutParams.WallWidth.Min) * inverseProgressFactor) + roomLayoutParams.WallWidth.Min;

            bool canSplitAlongX = roomVolume.Size.Y >= iterationFloorWidth + 2 * roomLayoutParams.RoomHeight.Min;
            // ensure ratio of room dimensions isn't out of whack
            if (canSplitAlongX && roomVolume.Area <= 175 && roomVolume.Size.X > roomVolume.Size.Y * 1.8)
                canSplitAlongX = false;
            bool canSplitAlongY = roomVolume.Size.X >= iterationWallWidth + 2 * roomLayoutParams.RoomWidth.Min;
            if (canSplitAlongY && roomVolume.Area <= 175 && roomVolume.Size.Y > roomVolume.Size.X * 1.8)
                canSplitAlongY = false;
            bool splitAlongX;

            if (canSplitAlongX && canSplitAlongY) {
                if (roomVolume.Size.Y > roomLayoutParams.RoomHeight.Max) {
                    splitAlongX = true;
                }
                else {
                    double xWeight = Math.Pow(1.0 / (xCutCount + 2), 3);
                    double yWeight = Math.Pow(1.0 / (yCutCount + 1), 3);
                    double totalWeight = xWeight + yWeight;
                    // xWeight divided by totalWeight represents chance of picking x cut
                    splitAlongX = Terraria.WorldGen.genRand.NextDouble() < xWeight / totalWeight;
                }
            }
            else if (!canSplitAlongX && canSplitAlongY) {
                splitAlongX = false;
            }
            else if (canSplitAlongX) {
                splitAlongX = true;
            }
            else {
                // if the room can't be split at all, don't add it back to the queue
                finishedRoomVolumes.Add(roomVolume);
                extraCuts++;
                continue;
            }

            if (splitAlongX)
                xCutCount++;
            else
                yCutCount++;

            int outerBoundaryWidth = splitAlongX ? roomLayoutParams.RoomHeight.Min : roomLayoutParams.RoomWidth.Min;
            Range validCutRange = new Range((splitAlongX ? roomVolume.BoundingBox.topLeft.Y : roomVolume.BoundingBox.topLeft.X) + outerBoundaryWidth,
                (splitAlongX ? roomVolume.BoundingBox.bottomRight.Y : roomVolume.BoundingBox.bottomRight.X) - outerBoundaryWidth - (splitAlongX ? iterationFloorWidth : iterationWallWidth)
            );

            List<int> predeterminedSplits = [];
            foreach (int predeterminedSplit in splitAlongX ? priorityXSplits : priorityYSplits)
                if (validCutRange.InRange(predeterminedSplit)) {
                    predeterminedSplits.Add(predeterminedSplit);
                }

            int splitStart = predeterminedSplits.Count == 0
                ? Terraria.WorldGen.genRand.Next(validCutRange.Min, validCutRange.Max + 1)
                : predeterminedSplits[Terraria.WorldGen.genRand.Next(predeterminedSplits.Count)];
            int splitEnd = splitAlongX
                ? splitStart + iterationFloorWidth - 1
                : splitStart + iterationWallWidth - 1;

            (Shape? lower, Shape? middle, Shape? higher) roomSubsections = roomVolume.CutTwice(splitAlongX, splitStart, splitEnd);
            if (roomSubsections.lower is not null) {
                if (roomLayoutParams.IsWithinMaxSize(roomSubsections.lower) && Terraria.WorldGen.genRand.NextDouble() < (1 - Math.Pow(1 - roomLayoutParams.LargeRoomChance, roomLayoutParams.Attempts)) * 0.35 &&
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
                if (roomLayoutParams.IsWithinMaxSize(roomSubsections.higher) && Terraria.WorldGen.genRand.NextDouble() < (1 - Math.Pow(1 - roomLayoutParams.LargeRoomChance, roomLayoutParams.Attempts)) * 0.35 &&
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
                if (splitAlongX) {
                    floorVolumes.Add(roomSubsections.middle);
                }
                else {
                    wallVolumes.Add(roomSubsections.middle);
                }
            }
        }

        finishedRoomVolumes.AddRange(roomQueue);
        return new RoomLayoutVolumes(floorVolumes, wallVolumes, finishedRoomVolumes);
    }

    /// <summary>
    ///     creates actual floor and wall objects from volumes
    /// </summary>
    public static (List<Floor> floors, List<Wall> walls) CreateFloorsAndWalls(RoomLayoutVolumes roomLayoutVolumes) {
        List<Floor> floors = [];
        List<Wall> walls = [];
        foreach (Shape shape in roomLayoutVolumes.FloorVolumes) {
            bool exterior = true;
            Point16 lowerPoint = new Point16(shape.BoundingBox.topLeft.X + shape.Size.X / 2, shape.BoundingBox.topLeft.Y - 1);
            Point16 higherPoint = new Point16(shape.BoundingBox.topLeft.X + shape.Size.X / 2, shape.BoundingBox.bottomRight.Y + 1);
            if (roomLayoutVolumes.InStructure(lowerPoint) && roomLayoutVolumes.InStructure(higherPoint)) {
                exterior = false;
            }

            floors.Add(new Floor(shape, exterior));
        }

        foreach (Shape shape in roomLayoutVolumes.WallVolumes) {
            bool exterior = true;
            Point16 lowerPoint = new Point16(shape.BoundingBox.topLeft.X - 1, shape.BoundingBox.topLeft.Y + shape.Size.Y / 2);
            Point16 higherPoint = new Point16(shape.BoundingBox.bottomRight.X + 1, shape.BoundingBox.topLeft.Y + shape.Size.Y / 2);
            if (roomLayoutVolumes.InStructure(lowerPoint) && roomLayoutVolumes.InStructure(higherPoint)) {
                exterior = false;
            }

            walls.Add(new Wall(shape, exterior));
        }

        return (floors, walls);
    }

    /// <returns>null if no room found</returns>
    public static Room? GetRoomFromPos(List<Room> rooms, Point16 point) {
        foreach (Room room in rooms)
            if (room.Volume.Contains(point))
                return room;

        return null;
    }

    /// <summary>
    /// Uses perimeter raycasting to find all possible gaps of any size
    /// </summary>
    /// <param name="roomLayoutVolumes"></param>
    /// <returns></returns>
    public static (List<Gap> gaps, List<Room> rooms) RaycastGaps(RoomLayoutVolumes roomLayoutVolumes) {
        var rooms = roomLayoutVolumes.RoomVolumes.Select(roomVolume => new Room(roomVolume, [])).ToList();
        List<Gap> gaps = [];

        foreach (Room room in rooms) {
            List<Gap> roomGaps = [];
            Room lastRoom = null!;
            List<Shape> curGapVolumes = [];
            bool isHorizontal = false;
            byte lastDirection = Directions.None;

            room.Volume.ExecuteOnPerimeter((x, y, direction) => {
                Point16 pos = new(x, y);
                Point16 step = new(0, 0);
                if (lastDirection == Directions.None) {
                    lastDirection = direction;
                }

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
                while (direction is Directions.Up or Directions.Down ? roomLayoutVolumes.InFloor(pos) : roomLayoutVolumes.InWall(pos)) {
                    pos += step;
                }

                Room? foundRoom = GetRoomFromPos(rooms, pos);
                if (foundRoom == room) {
                    foundRoom = null; // invalidate casts that find its own room
                }

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

            room.Gaps = roomGaps;
            gaps.AddRange(roomGaps);
        }

        return (gaps, rooms);
    }

    /// <summary>
    /// Removes duplicate gaps in-place
    /// </summary>
    /// <param name="gaps"></param>
    public static void RemoveDuplicateGaps(List<Gap> gaps) {
        for (int testingIndex = gaps.Count - 1; testingIndex >= 0; testingIndex--) {
            for (int removingIndex = testingIndex - 1; removingIndex >= 0; removingIndex--) {
                Gap gap = gaps[testingIndex];
                Gap removingGap = gaps[removingIndex];

                if (gap.RepresentsSimilarGap(removingGap)) {
                    if (removingGap.LowerRoom.Gaps.Remove(removingGap)) {
                        removingGap.LowerRoom.Gaps.Add(gap);
                    }

                    if (removingGap.HigherRoom!.Gaps.Remove(removingGap)) {
                        removingGap.HigherRoom!.Gaps.Add(gap);
                    }

                    gaps.RemoveAt(removingIndex);
                }
            }
        }
    }

    /// <summary>
    /// Removes impractically small gaps, moves and resizes large gaps, puts into 2 lists
    /// </summary>
    /// <param name="gaps"></param>
    /// <returns></returns>
    public static (List<Gap> floorGaps, List<Gap> wallGaps) ResizeAndSortGaps(List<Gap> gaps) {
        // prune gap sizes
        List<Gap> floorGaps = [];
        List<Gap> wallGaps = [];
        int maxFloorGapSize = gaps.Where(gap => !gap.IsHorizontal).Select(gap => gap.Volume.Size.X).Max();

        // filter out gaps which are too small, before attempting to chain
        for (int i = gaps.Count - 1; i >= 0; i--) {
            if (gaps[i].IsHorizontal) {
                if (gaps[i].Volume.Size.Y < 3) {
                    gaps.RemoveAt(i);
                }
            }
            else {
                if (gaps[i].Volume.Size.X < 2) {
                    gaps.RemoveAt(i);
                }
            }
        }

        foreach (Gap gap in gaps) {
            if (gap.IsHorizontal) {
                var points = gap.Volume.Points;
                for (int i = 0; i < gap.Volume.Points.Length; i++) {
                    // ensure doors aren't too tall
                    if (points[i].Y < gap.Volume.BoundingBox.bottomRight.Y - 2) {
                        points[i] = new Point16(points[i].X, gap.Volume.BoundingBox.bottomRight.Y - 2);
                    }
                }

                gap.Volume = new Shape(points);
                wallGaps.Add(gap);
            }
            else {
                // randomly move the gap, but if possible align it with the gap below
                Gap? potentialChainGap = gaps.Find(potentialGap =>
                    !potentialGap.IsHorizontal &&
                    potentialGap.HigherRoom == gap.LowerRoom &&
                    gap.Volume.BoundingBox.topLeft.X <= potentialGap.Volume.BoundingBox.topLeft.X &&
                    gap.Volume.BoundingBox.bottomRight.X >= potentialGap.Volume.BoundingBox.bottomRight.X
                );
                if (potentialChainGap != null && Terraria.WorldGen.genRand.NextDouble() < 0.6) {
                    // 60% chance to go for chain gaps
                    gap.IsChain = true;
                    gap.VerticalChainLower = potentialChainGap;
                    potentialChainGap.VerticalChainHigher = gap;
                    var points = gap.Volume.Points;
                    for (int i = 0; i < gap.Volume.Points.Length; i++) {
                        if (points[i].X < potentialChainGap.Volume.BoundingBox.topLeft.X) {
                            points[i] = new Point16(potentialChainGap.Volume.BoundingBox.topLeft.X, points[i].Y);
                        }

                        if (points[i].X > potentialChainGap.Volume.BoundingBox.bottomRight.X) {
                            points[i] = new Point16(potentialChainGap.Volume.BoundingBox.bottomRight.X, points[i].Y);
                        }
                    }

                    gap.Volume = new Shape(points);
                }
                else {
                    int suggestedSize = (int)(gap.Volume.Size.X / (float)maxFloorGapSize * 2 + 2);
                    // randomly move the gap, if there's space to do so
                    if (suggestedSize < gap.Volume.Size.X) {
                        int gapCenter = (int)(Terraria.WorldGen.genRand.NextDouble() * gap.Volume.Size.X) + gap.Volume.BoundingBox.topLeft.X;
                        int leftX = gapCenter - (int)Math.Floor((double)suggestedSize / 2);
                        int outOfBoundsDistance = Math.Max(0, gap.Volume.BoundingBox.topLeft.X - leftX);
                        leftX = Math.Max(leftX, gap.Volume.BoundingBox.topLeft.X);
                        int rightX = gapCenter + (int)Math.Ceiling((double)suggestedSize / 2) + outOfBoundsDistance;
                        var points = gap.Volume.Points;
                        for (int i = 0; i < gap.Volume.Points.Length; i++) {
                            if (points[i].X < leftX) {
                                points[i] = new Point16(leftX, points[i].Y);
                            }

                            if (points[i].X > rightX) {
                                points[i] = new Point16(rightX, points[i].Y);
                            }
                        }

                        gap.Volume = new Shape(points);
                    }
                }
                floorGaps.Add(gap);
            }
        }

        return (floorGaps, wallGaps);
    }

    /// <summary>
    ///     adds gaps between rooms, then optimizes and processes the gaps. essentially completes the whole gap making process from start to end
    /// </summary>
    /// <param name="roomLayoutVolumes"></param>
    /// <param name="roomLayoutParams"></param>
    /// <returns></returns>
    public static (List<Gap> floorGaps, List<Gap> wallGaps, List<Room> rooms) CreateGapsAndRooms(RoomLayoutVolumes roomLayoutVolumes, RoomLayoutParams roomLayoutParams) {
        var (allGaps, rooms) = RaycastGaps(roomLayoutVolumes);
        RemoveDuplicateGaps(allGaps);
        var (floorGaps, wallGaps) = ResizeAndSortGaps(allGaps);

        allGaps = [];
        allGaps.AddRange(floorGaps);
        allGaps.AddRange(wallGaps);
        // randomize so future iteration won't favor horizontal/vertical
        allGaps = allGaps.OrderBy(_ => Terraria.WorldGen.genRand.NextDouble()).ToList();

        // dither excessive gaps
        // double tallestVerticalGapReach = floorGaps.Max(gap => gap.LowerRoom.Volume.Size.Y);
        // for (int i = allGaps.Count - 1; i >= 0; i--) {
        //     Console.WriteLine(allGaps[i].LowerRoom.Gaps.Count < 2 || allGaps[i].HigherRoom!.Gaps.Count < 2);
        //     if (allGaps[i].LowerRoom.Gaps.Count < 2 || allGaps[i].HigherRoom!.Gaps.Count < 2) continue;
        //
        //     if (allGaps[i].IsHorizontal) {
        //         if (Terraria.WorldGen.genRand.NextDouble() < 0.4) {
        //             allGaps[i].LowerRoom.Gaps.Remove(allGaps[i]);
        //             allGaps[i].HigherRoom!.Gaps.Remove(allGaps[i]); // because these are all internal rooms, gap.HigherRoom will never be null
        //             wallGaps.Remove(allGaps[i]);
        //             allGaps.RemoveAt(i);
        //         }
        //     }
        //     else {
        //         if (Terraria.WorldGen.genRand.NextDouble() < allGaps[i].LowerRoom.Volume.Size.Y / tallestVerticalGapReach) {
        //             allGaps[i].LowerRoom.Gaps.Remove(allGaps[i]);
        //             allGaps[i].HigherRoom!.Gaps.Remove(allGaps[i]);
        //             floorGaps.Remove(allGaps[i]);
        //             allGaps.RemoveAt(i);
        //         }
        //     }
        // }

        return (floorGaps, wallGaps, rooms);
    }

    public static RoomLayout CreateRoomLayout(RoomLayoutVolumes roomLayoutVolumes, RoomLayoutParams roomLayoutParams) {
        var (floors, walls) = CreateFloorsAndWalls(roomLayoutVolumes);
        var (floorGaps, wallGaps, rooms) = CreateGapsAndRooms(roomLayoutVolumes, roomLayoutParams);

        return new RoomLayout(floors, floorGaps, walls, wallGaps, rooms);
    }
}