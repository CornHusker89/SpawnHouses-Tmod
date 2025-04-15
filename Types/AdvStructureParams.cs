using System;
using System.Linq;
using SpawnHouses.AdvStructures;
using SpawnHouses.AdvStructures.AdvStructureParts;
using Terraria.DataStructures;
using Range = SpawnHouses.Structures.Range;

namespace SpawnHouses.Types;

public class StructureParams {
    public Point16 End;
    public int Height;
    public int Housing;
    public Range HousingRange;
    public int Length;

    public TilePalette Palette;
    public Point16 Start;
    public StructureTag[] TagBlacklist;
    public StructureTag[] TagsRequired;
    public int Volume;
    public Range VolumeRange;

    public StructureParams(
        TilePalette tilePalette,
        StructureTag[] tagsRequired,
        StructureTag[] tagBlacklist,
        Point16 start,
        Point16 end,
        Range volumeRange,
        Range housingRange) {
        Palette = tilePalette;
        TagsRequired = tagsRequired;
        TagBlacklist = tagBlacklist;
        Start = start;
        End = end;
        if (Start.X > End.X)
            (Start, End) = (End, Start);
        Length = End.X - Start.X;
        VolumeRange = volumeRange;
        HousingRange = housingRange;

        if (VolumeRange.Min / HousingRange.Min < 50)
            throw new ArgumentException("Volume minimum is too small given the housing minimum");
        if (VolumeRange.Max / HousingRange.Max < 50)
            throw new ArgumentException("Volume maximum is too small given the housing maximum");

        ReRollRanges();
    }

    public static bool ContainsAny(StructureTag[] tags1, StructureTag[] tags2) {
        return tags1.Any(tag => tags2.Contains(tag));
    }

    public void ReRollRanges() {
        double scale = Terraria.WorldGen.genRand.NextDouble();
        Volume = (int)(VolumeRange.Min + (VolumeRange.Max - VolumeRange.Min) * scale);
        Height = Volume / (End.X - Start.X);
        if (Height <= 4)
            throw new ArgumentException(
                $"Volume ({Volume}) is too small compared to the length ({Length}) of the structure, resulting in a height of {Height}");

        if (HousingRange.Min < 0)
            throw new ArgumentException("Min housing cannot be less than 0");
        if (HousingRange.Max < 0)
            throw new ArgumentException("Max housing cannot be less than 0");
        if (HousingRange.Max < HousingRange.Min)
            throw new ArgumentException("Max Housing is less than min housing");
        if (HousingRange.Max == 0 && TagBlacklist.Contains(StructureTag.HasHousing))
            throw new ArgumentException(
                "Adv structure cannot have a max housing of 0 while blacklisting components with housing");
        Housing = (int)(HousingRange.Min + (HousingRange.Max - HousingRange.Min) * scale);
    }
}

public class RoomLayoutParams(
    StructureTag[] tagsRequired,
    StructureTag[] tagsBlacklist,
    Shape mainVolume,
    TilePalette tilePalette,
    int housing,
    Range roomHeight,
    Range roomWidth,
    Range floorWidth,
    Range wallWidth,
    float largeRoomChance = 0.2f,
    int attempts = 5
) {
    public readonly int Attempts = attempts;
    public readonly Range FloorWidth = floorWidth;
    public readonly int Housing = housing;
    public readonly float LargeRoomChance = largeRoomChance;
    public readonly Range RoomHeight = roomHeight;
    public readonly Range RoomWidth = roomWidth;
    public readonly StructureTag[] TagsBlacklist = tagsBlacklist;
    public readonly StructureTag[] TagsRequired = tagsRequired;
    public readonly TilePalette TilePalette = tilePalette;
    public readonly Range WallWidth = wallWidth;
    public Shape MainVolume = mainVolume;

    /// <summary>
    ///     true if volume's dimensions are not smaller than min sizes
    /// </summary>
    /// <param name="volume"></param>
    /// <param name="roomLayoutParams"></param>
    /// <returns></returns>
    public bool IsWithinMinSize(Shape volume) {
        return volume.Size.X >= RoomWidth.Min &&
               volume.Size.Y >= RoomHeight.Min;
    }

    /// <summary>
    ///     true if volume's dimensions are not larger than max sizes
    /// </summary>
    /// <param name="volume"></param>
    /// <param name="roomLayoutParams"></param>
    /// <returns></returns>
    public bool IsWithinMaxSize(Shape volume) {
        return volume.Size.X <= RoomWidth.Max &&
               volume.Size.Y <= RoomHeight.Max;
    }
}

public class ComponentParams(
    StructureTag[] tagsRequired,
    StructureTag[] tagsBlacklist,
    Shape volume,
    TilePalette tilePalette) {
    public readonly StructureTag[] TagsBlacklist = tagsBlacklist;
    public readonly StructureTag[] TagsRequired = tagsRequired;
    public readonly TilePalette TilePalette = tilePalette;
    public Shape Volume = volume;
}