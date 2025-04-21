using System;
using System.Linq;
using SpawnHouses.AdvStructures;
using SpawnHouses.AdvStructures.AdvStructureParts;
using Terraria.DataStructures;
using Range = SpawnHouses.Structures.Range;

namespace SpawnHouses.Types;

public class StructureParams {
    public StructureTag[] TagBlacklist;
    public StructureTag[] TagsRequired;
    public Point16 Start;
    public Point16 End;
    public EntryPoint[] EntryPoints;
    public TilePalette Palette;
    public Range VolumeRange;
    public Range HousingRange;

    public int Length;

    public int Housing;
    public int Height;
    public int Volume;

    public StructureParams(
        StructureTag[] tagsRequired,
        StructureTag[] tagBlacklist,
        Point16 start,
        Point16 end,
        EntryPoint[] entryPoints,
        TilePalette tilePalette,
        Range volumeRange,
        Range housingRange) {
        TagsRequired = tagsRequired;
        TagBlacklist = tagBlacklist;
        Start = start;
        End = end;
        if (Start.X > End.X) {
            (Start, End) = (End, Start);
        }
        Length = End.X - Start.X;
        EntryPoints = entryPoints;
        Palette = tilePalette;
        VolumeRange = volumeRange;
        HousingRange = housingRange;

        if (VolumeRange.Min / HousingRange.Min < 60)
            throw new ArgumentException($"Volume minimum of {VolumeRange.Min} is too small given the housing minimum of {HousingRange.Min}");
        if (VolumeRange.Max / HousingRange.Max < 60)
            throw new ArgumentException($"Volume maximum of {VolumeRange.Max} is too small given the housing maximum of {HousingRange.Max}");

        ReRollRanges();
    }

    public void ReRollRanges() {
        double scale = Terraria.WorldGen.genRand.NextDouble();
        Volume = (int)(VolumeRange.Min + (VolumeRange.Max - VolumeRange.Min) * scale);
        Height = Volume / (End.X - Start.X);
        if (Height <= 4) {
            throw new ArgumentException($"Volume ({Volume}) is too small compared to the length ({Length}) of the structure, resulting in a height of {Height}");
        }

        if (HousingRange.Min < 0) {
            throw new ArgumentException("Min housing cannot be less than 0");
        }
        if (HousingRange.Max < 0) {
            throw new ArgumentException("Max housing cannot be less than 0");
        }
        if (HousingRange.Max < HousingRange.Min) {
            throw new ArgumentException("Max Housing is less than min housing");
        }
        if (HousingRange.Max == 0 && TagBlacklist.Contains(StructureTag.HasHousing)) {
            throw new ArgumentException("Adv structure cannot have a max housing of 0 while blacklisting components with housing");
        }
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
    public RoomLayoutTag[] TagsBlacklist = tagsBlacklist;
    public RoomLayoutTag[] TagsRequired = tagsRequired;
    public Shape MainVolume = mainVolume;
    public EntryPoint[] EntryPoints = entryPoints;
    public TilePalette TilePalette = tilePalette;
    public int Housing = housing;
    public Range FloorWidth = floorWidth;
    public Range RoomHeight = roomHeight;
    public Range RoomWidth = roomWidth;
    public Range WallWidth = wallWidth;
    public float LargeRoomChance = largeRoomChance;
    public int Attempts = attempts;

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
    TilePalette tilePalette) {
    public readonly ComponentTag[] TagsBlacklist = tagsBlacklist;
    public readonly ComponentTag[] TagsRequired = tagsRequired;
    public readonly TilePalette TilePalette = tilePalette;
    public Shape Volume = volume;
}