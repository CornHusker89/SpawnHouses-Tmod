using System.Collections.Generic;
using SpawnHouses.AdvStructures.AdvStructureParts;

namespace SpawnHouses.Types;

public class RoomLayoutVolumes
(
    List<Shape> floorVolumes,
    List<Shape> wallVolumes,
    List<Shape> roomVolumes
)
{
    public readonly List<Shape> FloorVolumes = floorVolumes;
    public readonly List<Shape> WallVolumes = wallVolumes;
    public readonly List<Shape> RoomVolumes = roomVolumes;
}

public class RoomLayout(
    List<Shape> floorVolumes,
    List<Shape> wallVolumes,
    List<Room> rooms,
    List<Gap> gaps
)
{
    public readonly List<Shape> FloorVolumes = floorVolumes;
    public readonly List<Shape> WallVolumes = wallVolumes;
    public readonly List<Room> Rooms = rooms;
    public readonly List<Gap> Gaps = gaps;
}