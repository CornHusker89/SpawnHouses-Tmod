#nullable enable
using System.Collections.Generic;
using System.Linq;
using SpawnHouses.AdvStructures.AdvStructureParts;
using SpawnHouses.Common.Types.Geometry;
using SpawnHouses.Types.TagTypes;
using Terraria.DataStructures;
using Range = SpawnHouses.Structures.Range;

namespace SpawnHouses.Common.Parameters;

public class RoomLayoutParams : IParams {
    public AdvStructure Structure { get; init; }
    public TagMap TagsRequired { get; init; }

    public readonly List<Shape> FloorVolumes;
    public readonly List<Shape> WallVolumes;
    public readonly List<Shape> RoomVolumes;

    public readonly Shape Volume;
    public readonly EntryPoint[] EntryPoints;
    public readonly Range FloorWidth;
    public readonly Range WallWidth;
    public readonly Range RoomHeight;
    public readonly Range RoomWidth;
    public readonly float LargeRoomChance;
    public readonly int Attempts;

    public RoomLayoutParams(AdvStructure structure, List<Shape> floorVolumes, List<Shape> wallVolumes, List<Shape> roomVolumes, Shape volume, EntryPoint[] entryPoints, Range floorWidth,
        Range wallWidth, Range roomHeight, Range roomWidth, TagMap? tagsRequired = null, float largeRoomChance = 0.2f, int attempts = 5) {
        Structure = structure;
        TagsRequired = tagsRequired ?? new TagMap();
        FloorVolumes = floorVolumes;
        WallVolumes = wallVolumes;
        RoomVolumes = roomVolumes;
        Volume = volume;
        Attempts = attempts;
        EntryPoints = entryPoints;
        FloorWidth = floorWidth;
        LargeRoomChance = largeRoomChance;
        RoomHeight = roomHeight;
        RoomWidth = roomWidth;
        WallWidth = wallWidth;
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

    /// <summary>
    ///     if the point is within any floor volume
    /// </summary>
    /// <param name="point"></param>
    /// <returns></returns>
    public bool InFloor(Point16 point) {
        return FloorVolumes.Any(floorVolume => floorVolume.Contains(point));
    }

    /// <summary>
    ///     if the point is within any wall volume
    /// </summary>
    /// <param name="point"></param>
    /// <returns></returns>
    public bool InWall(Point16 point) {
        return WallVolumes.Any(wallVolume => wallVolume.Contains(point));
    }

    /// <summary>
    ///     if the point is within any room volume
    /// </summary>
    /// <param name="point"></param>
    /// <returns></returns>
    public bool InRoom(Point16 point) {
        return RoomVolumes.Any(roomVolume => roomVolume.Contains(point));
    }

    /// <summary>
    ///     checks if the point is contained within any volume
    /// </summary>
    /// <param name="point"></param>
    /// <returns></returns>
    public bool InStructure(Point16 point) {
        return FloorVolumes.Any(floorVolume => floorVolume.Contains(point)) || WallVolumes.Any(floorVolume => floorVolume.Contains(point)) || RoomVolumes.Any(floorVolume => floorVolume.Contains(point));
    }
}