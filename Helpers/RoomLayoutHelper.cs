#nullable enable
using System;
using System.Collections.Generic;
using System.Linq;
using SpawnHouses.AdvStructures.AdvStructureParts;
using SpawnHouses.Structures;
using SpawnHouses.Types;
using Terraria.DataStructures;
using Gap = SpawnHouses.AdvStructures.AdvStructureParts.Gap;

namespace SpawnHouses.Helpers;

public static class RoomLayoutHelper {
    public static bool IsValidHousingSize(Shape volume) => volume.GetExpandedShape(1).GetArea() >= 60;

    /// <summary>
    ///     gets closest room to the point using the perimeter of each room
    /// </summary>
    /// <param name="rooms"></param>
    /// <param name="point"></param>
    /// <returns></returns>
    public static Room GetClosestRoom(List<Room> rooms, Point16 point) {
        if (rooms.Count == 0)
            throw new Exception("BackgroundVolumes in given room layout was empty");

        int closestIndex = -1;
        double closestDistance = double.MaxValue;

        for (int i = 0; i < rooms.Count; i++) {
            double closestDistanceInShape = double.MaxValue;
            rooms[i].Volume.ExecuteOnPerimeter((x, y, _) => {
                double distance = Math.Sqrt(Math.Pow(point.X - x, 2) + Math.Pow(point.Y - y, 2));
                if (distance < closestDistanceInShape)
                    closestDistanceInShape = distance;
            });

            if (closestDistanceInShape < closestDistance) {
                closestDistance = closestDistanceInShape;
                closestIndex = i;
            }
        }

        return rooms[closestIndex];
    }

    /// <summary>
    ///     returns list of gaps directly touching the given volume
    /// </summary>
    /// <param name="volume"></param>
    /// <param name="gaps">list of all possible gaps</param>
    /// <returns></returns>
    public static List<Gap> GetAdjacentGaps(Shape volume, List<Gap> gaps) {
        Shape expandedVolume = volume.GetExpandedShape(1);
        return gaps.FindAll(gap => expandedVolume.HasIntersection(gap.Volume));
    }

