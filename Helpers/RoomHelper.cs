#nullable enable
using System;
using System.Collections.Generic;
using SpawnHouses.Common.Modules.Components;
using SpawnHouses.Common.Types;
using SpawnHouses.Common.Types.Geometry;
using Terraria.DataStructures;
using Gap = SpawnHouses.Common.Modules.Components.Gap;

namespace SpawnHouses.Helpers;

public static class RoomHelper {
    /// <summary>
    ///     gets closest room to the point using the perimeter of each room
    /// </summary>
    /// <param name="rooms"></param>
    /// <param name="point"></param>
    /// <returns></returns>
    /// <exception cref="Exception"></exception>
    public static Room GetClosestRoom(List<Room> rooms, Point16 point) {
        Room? closestRoom = null;
        double closestDistance = double.MaxValue;
        foreach (Room room in rooms) {
            closestRoom ??= room;

            double closestDistanceInShape = double.MaxValue;
            room.Geometry.ExecuteOnPerimeter((x, y, _) => {
                double distance = Math.Sqrt(Math.Pow(point.X - x, 2) + Math.Pow(point.Y - y, 2));
                if (distance < closestDistanceInShape)
                    closestDistanceInShape = distance;
            });

            if (closestDistanceInShape < closestDistance) {
                closestDistance = closestDistanceInShape;
                closestRoom = room;
            }
        }

        if (closestRoom == null)
            throw new Exception("given RoomLayouts had no rooms");
        return closestRoom;
    }

    /// <summary>
    ///     gets closest room to the point using the perimeter of each room
    /// </summary>
    /// <param name="roomLayout"></param>
    /// <param name="point"></param>
    /// <returns></returns>
    /// <exception cref="Exception"></exception>
    public static Room GetClosestRoom(RoomLayout roomLayout, Point16 point) => GetClosestRoom(roomLayout.Rooms, point);
    

    
    public static bool IsValidHousingSize(Shape volume) => volume.GetExpandedShape(1).GetArea() >= 60;

    /// <summary>
    ///     returns list of gaps directly touching the given volume
    /// </summary>
    /// <param name="volume"></param>
    /// <param name="gaps">list of all possible gaps</param>
    /// <returns></returns>
    public static List<Gap> GetAdjacentGaps(Shape volume, List<Gap> gaps) {
        Shape expandedVolume = volume.GetExpandedShape(1);
        return gaps.FindAll(gap => expandedVolume.HasIntersection(gap.Geometry));
    }

    /// <returns>null if no room found</returns>
    public static Room? GetRoomFromPos(List<Room> rooms, Point16 point) {
        foreach (Room room in rooms)
            if (room.Geometry.Contains(point))
                return room;

        return null;
    }
}