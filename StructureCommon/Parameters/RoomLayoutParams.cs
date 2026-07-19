#nullable enable
using SpawnHouses.StructureCommon.Tagging;
using SpawnHouses.StructureCommon.Types.DataStructures;
using SpawnHouses.StructureCommon.Types.Geometry;
using SpawnHouses.StructureCommon.Types.RootStructureTypes;

namespace SpawnHouses.StructureCommon.Parameters;

public class RoomLayoutParams : IParams {
    public AdvStructure Structure { get; set; }
    public TagMap TagsRequired { get; init; }

    public readonly NumRange FloorWidth;
    public readonly NumRange WallWidth;
    public readonly NumRange RoomHeight;
    public readonly NumRange RoomWidth;
    public readonly int Attempts;

    public RoomLayoutParams(AdvStructure structure, NumRange floorWidth,
        NumRange wallWidth, NumRange roomHeight, NumRange roomWidth, TagMap? tagsRequired = null, float largeRoomChance = 0.2f, int attempts = 5) {
        Structure = structure;
        FloorWidth = floorWidth;
        WallWidth = wallWidth;
        RoomHeight = roomHeight;
        RoomWidth = roomWidth;
        TagsRequired = tagsRequired ?? new TagMap();
        Attempts = attempts;
    }
    
    /// <summary>
    ///     true if volume's dimensions are not smaller than min sizes
    /// </summary>
    /// <param name="volume"></param>
    /// <returns></returns>
    public bool IsWithinMinSize(Shape volume) => volume.BoundingBox.Width >= RoomWidth.Min && volume.BoundingBox.Height >= RoomHeight.Min;

    /// <summary>
    ///     true if volume's dimensions are not larger than max sizes
    /// </summary>
    /// <param name="volume"></param>
    /// <returns></returns>
    public bool IsWithinMaxSize(Shape volume) => volume.BoundingBox.Width <= RoomWidth.Max && volume.BoundingBox.Height <= RoomHeight.Max;
}