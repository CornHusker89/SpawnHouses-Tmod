using System.Collections.Generic;
using SpawnHouses.AdvStructures.AdvStructureParts;
using SpawnHouses.Common.Modules.Components;
using SpawnHouses.Common.Parameters;
using SpawnHouses.Common.Types;
using SpawnHouses.Types.TagTypes;
using Terraria.DataStructures;

namespace SpawnHouses.Common.Modules;

public class RoomLayout : Generatable<RoomLayoutParams, RoomLayoutGenerator> {
    public List<Floor> Floors { get; private set; }
    public List<Wall> Walls { get; private set; }
    public List<Gap> Gaps { get; private set; }
    public List<Room> Rooms { get; private set; }

    public RoomLayout(RoomLayoutParams param) : base(param, new TagMap()) {
    }

    public void SetComponents(List<Floor> floors, List<Wall> walls, List<Gap> gaps, List<Room> rooms) {
        Floors = floors;
        Walls = walls;
        Gaps = gaps;
        Rooms = rooms;
    }

    public void Combine(params RoomLayout[] roomLayouts) {
        foreach (RoomLayout roomLayout in roomLayouts) {
            Floors.AddRange(roomLayout.Floors);
            Walls.AddRange(roomLayout.Walls);
            Gaps.AddRange(roomLayout.Gaps);
            Rooms.AddRange(roomLayout.Rooms);
        }
    }

    public void Offset(Point16 offset) {
        foreach (Floor floor in Floors) floor.Geometry.Offset(offset);
        foreach (Wall wall in Walls) wall.Geometry.Offset(offset);
        foreach (Gap gap in Gaps) gap.Geometry.Offset(offset);
        foreach (Room room in Rooms) room.Geometry.Offset(offset);
    }
}