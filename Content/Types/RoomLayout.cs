using System;
using System.Collections.Generic;
using System.Linq;
using SpawnHouses.Content.Modules.Components;
using SpawnHouses.Content.Parameters;
using SpawnHouses.Content.Types.Enums;
using SpawnHouses.Content.Types.Geometry;
using SpawnHouses.Content.Types.RootStructureTypes;
using SpawnHouses.Helpers;
using Terraria.DataStructures;

namespace SpawnHouses.Content.Types;

/// <summary>
///     arrangement of components that represents portions of a structure's internal layout
/// </summary>
public class RoomLayout {
    public AdvStructure Structure;

    /// <summary>
    ///     if true, this layout's components are represented by their corresponding objects. otherwise, they are represented by their respective volumes
    /// </summary>
    public bool ComponentMode;

    public List<Floor> Floors;
    public List<Wall> Walls;

    /// <summary>only includes gaps that are INSIDE this room layout; no external gaps</summary>
    public List<Gap> Gaps;
    public List<Room> Rooms;

    public List<(Shape volume, string name)> FloorVolumes;
    public List<(Shape volume, string name)> WallVolumes;
    public List<(Shape volume, string name)> RoomVolumes;

    /// <summary>
    ///     constructor which uses volumes to represent components
    /// </summary>
    /// <param name="structure"></param>
    /// <param name="floorVolumes"></param>
    /// <param name="wallVolumes"></param>
    /// <param name="roomVolumes"></param>
    /// <param name="convertToComponents"></param>
    public RoomLayout(AdvStructure structure, List<(Shape volume, string name)> floorVolumes, List<(Shape volume, string name)> wallVolumes, List<(Shape volume, string name)> roomVolumes, bool convertToComponents) {
        Structure = structure;
        FloorVolumes = floorVolumes;
        WallVolumes = wallVolumes;
        RoomVolumes = roomVolumes;
        if (convertToComponents)
            ConvertToComponents();
    }

    /// <summary>
    ///     constructor which uses components themselves. <see cref="ComponentMode" /> is initialized as true
    /// </summary>
    /// <param name="structure"></param>
    /// <param name="floors"></param>
    /// <param name="walls"></param>
    /// <param name="gaps"></param>
    /// <param name="rooms"></param>
    public RoomLayout(AdvStructure structure, List<Floor> floors, List<Wall> walls, List<Gap> gaps, List<Room> rooms) {
        Structure = structure;
        ComponentMode = true;
        Floors = floors;
        Walls = walls;
        Gaps = gaps;
        Rooms = rooms;
    }

    /// <summary>
    ///     if the point is within any floor volume
    /// </summary>
    /// <param name="point"></param>
    /// <returns></returns>
    public bool InFloors(Point16 point) => ComponentMode ? Floors.Any(floor => floor.Geometry.Contains(point)) : FloorVolumes.Any(floorVolume => floorVolume.volume.Contains(point));

    /// <summary>
    ///     if the point is within any wall volume
    /// </summary>
    /// <param name="point"></param>
    /// <returns></returns>
    public bool InWalls(Point16 point) => ComponentMode ? Walls.Any(wall => wall.Geometry.Contains(point)) : WallVolumes.Any(wallVolume => wallVolume.volume.Contains(point));

    /// <summary>
    ///     if the point is within any room volume
    /// </summary>
    /// <param name="point"></param>
    /// <returns></returns>
    public bool InGaps(Point16 point) {
        if (!ComponentMode)
            throw new Exception("gap volumes are not set in non-component mode");
        return Gaps.Any(gap => gap.Geometry.Contains(point));
    }

    /// <summary>
    ///     if the point is within any room volume
    /// </summary>
    /// <param name="point"></param>
    /// <returns></returns>
    public bool InRooms(Point16 point) => ComponentMode ? Rooms.Any(room => room.Geometry.Contains(point)) : RoomVolumes.Any(roomVolume => roomVolume.volume.Contains(point));

    /// <summary>
    ///     checks if the point is contained within any volume
    /// </summary>
    /// <param name="point"></param>
    /// <returns></returns>
    public bool InStructure(Point16 point) => InFloors(point) || InWalls(point) || InGaps(point) || InRooms(point);

