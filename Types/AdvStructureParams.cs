using System;
using System.Linq;
using SpawnHouses.AdvStructures.AdvStructureParts;
using SpawnHouses.Structures;
using Terraria.DataStructures;
using Range = SpawnHouses.Structures.Range;

namespace SpawnHouses.Types;

public class StructureParams {
    public bool CanAddEntryPoints;
    public int EndEntryPointX;
    public EntryPoint[] EntryPoints;
    public int Height;

    public int Housing;
    public Range HousingRange;

    public int Length;
    public TilePalette Palette;

    public int StartEntryPointX;
    public StructureTag[] TagBlacklist;
    public StructureTag[] TagsRequired;
    public int Volume;
    public Range VolumeRange;

    public StructureParams(
        StructureTag[] tagsRequired,
        StructureTag[] tagBlacklist,
        EntryPoint[] entryPoints,
        TilePalette tilePalette,
        Range volumeRange,
        Range housingRange,
        bool canAddEntryPoints) {
        TagsRequired = tagsRequired;
        TagBlacklist = tagBlacklist;
        EntryPoints = entryPoints;
        StartEntryPointX = EntryPoints.Select(entryPoint => entryPoint.Start.X).Min();
        EndEntryPointX = EntryPoints.Select(entryPoint => entryPoint.Direction is Directions.Left or Directions.Right ? entryPoint.Start.X : entryPoint.Start.X + entryPoint.Size).Max();
        Length = EndEntryPointX - StartEntryPointX;
        Palette = tilePalette;
        VolumeRange = volumeRange;
        HousingRange = housingRange;
        CanAddEntryPoints = canAddEntryPoints;

        int centerXMin = EntryPoints.Min(entryPoint => entryPoint.Start.X);
        int centerXMax = EntryPoints.Max(entryPoint => entryPoint.End.X);
        int centerYMin = EntryPoints.Min(entryPoint => entryPoint.Start.Y);
        int centerYMax = EntryPoints.Max(entryPoint => entryPoint.End.Y);
        Center = new Point16(centerXMin + (centerXMin + centerXMax) / 2, centerXMin + (centerYMin + centerYMax) / 2);

        if (VolumeRange.Min / HousingRange.Min < 60)
            throw new ArgumentException($"Volume minimum of {VolumeRange.Min} is too small given the housing minimum of {HousingRange.Min}");
        if (VolumeRange.Max / HousingRange.Max < 60)
            throw new ArgumentException($"Volume maximum of {VolumeRange.Max} is too small given the housing maximum of {HousingRange.Max}");

        ReRollRanges();
    }

    /// <summary>calculated using the entry points</summary>
    public Point16 Center { get; private set; }

    public Point16 TopLeft => new(StartEntryPointX, EntryPoints.Select(entryPoint => entryPoint.End.Y).Max() - Height);

    public void ReRollRanges() {
        double scale = Terraria.WorldGen.genRand.NextDouble();
        Volume = (int)(VolumeRange.Min + (VolumeRange.Max - VolumeRange.Min) * scale);
        Height = Volume / Length;
        if (Height <= 4) throw new ArgumentException($"Volume ({Volume}) is too small compared to the length ({Length}) of the structure, resulting in an unacceptable total height of {Height}");

        if (HousingRange.Min < 0) throw new ArgumentException("Min housing cannot be less than 0");

        if (HousingRange.Max < 0) throw new ArgumentException("Max housing cannot be less than 0");

        if (HousingRange.Max < HousingRange.Min) throw new ArgumentException("Max Housing is less than min housing");

        if (HousingRange.Max == 0 && TagBlacklist.Contains(StructureTag.HasHousing)) throw new ArgumentException("Adv structure cannot have a max housing of 0 while blacklisting components with housing");

        Housing = (int)(HousingRange.Min + (HousingRange.Max - HousingRange.Min) * scale);
    }
}

public class RoomLayoutParams(
    RoomLayoutTag[] tagsRequired,
    RoomLayoutTag[] tagsBlacklist,
    Shape mainVolume,
    EntryPoint[] entryPoints,
    TilePalette tilePalette,
    int housing,
    Range roomHeight,
    Range roomWidth,
    Range floorWidth,
    Range wallWidth,
    float largeRoomChance = 0.2f,
    int attempts = 5
) {
    public int Attempts = attempts;
    public EntryPoint[] EntryPoints = entryPoints;
    public Range FloorWidth = floorWidth;
    public int Housing = housing;
    public float LargeRoomChance = largeRoomChance;
    public Shape MainVolume = mainVolume;
    public Range RoomHeight = roomHeight;
    public Range RoomWidth = roomWidth;
    public RoomLayoutTag[] TagsBlacklist = tagsBlacklist;
    public RoomLayoutTag[] TagsRequired = tagsRequired;
    public TilePalette TilePalette = tilePalette;
    public Range WallWidth = wallWidth;

    /// <summary>
    ///     true if volume's dimensions are not smaller than min sizes
    /// </summary>
    /// <param name="volume"></param>
    /// <param name="roomLayoutParams"></param>
    /// <returns></returns>
    public bool IsWithinMinSize(Shape volume) {
        return volume.Size.X >= RoomWidth.Min && volume.Size.Y >= RoomHeight.Min;
    }

    /// <summary>
    ///     true if volume's dimensions are not larger than max sizes
    /// </summary>
    /// <param name="volume"></param>
    /// <returns></returns>
    public bool IsWithinMaxSize(Shape volume) {
        return volume.Size.X <= RoomWidth.Max && volume.Size.Y <= RoomHeight.Max;
    }
}

public class ComponentParams(
    ComponentTag[] tagsRequired,
    ComponentTag[] tagsBlacklist,
    Shape volume,
    TilePalette tilePalette,
    StructureTilemap tilemap) {
    public readonly ComponentTag[] TagsBlacklist = tagsBlacklist;
    public readonly ComponentTag[] TagsRequired = tagsRequired;
    public readonly StructureTilemap Tilemap = tilemap;
    public readonly TilePalette TilePalette = tilePalette;
    public Shape Volume = volume;
}