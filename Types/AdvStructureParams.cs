#nullable enable
using System;
using System.Collections.Generic;
using System.Linq;
using SpawnHouses.AdvStructures.AdvStructureParts;
using Terraria.DataStructures;
using Range = SpawnHouses.Structures.Range;

namespace SpawnHouses.Types;

public class StructureParams {
    public readonly bool CanAddEntryPoints;
    public readonly EntryPoint[] EntryPoints;
    public readonly TilePalette Palette;
    public readonly Dictionary<StructureTag, object?> TagsRequired;
    public readonly HashSet<StructureTag> TagsBlocklist;
    public readonly int Volume;

    public StructureParams(
        Dictionary<StructureTag, object?> tagsRequired,
        HashSet<StructureTag> tagsBlocklist,
        EntryPoint[] entryPoints,
        TilePalette tilePalette,
        int volume,
        bool canAddEntryPoints) {
        TagsRequired = tagsRequired;
        TagsBlocklist = tagsBlocklist;
        EntryPoints = entryPoints;
        Palette = tilePalette;
        Volume = volume;
        CanAddEntryPoints = canAddEntryPoints;

        if (EntryPoints.Select(entryPoint => entryPoint.Start.Y).Max() - EntryPoints.Select(entryPoint => entryPoint.Start.Y).Min() + 4 > Volume / Length)
            throw new ArgumentException($"Entry points are too far away vertically for a minimum height of {Volume / Length} (determined by min volume / length)");
        if (tagsRequired.ContainsKey(StructureTag.HasOnlyRectangleRooms) && tagsRequired.ContainsKey(StructureTag.HasNoRectangleRooms))
            throw new ArgumentException("Cannot require mutually exclusive structure tags \"HasOnlyRectangleRooms\" (id 3) and \"HasNoRectangleRooms\" (id 4)");
        if (tagsRequired.ContainsKey(StructureTag.AboveGround) && tagsRequired.ContainsKey(StructureTag.UnderGround))
            throw new ArgumentException("Cannot require mutually exclusive structure tags \"AboveGround\" (id 9) and \"UnderGround\" (id 10)");

        if (Height <= 4)
            throw new ArgumentException($"Volume ({Volume}) is too small compared to the length ({Length}) of the structure, resulting in a too-low total height of {Height}");

        if (TagsRequired.ContainsKey(StructureTag.HasHousing)) {
            if (!TagsRequired.ContainsKey(StructureTag.HasRooms)) throw new ArgumentException("Must have rooms tag to have housing");
            int roomCount = GetTagData<int>(StructureTag.HasRooms);
            int housing = GetTagData<int>(StructureTag.HasHousing);
            if (Volume / housing < 60)
                throw new ArgumentException($"Volume minimum of {Volume} is too small given the housing minimum of {housing}");
            if (Volume / housing < 60)
                throw new ArgumentException($"Volume maximum of {Volume} is too small given the housing maximum of {housing}");
            if (housing < 1)
                throw new ArgumentException("housing must be greater than 0");
            if (TagsBlocklist.Contains(StructureTag.HasHousing))
                throw new ArgumentException("Structure cannot have a max housing > 0 while blocklisting components with housing");
            if (roomCount > housing) throw new ArgumentException($"Room count ({roomCount}) must be greater than or equal to housing ({housing})");
        }
    }

    public int LeftEntryPointX => EntryPoints.Min(entryPoint => entryPoint.Start.X);
    public int RightEntryPointX => EntryPoints.Max(entryPoint => entryPoint.End.X);

    public int Length => RightEntryPointX - LeftEntryPointX;
    public int Height => Volume / Length;

    private int CenterYMin => EntryPoints.Min(entryPoint => entryPoint.Start.Y);
    private int CenterYMax => EntryPoints.Max(entryPoint => entryPoint.End.Y);

    /// <summary>calculated using entry points</summary>
    public Point16 Center => new(LeftEntryPointX + (LeftEntryPointX + RightEntryPointX) / 2, CenterYMin + (CenterYMin + CenterYMax) / 2);

    public T GetTagData<T>(StructureTag targetTag) {
        if (!TagsRequired.TryGetValue(targetTag, out object? value)) throw new Exception("targetTag not found within given tag list");

        if (value == null) throw new Exception($"tag data for {targetTag} is null, could not return any data");
        T typedValue = (T)value;
        if (typedValue == null) throw new Exception($"tag data for {targetTag} could not be cast to target type of \"{typeof(T).Name}\"");
        return typedValue;
    }
}

public class RoomLayoutParams(
    Shape mainVolume,
    EntryPoint[] entryPoints,
    TilePalette tilePalette,
    Dictionary<StructureTag, object?> tagsRequired,
    HashSet<StructureTag> tagsBlocklist,
    Range roomHeight,
    Range roomWidth,
    Range floorWidth,
    Range wallWidth,
    float largeRoomChance = 0.2f,
    int attempts = 5
) {
    public readonly int Attempts = attempts;
    public readonly EntryPoint[] EntryPoints = entryPoints;
    public readonly Range FloorWidth = floorWidth;
    public readonly Dictionary<StructureTag, object?> TagsRequired = tagsRequired;
    public readonly HashSet<StructureTag> TagsBlocklist = tagsBlocklist;
    public readonly float LargeRoomChance = largeRoomChance;
    public readonly Range RoomHeight = roomHeight;
    public readonly Range RoomWidth = roomWidth;
    public readonly TilePalette TilePalette = tilePalette;
    public readonly Range WallWidth = wallWidth;
    
    public Shape MainVolume = mainVolume;

    /// <summary>
    ///     returns a shallow copy of these params
    /// </summary>
    /// <returns></returns>
    public RoomLayoutParams Clone() {
        return new RoomLayoutParams(
            MainVolume,
            EntryPoints,
            TilePalette,
            TagsRequired,
            TagsBlocklist,
            RoomHeight,
            RoomWidth,
            FloorWidth,
            WallWidth,
            LargeRoomChance,
            Attempts
        );
    }

    public T GetTagData<T>(StructureTag targetTag) {
        if (!TagsRequired.TryGetValue(targetTag, out object? value)) throw new Exception("targetTag not found within given tag list");
        if (value == null) throw new Exception($"tag data for {targetTag} is null, could not return any data");
        T typedValue = (T)value;
        if (typedValue == null) throw new Exception($"tag data for {targetTag} could not be cast to target type of \"{typeof(T).Name}\"");
        return typedValue;
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

public abstract class ComponentParams {
    public Component Component { get; protected init; }
    public StructureTilemap Tilemap { get; protected init; }
    public TilePalette Palette { get; protected init; }
}

public class VolumeComponentParams : ComponentParams {
    public new VolumeComponent Component { get; }

    public VolumeComponentParams(VolumeComponent component, TilePalette tilePalette, StructureTilemap tilemap) {
        Component = component;
        base.Component = component;
        Tilemap = tilemap;
        Palette = tilePalette;
    }
}

public class PathComponentParams : ComponentParams {
    public new PathComponent Component { get; }

    public PathComponentParams(PathComponent component, TilePalette tilePalette, StructureTilemap tilemap) {
        Component = component;
        base.Component = component;
        Tilemap = tilemap;
        Palette = tilePalette;
    }
}