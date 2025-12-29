#nullable enable
using SpawnHouses.AdvStructures;
using SpawnHouses.AdvStructures.AdvStructureParts;
using Range = SpawnHouses.Structures.Range;

namespace SpawnHouses.Types;

public class RoomLayoutParams {
    public readonly int Attempts;
    public readonly EntryPoint[] EntryPoints;
    public readonly Range FloorWidth;
    public readonly float LargeRoomChance;
    public readonly Range RoomHeight;
    public readonly Range RoomWidth;
    public readonly Range WallWidth;

    public RoomLayoutParams(
        AdvStructure structure,
        EntryPoint[] entryPoints,
        Range roomHeight,
        Range roomWidth,
        Range floorWidth,
        Range wallWidth,
        float largeRoomChance = 0.2f,
        int attempts = 5
    ) {
        Structure = structure;
        Attempts = attempts;
        EntryPoints = entryPoints;
        FloorWidth = floorWidth;
        LargeRoomChance = largeRoomChance;
        RoomHeight = roomHeight;
        RoomWidth = roomWidth;
        WallWidth = wallWidth;
    }

    public AdvStructure Structure { get; init; }

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

public class ComponentParams : Params {
    protected ComponentParams(AdvStructure structure) : base(structure) {
    }
}

public class VolumeComponentParams : ComponentParams {
    public VolumeComponentParams(AdvStructure structure) : base(structure) {
    }
}

public class PathComponentParams : ComponentParams {
    public PathComponentParams(AdvStructure structure) : base(structure) {
    }
}