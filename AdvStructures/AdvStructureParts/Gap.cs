#nullable enable
namespace SpawnHouses.AdvStructures.AdvStructureParts;

public class Gap {
    /// <summary>This will be null if the gap leads to an exterior</summary>
    public Room? HigherRoom;

    public bool IsChain;

    public bool IsExterior;

    /// <summary>If the gap has rooms on it's left/right</summary>
    public bool IsHorizontal;

    public Room? LowerRoom;

    /// <summary>If one room is the parent of the other, this is the parent room</summary>
    public Room? ParentRoom;

    /// <summary>
    ///     If the gap is vertical and is horizontally aligned with a gap, this is the next gap with a higher y in the
    ///     chain
    /// </summary>
    public Gap? VerticalChainHigher;

    /// <summary>
    ///     If the gap is vertical and is horizontally aligned with a gap, this is the next gap with a lower y in the
    ///     chain
    /// </summary>
    public Gap? VerticalChainLower;

    public Shape Volume;

    /// <summary>
    ///     higher/lower room is determined by the sum of the center coordinates for both room
    /// </summary>
    /// <param name="volume"></param>
    /// <param name="room1"></param>
    /// <param name="room2"></param>
    /// <param name="isHorizontal">Has rooms on it's left/right</param>
    public Gap(Shape volume, Room? room1, Room? room2, bool isHorizontal) {
        Volume = volume;
        IsHorizontal = isHorizontal;
        IsExterior = room2 == null;

        if (IsExterior) {
            LowerRoom = room1;
        }
        else {
            if (room1?.Volume.Center.X + room1?.Volume.Center.Y >= room2?.Volume.Center.X + room2?.Volume.Center.Y) {
                LowerRoom = room2!;
                HigherRoom = room1;
            }
            else {
                LowerRoom = room1;
                HigherRoom = room2;
            }
        }

        if (room1?.ParentRoom != null)
            ParentRoom = room1.ParentRoom;
        else if (room2?.ParentRoom != null) ParentRoom = room2.ParentRoom;
    }

    /// <summary>
    ///     tests if the rooms and direction are the same, and the volumes collide
    /// </summary>
    public bool RepresentsSimilarGap(Gap other) {
        if (LowerRoom != other.LowerRoom || HigherRoom != other.HigherRoom || IsHorizontal != other.IsHorizontal)
            return false;
        return Volume.HasIntersection(other.Volume);
    }
}