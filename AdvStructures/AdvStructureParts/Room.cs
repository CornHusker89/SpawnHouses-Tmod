#nullable enable
using System;
using System.Collections.Generic;

namespace SpawnHouses.AdvStructures.AdvStructureParts;

public class Room {
    public Shape Volume;
    public List<Gap> Gaps;
    public bool IsEntryRoom;
    public Room? ParentRoom;

    /// <summary>If the room has important things inside, alters generation such as gaps</summary>
    public bool HasContents;

    public Room(Shape volume, List<Gap>? gaps = null) {
        Volume = volume;
        Gaps = gaps ?? [];

        IsEntryRoom = true;
        ParentRoom = null;
    }

    public void SetParent(Room parent) {
        IsEntryRoom = false;
        ParentRoom = parent;
    }

    /// <summary>
    ///     Gets the room on the other side of the gap. Gap must be in this room's gaps
    /// </summary>
    /// <param name="gap"></param>
    /// <param name="throwException">if the gap is not found and this is true, the method will throw an error </param>
    /// <returns>The other room, null if it doesn't exist</returns>
    public Room? TraverseGap(Gap gap) {
        if (!Gaps.Contains(gap)) {
            throw new Exception("Gap not found in this room's gaps");
        }

        return this == gap.HigherRoom ? gap.LowerRoom : gap.HigherRoom;
    }

    /// <summary>
    ///     Gets a list of other rooms that are connected by gaps
    /// </summary>
    /// <returns></returns>
    public List<Room> GetConnections() {
        List<Room> connections = [];
        foreach (Gap gap in Gaps) {
            Room? otherRoom = TraverseGap(gap);
            if (otherRoom != null) {
                connections.Add(otherRoom);
            }
        }

        return connections;
    }
}