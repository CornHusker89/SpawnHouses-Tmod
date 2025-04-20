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
    List<Gap> floorGaps,
    List<Wall> walls,
    List<Gap> wallGaps,
    List<Room> rooms
) {
    public readonly List<Gap> FloorGaps = floorGaps;
    public readonly List<Floor> Floors = floors;
    public readonly List<Gap> WallGaps = wallGaps;
    public readonly List<Wall> Walls = walls;
    public readonly List<Room> Rooms = rooms;

    /// <summary>
    ///     gets closest room to the point
    /// </summary>
    /// <param name="roomLayout"></param>
    /// <param name="point"></param>
    /// <returns></returns>
    public Room GetClosestRoom(Point16 point) {
        if (Rooms.Count == 0)
            throw new Exception("BackgroundVolumes in given room layout was empty");

        int closestIndex = -1;
        double closestDistance = double.MaxValue;

        for (int i = 0; i < Rooms.Count; i++) {
            double closestDistanceInShape = double.MaxValue;
            Rooms[i].Volume.ExecuteOnPerimeter((x, y, _) => {
                double distance = Math.Sqrt(Math.Pow(point.X - x, 2) + Math.Pow(point.Y - y, 2));
                if (distance < closestDistanceInShape)
                    closestDistanceInShape = distance;
            });

            if (closestDistanceInShape < closestDistance) {
                closestDistance = closestDistanceInShape;
                closestIndex = i;
            }
        }

        return Rooms[closestIndex];
    }
}

public class ExternalLayout(
    List<Floor> floors,
    List<Gap> floorGaps,
    List<Wall> walls,
    List<Gap> wallGaps
) {
    public readonly List<Gap> FloorGaps = floorGaps;
    public readonly List<Floor> Floors = floors;
    public readonly List<Gap> WallGaps = wallGaps;
    public readonly List<Wall> Walls = walls;
}