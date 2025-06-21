using SpawnHouses.Structures;
using Terraria.DataStructures;

namespace SpawnHouses.AdvStructures.AdvStructureParts;

public class EntryPoint {
    /// <summary>the upper left point</summary>
    public Point16 Start;
    public int Size;
    public byte Direction;

    public bool IsHorizontal => Direction is Directions.Right or Directions.Left;

    /// <summary>the lower right point</summary>
    public Point16 End => Start + (IsHorizontal ? new Point16(0, Size) : new Point16(Size, 0));

    public Point16 Middle => (Start + End) / new Point16(2, 2);

    /// <param name="start"></param>
    /// <param name="size"></param>
    /// <param name="direction">the direction going into the structure. ex. if it's on the left wall, it should be Directions.Right</param>
    public EntryPoint(Point16 start, int size, byte direction) {
        Start = start;
        Size = size;
        Direction = direction;
    }
}