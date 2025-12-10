#nullable enable
using System;
using System.Collections.Generic;
using System.Reflection;
using SpawnHouses.Helpers;

namespace SpawnHouses.Types;

[AttributeUsage(AttributeTargets.Field)]
public class TagData(Type type) : Attribute {
    public Type Type = type;
}

public enum StructureTag : ushort {
    // current highest tag number is 16
    IsSymmetric = 1,

    [TagData(typeof(int))]
    HasRooms = 16,

    [TagData(typeof(int))]
    HasHousing = 2,

    HasOnlyRectangleRooms = 3,
    HasNoRectangleRooms = 4,

    /// has both rectangular and non-uniform rooms
    HasSomeRectangleRooms = 5,

    /// there is a convenient large room intended for general use
    HasLargeRoom = 6,

    /// there is a convenient large room intended for storage
    HasStorage = 7,

    /// the main floor has horizontal gaps wherever possible
    MainFloorConnected = 8,

    /// structure is categorized as being above ground (typically has a roof)
    AboveGround = 9,

    /// structure is categorized as being below ground (typically has a no dedicated roof)
    UnderGround = 10,

    /// structure is categorized as having an overall forest theme
    Forest = 11,

    /// structure is categorized as having an overall icy/cold theme
    Ice = 12,

    /// structure is categorized as having an overall beach theme
    Beach = 13,

    /// structure is categorized as having an overall jungle theme
    Jungle = 14,

    /// structure is categorized as having an overall cavern/underground theme
    Cavern = 15
}

public enum ComponentTag {
    // current highest tag number is: 41
    // ===== all =====
    External = 4,

    /// use sloping algorithm when filling volumes
    [TagData(typeof(SlopingAlgorithm))]
    ApplySloping = 28,

    /// apply sloping algorithm with different contexts
    [TagData(typeof(SlopeModifier))]
    SlopingModifier = 31,

    HasDebris = 40,

    // ===== floor =====
    IsFloorGap = 6,

    /// floor is not hollow
    FloorSolid = 7,
    FloorHollow = 8,


    // ===== wall =====
    IsWallGap = 9,


    // ===== room =====
    RoomTypeStorage = 37,
    RoomTypeLiving = 38,
    RoomTypeBedroom = 39,
    RoomTypeWorkshop = 40,
    RoomHasWindow = 32,
    RoomHousingNotValid = 33,
    RoomHousingValid = 34,

    RoomHasArbitraryBeams = 35,

    [TagData(typeof(int[]))]
    RoomHasSpecificBeams = 36,
    

    // ===== stairway =====
    StairwayRequiresJumping = 13,
    StairwayNotRequiresJumping = 14,


    // ===== decor =====


    // ===== roof =====

    /// roof is tall enough that it doesn't follow the contour of the tiles it is placed on
    RoofTall = 15,

    /// roof is short enough to generally follow the contour of the path
    RoofShort = 16,

    [TagData(typeof(int[]))]
    RoofHasChimney = 17,

    /// roof has an overhang of more than 1 tile
    RoofHasLargeOverhang = 22


    // ===== gap =====
}

public enum PaletteTag {
    Wood = 1,
    Stone = 2,
    DarkGrey = 3,
    LightGrey = 4,
    MediumGrey = 5,
    DarkBrown = 6,
    LightBrown = 7,
    MediumBrown = 8,
    Red = 9,
    Turquoise = 10
}

public abstract class StructureTagSystem {
    public Dictionary<StructureTag, object?> TagsRequired;
    public HashSet<StructureTag> TagsBlocklist;

    /// <summary>
    ///     gets tag data from component's tag. throws when tag doesn't exist, has no data, or when cast fails.
    ///     for safe version see <see cref="GetTagDataSafe{T}" />
    /// </summary>
    /// <param name="targetTag"></param>
    /// <typeparam name="T"></typeparam>
    /// <returns></returns>
    /// <exception cref="Exception"></exception>
    public T GetTagData<T>(StructureTag targetTag) {
        if (!TagsRequired.TryGetValue(targetTag, out object? value)) throw new Exception("targetTag not found within given tag list");
        if (value == null) throw new Exception($"tag data for {targetTag} is null, could not return any data");
        T typedValue = (T)value;
        if (typedValue == null) throw new Exception($"tag data for {targetTag} could not be cast to target type of \"{typeof(T).Name}\"");
        return typedValue;
    }

    /// <summary>
    ///     gets tag data from tags. returns default (recommended to use nullable types) when tag isn't found, but throws if tag is found bus has no data or if cast fails.
    ///     for unsafe version see <see cref="GetTagData{T}" />
    /// </summary>
    /// <param name="targetTag"></param>
    /// <typeparam name="T"></typeparam>
    /// <returns></returns>
    /// <exception cref="Exception"></exception>
    public T? GetTagDataSafe<T>(StructureTag targetTag) {
        if (!TagsRequired.TryGetValue(targetTag, out object? value)) return default;

        if (value == null) throw new Exception($"Target tag \"{targetTag}\" found but had no associated data");
        if (value is not T typedValue) throw new ArgumentException($"tag data for {targetTag} could not be cast to target type of \"{typeof(T).Name}\"");
        return typedValue;
    }
}

