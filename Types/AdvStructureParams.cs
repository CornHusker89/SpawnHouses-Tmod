#nullable enable
using System;
using System.Linq;
using SpawnHouses.AdvStructures;
using SpawnHouses.AdvStructures.AdvStructureParts;
using SpawnHouses.Types.Palette;
using SpawnHouses.Types.TagTypes;
using Terraria.DataStructures;
using Range = SpawnHouses.Structures.Range;

namespace SpawnHouses.Types;

public abstract class Params {
    public required AdvStructure Structure { get; init; }

    protected Params(AdvStructure structure) {
        Structure = structure;
    }
}

public class StructureParams : Params {
    public readonly TagMap TagsRequired;
    public readonly EntryPoint[] EntryPoints;
    public readonly int Size;
    public readonly bool CanAddEntryPoints;

    public StructureParams(TilePalette palette, TagMap tagsRequired, EntryPoint[] entryPoints, int size, bool canAddEntryPoints, int seed = -1, bool generate = true) : base(new AdvStructure(this, palette, seed, generate)) {
        TagsRequired = tagsRequired;
        EntryPoints = entryPoints;
        Size = size;
        CanAddEntryPoints = canAddEntryPoints;

        if (EntryPoints.Select(entryPoint => entryPoint.Start.Y).Max() - EntryPoints.Select(entryPoint => entryPoint.Start.Y).Min() + 4 > Size / Length)
            throw new ArgumentException($"Entry points are too far away vertically for a minimum height of {Size / Length} (determined by min volume / length)");

        if (Height <= 4)
            throw new ArgumentException($"Volume ({Size}) is too small compared to the length ({Length}) of the structure, resulting in a too-low total height of {Height}");

        bool hasHousing = TagsRequired.GetValueSafe(Tags.HasHousing, out int housingCount);

        if (hasHousing) {
            bool hasRooms = TagsRequired.GetValueSafe(Tags.HasRooms, out int roomCount);
            if (!hasRooms) throw new ArgumentException("Must have rooms tag to have housing");

            if (Size / housingCount < 60)
                throw new ArgumentException($"Volume minimum of {Size} is too small given the housing count minimum of {housingCount}");
            if (Size / housingCount < 60)
                throw new ArgumentException($"Volume maximum of {Size} is too small given the housing count maximum of {housingCount}");
            if (housingCount < 1)
                throw new ArgumentException("housing must be greater than 0");
            if (roomCount > housingCount) throw new ArgumentException($"Room count ({roomCount}) must be greater than or equal to housing count ({housingCount})");
        }
    }

    private int CenterYMin => EntryPoints.Min(entryPoint => entryPoint.Start.Y);
    private int CenterYMax => EntryPoints.Max(entryPoint => entryPoint.End.Y);

    public int LeftEntryPointX => EntryPoints.Min(entryPoint => entryPoint.Start.X);
    public int RightEntryPointX => EntryPoints.Max(entryPoint => entryPoint.End.X);

    public int Length => RightEntryPointX - LeftEntryPointX;
    public int Height => Size / Length;
    
    /// <summary>calculated using entry points</summary>
    public Point16 Center => new(LeftEntryPointX + (LeftEntryPointX + RightEntryPointX) / 2, CenterYMin + (CenterYMin + CenterYMax) / 2);
}

public class RoomLayoutParams : Params {
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
    ) : base(structure) {
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