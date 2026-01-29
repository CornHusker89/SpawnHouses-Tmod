#nullable enable
using SpawnHouses.Common.Tagging;
using SpawnHouses.Common.Types.Geometry;
using Range = SpawnHouses.Structures.Range;

namespace SpawnHouses.Common.Parameters;

public class RoomLayoutParams : IParams {
    public AdvStructure Structure { get; set; }
    public TagMap TagsRequired { get; init; }
    
    public readonly Range FloorWidth;
    public readonly Range WallWidth;
    public readonly Range RoomHeight;
    public readonly Range RoomWidth;
    public readonly float LargeRoomChance;
    public readonly int Attempts;

    public RoomLayoutParams(AdvStructure structure, Range floorWidth,
        Range wallWidth, Range roomHeight, Range roomWidth, TagMap? tagsRequired = null, float largeRoomChance = 0.2f, int attempts = 5) {
        Structure = structure;
        FloorWidth = floorWidth;
        WallWidth = wallWidth;
        RoomHeight = roomHeight;
        RoomWidth = roomWidth;
        TagsRequired = tagsRequired ?? new TagMap();
        LargeRoomChance = largeRoomChance;
        Attempts = attempts;
    }
    
    /// <summary>
    ///     true if volume's dimensions are not smaller than min sizes
    /// </summary>
    /// <param name="volume"></param>
    /// <returns></returns>
    public bool IsWithinMinSize(Shape volume) => volume.Size.X >= RoomWidth.Min && volume.Size.Y >= RoomHeight.Min;

    /// <summary>
    ///     true if volume's dimensions are not larger than max sizes
    /// </summary>
    /// <param name="volume"></param>
    /// <returns></returns>
    public bool IsWithinMaxSize(Shape volume) => volume.Size.X <= RoomWidth.Max && volume.Size.Y <= RoomHeight.Max;
}