    public void AssertNonComponentMode() {
        if (ComponentMode) throw new Exception("this RoomLayout should not be in component mode");
    }

    public void AssertComponentMode() {
        if (!ComponentMode) throw new Exception("this RoomLayout should be in component mode");
    }

    /// <summary>
    ///     mergers other <see cref="RoomLayout" />s into this one
    /// </summary>
    /// <param name="roomLayouts"></param>
    /// <exception cref="Exception"></exception>
    public void CombineComponents(params RoomLayout[] roomLayouts) {
        if (!ComponentMode)
            throw new Exception("this RoomLayout must be in component mode to combine components");
        foreach (RoomLayout roomLayout in roomLayouts) {
            if (!roomLayout.ComponentMode)
                throw new Exception("all RoomLayouts passed must be in component to combine components");
            Floors.AddRange(roomLayout.Floors);
            Walls.AddRange(roomLayout.Walls);
            Gaps.AddRange(roomLayout.Gaps);
            Rooms.AddRange(roomLayout.Rooms);
        }
    }

    /// <summary>
    ///     mergers other <see cref="RoomLayout" />s into this one
    /// </summary>
    /// <param name="roomLayouts"></param>
    /// <exception cref="Exception"></exception>
    public void CombineVolumes(params RoomLayout[] roomLayouts) {
        if (!ComponentMode)
            throw new Exception("this RoomLayout must not be in component mode to combine volumes");
        foreach (RoomLayout roomLayout in roomLayouts) {
            if (!roomLayout.ComponentMode)
                throw new Exception("all RoomLayouts passed must be in component mode to combine volumes");
            FloorVolumes.AddRange(roomLayout.FloorVolumes);
            WallVolumes.AddRange(roomLayout.WallVolumes);
            RoomVolumes.AddRange(roomLayout.RoomVolumes);
        }
    }

    /// <summary>
    ///     moves every shape and component (if applicable) by the offset. ex. if offset = (3, 0) will move shape 3 to the right in world coordinates
    /// </summary>
    /// <param name="offset"></param>
    public void Offset(Point16 offset) {
        foreach ((Shape volume, _) in FloorVolumes) volume.Move(offset);
        foreach ((Shape volume, _) in WallVolumes) volume.Move(offset);
        foreach ((Shape volume, _) in RoomVolumes) volume.Move(offset);

        if (!ComponentMode) return;

        foreach (Floor floor in Floors) floor.Geometry.Move(offset);
        foreach (Wall wall in Walls) wall.Geometry.Move(offset);
        foreach (Gap gap in Gaps) gap.Geometry.Move(offset);
        foreach (Room room in Rooms) room.Geometry.Move(offset);
    }

    #region Gap Helpers

    /// <summary>
    ///     Uses perimeter raycasting to find all possible gaps of any size
    /// </summary>
    /// <returns></returns>
    /// <remarks>this RoomLayout must be in non-component mode</remarks>
    private void RaycastGaps() {
        AssertComponentMode();

        foreach (Room room in Rooms) {
            List<Gap> roomGaps = [];
            Room lastRoom = null!;
            List<Shape> curGapVolumes = [];
            bool isHorizontal = false;
            Direction lastDirection = Direction.None;

            room.Geometry.ExecuteOnPerimeter((x, y, direction) => {
                Point16 pos = new(x, y);
                if (lastDirection == Direction.None) lastDirection = direction;

                Point16 step = direction switch {
                    Direction.Up => new Point16(0, -1),
                    Direction.Down => new Point16(0, 1),
                    Direction.Left => new Point16(-1, 0),
                    Direction.Right => new Point16(1, 0),
                    _ => new Point16(0, 0)
                };

                pos += step;
                while (direction is Direction.Up or Direction.Down ? InFloors(pos) : InWalls(pos))
                    pos += step;

                Room foundRoom = RoomHelper.GetRoomFromPos(Rooms, pos);
                if (foundRoom?.InteriorRank > room.InteriorRank) foundRoom = null; // invalidate casts that find a room more interior than this one
                if (foundRoom == room) foundRoom = null; // invalidate casts that find its own room

                if (foundRoom != null) {
                    if (curGapVolumes.Count == 0) {
                        isHorizontal = direction is Direction.Left or Direction.Right;
                        lastRoom = foundRoom;
                    }
                    else if (direction != lastDirection) {
                        roomGaps.Add(new Gap(new VolumeComponentParams(Structure), Shape.Union(curGapVolumes), room, lastRoom, isHorizontal, isHorizontal ? "Interior_Horizontal_Gap_#" : "Interior_Vertical_Gap_#"));
                        curGapVolumes = [];
                    }

                    curGapVolumes.Add(new Shape(pos - step, new Point16(x, y) + step));
                }
                else {
                    // reset curGapSection
                    if (curGapVolumes.Count != 0) {
                        roomGaps.Add(new Gap(Structure, Shape.Union(curGapVolumes), room, lastRoom, isHorizontal, isHorizontal ? "Interior_Horizontal_Gap_#" : "Interior_Vertical_Gap_#"));
                        curGapVolumes = [];
                    }
                }

                lastDirection = direction;
            });

            room.Gaps = roomGaps;
            Gaps.AddRange(roomGaps);
        }
    }

