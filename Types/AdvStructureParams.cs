using System;
using System.Linq;
using SpawnHouses.AdvStructures.AdvStructureParts;
using Terraria.DataStructures;
using Range = SpawnHouses.Structures.Range;

namespace SpawnHouses.Types;

public class StructureParams {
    public bool CanAddEntryPoints;
    public EntryPoint[] EntryPoints;
    public int Height;
    public int Housing;
    public Range HousingRange;
    public TilePalette Palette;
    public StructureTag[] TagsBlacklist;
    public StructureTag[] TagsRequired;
    public int Volume;
    public Range VolumeRange;

    public StructureParams(
        StructureTag[] tagsRequired,
        StructureTag[] tagsBlacklist,
        EntryPoint[] entryPoints,
        TilePalette tilePalette,
        Range volumeRange,
        Range housingRange,
        bool canAddEntryPoints) {
        TagsRequired = tagsRequired;
        TagsBlacklist = tagsBlacklist;
        EntryPoints = entryPoints;
        Palette = tilePalette;
        VolumeRange = volumeRange;
        HousingRange = housingRange;
        CanAddEntryPoints = canAddEntryPoints;

        if (EntryPoints.Select(entryPoint => entryPoint.Start.Y).Max() - EntryPoints.Select(entryPoint => entryPoint.Start.Y).Min() + 4 > VolumeRange.Min / Length)
            throw new ArgumentException($"Entry points are too far away vertically for a minimum height of {VolumeRange.Min / Length} (determined by min volume / length)");
        if (tagsRequired.Contains(StructureTag.HasOnlyRectangleRooms) && tagsRequired.Contains(StructureTag.HasNoRectangleRooms))
            throw new ArgumentException("Cannot require mutually exclusive structure tags \"HasOnlyRectangleRooms\" (id 3) and \"HasNoRectangleRooms\" (id 4)");
        if (tagsRequired.Contains(StructureTag.AboveGround) && tagsRequired.Contains(StructureTag.UnderGround))
            throw new ArgumentException("Cannot require mutually exclusive structure tags \"AboveGround\" (id 9) and \"UnderGround\" (id 10)");

        ReRollRanges();
    }

    public int StartEntryPointX => EntryPoints.Min(entryPoint => entryPoint.Start.X);

    public int EndEntryPointX => EntryPoints.Max(entryPoint => entryPoint.End.X);

    public int Length => EndEntryPointX - StartEntryPointX;

    private int CenterYMin => EntryPoints.Min(entryPoint => entryPoint.Start.Y);
    private int CenterYMax => EntryPoints.Max(entryPoint => entryPoint.End.Y);

    /// <summary>calculated using entry points</summary>
    public Point16 Center => new(StartEntryPointX + (StartEntryPointX + EndEntryPointX) / 2, CenterYMin + (CenterYMin + CenterYMax) / 2);

    public void ReRollRanges() {
        double scale = Terraria.WorldGen.genRand.NextDouble();
        Volume = (int)(VolumeRange.Min + (VolumeRange.Max - VolumeRange.Min) * scale);
        Height = Volume / Length;
        Housing = (int)(HousingRange.Min + (HousingRange.Max - HousingRange.Min) * scale);

        if (VolumeRange.Min / HousingRange.Min < 60)
            throw new ArgumentException($"Volume minimum of {VolumeRange.Min} is too small given the housing minimum of {HousingRange.Min}");
        if (VolumeRange.Max / HousingRange.Max < 60)
            throw new ArgumentException($"Volume maximum of {VolumeRange.Max} is too small given the housing maximum of {HousingRange.Max}");
        if (Height <= 4)
            throw new ArgumentException($"Volume ({Volume}) is too small compared to the length ({Length}) of the structure, resulting in a too-low total height of {Height}");
        if (HousingRange.Min < 0)
            throw new ArgumentException("Min housing cannot be less than 0");
        if (HousingRange.Max < 0)
            throw new ArgumentException("Max housing cannot be less than 0");
        if (HousingRange.Max < HousingRange.Min)
            throw new ArgumentException("Max Housing is less than min housing");
        if (HousingRange.Max > 0 && TagsBlacklist.Contains(StructureTag.HasHousing))
            throw new ArgumentException("Structure cannot have a max housing > 0 while blacklisting components with housing");
    }
}

public class RoomLayoutParams(
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
    public TilePalette TilePalette = tilePalette;
    public Range WallWidth = wallWidth;

    /// <summary>
    ///     returns a shallow copy of these params
    /// </summary>
    /// <returns></returns>
    public RoomLayoutParams Clone() {
        return new RoomLayoutParams(
            MainVolume,
            EntryPoints,
            TilePalette,
            Housing,
            RoomHeight,
            RoomWidth,
            FloorWidth,
            WallWidth,
            LargeRoomChance,
            Attempts
        );
    }

    /// <summary>
    ///     true if volume's dimensions are not smaller than min sizes
    /// </summary>
    /// <param name="volume"></param>
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

public static class ComponentParamsUtils {
    public static ComponentParams CreateComponentParamsForType(IComponent component, TilePalette tilePalette, StructureTilemap tilemap) {
        Type componentType = component.GetType();
        if (componentType == typeof(VolumeComponentParams)) return new VolumeComponentParams((IVolumeComponent)component, tilePalette, tilemap);

        if (componentType == typeof(PathComponentParams)) return new PathComponentParams((IPathComponent)component, tilePalette, tilemap);

        throw new Exception($"Component type \"{componentType.FullName}\" does not have an associated parameter type");
    }
}

public class ComponentParams {
    public IComponent Component { get; set; }
    public StructureTilemap Tilemap { get; set; }
    public TilePalette Palette { get; set; }
}

public class VolumeComponentParams : ComponentParams {
    public new IVolumeComponent Component { get; set; }

    public VolumeComponentParams(IVolumeComponent component, TilePalette tilePalette, StructureTilemap tilemap) {
        Component = component;
        Tilemap = tilemap;
        Palette = tilePalette;
    }
}

public class PathComponentParams : ComponentParams {
    public new IPathComponent Component { get; set; }

    public PathComponentParams(IPathComponent component, TilePalette tilePalette, StructureTilemap tilemap) {
        Component = component;
        Tilemap = tilemap;
        Palette = tilePalette;
    }
}