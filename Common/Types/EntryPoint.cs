using SpawnHouses.Legacy.Structures;
using Terraria.DataStructures;

namespace SpawnHouses.Common.Types;

public class EntryPoint {
    /// <summary>the upper/left point, not offset</summary>
    private readonly Point16 _baseStart;
    
    /// <summary>the direction going into the structure. ex. if it's on the left wall, it should be Directions.Right</summary>
    public readonly byte Direction;
    
    public readonly int Size;
    
    /// <summary>optional offset, applied to <see cref="Start" /> and <see cref="End" /></summary>
    public Point16 Offset = Point16.Zero;
    
    /// <param name="start"></param>
    /// <param name="size"></param>
    /// <param name="direction">
    ///     the direction going into the structure. ex. if it's on the left wall, it should be
    ///     Directions.Right
    /// </param>
    public EntryPoint(Point16 start, int size, byte direction) {
        _baseStart = start;
        Size = size;
        Direction = direction;
    }

    public bool IsHorizontal => Direction is LegacyDirections.Right or LegacyDirections.Left;
    
    /// <summary>the upper/left point, with offset applied</summary>
    public Point16 Start => _baseStart + Offset;
    
    /// <summary>the bottom/right point, with offset applied</summary>
    public Point16 End => Start + (IsHorizontal ? new Point16(0, Size - 1) : new Point16(Size - 1, 0));
    
    public Point16 Center => (Start + End) / new Point16(2, 2);
    
    public void SetOffset(Point16 offset) {
        Offset += offset;
    }

    // /// <summary>
    // ///     the bottom/right point of this entry point, depending on the direction. 
    // /// </summary>
    // public Point BottomRight;
    //
    // /// <summary>
    // ///     the direction of the entry point, facing into the structure
    // /// </summary>
    // public readonly byte Direction;
    //
    // public EntryPoint(Point bottomRight, byte direction) {
    //     BottomRight = bottomRight;
    //     Direction = direction;
    // }
    //
    // public bool IsHorizontal => Direction is Directions.Left or Directions.Right;
}