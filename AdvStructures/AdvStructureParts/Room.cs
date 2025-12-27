#nullable enable
using System;
using System.Collections.Generic;

namespace SpawnHouses.AdvStructures.AdvStructureParts;

public class Room : VolumeComponent {
    public List<Gap> Gaps;
    public bool IsEntryRoom;
    public List<MultiTile> MultiTiles;
    public Room? ParentRoom;
    public List<Stairway> Stairways;

    public Room(Shape volume, List<Gap>? gaps = null, List<Stairway>? stairways = null) {
        Volume = volume;

        Gaps = gaps ?? [];
        Stairways = stairways ?? [];
        MultiTiles = [];
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
    /// <returns>The other room, null if it doesn't exist</returns>
    public Room? TraverseGap(Gap gap) {
        if (!Gaps.Contains(gap)) throw new Exception("Gap not found in this room's gaps");

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
            if (otherRoom != null) connections.Add(otherRoom);
        }

        return connections;
    }
}