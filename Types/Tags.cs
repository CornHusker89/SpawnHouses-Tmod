#nullable enable
using System;
using System.Collections.Generic;
using System.Reflection;

namespace SpawnHouses.Types;

[AttributeUsage(AttributeTargets.Field)]
public class TagData(Type type) : Attribute {
    public Type Type = type;
}

public enum StructureTag : ushort {
    // current highest tag number is 16
    IsSymmetric = 1,

    [TagData(typeof(int))] HasRooms = 16,

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
    // current highest tag number is: 27
    // ===== all =====
    Elevated = 1,
    GroundLevel = 2,
    UnderGround = 3,
    External = 4,

    /// when filling volumes, use
    /// <see cref="Helpers.SlopeHelper.SimpleSlopes" />
    UseSimpleSloping = 25,

    /// when filling volumes, use
    /// <see cref="Helpers.SlopeHelper.GothicSlopes" />
    UseGothicSloping = 26,

    /// when filling volumes, use
    /// <see cref="Helpers.SlopeHelper.HalfSlopes" />
    UseHalfSloping = 27,


    // ===== floor =====
    IsFloorGap = 6,

    /// floor is not hollow
    FloorSolid = 7,
    FloorHollow = 8,


    // ===== wall =====
    IsWallGap = 9,


    // ===== background =====
    BackgroundHasWindow = 10,
    BackgroundIsHousingInvalid = 11,
    BackgroundIsHousingValid = 12,


    // ===== stairway =====
    StairwayRequiresJumping = 13,
    StairwayNotRequiresJumping = 14,


    // ===== decor =====


    // ===== roof =====

    /// roof is tall enough that it doesn't follow the contour of the tiles it is placed on
    RoofTall = 15,

    /// roof follows contour of the roof, and is within 4 blocks of the top
    RoofShort = 16,
    
    RoofHasChimney = 17,
    RoofSlope1To1 = 18,
    RoofSlopeLessThan1 = 19,
    RoofSlopeGreaterThan1 = 20,
    RoofSlopeNone = 21,

    /// roof has an overhang of more than 1 tile
    RoofHasLargeOverhang = 22,


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

public static class TagUtils {

    /// <summary>
    ///     a component's tags are considered invalid if a component has all tags in any set
    /// </summary>
    public static HashSet<ComponentTag>[] MutuallyExclusiveComponentTagsRequired { get; } = [
        [ComponentTag.UseSimpleSloping, ComponentTag.UseGothicSloping],
        [ComponentTag.UseHalfSloping, ComponentTag.UseSimpleSloping],
        [ComponentTag.UseHalfSloping, ComponentTag.UseGothicSloping]
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