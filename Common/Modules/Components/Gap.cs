#nullable enable
using SpawnHouses.Common.Parameters;
using SpawnHouses.Common.Types.Geometry;

namespace SpawnHouses.Common.Modules.Components;

public class Gap : VolumeComponent {
    /// <summary>If the gap has rooms on it's left/right</summary>
    public bool IsHorizontal;

    /// <summary>the room which is considered more interior (has a higher <see cref="Room.InteriorRank" />)</summary>
    public Room InteriorRoom;

    /// <summary>the room which is considered more exterior (has a lower <see cref="Room.InteriorRank" />). will be null if the gap is external</summary>
    public Room? ExteriorRoom;

    /// <summary>
    ///     constructor that requires params object
    /// </summary>
    /// <param name="p"></param>
    /// <param name="shape"></param>
    /// <param name="room1"></param>
    /// <param name="room2"></param>
    /// <param name="isHorizontal">Has rooms on it's left/right</param>
    public Gap(VolumeComponentParams p, Shape shape, Room room1, Room? room2, bool isHorizontal) : base(p, shape) {
        IsHorizontal = isHorizontal;

        bool room1IsInterior = room2 == null || room1.InteriorRank > room2.InteriorRank;
        InteriorRoom = room1IsInterior ? room1 : room2!;
        if (room2 != null)
            ExteriorRoom = room1IsInterior ? room2 : room1;
    }
    
    /// <summary>
    ///     constructor that automatically creates a new set of params
    /// </summary>
    /// <param name="structure"></param>
    /// <param name="shape"></param>
    /// <param name="room1"></param>
    /// <param name="room2"></param>
    /// <param name="isHorizontal"></param>
    public Gap(AdvStructure structure, Shape shape, Room room1, Room? room2, bool isHorizontal) : base(new VolumeComponentParams(structure), shape) {
        IsHorizontal = isHorizontal;

        bool room1IsInterior = room2 == null || room1.InteriorRank > room2.InteriorRank;
        InteriorRoom = room1IsInterior ? room1 : room2!;
        if (room2 != null)
            ExteriorRoom = room1IsInterior ? room2 : room1;
    }

    // /// <summary>
    // ///     tests if the rooms and direction are the same, and the volumes collide
    // /// </summary>
    // public bool RepresentsSimilarGap(Gap other) {
    //     if (LowerRoom != other.LowerRoom || HigherRoom != other.HigherRoom || IsHorizontal != other.IsHorizontal)
    //         return false;
    //     return Volume.HasIntersection(other.Volume);
    // }
}