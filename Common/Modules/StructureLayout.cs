using System.Collections.Generic;
using SpawnHouses.Common.Modules.Components;
using SpawnHouses.Common.Parameters;
using SpawnHouses.Common.Tagging;
using SpawnHouses.Common.Types;
using Terraria.DataStructures;

namespace SpawnHouses.Common.Modules;

public class StructureLayout : Generatable<StructureLayout, StructureLayoutParams, StructureLayoutGenerator> {
    public List<Floor> ExternalFloors { get; private set; }
    public List<Wall> ExternalWalls { get; private set; }
    public List<Gap> ExternalGaps { get; private set; }
    public List<Roof> Roofs { get; private set; }
    public List<RoomLayout> RoomLayouts { get; private set; }

    public List<IComponent> ExternalComponents { get; private set; }

    /// <summary>
    ///     any <see cref="Room" />s are at the very end of the list
    /// </summary>
    public List<IComponent> AllComponents { get; private set; }

    public StructureLayout(StructureLayoutParams param) : base(param, new TagMap()) {
    }

    public Room[] Rooms {
        get {
            int len = 0;
            foreach (RoomLayout roomLayout in RoomLayouts) len += roomLayout.Rooms.Count;
            var rooms = new Room[len];
            int count = 0;
            foreach (RoomLayout roomLayout in RoomLayouts) {
                foreach (Room room in roomLayout.Rooms) {
                    rooms[count] = room;
                    count++;
                }
            }

            return rooms;
        }
    }

    /// <summary>
    ///     sets the components of this structure layout, and calls <see cref="UpdateComponentList" />
    /// </summary>
    /// <param name="externalFloors"></param>
    /// <param name="externalWalls"></param>
    /// <param name="externalGaps"></param>
    /// <param name="roofs"></param>
    /// <param name="roomLayouts"></param>
    public void SetComponents(List<Floor> externalFloors, List<Wall> externalWalls, List<Gap> externalGaps, List<Roof> roofs, List<RoomLayout> roomLayouts) {
        ExternalFloors = externalFloors;
        ExternalWalls = externalWalls;
        ExternalGaps = externalGaps;
        Roofs = roofs;
        RoomLayouts = roomLayouts;
        ExternalComponents = [];
        AllComponents = [];
        
        UpdateComponentList();
    }

    /// <summary>
    ///     resets <see cref="AllComponents" /> and rebuilds the list using the current lists of components
    /// </summary>
    public void UpdateComponentList() {
        ExternalComponents.Clear();
        AllComponents.Clear();

        ExternalComponents.AddRange(ExternalFloors);
        ExternalComponents.AddRange(ExternalWalls);
        ExternalComponents.AddRange(ExternalGaps);
        ExternalComponents.AddRange(Roofs);

        AllComponents.AddRange(ExternalComponents);
        foreach (RoomLayout roomLayout in RoomLayouts) {
            AllComponents.AddRange(roomLayout.Floors);
            AllComponents.AddRange(roomLayout.Walls);
            AllComponents.AddRange(roomLayout.Gaps);
        }

        // put rooms at the very end of the list
        foreach (RoomLayout roomLayout in RoomLayouts) {
            AllComponents.AddRange(roomLayout.Rooms);
        }
    }

    /// <summary>
    ///     moves everything in the layout by the offset. ex. if offset = (3, 0) will move everything in the layout 3 to the right in world coordinates
    /// </summary>
    /// <param name="offset"></param>
    public void Offset(Point16 offset) {
        foreach (Floor floor in ExternalFloors) floor.Geometry.Move(offset);
        foreach (Wall wall in ExternalWalls) wall.Geometry.Move(offset);
        foreach (Gap gap in ExternalGaps) gap.Geometry.Move(offset);
        foreach (Roof roof in Roofs) roof.Geometry.Move(offset);
        foreach (RoomLayout roomSection in RoomLayouts) roomSection.Offset(offset);
    }
}