    // /// <summary>
    // ///     Removes duplicate gaps in-place
    // /// </summary>
    // /// <param name="gaps"></param>
    // private void RemoveDuplicateGaps(List<Gap> gaps) {
    //     for (int testingIndex = gaps.Count - 1; testingIndex >= 0; testingIndex--)
    //     for (int removingIndex = testingIndex - 1; removingIndex >= 0; removingIndex--) {
    //         if (testingIndex >= gaps.Count) break;
    //         Gap gap = gaps[testingIndex];
    //         Gap removingGap = gaps[removingIndex];
    //
    //         if (gap.RepresentsSimilarGap(removingGap)) {
    //             if (removingGap.LowerRoom.Gaps.Remove(removingGap)) removingGap.LowerRoom.Gaps.Add(gap);
    //
    //             if (removingGap.HigherRoom!.Gaps.Remove(removingGap)) removingGap.HigherRoom!.Gaps.Add(gap);
    //
    //             gaps.RemoveAt(removingIndex);
    //         }
    //     }
    // }

    /// <summary>
    ///     Removes impractically small gaps, moves and shrinks large gaps
    /// </summary>
    /// <returns></returns>
    private void ResizeAndMoveGaps() {
        short[] horizontalGapSizes = Gaps.Where(gap => !gap.IsHorizontal).Select(gap => gap.Geometry.BoundingBox.Width).ToArray();
        int maxFloorGapSize = horizontalGapSizes.Length != 0 ? horizontalGapSizes.Max() : 7;

        // filter out gaps which are too small and resize gaps
        for (int gapIndex = Gaps.Count - 1; gapIndex >= 0; gapIndex--) {
            Gap gap = Gaps[gapIndex];
            if (gap.IsHorizontal) {
                if (gap.Geometry.BoundingBox.Height < 3) {
                    Gaps.RemoveAt(gapIndex);
                    continue;
                }

                var points = gap.Geometry.Points;
                for (int pointIndex = 0; pointIndex < gap.Geometry.Points.Length; pointIndex++)
                    // ensure doors aren't too tall
                    if (points[pointIndex].Y < gap.Geometry.BoundingBox.Bottom - 2)
                        points[pointIndex] = new Point16(points[pointIndex].X, gap.Geometry.BoundingBox.Bottom - 2);

                gap.Geometry = new Shape(points);
            }
            else {
                if (gap.Geometry.BoundingBox.Width < 2) {
                    Gaps.RemoveAt(gapIndex);
                    continue;
                }

                int suggestedSize = (int)(gap.Geometry.BoundingBox.Width / (float)maxFloorGapSize * 2 + 2);
                // randomly move the gap, if there's space to do so
                if (suggestedSize < gap.Geometry.BoundingBox.Width) {
                    int gapCenter = (int)(Structure.LayoutRandom.NextDouble() * gap.Geometry.BoundingBox.Width) + gap.Geometry.BoundingBox.Left;
                    int leftX = gapCenter - (int)Math.Floor((double)suggestedSize / 2);
                    int outOfBoundsDistance = Math.Max(0, gap.Geometry.BoundingBox.Left - leftX);
                    leftX = Math.Max(leftX, gap.Geometry.BoundingBox.Left);
                    int rightX = gapCenter + (int)Math.Ceiling((double)suggestedSize / 2) + outOfBoundsDistance;
                    var points = gap.Geometry.Points;
                    for (int pointIndex = 0; pointIndex < gap.Geometry.Points.Length; pointIndex++) {
                        if (points[pointIndex].X < leftX) points[pointIndex] = new Point16(leftX, points[pointIndex].Y);

                        if (points[pointIndex].X > rightX) points[pointIndex] = new Point16(rightX, points[pointIndex].Y);
                    }

                    gap.Geometry = new Shape(points);
                }
            }
        }

        // attempt to chain vertical gaps
        for (int gapIndex = Gaps.Count - 1; gapIndex >= 0; gapIndex--) {
            Gap gap = Gaps[gapIndex];
            if (gap.IsHorizontal) continue;

            // randomly move the gap, but if possible align it with the gap below
            Gap potentialChainGap = Gaps.Find(potentialGap =>
                !potentialGap.IsHorizontal &&
                potentialGap.InteriorRoom == gap.ExteriorRoom &&
                gap.Geometry.BoundingBox.Left <= potentialGap.Geometry.BoundingBox.Left &&
                gap.Geometry.BoundingBox.Right >= potentialGap.Geometry.BoundingBox.Right
            );

            // 60% chance to go for chain gaps
            if (potentialChainGap != null && Structure.LayoutRandom.NextDouble() < 0.6) {
                var points = gap.Geometry.Points;
                for (int pointIndex = 0; pointIndex < gap.Geometry.Points.Length; pointIndex++) {
                    if (points[pointIndex].X < potentialChainGap.Geometry.BoundingBox.Left)
                        points[pointIndex] = new Point16(potentialChainGap.Geometry.BoundingBox.Left, points[pointIndex].Y);

                    if (points[pointIndex].X > potentialChainGap.Geometry.BoundingBox.Right)
                        points[pointIndex] = new Point16(potentialChainGap.Geometry.BoundingBox.Right, points[pointIndex].Y);
                }

                gap.Geometry = new Shape(points);
            }
            else {
                Gaps.RemoveAt(gapIndex);
            }
        }
    }

