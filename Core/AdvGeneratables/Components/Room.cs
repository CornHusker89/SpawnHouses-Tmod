#nullable enable
using System;
using System.Collections.Generic;
using SpawnHouses.Core.Geometry;
using SpawnHouses.Core.Parameters;
using SpawnHouses.Core.RootStructureTypes;
using SpawnHouses.Core.Tiles;

namespace SpawnHouses.Core.AdvGeneratables.Components;

public class Room : VolumeComponent {
    /// <summary>ranking of how "interior" this room is. 0 is unassigned and ascends from 1 as the rooms get closer to the middle</summary>
    public ushort InteriorRank;
    
    public List<Gap> Gaps;
    public List<MultiTile> MultiTiles;
    public List<Stairway> Stairways;

    /// <summary>
    ///     constructor that requires params object
    /// </summary>
    /// <param name="p"></param>
    /// <param name="shape"></param>
    /// <param name="name"></param>
    /// <param name="gaps"></param>
    /// <param name="stairways"></param>
    public Room(VolumeComponentParams p, Shape shape, string name, List<Gap>? gaps = null, List<Stairway>? stairways = null) : base(p, shape, name) {
        Gaps = gaps ?? [];
        Stairways = stairways ?? [];
        MultiTiles = [];
    }

    /// <summary>
    ///     constructor that automatically creates a new set of params
    /// </summary>
    /// <param name="structure"></param>
    /// <param name="shape"></param>
    /// <param name="name"></param>
    /// <param name="gaps"></param>
    /// <param name="stairways"></param>
    public Room(AdvStructure structure, Shape shape, string name, List<Gap>? gaps = null, List<Stairway>? stairways = null) : base(new VolumeComponentParams(structure), shape, name) {
        Gaps = gaps ?? [];
        Stairways = stairways ?? [];
        MultiTiles = [];
    }

    /// <summary>
    ///     Gets the room on the other side of the gap. Gap must be in this room's gaps
    /// </summary>
    /// <param name="gap"></param>
    /// <returns>The other room, null if it doesn't exist</returns>
    public Room? TraverseGap(Gap gap) {
        if (!Gaps.Contains(gap)) throw new Exception("Gap not found in this room's gaps");

        return this == gap.InteriorRoom ? gap.ExteriorRoom : gap.InteriorRoom;
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