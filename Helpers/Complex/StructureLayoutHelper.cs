#nullable enable

using System;
using System.Collections.Generic;
using System.Linq;
using SpawnHouses.Core.AdvGeneratables;
using SpawnHouses.Core.AdvGeneratables.Components;
using SpawnHouses.Core.AdvStructureCore;
using SpawnHouses.Core.DataStructures;
using SpawnHouses.Core.Enums;
using SpawnHouses.Core.Geometry;
using SpawnHouses.Core.Parameters;
using SpawnHouses.Core.RootStructureTypes;
using SpawnHouses.Core.Tagging;
using SpawnHouses.Core.Tiles;
using SpawnHouses.Legacy.Helpers;
using Terraria;
using Terraria.DataStructures;
using Terraria.Utilities.Terraria.Utilities;

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
            Tags.Structure_HasRooms,
            Tags.Structure_HasOnlyRectangleRooms,
            Tags.Structure_HasSomeRectangleRooms,
            Tags.Structure_HasLargeRoom
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
                roomVolume.BoundingBox.Height >= floorWidth + 2 * param.RoomHeight.Min // make sure volume is tall enough to be split
                && expandedArea > 90 // make sure area is large enough to be split (area is approximated and it's thoroughly checked later, so it's low balled)
                && roomVolume.BoundingBox.Width < roomVolume.BoundingBox.Height * 1.3; // make sure width/height ratio of resulting rooms isn't stupid

            bool canSplitAlongY =
                roomVolume.BoundingBox.Width >= wallWidth + 2 * param.RoomWidth.Min
                && expandedArea > 90
                && roomVolume.BoundingBox.Height < roomVolume.BoundingBox.Width * 1.6;

            switch (canSplitAlongX) {
                case true when canSplitAlongY: {
                    if (roomVolume.BoundingBox.Height > param.RoomHeight.Max) return (true, true);
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
                Shape? cutShape = shape.SplitOnce(splitAlongX, splitStartPos + iterationSplitWidth - 1, true, false);
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
        private static HashSet<int> GetValidSplits(Shape roomVolume, PriorityCollection<PartialPoint> prioritySplits, bool splitAlongX, int splitWidth,
            HashSet<int> verticalGapXs, HashSet<int> horizontalGapYs, NumRange validCutRange) {
            // ensure that, taking blocklisted coordinates into account, there is valid places for the split
            HashSet<int> validSplitStarts = [];
            foreach (var tuple in prioritySplits.ToSortedHashSetArray()) {
                foreach (PartialPoint split in tuple.set) {
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
        private static RoomLayout SplitBsp(RoomLayoutParams param, Room room, PriorityCollection<PartialPoint> prioritySplits, bool prioritizeSplitsOnGapFloors, int targetRoomCount) {
            if (param.RoomHeight.Max < param.FloorWidth.Max + 2 * param.RoomHeight.Min)
                SpawnHousesMod.Instance.Logger.Warn(
                    $"a max room height of {param.RoomHeight.Max} was given, but at least {param.FloorWidth.Max + 2 * param.RoomHeight.Min} is required");
            if (param.RoomWidth.Max < param.WallWidth.Max + 2 * param.RoomWidth.Min)
                SpawnHousesMod.Instance.Logger.Warn(
                    $"a max room height of {param.RoomWidth.Max} was given, but at least {param.WallWidth.Max + 2 * param.RoomWidth.Min} is required");

            List<(Shape volume, string name)> floorVolumes = [], wallVolumes = [], finishedRoomVolumes = [];
            var roomQueue = new Queue<Shape>([room.Geometry]);
            int extraCuts = 0, curLargeRoomCount = 0, xCutCount = 0, yCutCount = 0;
            bool hasLargeRooms = param.TagsRequired.GetValueSafe(Tags.Structure_HasLargeRoom, out int targetLargeRoomCount);
            float largeRoomChance = hasLargeRooms ? (float)targetLargeRoomCount / targetRoomCount : 0;
            int maxLargeRooms = (int)Math.Ceiling(largeRoomChance * targetRoomCount);
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
                        for (int y = gap.Geometry.BoundingBox.Top; y <= gap.Geometry.BoundingBox.Bottom; y++) {
                            iterationHorizontalGapYs.Add(y);

                            if (prioritizeSplitsOnGapFloors && gap.Geometry.BoundingBox.Bottom != room.Geometry.BoundingBox.Bottom)
                                prioritySplits.AddItem(new PartialPoint(0, gap.Geometry.BoundingBox.Bottom + 1, false), 0);
                        }
                    else
                        for (int x = gap.Geometry.BoundingBox.Left; x <= gap.Geometry.BoundingBox.Right; x++)
                            iterationVerticalGapXs.Add(x);

                // set this iteration's parameters
                double inverseProgressFactor = double.Max(1 - (double)curHousing / targetRoomCount, 0);
                int iterationFloorWidth = (int)Math.Round((param.FloorWidth.Max - param.FloorWidth.Min) * inverseProgressFactor) + param.FloorWidth.Min;
                int iterationWallWidth = (int)Math.Round((param.WallWidth.Max - param.WallWidth.Min) * inverseProgressFactor) + param.WallWidth.Min;

                (bool valid, bool splitAlongX) = EvaluateCutOnX(param, roomVolume, iterationFloorWidth, iterationWallWidth, xCutCount, yCutCount);

                if (!valid) {
                    // if the room can't be split at all, don't add it back to the Queue
                    finishedRoomVolumes.Add((roomVolume, $"Rm_BSP_GenOrder{finishedRoomVolumes.Count}"));
                    extraCuts++;
                    continue;
                }

                int iterationSplitWidth = splitAlongX ? iterationFloorWidth : iterationWallWidth;
                int outerBoundaryWidth = splitAlongX ? param.RoomHeight.Min : param.RoomWidth.Min;
                NumRange validCutRange = new((splitAlongX ? roomVolume.BoundingBox.Top : roomVolume.BoundingBox.Left) + outerBoundaryWidth,
                    (splitAlongX ? roomVolume.BoundingBox.Bottom : roomVolume.BoundingBox.Right) - outerBoundaryWidth - iterationSplitWidth
                );

                var validSplitStarts = GetValidSplits(roomVolume, prioritySplits, splitAlongX, iterationSplitWidth, iterationVerticalGapXs, iterationHorizontalGapYs, validCutRange);

                if (validSplitStarts.Count == 0) {
                    finishedRoomVolumes.Add((roomVolume, $"Rm_BSP_GenOrder{finishedRoomVolumes.Count}"));
                    extraCuts++;
                    continue;
                }

                int splitStart = param.Structure.LayoutRandom.NextFromCollection(validSplitStarts.ToList());
                int splitEnd = splitStart + iterationSplitWidth - 1;

                (Shape? lower, Shape? middle, Shape? higher) roomSubsections = roomVolume.SplitTwice(splitAlongX, splitStart, splitEnd);

                if (splitAlongX)
                    xCutCount++;
                else
                    yCutCount++;

                prioritySplits.AddToBlocklist(new PartialPoint(splitStart, splitStart, !splitAlongX, splitAlongX));

                if (roomSubsections.lower is not null) {
                    if (param.IsWithinMaxSize(roomSubsections.lower) && param.Structure.LayoutRandom.NextDouble() < (1 - Math.Pow(1 - largeRoomChance, param.Attempts)) * 0.35 &&
                        curLargeRoomCount < maxLargeRooms && inverseProgressFactor < 0.92) {
                        curLargeRoomCount++;
                        finishedRoomVolumes.Add((roomSubsections.lower, $"Rm_BSP_GenOrder{finishedRoomVolumes.Count}"));
                        extraCuts++;
                    }
                    else {
                        roomQueue.Enqueue(roomSubsections.lower);
                    }
                }

                if (roomSubsections.higher is not null) {
                    if (param.IsWithinMaxSize(roomSubsections.higher) && param.Structure.LayoutRandom.NextDouble() < (1 - Math.Pow(1 - largeRoomChance, param.Attempts)) * 0.35 &&
                        curLargeRoomCount < maxLargeRooms && inverseProgressFactor < 0.92) {
                        curLargeRoomCount++;
                        finishedRoomVolumes.Add((roomSubsections.higher, $"Rm_BSP_GenOrder{finishedRoomVolumes.Count}"));
                        extraCuts++;
                    }
                    else {
                        roomQueue.Enqueue(roomSubsections.higher);
                    }
                }

                if (roomSubsections.middle is not null) {
                    if (splitAlongX)
                        floorVolumes.Add((roomSubsections.middle, $"F_BSPInterior_GenOrder{floorVolumes.Count}"));
                    else
                        wallVolumes.Add((roomSubsections.middle, $"W_BSPInterior_GenOrder{finishedRoomVolumes.Count}"));
                }
            }

            // finalize any rooms left over after we have the target housing
            List<(Shape Shape, string name)> namedRoomQueue = roomQueue.Select(r => (r, $"Rm_BSPLeftover_GenOrder{finishedRoomVolumes.Count}")).ToList();
            finishedRoomVolumes.AddRange(namedRoomQueue);
            
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
            PriorityCollection<PartialPoint> prioritySplits = new((thisObj, otherObj) => (thisObj.X == otherObj.X || !thisObj.HasX) && (thisObj.Y == otherObj.Y || thisObj.HasY));
            foreach (PartialPoint corner in room.Geometry.GetCorners())
                prioritySplits.AddItem(corner, 1);

            int targetRoomCount = p.TagsRequired.GetValue(Tags.Structure_HasRooms);

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
                    gap.InteriorRoom = RoomHelper.GetClosestRoom(pickedLayout.Rooms, gap.Geometry.BoundingBox.CenterPoint16);
            }

            p.Structure.StructureLayout.TagsCurrent.Add(Tags.Structure_HasRooms, pickedLayout.Rooms.Count);
            p.Structure.StructureLayout.TagsCurrent.Add(room.Geometry.IsBox ? Tags.Structure_HasOnlyRectangleRooms : Tags.Structure_HasSomeRectangleRooms);
            if (p.TagsRequired.HasTag(Tags.Structure_HasLargeRoom))
                p.Structure.StructureLayout.TagsCurrent.Add(Tags.Structure_HasLargeRoom);
            
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
                    Point16 gapBottom = gap.Geometry.BoundingBox.BottomRightPoint16;
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
                            entryPoint.Start + new Point16(entryPoint.EntryDirection is Direction.Right ? -wallWidth + 1 : wallWidth - 1, 0),
                            entryPoint.End + new Point16(entryPoint.EntryDirection is Direction.Right ? -wallWidth + 1 : wallWidth - 1, 0)
                        ),
                        null!,
                        null,
                        true,
                        entryPoint.EntryDirection is Direction.Left ? $"EP_Left_GenOrder{i}" : $"EP_Right_GenOrder{i}"
                    );
                else
                    gaps[i] = new Gap(
                        structure,
                        new Shape(
                            true,
                            entryPoint.Start,
                            entryPoint.End + new Point16(0, entryPoint.EntryDirection is Direction.Down ? floorWidth - 1 : -floorWidth + 1)
                        ),
                        null!,
                        null,
                        false,
                        entryPoint.EntryDirection is LegacyDirections.Up ? $"EntryPoint_Up_GenOrder{i}" : $"EntryPoint_Down_GenOrder{i}"
                    );
            }

            return gaps;
        }

        /// <summary>
        ///     gets a shape that represents the interior of the structure, and excludes any exterior components
        /// </summary>
        /// <param name="tilemap"></param>
        /// <returns></returns>
        /// <exception cref="Exception"></exception>
        /// <remarks>assumes only one interior in the tilemap</remarks>
        private static Shape GetStructureInterior(StructureTilemap tilemap) {
            // Build the boundary by collecting all exterior-facing edges of interior tiles,
            // then stitch those edges into a single polygon. This approach is more robust
            // than a fragile marching-squares walker that can miss corners in complex shapes.

            int width = tilemap.Width;
            int height = tilemap.Height;

            // tilemap.InInterior is a method: use tilemap.InInterior(x,y) to query

            // Collect directed boundary edges (from -> to). We orient each tile's
            // boundary edges in a consistent CCW order so edges chain cleanly.
            var edges = new HashSet<string>(); // key: "ax,ay->bx,by"

            for (int y = 0; y < height; y++)
            for (int x = 0; x < width; x++) {
                if (!tilemap.InInterior(x, y)) continue;

                // neighbors: treat out-of-bounds as not-inside
                bool up = y - 1 >= 0 && tilemap.InInterior(x, y - 1);
                bool right = x + 1 < width && tilemap.InInterior(x + 1, y);
                bool down = y + 1 < height && tilemap.InInterior(x, y + 1);
                bool left = x - 1 >= 0 && tilemap.InInterior(x - 1, y);

                // oriented CCW around the tile: (x,y) -> (x+1,y) -> (x+1,y+1) -> (x,y+1)
                if (!up) edges.Add($"{x},{y}->{x + 1},{y}"); // top edge
                if (!right) edges.Add($"{x + 1},{y}->{x + 1},{y + 1}"); // right edge
                if (!down) edges.Add($"{x + 1},{y + 1}->{x},{y + 1}"); // bottom edge
                if (!left) edges.Add($"{x},{y + 1}->{x},{y}"); // left edge
            }

            if (edges.Count == 0) throw new Exception("tilemap did not have any interior tiles");

            // Build adjacency map from the directed edges
            var adj = new Dictionary<Point16, List<Point16>>();

            static Point16 ParsePoint(string s) {
                string[] parts = s.Split(',');
                return new Point16(int.Parse(parts[0]), int.Parse(parts[1]));
            }

            foreach (string key in edges) {
                string[] parts = key.Split("->");
                Point16 a = ParsePoint(parts[0]);
                Point16 b = ParsePoint(parts[1]);

                if (!adj.TryGetValue(a, out var list)) {
                    list = new List<Point16>();
                    adj[a] = list;
                }

                // avoid duplicates
                if (!list.Contains(b)) list.Add(b);
            }

            // start from the top-most, then left-most vertex to produce a stable start
            Point16 start = adj.Keys.OrderBy(p => p.Y).ThenBy(p => p.X).First();

            var outline = new List<Point16> { start };
            var usedEdges = new HashSet<string>();
            Point16 cur = start;

            // Walk the directed edges until we return to start. At each vertex pick the
            // next outgoing edge that hasn't been used yet. For well-formed single
            // interior shapes this will produce the outer boundary in order.
            while (true) {
                if (!adj.TryGetValue(cur, out var outs) || outs.Count == 0) break;

                Point16? next = null;
                foreach (Point16 candidate in outs) {
                    string ekey = $"{cur.X},{cur.Y}->{candidate.X},{candidate.Y}";
                    if (!usedEdges.Contains(ekey)) {
                        next = candidate;
                        usedEdges.Add(ekey);
                        break;
                    }
                }

                if (next == null) break; // no unused outgoing edge

                if (next.Value.Equals(start))
                    // closed loop complete
                    break;

                outline.Add(next.Value);
                cur = next.Value;
            }

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
        /// <param name="structureLayout"></param>
        /// <param name="exteriorFloors"></param>
        /// <param name="exteriorWalls"></param>
        /// <param name="externalFloorThickness"></param>
        /// <param name="externalWallThickness"></param>
        /// <returns></returns>
        public static Room Action(StructureLayoutParams param, StructureLayout structureLayout, List<Floor> exteriorFloors, List<Wall> exteriorWalls, int externalFloorThickness, int externalWallThickness) {
            var externalGaps = GapsFromEntryPoints(param.Structure, param.EntryPoints, externalFloorThickness, externalWallThickness).ToList();
            structureLayout.SetExternalGapComponents(externalGaps);
            TagMap.AddRequiredToEach(externalGaps, Tags.External);
            SetTilesExternalStatus(param, exteriorFloors, exteriorWalls, externalGaps);
            Room internalRoom = new(param.Structure, GetStructureInterior(param.Structure.Tilemap), "Rm_InteriorBase", externalGaps);

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
        private static List<AnnotatedPoint16> SinglePeakOnly(AdvStructure structure, Point16 left, Point16 right, float roofSlope = -1) {
            if (Math.Abs(roofSlope - -1) < 0.001f)
                roofSlope = structure.LayoutRandom.NextFromList(1f, 1.33f, 1.67f);
            bool hasHigherSide = left.Y != right.Y;
            bool leftRoofHigher = left.Y < right.Y;
            int length = right.X - left.X;
            int upperRoofBottomY = leftRoofHigher ? left.Y : right.Y;
            int lowerRoofBottomY = leftRoofHigher ? right.Y : left.Y;

            List<AnnotatedPoint16> path;
            if (hasHigherSide) {
                double middleX = (roofSlope * (left.X + right.X) - (right.Y - left.Y)) / (2 * roofSlope);
                AnnotatedPoint16 middlePoint = new((int)Math.Ceiling(middleX), (int)(left.Y - roofSlope * (middleX - left.X)), "Left_MainSlope");

                path = [
                    new AnnotatedPoint16(left.X, leftRoofHigher ? upperRoofBottomY : lowerRoofBottomY, ""),
                    middlePoint,
                    new AnnotatedPoint16(right.X, !leftRoofHigher ? upperRoofBottomY : lowerRoofBottomY, "Right_MainSlope")
                ];
                if (length % 2 == 1)
                    path.Insert(2, new AnnotatedPoint16(left.X + 1 + length / 2, upperRoofBottomY - (int)(roofSlope * length * 0.5), "Middle_SideEvening"));
            }
            else {
                path = [
                    new AnnotatedPoint16(left.X, upperRoofBottomY, ""),
                    new AnnotatedPoint16(left.X + length / 2, upperRoofBottomY - (int)(roofSlope * length * 0.5), "Left_MainSlope"),
                    new AnnotatedPoint16(right.X, upperRoofBottomY, "Right_MainSlope")
                ];
                if (length % 2 == 1)
                    path.Insert(2, new AnnotatedPoint16(left.X + 1 + length / 2, upperRoofBottomY - (int)(roofSlope * length * 0.5), "Middle_SideEvening"));
            }

            return path;
        }

        /// <summary>
        ///     creates a roof path with a single peak with flat sections on the side. start and end can be at different heights
        /// </summary>
        /// <returns></returns>
        private static List<AnnotatedPoint16> SinglePeakPathWithFlats(AdvStructure structure, Point16 left, Point16 right) {
            FloatRange possiblePeakLengths = left.Y == right.Y ? new FloatRange(0.6f, 0.7f) : new FloatRange(0.48f, 0.62f);
            int peakSectionLength = (int)((right.X - left.X) * structure.LayoutRandom.NextFloat(possiblePeakLengths));
            int offsetRange = right.X - left.X - peakSectionLength;
            float[] possibleOffsetProportions = left.Y == right.Y ? [0f, 0.33f, 0.66f, 1f] : [0.5f];
            int offset = (int)(offsetRange * structure.LayoutRandom.NextFromList(possibleOffsetProportions));
            float peakRoofSlope = structure.LayoutRandom.NextFromList(0.67f, 1f, 1.33f);
            int heightDelta = Math.Abs(left.Y - right.Y);

            while (peakRoofSlope * peakSectionLength - heightDelta < (left.X - right.X) / 3.0) peakRoofSlope += 0.33f;

            var path = SinglePeakOnly(structure, left + new Point16(offset, 0), new Point16(left.X + peakSectionLength - 1 + offset, right.Y), peakRoofSlope);
            path.Insert(0, new AnnotatedPoint16(left.X, left.Y, "Left_Base"));
            path.Add(new AnnotatedPoint16(right.X, right.Y, "Right_Base"));

            return path;
        }

        /// <summary>
        ///     creates a roof path with a peak towards either the left or right end, a flat section in the middle,
        ///     and finishing with either a slope or another flat section on the low side
        /// </summary>
        /// <returns></returns>
        public static List<AnnotatedPoint16> WavyPeak(AdvStructure structure, Point16 left, Point16 right) {
            bool finishWithFlat = structure.LayoutRandom.NextBool();
            int length = right.X - left.X;
            int verticalSideOffset = (int)(length * structure.LayoutRandom.NextFloat(0.15f, 0.35f)); // will only be used if the sides are even
            float roofSlope = structure.LayoutRandom.NextFromList(0.67f, 1f, 1.33f);

            // move one side up if they're the same y level
            bool offsetLeftUp = false, offsetRightUp = false;
            if (left.Y == right.Y) {
                if (structure.LayoutRandom.NextBool()) {
                    left += new Point16(0, -verticalSideOffset);
                    offsetLeftUp = true;
                }
                else {
                    right += new Point16(0, -verticalSideOffset);
                    offsetRightUp = true;
                }
            }

            // create higher section
            List<AnnotatedPoint16> path;
            if (left.Y < right.Y) {
                path = SinglePeakOnly(structure, left, left + new Point16((int)(length * 0.4f), 0), roofSlope);
                path.Add(new AnnotatedPoint16((int)(right.X - (right.Y - left.Y) / roofSlope - (finishWithFlat ? length * 0.23f : 0)), left.Y, "Middle_Flat"));
                path.Add(new AnnotatedPoint16(finishWithFlat ? right - new Point16((int)(length * 0.23f), 0) : right, finishWithFlat ? "Right_SlopeToEdgeFlat" : "Right_EdgeSlope"));
            }
            else {
                path = [
                    new AnnotatedPoint16(finishWithFlat ? left + new Point16((int)(length * 0.23f), 0) : left, ""),
                    new AnnotatedPoint16((int)(left.X + (left.Y - right.Y) / roofSlope + (finishWithFlat ? length * 0.23f : 0)), right.Y, finishWithFlat ? "Left_SlopeToEdgeFlat" : "Left_EdgeSlope")
                ];
                path.AddRange(SinglePeakOnly(structure, right - new Point16((int)(length * 0.4f), 0), right, roofSlope));
            }

            // and add lower flat if required
            if (finishWithFlat) {
                if (left.Y < right.Y)
                    path.Add(new AnnotatedPoint16(right, "Right_EdgeFlat"));
                else
                    path.Insert(0, new AnnotatedPoint16(left, "Left_EdgeFlat"));
            }

            // make sure roof path doesn't overlap
            int requiredOffsetDistance = (int)(length * 0.12f);
            int checkIndexOffset = finishWithFlat ? 1 : 0;
            AnnotatedPoint16 oldCheckPoint1 = path[^(2 + checkIndexOffset)];
            AnnotatedPoint16 oldCheckPoint2 = path[1 + checkIndexOffset];
            if (left.Y < right.Y && oldCheckPoint1.Point.X < path[^(3 + checkIndexOffset)].Point.X + requiredOffsetDistance)
                path[^(2 + checkIndexOffset)] = new AnnotatedPoint16(path[^(3 + checkIndexOffset)].Point.X + requiredOffsetDistance, oldCheckPoint1.Point.Y, oldCheckPoint1.Name);
            else if (path[1 + checkIndexOffset].Point.X > path[2 + checkIndexOffset].Point.X - requiredOffsetDistance) path[1 + checkIndexOffset] = new AnnotatedPoint16(path[2 + checkIndexOffset].Point.X - requiredOffsetDistance, oldCheckPoint2.Point.Y, oldCheckPoint2.Name);


            // add extra wall to make sure roof seals with the structure
            if (offsetLeftUp) path.Insert(0, new AnnotatedPoint16(left.X, left.Y + verticalSideOffset, "Left_RoofSeal"));
            if (offsetRightUp) path.Add(new AnnotatedPoint16(right.X, right.Y + verticalSideOffset, "Right_RoofSeal"));

            return path;
        }

        /// <summary>
        ///     creates a roof path with 2 distinct sections, one higher and one lower.
        ///     the upper end can have various slopes, and the lower end can have a slope that compliments the higher end
        /// </summary>
        /// <returns></returns>
        private static List<AnnotatedPoint16> SplitRoof(AdvStructure structure, Point16 left, Point16 right) {
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

            List<AnnotatedPoint16> path;
            if (hasSlopedSideRoof) {
                int lowerRoofOffset = (int)(sideRoofSlope * lowerRoofLength);
                path = [
                    new AnnotatedPoint16(left.X, leftRoofHigher ? upperRoofBottomY : lowerRoofBottomY, ""),
                    new AnnotatedPoint16(unevenRoofStartX, leftRoofHigher ? upperRoofBottomY : lowerRoofBottomY - lowerRoofOffset, "Left_Section"),
                    new AnnotatedPoint16(unevenRoofStartX, !leftRoofHigher ? upperRoofBottomY : lowerRoofBottomY - lowerRoofOffset, "Middle_Split"),
                    new AnnotatedPoint16(right.X, !leftRoofHigher ? upperRoofBottomY : lowerRoofBottomY, "Right_Section")
                ];
            }
            else {
                path = [
                    new AnnotatedPoint16(left.X, leftRoofHigher ? upperRoofBottomY : lowerRoofBottomY, ""),
                    new AnnotatedPoint16(unevenRoofStartX, leftRoofHigher ? upperRoofBottomY : lowerRoofBottomY, "Left_Flat"),
                    new AnnotatedPoint16(unevenRoofStartX, !leftRoofHigher ? upperRoofBottomY : lowerRoofBottomY, "Middle_Split"),
                    new AnnotatedPoint16(right.X, !leftRoofHigher ? upperRoofBottomY : lowerRoofBottomY, "Right_Flat")
                ];
            }

            // add roof peak if required
            if (hasPeak) {
                int higherRoofLength = length - lowerRoofLength;
                int roofPeakX = leftRoofHigher
                    ? (int)Math.Ceiling(left.X + higherRoofLength * 0.5)
                    : (int)Math.Ceiling(unevenRoofStartX + higherRoofLength * 0.5);
                int roofPeakOffset = (int)(peakRoofSlope * 0.5 * higherRoofLength);
                path.Insert(leftRoofHigher ? 1 : 3, new AnnotatedPoint16(roofPeakX, upperRoofBottomY - roofPeakOffset, "MiddlePeak"));
                if (higherRoofLength % 2 == 1) path.Insert(leftRoofHigher ? 1 : 3, new AnnotatedPoint16(roofPeakX - 1, upperRoofBottomY - roofPeakOffset, "MiddlePeak_SideEvening"));
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
            bool forceFlat = param.TagsRequired.HasTag(Tags.Structure_HasOnlyRectangleRooms);
            bool isFlat = left.Y == right.Y;
            int fullLength = right.X - left.X - 2 + 2 * wallThickness;

            // make a switch; split roof can also be when rectangle rooms are forced
            List<AnnotatedPoint16> path;
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
            TagMap.AddRequiredToEach(result.floors, Tags.Component_SlopingAlgorithm, SlopeHelper.SimpleSlopes);
            TagMap.AddRequiredToEach(result.floors, Tags.Component_SlopeGrouping, SlopeGrouping.GlobalOnlySloping);
            return result;
        }
    }
}