#nullable enable

using System;
using System.Collections.Generic;
using System.Linq;
using SpawnHouses.Common;
using SpawnHouses.Common.Modules.Components;
using SpawnHouses.Common.Parameters;
using SpawnHouses.Common.Tagging;
using SpawnHouses.Common.Tiles;
using SpawnHouses.Common.Types;
using SpawnHouses.Common.Types.Geometry;
using SpawnHouses.Structures;
using Terraria;
using Terraria.DataStructures;
using Terraria.ModLoader;
using Terraria.Utilities.Terraria.Utilities;
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
                    return (true, param.Structure.LayoutRandom.NextDouble() < xWeight / totalWeight);
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
        /// <param name="param"></param>
        /// <param name="prioritySplits"></param>
        /// <param name="room"></param>
        /// <param name="prioritizeSplitsOnGapFloors"></param>
        /// <param name="targetRoomCount"></param>
        /// <returns>RoomLayout is NOT in component mode</returns>
        /// <remarks>fully clears blocklist before returning</remarks>
        private static RoomLayout SplitBsp(RoomLayoutParams param, Room room, PriorityCollection<PartialPoint16> prioritySplits, bool prioritizeSplitsOnGapFloors, int targetRoomCount) {
            if (param.RoomHeight.Max < param.FloorWidth.Max + 2 * param.RoomHeight.Min)
                ModContent.GetInstance<SpawnHouses>().Logger.Warn(
                    $"a max room height of {param.RoomHeight.Max} was given, but at least {param.FloorWidth.Max + 2 * param.RoomHeight.Min} is required");
            if (param.RoomWidth.Max < param.WallWidth.Max + 2 * param.RoomWidth.Min)
                ModContent.GetInstance<SpawnHouses>().Logger.Warn(
                    $"a max room height of {param.RoomWidth.Max} was given, but at least {param.WallWidth.Max + 2 * param.RoomWidth.Min} is required");

            List<Shape> floorVolumes = [], wallVolumes = [];
            var roomQueue = new Queue<Shape>([room.Geometry]);
            List<Shape> finishedRoomVolumes = [];
            int extraCuts = 0, largeRoomCount = 0, xCutCount = 0, yCutCount = 0;
            int maxLargeRooms = (int)Math.Ceiling(param.LargeRoomChance * targetRoomCount);
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
                int iterationFloorWidth = (int)Math.Round((param.FloorWidth.Max - param.FloorWidth.Min) * inverseProgressFactor) + param.FloorWidth.Min;
                int iterationWallWidth = (int)Math.Round((param.WallWidth.Max - param.WallWidth.Min) * inverseProgressFactor) + param.WallWidth.Min;

                (bool valid, bool splitAlongX) = EvaluateCutOnX(param, roomVolume, iterationFloorWidth, iterationWallWidth, xCutCount, yCutCount);

                if (!valid) {
                    // if the room can't be split at all, don't add it back to the queue
                    finishedRoomVolumes.Add(roomVolume);
                    extraCuts++;
                    continue;
                }

                int iterationSplitWidth = splitAlongX ? iterationFloorWidth : iterationWallWidth;
                int outerBoundaryWidth = splitAlongX ? param.RoomHeight.Min : param.RoomWidth.Min;
                Range validCutRange = new((splitAlongX ? roomVolume.BoundingBox.topLeft.Y : roomVolume.BoundingBox.topLeft.X) + outerBoundaryWidth,
                    (splitAlongX ? roomVolume.BoundingBox.bottomRight.Y : roomVolume.BoundingBox.bottomRight.X) - outerBoundaryWidth - iterationSplitWidth
                );

                var validSplitStarts = GetValidSplits(roomVolume, prioritySplits, splitAlongX, iterationSplitWidth, iterationVerticalGapXs, iterationHorizontalGapYs, validCutRange);

                if (validSplitStarts.Count == 0) {
                    finishedRoomVolumes.Add(roomVolume);
                    extraCuts++;
                    continue;
                }

                int splitStart = param.Structure.LayoutRandom.NextFromCollection(validSplitStarts.ToList());
                int splitEnd = splitStart + iterationSplitWidth - 1;

                (Shape? lower, Shape? middle, Shape? higher) roomSubsections = roomVolume.CutTwice(splitAlongX, splitStart, splitEnd);

                if (splitAlongX)
                    xCutCount++;
                else
                    yCutCount++;

                prioritySplits.AddToBlocklist(new PartialPoint16(splitStart, splitStart, !splitAlongX, splitAlongX));

                if (roomSubsections.lower is not null) {
                    if (param.IsWithinMaxSize(roomSubsections.lower) && param.Structure.LayoutRandom.NextDouble() < (1 - Math.Pow(1 - param.LargeRoomChance, param.Attempts)) * 0.35 &&
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
                    if (param.IsWithinMaxSize(roomSubsections.higher) && param.Structure.LayoutRandom.NextDouble() < (1 - Math.Pow(1 - param.LargeRoomChance, param.Attempts)) * 0.35 &&
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
            return new RoomLayout(room.Params.Structure, floorVolumes, wallVolumes, finishedRoomVolumes, false);
        }

        /// <summary>
        ///     procedural BSP algorithm to split rooms
        /// </summary>
        /// <param name="room">room to split, must be in the given roomLayout</param>
        /// <param name="p"></param>
        /// <param name="prioritizeSplitsOnGapFloors"></param>
        /// <returns>RoomLayout is in component mode</returns>
        public static RoomLayout Action(Room room, RoomLayoutParams p, bool prioritizeSplitsOnGapFloors = true) {
            if (room.Geometry.GetArea(true) < 92) return new RoomLayout(room.Params.Structure, [], [], [], [room]);

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

            // connect the original external gaps to proper rooms
            pickedLayout.ConvertToComponents();
            foreach (Gap gap in room.Gaps) {
                if (gap.InteriorRoom == room)
                    gap.InteriorRoom = RoomHelper.GetClosestRoom(pickedLayout.Rooms, gap.Geometry.Center);
            }
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
        private static Stairway CreateStairwayToGap(StructureLayoutParams param, Point16 gapBottom) => throw new NotImplementedException();

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
                        //somehow make sure that the stair ways dont collide and take up too much space and stuff
                        throw new NotImplementedException();
                    }
                }

            room.Stairways = stairways;
            return stairways;
        }
    }

    /// <summary>
    ///     creates the interior room for a structure. assumes that the structure has a single volume
    /// </summary>
    public abstract class InitializeStructureInterior : IStructureLayoutHelper {
        public static HashSet<Tag> PossibleTags => [
        ];

        /// <summary>
        /// </summary>
        /// <param name="structure"></param>
        /// <param name="entryPoints"></param>
        /// <param name="floorWidth"></param>
        /// <param name="wallWidth"></param>
        /// <remarks>returned gaps have BOTH rooms set to null</remarks>
        /// <returns></returns>
        private static Gap[] GapsFromEntryPoints(AdvStructure structure, EntryPoint[] entryPoints, int floorWidth, int wallWidth) {
            var gaps = new Gap[entryPoints.Length];
            for (int i = 0; i < gaps.Length; i++) {
                EntryPoint entryPoint = entryPoints[i];
                if (entryPoint.IsHorizontal)
                    gaps[i] = new Gap(
                        structure,
                        new Shape(
                            true,
                            entryPoint.Start,
                            entryPoint.End + new Point16(entryPoint.Direction is Directions.Right ? wallWidth - 1 : -wallWidth + 1, 0)
                        ),
                        null!, null, entryPoint.Direction is Directions.Left or Directions.Right
                    );
                else
                    gaps[i] = new Gap(
                        structure,
                        new Shape(
                            true,
                            entryPoint.Start,
                            entryPoint.End + new Point16(0, entryPoint.Direction is Directions.Down ? floorWidth - 1 : -floorWidth + 1)
                        ),
                        null!, null, entryPoint.Direction is Directions.Left or Directions.Right
                    );
            }

            return gaps;
        }

        /// <summary>
        ///     converts 2x2 grid cell into a marching square index
        /// </summary>
        /// <param name="tilemap"></param>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <returns></returns>
        private static int GetMarchingSquareIndex(StructureTilemap tilemap, int x, int y) {
            int value = 0;
            // bottom-left
            if (tilemap.InInterior(x, y))
                value |= 1;

            // bottom-right
            if (tilemap.InInterior(x + 1, y))
                value |= 2;

            // top-right
            if (tilemap.InInterior(x + 1, y - 1))
                value |= 4;

            // top-left
            if (tilemap.InInterior(x, y - 1))
                value |= 8;

            return value;
        }

        /// <summary>
        ///     gets a shape that represents the interior of the structure, and excludes any exterior components
        /// </summary>
        /// <param name="tilemap"></param>
        /// <returns></returns>
        /// <exception cref="Exception"></exception>
        /// <remarks>assumes only one interior in the tilemap</remarks>
        private static Shape GetStructureInterior(StructureTilemap tilemap) {
            List<Point16> outline = [];

            Point16? start = null;
            for (int y = 0; y < tilemap.Height - 1 && start == null; y++)
            for (int x = 0; x < tilemap.Width - 1; x++)
                if (GetMarchingSquareIndex(tilemap, x, y) != 0) {
                    start = new Point16(x, y);
                    break;
                }

            if (start == null)
                throw new Exception("tilemap did not have any interior tiles");

            Point16 pos = start.Value;
            Point16 dir = new(0, 1);
            HashSet<Point16> visited = [];
            int steps = 0;
            int maxSteps = tilemap.Width * tilemap.Height * 4;

            do {
                // add previous iteration's position
                visited.Add(pos);

                int value = GetMarchingSquareIndex(tilemap, pos.X, pos.Y);

                // assumes clockwise direction
                Point16 nextDir = value switch {
                    1 => new Point16(0, 1), // BL only: down
                    2 => new Point16(1, 0), // BR only: right
                    3 => new Point16(1, 0), // BL + BR: right
                    4 => new Point16(0, -1), // TR only: up
                    5 => dir.X == -1 ? new Point16(0, -1) : new Point16(0, 1), // BL + TR: up if we were going left, otherwise down
                    6 => new Point16(0, -1), // BR + TR: up
                    7 => new Point16(0, -1), // BL + BR + TR: up
                    8 => new Point16(-1, 0), // TL only: left
                    9 => new Point16(0, 1), // BL + TL: down
                    10 => dir.Y == -1 ? new Point16(0, 1) : new Point16(0, -1), // BR + TL: down if we were going left, otherwise up
                    11 => new Point16(1, 0), // BL + BR + TL: right
                    12 => new Point16(-1, 0), // TR + TL: left
                    13 => new Point16(0, 1), // BL + TR + TL: down
                    14 => new Point16(-1, 0), // BR + TR + TL: left
                    _ => new Point16(0, 0) // 0 or 15
                };

                Point16 outlineOffset = value switch {
                    1 => new Point16(0, 0), // BL only: BL
                    2 => new Point16(1, 0), // BR only: BR
                    3 => new Point16(0, 0), // BL + BR: BL
                    4 => new Point16(1, -1), // TR only: TR
                    5 => dir.X == -1 ? new Point16(1, -1) : new Point16(0, 0), // BL + TR: TR if we were going left, otherwise BL
                    6 => new Point16(1, 0), // BR + TR: BR
                    7 => new Point16(1, 0), // BL + BR + TR: BR
                    8 => new Point16(0, -1), // TL only: TL
                    9 => new Point16(0, -1), // BL + TL: TL
                    10 => dir.Y == -1 ? new Point16(1, 0) : new Point16(0, -1), // BR + TL: BR if we were going left, otherwise TL
                    11 => new Point16(0, 0), // BL + BR + TL: BL
                    12 => new Point16(1, -1), // TR + TL: TR
                    13 => new Point16(0, -1), // BL + TR + TL: TL
                    14 => new Point16(1, -1), // BR + TR + TL: TR
                    _ => new Point16(0, 0) // 0 or 15
                };

                if (nextDir != dir) outline.Add(pos + outlineOffset);

                pos += nextDir;
                dir = nextDir;
                steps++;
            } while (pos != start.Value && !visited.Contains(pos) && steps < maxSteps);

            return new Shape(outline);
        }

        /// <summary>
        ///     sets the outside/exterior component/inside tile data within the tilemap, based on the current external layout
        /// </summary>
        private static void SetTilesExternalStatus(StructureLayoutParams param, List<Floor> floors, List<Wall> walls, List<Gap> gaps) {
            StructureTilemap tilemap = param.Structure.Tilemap;
            foreach (Shape shape in floors.Select(floor => floor.Geometry))
                shape.ExecuteInArea((x, y) => {
                    StructureTile tile = tilemap[x, y];
                    tile.IsExteriorComponent = true;
                    tile.IsFloor = true;
                });

            foreach (Shape shape in walls.Select(wall => wall.Geometry))
                shape.ExecuteInArea((x, y) => {
                    StructureTile tile = tilemap[x, y];
                    tile.IsExteriorComponent = true;
                    tile.IsWall = true;
                });

            foreach (Shape shape in gaps.Select(gap => gap.Geometry))
                shape.ExecuteInArea((x, y) => {
                    StructureTile tile = tilemap[x, y];
                    tile.IsExteriorComponent = true;
                    tile.IsGap = true;
                });

            SearchOutside(0, 0);
            for (int x = 0; x < tilemap.Width; x++)
            for (int y = 0; y < tilemap.Height; y++) {
                StructureTile tile = tilemap[x, y];
                if (tile is { IsOutside: false, IsExteriorComponent: false }) tile.IsInside = true;
            }

            return;

            void SearchOutside(int x, int y) {
                StructureTile thisTile = tilemap[x, y];
                thisTile.IsOutside = true;
                thisTile.IsNullTile = true;
                thisTile.IsNullWall = true;

                foreach ((int dx, int dy) in ((int, int)[]) [(1, 0), (-1, 0), (0, 1), (0, -1)]) {
                    if (!tilemap.InBounds(x + dx, y + dy)) continue;
                    StructureTile nextTile = tilemap[x + dx, y + dy];
                    if (nextTile.IsOutside || nextTile.IsExteriorComponent) continue;

                    SearchOutside(x + dx, y + dy);
                }
            }
        }

        /// <summary>
        ///     creates a room
        /// </summary>
        /// <param name="param"></param>
        /// <param name="exteriorFloors"></param>
        /// <param name="exteriorWalls"></param>
        /// <param name="externalFloorThickness"></param>
        /// <param name="externalWallThickness"></param>
        /// <returns></returns>
        public static Room Action(StructureLayoutParams param, List<Floor> exteriorFloors, List<Wall> exteriorWalls, int externalFloorThickness, int externalWallThickness) {
            var externalGaps = GapsFromEntryPoints(param.Structure, param.EntryPoints, externalFloorThickness, externalWallThickness).ToList();
            TagMap.AddRequiredToEach(externalGaps, Tags.External);
            SetTilesExternalStatus(param, exteriorFloors, exteriorWalls, externalGaps);
            Room internalRoom = new(param.Structure, GetStructureInterior(param.Structure.Tilemap), externalGaps);

            foreach (Gap gap in externalGaps)
                gap.InteriorRoom = internalRoom;
            return internalRoom;
        }
    }

    public abstract class CreateRoof : IStructureLayoutHelper {
        public static HashSet<Tag> PossibleTags => [
        ];

        /// <summary>
        ///     simple flat roof path
        /// </summary>
        /// <returns></returns>
        private static List<Point16> FlatRoof(Point16 left, Point16 right) => [left, right];

        /// <summary>
        ///     creates a roof path with a single peak, no flat sections on the sides. start and end can be at different heights
        /// </summary>
        /// <returns></returns>
        private static List<Point16> SinglePeakOnly(AdvStructure structure, Point16 left, Point16 right, float roofSlope = -1) {
            if (Math.Abs(roofSlope - -1) < 0.001f)
                roofSlope = structure.LayoutRandom.NextFromList(1f, 1.33f, 1.67f);
            bool hasHigherSide = left.Y != right.Y;
            bool leftRoofHigher = left.Y < right.Y;
            int length = right.X - left.X;
            int upperRoofBottomY = leftRoofHigher ? left.Y : right.Y;
            int lowerRoofBottomY = leftRoofHigher ? right.Y : left.Y;

            List<Point16> path;
            if (hasHigherSide) {
                double middleX = (roofSlope * (left.X + right.X) - (right.Y - left.Y)) / (2 * roofSlope);
                Point16 middlePoint = new((int)Math.Ceiling(middleX), (int)(left.Y - roofSlope * (middleX - left.X)));

                path = [
                    new Point16(left.X, leftRoofHigher ? upperRoofBottomY : lowerRoofBottomY),
                    middlePoint,
                    new Point16(right.X, !leftRoofHigher ? upperRoofBottomY : lowerRoofBottomY)
                ];
                if (length % 2 == 1)
                    path.Insert(2, new Point16(left.X + 1 + length / 2, upperRoofBottomY - (int)(roofSlope * length * 0.5)));
            }
            else {
                path = [
                    new Point16(left.X, upperRoofBottomY),
                    new Point16(left.X + length / 2, upperRoofBottomY - (int)(roofSlope * length * 0.5)),
                    new Point16(right.X, upperRoofBottomY)
                ];
                if (length % 2 == 1)
                    path.Insert(2, new Point16(left.X + 1 + length / 2, upperRoofBottomY - (int)(roofSlope * length * 0.5)));
            }

            return path;
        }

        /// <summary>
        ///     creates a roof path with a single peak with flat sections on the side. start and end can be at different heights
        /// </summary>
        /// <returns></returns>
        private static List<Point16> SinglePeakPathWithFlats(AdvStructure structure, Point16 left, Point16 right) {
            FloatRange possiblePeakLengths = left.Y == right.Y ? new FloatRange(0.6f, 0.7f) : new FloatRange(0.48f, 0.62f);
            int peakSectionLength = (int)((right.X - left.X) * structure.LayoutRandom.NextFloat(possiblePeakLengths));
            int offsetRange = right.X - left.X - peakSectionLength;
            float[] possibleOffsetProportions = left.Y == right.Y ? [0f, 0.33f, 0.66f, 1f] : [0.5f];
            int offset = (int)(offsetRange * structure.LayoutRandom.NextFromList(possibleOffsetProportions));
            float peakRoofSlope = structure.LayoutRandom.NextFromList(0.67f, 1f, 1.33f);
            int heightDelta = Math.Abs(left.Y - right.Y);

            while (peakRoofSlope * peakSectionLength - heightDelta < (left.X - right.X) / 3.0) peakRoofSlope += 0.33f;

            var path = SinglePeakOnly(structure, left + new Point16(offset, 0), new Point16(left.X + peakSectionLength - 1 + offset, right.Y), peakRoofSlope);
            path.Insert(0, new Point16(left.X, left.Y));
            path.Add(new Point16(right.X, right.Y));

            return path;
        }

        /// <summary>
        ///     creates a roof path with a peak towards either the left or right end, a flat section in the middle,
        ///     and finishing with either a slope or another flat section on the low side
        /// </summary>
        /// <returns></returns>
        public static List<Point16> WavyPeak(AdvStructure structure, Point16 left, Point16 right) {
            bool finishWithFlat = structure.LayoutRandom.NextBool();
            int length = right.X - left.X;
            int verticalSideOffset = (int)(length * structure.LayoutRandom.NextFloat(0.15f, 0.35f)); // will only be used if the sides are even
            float roofSlope = structure.LayoutRandom.NextFromList(0.67f, 1f, 1.33f);
            if (left.Y == right.Y) {
                if (structure.LayoutRandom.NextBool()) {
                    left += new Point16(0, -verticalSideOffset);
                }
                else
                    right += new Point16(0, -verticalSideOffset);
            }

            // create higher section
            List<Point16> path;
            if (left.Y < right.Y) {
                path = SinglePeakOnly(structure, left, left + new Point16((int)(length * 0.4f), 0), roofSlope);
                path.Add(new Point16((int)(right.X - (right.Y - left.Y) / roofSlope - (finishWithFlat ? length * 0.23f : 0)), left.Y));
                path.Add(finishWithFlat ? right - new Point16((int)(length * 0.23f), 0) : right);
            }
            else {
                path = [
                    finishWithFlat ? left + new Point16((int)(length * 0.23f), 0) : left,
                    new Point16((int)(left.X + (left.Y - right.Y) / roofSlope + (finishWithFlat ? length * 0.23f : 0)), right.Y)
                ];
                path.AddRange(SinglePeakOnly(structure, right - new Point16((int)(length * 0.4f), 0), right, roofSlope));
            }

            // and add lower flat if required
            if (finishWithFlat) {
                if (left.Y < right.Y)
                    path.Add(right);
                else
                    path.Insert(0, left);
            }

            // make sure roof path doesn't overlap
            int requiredOffsetDistance = (int)(length * 0.12f);
            int checkIndexOffset = finishWithFlat ? 1 : 0;
            if (left.Y < right.Y && path[^(2 + checkIndexOffset)].X < path[^(3 + checkIndexOffset)].X + requiredOffsetDistance)
                path[^(2 + checkIndexOffset)] = new Point16(path[^(3 + checkIndexOffset)].X + requiredOffsetDistance, path[^(2 + checkIndexOffset)].Y);
            else if (path[1 + checkIndexOffset].X > path[2 + checkIndexOffset].X - requiredOffsetDistance)
                path[1 + checkIndexOffset] = new Point16(path[2 + checkIndexOffset].X - requiredOffsetDistance, path[1 + checkIndexOffset].Y);

            return path;
        }

        /// <summary>
        ///     creates a roof path with 2 distinct sections, one higher and one lower.
        ///     the upper end can have various slopes, and the lower end can have a slope that compliments the higher end
        /// </summary>
        /// <returns></returns>
        private static List<Point16> SplitRoof(AdvStructure structure, Point16 left, Point16 right) {
            bool leftRoofHigher = left.Y < right.Y;
            int length = right.X - left.X;
            int upperRoofBottomY = leftRoofHigher ? left.Y : right.Y;
            int lowerRoofBottomY = leftRoofHigher ? right.Y : left.Y;
            int unevenRoofStartX = leftRoofHigher
                ? structure.LayoutRandom.Next(left.X + (int)(length * 0.5), right.X - (int)(length * 0.35))
                : structure.LayoutRandom.Next(left.X + (int)(length * 0.35), right.X - (int)(length * 0.5));
            float peakRoofSlope = structure.LayoutRandom.NextFromList(0.67f, 1f, 1.5f, 2f);
            float sideRoofSlope = float.Min(peakRoofSlope, 0.5f);
            int lowerRoofLength = leftRoofHigher
                ? right.X - unevenRoofStartX
                : unevenRoofStartX - left.X;
            bool hasSlopedSideRoof = structure.LayoutRandom.NextBool(3, 4);
            bool hasPeak = structure.LayoutRandom.NextBool(3, 4);

            List<Point16> path;
            if (hasSlopedSideRoof) {
                int lowerRoofOffset = (int)(sideRoofSlope * lowerRoofLength);
                path = [
                    new Point16(left.X, leftRoofHigher ? upperRoofBottomY : lowerRoofBottomY),
                    new Point16(unevenRoofStartX, leftRoofHigher ? upperRoofBottomY : lowerRoofBottomY - lowerRoofOffset),
                    new Point16(unevenRoofStartX, !leftRoofHigher ? upperRoofBottomY : lowerRoofBottomY - lowerRoofOffset),
                    new Point16(right.X, !leftRoofHigher ? upperRoofBottomY : lowerRoofBottomY)
                ];
            }
            else {
                path = [
                    new Point16(left.X, leftRoofHigher ? upperRoofBottomY : lowerRoofBottomY),
                    new Point16(unevenRoofStartX, leftRoofHigher ? upperRoofBottomY : lowerRoofBottomY),
                    new Point16(unevenRoofStartX, !leftRoofHigher ? upperRoofBottomY : lowerRoofBottomY),
                    new Point16(right.X, !leftRoofHigher ? upperRoofBottomY : lowerRoofBottomY)
                ];
            }

            // add roof peak if required
            if (hasPeak) {
                int higherRoofLength = length - lowerRoofLength;
                int roofPeakX = leftRoofHigher
                    ? (int)Math.Ceiling(left.X + higherRoofLength * 0.5)
                    : (int)Math.Ceiling(unevenRoofStartX + higherRoofLength * 0.5);
                int roofPeakOffset = (int)(peakRoofSlope * 0.5 * higherRoofLength);
                path.Insert(leftRoofHigher ? 1 : 3, new Point16(roofPeakX, upperRoofBottomY - roofPeakOffset));
                if (higherRoofLength % 2 == 1) path.Insert(leftRoofHigher ? 1 : 3, new Point16(roofPeakX - 1, upperRoofBottomY - roofPeakOffset));
            }

            return path;
        }

        /// <summary>
        ///     creates a basic or muti-segment roofs with a variety of different slopes and can handle different starting and ending Ys
        /// </summary>
        /// <param name="param"></param>
        /// <param name="left">X must be less than <see cref="right" />'s X</param>
        /// <param name="right">X must be greater than <see cref="left" />'s X</param>
        /// <param name="floorThickness"></param>
        /// <param name="wallThickness"></param>
        /// <returns></returns>
        public static (List<Floor> floors, List<Wall> walls, List<Roof> roofs) Action(StructureLayoutParams param, Point16 left, Point16 right, int floorThickness, int wallThickness) {
            bool forceFlat = param.TagsRequired.HasTag(Tags.HasOnlyRectangleRooms);
            bool isFlat = left.Y == right.Y;
            int fullLength = right.X - left.X - 2 + 2 * wallThickness;

            // make a switch; split roof can also be when rectangle rooms are forced
            List<Point16> path;
            path = WavyPeak(param.Structure, left, right);
            // if (forceFlat)
            //     path = FlatRoof(left, right);
            // else {
            //     if (isFlat && param.Structure.LayoutRandom.NextBool(1, 4))
            //         path = SinglePeakOnly(param.Structure, left, right);
            //     else if (param.Structure.LayoutRandom.NextBool(1, 3))
            //         path = SinglePeakPathWithFlats(param.Structure, left, right);
            //     else if (param.Structure.LayoutRandom.NextBool(1, 2))
            //         path = WavyPeak(param.Structure, left, right);
            //     else
            //         path = SplitRoof(param.Structure, left, right);
            // }

            var result = ExternalLayoutHelper.CreateTopFloorsWallsRoofs(param.Structure, path, floorThickness, true, wallThickness);
            TagMap.AddRequiredToEach(result.floors, Tags.SlopingAlgorithm, SlopeHelper.SimpleSlopes);
            TagMap.AddRequiredToEach(result.floors, Tags.SlopeGrouping, SlopeGrouping.GlobalOnlySloping);
            TagMap.AddRequiredToEach(result.roofs, Tags.HasCustomSloping);
            return result;
        }
    }
}