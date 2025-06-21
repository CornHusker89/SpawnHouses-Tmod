using System;
using System.Collections.Generic;
using System.Linq;
using SpawnHouses.AdvStructures.AdvStructureParts;
using Terraria.DataStructures;

namespace SpawnHouses.Types;

public class RoomLayoutVolumes(
    List<Shape> floorVolumes,
    List<Shape> wallVolumes,
    List<Shape> roomVolumes
) {
    public readonly List<Shape> FloorVolumes = floorVolumes;
    public readonly List<Shape> RoomVolumes = roomVolumes;
    public readonly List<Shape> WallVolumes = wallVolumes;

    public bool InFloor(Point16 point) {
        return FloorVolumes.Any(floorVolume => floorVolume.Contains(point));
    }

    public bool InWall(Point16 point) {
        return WallVolumes.Any(wallVolume => wallVolume.Contains(point));
    }

    public bool InRoom(Point16 point) {
        return RoomVolumes.Any(roomVolume => roomVolume.Contains(point));
    }

    /// <summary>
    ///     checks if the point is contained within any volume
    /// </summary>
    /// <param name="point"></param>
    /// <returns></returns>
    public bool InStructure(Point16 point) {
        return FloorVolumes.Any(floorVolume => floorVolume.Contains(point)) || WallVolumes.Any(floorVolume => floorVolume.Contains(point)) || RoomVolumes.Any(floorVolume => floorVolume.Contains(point));
    }
}

public class RoomLayout(
    List<Floor> floors,
    List<Wall> walls,
    List<Gap> gaps,
    List<Room> rooms
) {
    public readonly List<Floor> Floors = floors;
    public readonly List<Wall> Walls = walls;
    public readonly List<Gap> Gaps = gaps;
    public readonly List<Room> Rooms = rooms;

    public static RoomLayout Union(params RoomLayout[] roomLayouts) {
        List<Floor> floors = [];
        List<Wall> walls = [];
        List<Gap> gaps = [];
        List<Room> rooms = [];

        foreach (RoomLayout roomLayout in roomLayouts) {
            floors.AddRange(roomLayout.Floors);
            walls.AddRange(roomLayout.Walls);
            gaps.AddRange(roomLayout.Gaps);
            rooms.AddRange(roomLayout.Rooms);
        }

        return new RoomLayout(floors, walls, gaps, rooms);
    }
}

public class ExternalLayout(
    List<Floor> floors,
    List<Wall> walls,
    List<Gap> gaps
) {
    public readonly List<Floor> Floors = floors;
    public readonly List<Gap> Gaps = gaps;
    public readonly List<Wall> Walls = walls;
}