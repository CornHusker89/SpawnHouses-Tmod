#nullable enable
using System;
using System.Collections.Generic;
using System.Linq;
using SpawnHouses.AdvStructures.AdvStructureParts;
using Terraria.DataStructures;
using Range = SpawnHouses.Structures.Range;

namespace SpawnHouses.Types;

public class StructureParams : StructureTagSystem {
    public readonly bool CanAddEntryPoints;
    public readonly EntryPoint[] EntryPoints;
    public readonly TilePalette Palette;
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

        int? housing = GetTagDataSafe<int?>(StructureTag.HasHousing);

        if (housing != null) {
            int? roomCount = GetTagDataSafe<int?>(StructureTag.HasRooms);
            if (roomCount == null) throw new ArgumentException("Must have rooms tag to have housing");
            
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
}

public class RoomLayoutParams : StructureTagSystem {
    public readonly int Attempts;
    public readonly EntryPoint[] EntryPoints;
    public readonly Range FloorWidth;
    public readonly float LargeRoomChance;
    public readonly Range RoomHeight;
    public readonly Range RoomWidth;
    public readonly TilePalette TilePalette;
    public readonly Range WallWidth;

    public Shape MainVolume;


    public RoomLayoutParams(
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
        TagsRequired = tagsRequired;
        TagsBlocklist = tagsBlocklist;

        MainVolume = mainVolume;

        Attempts = attempts;
        EntryPoints = entryPoints;
        FloorWidth = floorWidth;
        LargeRoomChance = largeRoomChance;
        RoomHeight = roomHeight;
        RoomWidth = roomWidth;
        TilePalette = tilePalette;
        WallWidth = wallWidth;
    }

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