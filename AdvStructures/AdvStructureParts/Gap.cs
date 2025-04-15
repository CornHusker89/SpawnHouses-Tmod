#nullable enable
namespace SpawnHouses.AdvStructures.AdvStructureParts;

public class Gap {
    /// <summary>This will be null if the gap leads to an exterior</summary>
    public Room? HigherRoom;

    public bool IsExterior;
    public bool IsHorizontal;
    public Room LowerRoom;
    public Shape Volume;

    /// <summary>
    ///     higher/lower room is determined by the sum of the center coordinates for both room
    /// </summary>
    /// <param name="volume"></param>
    /// <param name="room1"></param>
    /// <param name="room2"></param>
    /// <param name="isHorizontal"></param>
    public Gap(Shape volume, Room room1, Room? room2, bool isHorizontal) {
        Volume = volume;
        IsHorizontal = isHorizontal;
        IsExterior = room2 == null;

        if (IsExterior) {
            LowerRoom = room1;
        }
        else {
            if (room1.Volume.Center.X + room1.Volume.Center.Y >= room2?.Volume.Center.X + room2?.Volume.Center.Y) {
                LowerRoom = room2!;
                HigherRoom = room1;
            }
            else {
                LowerRoom = room1;
                HigherRoom = room2;
            }
        }
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