public abstract class ComponentTagSystem {
    public Dictionary<ComponentTag, object?> TagsRequired { get; set; }
    public HashSet<ComponentTag> TagsBlocklist { get; set; }

    public void AddRequiredTag(ComponentTag tag) {
        TagsRequired[tag] = null;
    }

    public void AddRequiredTag<T>(ComponentTag tag, T tagData) {
        TagsRequired[tag] = tagData;
    }

    /// <summary>
    ///     gets tag data from component's tag. throws when tag doesn't exist, has no data, or when cast fails.
    ///     for safe version see <see cref="GetTagDataSafe{T}" />
    /// </summary>
    /// <param name="targetTag"></param>
    /// <typeparam name="T"></typeparam>
    /// <returns></returns>
    /// <exception cref="Exception"></exception>
    public T GetTagData<T>(ComponentTag targetTag) {
        if (!TagsRequired.TryGetValue(targetTag, out object? value)) throw new ArgumentException("targetTag not found within given tag list");

        if (value == null) throw new Exception($"Target tag \"{targetTag}\" found but had no associated data");
        if (value is not T typedValue) throw new Exception($"tag data for {targetTag} could not be cast to target type of \"{typeof(T).Name}\"");
        return typedValue;
    }

    /// <summary>
    ///     gets tag data from component's tag. returns default (recommended to use nullable types) when tag isn't found, but throws if tag is found bus has no data or if cast fails.
    ///     for unsafe version see <see cref="GetTagData{T}" />
    /// </summary>
    /// <param name="targetTag"></param>
    /// <typeparam name="T"></typeparam>
    /// <returns></returns>
    /// <exception cref="Exception"></exception>
    public T? GetTagDataSafe<T>(ComponentTag targetTag) {
        if (!TagsRequired.TryGetValue(targetTag, out object? value)) return default;

        if (value == null) throw new Exception($"Target tag \"{targetTag}\" found but had no associated data");
        if (value is not T typedValue) throw new ArgumentException($"tag data for {targetTag} could not be cast to target type of \"{typeof(T).Name}\"");
        return typedValue;
    }
}

public static class TagUtils {
    /// <summary>
    ///     a component's tags are considered invalid if a component has all tags in any set
    /// </summary>
    public static HashSet<ComponentTag>[] MutuallyExclusiveComponentTagsRequired { get; } = [
    ];

    /// <summary>
    ///     a component's tags are considered invalid if a component has all tags in any set
    /// </summary>
    public static HashSet<ComponentTag>[] MutuallyExclusiveComponentTagsBlocklist { get; } = [
    ];

    public static void ValidateExclusiveTagsRequired(HashSet<ComponentTag> tagsRequired) {
        foreach (var exclusiveSet in MutuallyExclusiveComponentTagsRequired) {
            if (!exclusiveSet.IsSubsetOf(tagsRequired)) continue;

            string message = "Component has mutually exclusive component tags required {";
            foreach (ComponentTag tag in exclusiveSet) message += $"{tag} (id {(ushort)tag}), ";
            throw new Exception(message.Remove(message.Length - 2));
        }
    }

    public static void ValidateExclusiveTagsBlocklist(HashSet<ComponentTag> tagsRequired) {
        foreach (var exclusiveSet in MutuallyExclusiveComponentTagsBlocklist) {
            if (!exclusiveSet.IsSubsetOf(tagsRequired)) continue;

            string message = "Component has mutually exclusive component tags blocklisted {";
            foreach (ComponentTag tag in exclusiveSet) message += $"{tag} (id {(ushort)tag}), ";
            throw new Exception(message.Remove(message.Length - 2));
        }
    }

    public static void ValidateTagDataTypes(Dictionary<ComponentTag, object?> tagDataTypes) {
        foreach (var tagValuePair in tagDataTypes) {
            FieldInfo enumField = typeof(ComponentTag).GetField(tagValuePair.Key.ToString())!;
            TagData? attribute = enumField.GetCustomAttribute<TagData>();
            if (attribute != null && tagValuePair.Value != null && attribute.Type != tagValuePair.Value.GetType()) throw new Exception($"Expected tag data type ({attribute.Type}( does not match actual type ({tagValuePair.Value.GetType()})");
        }
    }

    public static void ValidateTagDataTypes(Dictionary<StructureTag, object?> tagDataTypes) {
        foreach (var tagValuePair in tagDataTypes) {
            FieldInfo enumField = typeof(StructureTag).GetField(tagValuePair.Key.ToString())!;
            TagData? attribute = enumField.GetCustomAttribute<TagData>();
            if (attribute != null && tagValuePair.Value != null && attribute.Type != tagValuePair.Value.GetType()) throw new Exception($"Expected tag data type ({attribute.Type}( does not match actual type ({tagValuePair.Value.GetType()})");
        }
    }
}