    /// <summary>
    ///     creates actual floor and wall objects from volumes
    /// </summary>
    public static (List<Floor> floors, List<Wall> walls) CreateFloorsAndWalls(RoomLayoutVolumes roomLayoutVolumes) {
        List<Floor> floors = [];
        List<Wall> walls = [];
        foreach (Shape shape in roomLayoutVolumes.FloorVolumes) {
            bool exterior = true;
            Point16 lowerPoint = new(shape.BoundingBox.topLeft.X + shape.Size.X / 2, shape.BoundingBox.topLeft.Y - 1);
            Point16 higherPoint = new(shape.BoundingBox.topLeft.X + shape.Size.X / 2, shape.BoundingBox.bottomRight.Y + 1);
            if (roomLayoutVolumes.InStructure(lowerPoint) && roomLayoutVolumes.InStructure(higherPoint)) exterior = false;

            floors.Add(new Floor(shape, exterior));
        }

        foreach (Shape shape in roomLayoutVolumes.WallVolumes) {
            bool exterior = true;
            Point16 lowerPoint = new(shape.BoundingBox.topLeft.X - 1, shape.BoundingBox.topLeft.Y + shape.Size.Y / 2);
            Point16 higherPoint = new(shape.BoundingBox.bottomRight.X + 1, shape.BoundingBox.topLeft.Y + shape.Size.Y / 2);
            if (roomLayoutVolumes.InStructure(lowerPoint) && roomLayoutVolumes.InStructure(higherPoint)) exterior = false;

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

    #region Gap Methods

    /// <summary>
    ///     Uses perimeter raycasting to find all possible gaps of any size
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
                if (lastDirection == Directions.None) lastDirection = direction;

                Point16 step = direction switch {
                    Directions.Up => new Point16(0, -1),
                    Directions.Down => new Point16(0, 1),
                    Directions.Left => new Point16(-1, 0),
                    Directions.Right => new Point16(1, 0),
                    _ => new Point16(0, 0)
                };

                pos += step;
                while (direction is Directions.Up or Directions.Down ? roomLayoutVolumes.InFloor(pos) : roomLayoutVolumes.InWall(pos))
                    pos += step;

                Room? foundRoom = GetRoomFromPos(rooms, pos);
                if (foundRoom == room) foundRoom = null; // invalidate casts that find its own room

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
    ///     Removes duplicate gaps in-place
    /// </summary>
    /// <param name="gaps"></param>
    public static void RemoveDuplicateGaps(List<Gap> gaps) {
        for (int testingIndex = gaps.Count - 1; testingIndex >= 0; testingIndex--)
        for (int removingIndex = testingIndex - 1; removingIndex >= 0; removingIndex--) {
            if (testingIndex >= gaps.Count) break;
            Gap gap = gaps[testingIndex];
            Gap removingGap = gaps[removingIndex];

            if (gap.RepresentsSimilarGap(removingGap)) {
                if (removingGap.LowerRoom.Gaps.Remove(removingGap)) removingGap.LowerRoom.Gaps.Add(gap);

                if (removingGap.HigherRoom!.Gaps.Remove(removingGap)) removingGap.HigherRoom!.Gaps.Add(gap);

                gaps.RemoveAt(removingIndex);
            }
        }
    }

    /// <summary>
    ///     Removes impractically small gaps, moves and shrinks large gaps
    /// </summary>
    /// <param name="gaps"></param>
    /// <returns></returns>
    public static void ResizeAndMoveGaps(List<Gap> gaps) {
        short[] horizontalGapSizes = gaps.Where(gap => !gap.IsHorizontal).Select(gap => gap.Volume.Size.X).ToArray();
        short maxFloorGapSize = horizontalGapSizes.Length != 0 ? horizontalGapSizes.Max() : (short)7;

        // filter out gaps which are too small and resize gaps
        for (int gapIndex = gaps.Count - 1; gapIndex >= 0; gapIndex--) {
            Gap gap = gaps[gapIndex];
            if (gap.IsHorizontal) {
                if (gap.Volume.Size.Y < 3) {
                    gaps.RemoveAt(gapIndex);
                    continue;
                }

                var points = gap.Volume.Points;
                for (int pointIndex = 0; pointIndex < gap.Volume.Points.Length; pointIndex++)
                    // ensure doors aren't too tall
                    if (points[pointIndex].Y < gap.Volume.BoundingBox.bottomRight.Y - 2)
                        points[pointIndex] = new Point16(points[pointIndex].X, gap.Volume.BoundingBox.bottomRight.Y - 2);

                gap.Volume = new Shape(points);
            }
            else {
                if (gap.Volume.Size.X < 2) {
                    gaps.RemoveAt(gapIndex);
                    continue;
                }

                int suggestedSize = (int)(gap.Volume.Size.X / (float)maxFloorGapSize * 2 + 2);
                // randomly move the gap, if there's space to do so
                if (suggestedSize < gap.Volume.Size.X) {
                    int gapCenter = (int)(Terraria.WorldGen.genRand.NextDouble() * gap.Volume.Size.X) + gap.Volume.BoundingBox.topLeft.X;
                    int leftX = gapCenter - (int)Math.Floor((double)suggestedSize / 2);
                    int outOfBoundsDistance = Math.Max(0, gap.Volume.BoundingBox.topLeft.X - leftX);
                    leftX = Math.Max(leftX, gap.Volume.BoundingBox.topLeft.X);
                    int rightX = gapCenter + (int)Math.Ceiling((double)suggestedSize / 2) + outOfBoundsDistance;
                    var points = gap.Volume.Points;
                    for (int pointIndex = 0; pointIndex < gap.Volume.Points.Length; pointIndex++) {
                        if (points[pointIndex].X < leftX) points[pointIndex] = new Point16(leftX, points[pointIndex].Y);

                        if (points[pointIndex].X > rightX) points[pointIndex] = new Point16(rightX, points[pointIndex].Y);
                    }

                    gap.Volume = new Shape(points);
                }
            }
        }

        // attempt to chain vertical gaps
        for (int gapIndex = gaps.Count - 1; gapIndex >= 0; gapIndex--) {
            Gap gap = gaps[gapIndex];
            if (gap.IsHorizontal) continue;

            // randomly move the gap, but if possible align it with the gap below
            Gap? potentialChainGap = gaps.Find(potentialGap =>
                !potentialGap.IsHorizontal &&
                potentialGap.HigherRoom == gap.LowerRoom &&
                gap.Volume.BoundingBox.topLeft.X <= potentialGap.Volume.BoundingBox.topLeft.X &&
                gap.Volume.BoundingBox.bottomRight.X >= potentialGap.Volume.BoundingBox.bottomRight.X
            );

            // 60% chance to go for chain gaps
            if (potentialChainGap != null && Terraria.WorldGen.genRand.NextDouble() < 0.6) {
                gap.IsChain = true;
                gap.VerticalChainLower = potentialChainGap;
                potentialChainGap.VerticalChainHigher = gap;
                var points = gap.Volume.Points;
                for (int pointIndex = 0; pointIndex < gap.Volume.Points.Length; pointIndex++) {
                    if (points[pointIndex].X < potentialChainGap.Volume.BoundingBox.topLeft.X) points[pointIndex] = new Point16(potentialChainGap.Volume.BoundingBox.topLeft.X, points[pointIndex].Y);

                    if (points[pointIndex].X > potentialChainGap.Volume.BoundingBox.bottomRight.X) points[pointIndex] = new Point16(potentialChainGap.Volume.BoundingBox.bottomRight.X, points[pointIndex].Y);
                }

                gap.Volume = new Shape(points);
            }
            else {
                gaps.RemoveAt(gapIndex);
            }
        }
    }

    public static void PruneGaps(List<Room> rooms, EntryPoint[] entryPoints) {
        HashSet<Gap> requiredGaps = [];
        var visitedRooms = rooms.ToHashSet();
        var visitQueue = new Queue<(Room room, Room? parentRoom)>();

        foreach (EntryPoint entryPoint in entryPoints) {
            Room potentialRoom = GetClosestRoom(rooms, entryPoint.Center);
            visitQueue.Enqueue((potentialRoom, null));
        }

        while (visitQueue.Count > 0) {
            (Room room, Room? parentRoom) = visitQueue.Dequeue();
            if (!visitedRooms.Add(room)) continue;

            room.SetParent(parentRoom);
            foreach (Room connection in room.GetConnections()) visitQueue.Enqueue((connection, room));
        }
    }

    /// <summary>
    ///     adds gaps between rooms, then optimizes and processes the gaps. essentially completes the whole gap making process
    ///     from start to end
    /// </summary>
    /// <param name="roomLayoutVolumes"></param>
    /// <param name="roomLayoutParams"></param>
    /// <returns></returns>
    public static (List<Gap> gaps, List<Room> rooms) CreateGapsAndRooms(RoomLayoutVolumes roomLayoutVolumes, RoomLayoutParams roomLayoutParams) {
        var (allGaps, rooms) = RaycastGaps(roomLayoutVolumes);
        RemoveDuplicateGaps(allGaps);
        ResizeAndMoveGaps(allGaps);
        //PruneGaps(rooms, roomLayoutParams.EntryPoints);

        return (allGaps, rooms);
    }

    public static RoomLayout CreateRoomLayoutFromVolumes(RoomLayoutVolumes roomLayoutVolumes, RoomLayoutParams roomLayoutParams) {
        var (floors, walls) = CreateFloorsAndWalls(roomLayoutVolumes);
        var (gaps, rooms) = CreateGapsAndRooms(roomLayoutVolumes, roomLayoutParams);

        return new RoomLayout(floors, walls, gaps, rooms);
    }

    /// <summary>
    /// </summary>
    /// <param name="entryPoints"></param>
    /// <param name="floorWidth"></param>
    /// <param name="wallWidth"></param>
    /// <remarks>returned gaps have both rooms set to null</remarks>
    /// <returns></returns>
    public static Gap[] GapsFromEntryPoints(EntryPoint[] entryPoints, int floorWidth, int wallWidth) {
        var gaps = new Gap[entryPoints.Length];
        for (int i = 0; i < gaps.Length; i++) {
            EntryPoint entryPoint = entryPoints[i];
            if (entryPoint.IsHorizontal)
                gaps[i] = new Gap(
                    new Shape(
                        true,
                        entryPoint.Start,
                        entryPoint.End + new Point16(entryPoint.Direction is Directions.Right ? wallWidth - 1 : -wallWidth + 1, 0)
                    ),
                    null, null, entryPoint.Direction is Directions.Left or Directions.Right
                );
            else
                gaps[i] = new Gap(
                    new Shape(
                        true,
                        entryPoint.Start,
                        entryPoint.End + new Point16(0, entryPoint.Direction is Directions.Down ? floorWidth - 1 : -floorWidth + 1)
                    ),
                    null, null, entryPoint.Direction is Directions.Left or Directions.Right
                );
        }

        return gaps;
    }

    #endregion
}