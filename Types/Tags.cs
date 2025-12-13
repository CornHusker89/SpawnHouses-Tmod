#nullable enable
using System;
using System.Collections.Generic;
using System.Reflection;
using SpawnHouses.Helpers;

namespace SpawnHouses.Types;

[AttributeUsage(AttributeTargets.Field)]
public class TagData(Type type) : Attribute {
    public readonly Type Type = type;
}

public enum StructureTag : ushort {
    // current highest tag number is 17
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

    /// the structure is NOT made of materials that have potential to significantly screw up progression (ex hard mode ores)
    ProgressionSafe = 17,

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

public enum ComponentTag : ushort {
    // current highest tag number is: 44
    // ===== all =====
    External = 4,

    /// use sloping algorithm when filling volumes
    [TagData(typeof(SlopingAlgorithm))]
    ApplySloping = 28,

    /// apply sloping algorithm with different contexts
    [TagData(typeof(SlopeModifier))]
    SlopingModifier = 31,

    HasDebris = 41,

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

    RoomBeamsAreTiles = 43,
    RoomBeamsAreNotTiles = 44,
    

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

    [TagData(typeof((int, int)))]
    RoofHasOverhang = 42


    // ===== gap =====
}

public enum PaletteTag : byte {
    // Current highest tag number is: 11

    ProgressionSafe = 11,
    
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

public abstract class TagSystem<T> where T : Enum {
    public Dictionary<T, object?> TagsRequired { get; } = new();
    public Dictionary<T, object?> TagsCurrent { get; } = new();

    public static HashSet<T> NewTagSet(HashSet<T> tagSet) {
        return tagSet;
    }

    public static HashSet<T> NewTagSet(HashSet<T> tagSet, params HashSet<T>[] otherTagSets) {
        foreach (var otherTagSet in otherTagSets) tagSet.UnionWith(otherTagSet);
        return tagSet;
    }

    public void AddRequiredTag(T tag) {
        TagsRequired[tag] = null;
    }

    public void AddRequiredTag<TData>(T tag, TData tagData) {
        TagsRequired[tag] = tagData;
    }

    public void AddCurrentTag(T tag) {
        TagsCurrent[tag] = null;
    }

    public void AddCurrentTag<TData>(T tag, TData tagData) {
        TagsCurrent[tag] = tagData;
    }
    
    /// <summary>
    ///     gets tag data from component's tag. throws when tag doesn't exist, has no data, or when cast fails.
    ///     for safe version see <see cref="GetDataSafe{T}" />
    /// </summary>
    /// <param name="targetTag"></param>
    /// <param name="tagSet"></param>
    /// <typeparam name="TData"></typeparam>
    /// <returns></returns>
    /// <exception cref="Exception"></exception>
    private static TData GetData<TData>(T targetTag, Dictionary<T, object?> tagSet) {
        if (!tagSet.TryGetValue(targetTag, out object? value)) throw new ArgumentException("targetTag not found within given tag list");

        if (value == null) throw new Exception($"Target tag \"{targetTag}\" found but had no associated data");
        if (value is not TData typedValue) throw new Exception($"tag data for {targetTag} could not be cast to target type of \"{typeof(T).Name}\"");
        return typedValue;
    }

    /// <summary>
    ///     gets tag data from component's tag. throws when tag doesn't exist, has no data, or when cast fails.
    ///     for safe version see <see cref="GetTagRequiredDataSafe{T}" />
    /// </summary>
    /// <param name="targetTag"></param>
    /// <typeparam name="TData"></typeparam>
    /// <returns></returns>
    public TData GetTagRequiredData<TData>(T targetTag) {
        return GetData<TData>(targetTag, TagsRequired);
    }
    
    /// <summary>
    ///     gets tag data from component's tag. throws when tag doesn't exist, has no data, or when cast fails.
    ///     for safe version see <see cref="GetTagCurrentDataSafe{T}" />
    /// </summary>
    /// <param name="targetTag"></param>
    /// <typeparam name="TData"></typeparam>
    /// <returns></returns>
    public TData GetTagCurrentData<TData>(T targetTag) {
        return GetData<TData>(targetTag, TagsCurrent);
    }
    
    /// <summary>
    ///     gets tag data from component's tag. returns default (recommended to use nullable types) when tag isn't found, but throws if tag is found but has no data or if cast fails.
    ///     for unsafe version see <see cref="GetData{T}" />
    /// </summary>
    /// <param name="targetTag"></param>
    /// <param name="tagSet"></param>
    /// <typeparam name="TData"></typeparam>
    /// <returns></returns>
    /// <exception cref="Exception"></exception>
    public TData? GetDataSafe<TData>(T targetTag, Dictionary<T, object?> tagSet) {
        if (!tagSet.TryGetValue(targetTag, out object? value)) return default;

        if (value == null) throw new Exception($"Target tag \"{targetTag}\" found but had no associated data");
        if (value is not TData typedValue) throw new ArgumentException($"tag data for {targetTag} could not be cast to target type of \"{typeof(T).Name}\"");
        return typedValue;
    }

    /// <summary>
    ///     gets tag data from component's tag. returns default (recommended to use nullable types) when tag isn't found, but throws if tag is found but has no data or if cast fails.
    ///     for unsafe version see <see cref="GetTagRequiredData{T}" />
    /// </summary>
    /// <param name="targetTag"></param>
    /// <typeparam name="TData"></typeparam>
    /// <returns></returns>
    public TData? GetTagRequiredDataSafe<TData>(T targetTag) {
        return GetDataSafe<TData?>(targetTag, TagsRequired);
    }

    /// <summary>
    ///     gets tag data from component's tag. returns default (recommended to use nullable types) when tag isn't found, but throws if tag is found but has no data or if cast fails.
    ///     for unsafe version see <see cref="GetTagRequiredData{T}" />
    /// </summary>
    /// <param name="targetTag"></param>
    /// <typeparam name="TData"></typeparam>
    /// <returns></returns>
    public TData? GetTagCurrentDataSafe<TData>(T targetTag) {
        return GetDataSafe<TData?>(targetTag, TagsCurrent);
    }
}

public abstract class StructureTagSystem : TagSystem<StructureTag> {
}

public abstract class ComponentTagSystem : TagSystem<ComponentTag> {
}

public static class TagUtils {
    /// <summary>
    ///     a component's tags are considered invalid if a component has all tags in any set
    /// </summary>
    public static HashSet<ComponentTag>[] MutuallyExclusiveComponentTagsRequired { get; } = [
    ];

    public static void ValidateExclusiveTagsRequired(HashSet<ComponentTag> tagsRequired) {
        foreach (var exclusiveSet in MutuallyExclusiveComponentTagsRequired) {
            if (!exclusiveSet.IsSubsetOf(tagsRequired)) continue;

            string message = "Component has mutually exclusive component tags required {";
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