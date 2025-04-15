using System;
using System.Collections.Generic;
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
        foreach (Shape floorVolume in FloorVolumes)
            if (floorVolume.Contains(point))
                return true;
        return false;
    }

    public bool InWall(Point16 point) {
        foreach (Shape wallVolume in WallVolumes)
            if (wallVolume.Contains(point))
                return true;
        return false;
    }
}

public class RoomLayout(
    List<Shape> floorVolumes,
    List<Gap> floorGaps,
    List<Shape> wallVolumes,
    List<Gap> wallGaps,
    List<Room> rooms
) {
    public readonly List<Gap> FloorGaps = floorGaps;
    public readonly List<Shape> FloorVolumes = floorVolumes;
    public readonly List<Room> Rooms = rooms;
    public readonly List<Gap> WallGaps = wallGaps;
    public readonly List<Shape> WallVolumes = wallVolumes;

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
    List<Shape> floorVolumes,
    List<Gap> floorGaps,
    List<Shape> wallVolumes,
    List<Gap> wallGaps
) {
    public readonly List<Gap> FloorGaps = floorGaps;
    public readonly List<Shape> FloorVolumes = floorVolumes;
    public readonly List<Gap> WallGaps = wallGaps;
    public readonly List<Shape> WallVolumes = wallVolumes;
}