using SpawnHouses.Structures;

namespace SpawnHouses.AdvStructures.AdvStructureParts;

public class EntryPoint {
    public Shape Volume;
    public byte Direction;

    public bool IsHorizontal => Direction is Directions.Right or Directions.Left;

    /// <param name="volume"></param>
    /// <param name="direction">the direction going into the structure. ex. if it's on the left wall, it should be Directions.Right</param>
    public EntryPoint(Shape volume, byte direction) {
        Volume = volume;
        Direction = direction;
    }
}