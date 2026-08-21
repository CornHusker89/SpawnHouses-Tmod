using SpawnHouses.Core.Enums;
using Terraria.DataStructures;

namespace SpawnHouses.Core;

public class EntryPoint {
    /// <summary>the upper/left point, not offset</summary>
    private readonly Point16 _baseStart;

    /// <summary>the direction going into the structure. ex. if it's on the left wall, it should be LegacyDirections.Right</summary>
    public readonly Direction EntryDirection;

    public readonly EntryPointPurpose Purpose;

    /// <summary>
    ///     the size of the entry point, in tiles. ex. if it's a 2W x 3H horizontal-passage door, this would be 3
    /// </summary>
    public readonly int Size;

    /// <summary>
    ///     optional offset, applied to <see cref="Start" /> and <see cref="End" />
    ///     used to change position of entry point after creation, typically represents the world position of the parent structure
    /// </summary>
    public Point16 Offset = Point16.Zero;
    
    /// <param name="start"></param>
    /// <param name="size"></param>
    /// <param name="entryDirection">
    ///     the entryDirection going into the structure. ex. if it's on the left wall, it should be
    ///     <see cref="Direction.Right"/>
    /// </param>
    /// <param name="purpose"></param>
    public EntryPoint(Point16 start, int size, Direction entryDirection, EntryPointPurpose purpose) {
        _baseStart = start;
        Size = size;
        EntryDirection = entryDirection;
        Purpose = purpose;
    }

    public bool IsHorizontal => EntryDirection is Direction.Right or Direction.Left;
    
    /// <summary>the upper/left point, with offset applied</summary>
    public Point16 Start => _baseStart + Offset;
    
    /// <summary>the bottom/right point, with offset applied</summary>
    public Point16 End => Start + (IsHorizontal ? new Point16(0, Size - 1) : new Point16(Size - 1, 0));
    
    public Point16 Center => (Start + End) / new Point16(2, 2);

    public Point16 LowerOutside => Point16.NegativeOne;

    public void SetOffset(Point16 offset) {
        Offset += offset;
    }
}