    private void PruneGaps(EntryPoint[] entryPoints) {
        HashSet<Gap> requiredGaps = [];
        var visitedRooms = Rooms.ToHashSet();
        var visitQueue = new Queue<(Room room, Room parentRoom)>();

        foreach (EntryPoint entryPoint in entryPoints) {
            Room potentialRoom = RoomHelper.GetClosestRoom(this, entryPoint.Center);
            visitQueue.Enqueue((potentialRoom, null));
        }

        while (visitQueue.Count > 0) {
            (Room room, Room parentRoom) = visitQueue.Dequeue();
            if (!visitedRooms.Add(room)) continue;

            foreach (Room connection in room.GetConnections()) visitQueue.Enqueue((connection, room));
        }
    }

    #endregion

    public void ConvertToComponents() {
        if (ComponentMode) throw new Exception("this RoomLayout must not already be in component mode");
        ComponentMode = true;

        Floors = [];
        Walls = [];
        Gaps = [];
        Rooms = [];

        foreach ((Shape volume, string name) in FloorVolumes) Floors.Add(new Floor(new VolumeComponentParams(Structure), volume, name));
        foreach ((Shape volume, string name) in WallVolumes) Walls.Add(new Wall(new VolumeComponentParams(Structure), volume, name));
        foreach ((Shape volume, string name) in RoomVolumes) Rooms.Add(new Room(new VolumeComponentParams(Structure), volume, name, []));

        FloorVolumes = null;
        WallVolumes = null;
        RoomVolumes = null;

        RaycastGaps();
        // RemoveDuplicateGaps(allGaps);
        ResizeAndMoveGaps();
        // PruneGaps(rooms, roomLayoutParams.EntryPoints);

    }
}