#nullable enable
using System.Reflection;
using SpawnHouses.Content.Types;
using SpawnHouses.Content.Types.Geometry;
using SpawnHouses.Helpers;

namespace SpawnHouses.Content.Tagging;

// ReSharper disable InconsistentNaming

/// <summary>
///     contains all tags used in generated housing
/// </summary>
/// <remarks>Tag saving/loading is based on the tag's name/identifier, so don't change them once implemented</remarks>
public static class Tags {
    
    #region Structure

    public static Tag Structure_IsSymmetric = new();

    public static Tag<int> Structure_HasRooms = new();

    public static Tag<int> Structure_HasHousing = new();

    public static Tag Structure_HasOnlyRectangleRooms = new();

    public static Tag Structure_HasNoRectangleRooms = new();

    /// has both rectangular and non-uniform rooms
    public static Tag Structure_HasSomeRectangleRooms = new();

    /// there are a number of convenient large rooms intended for general use
    public static Tag<int> Structure_HasLargeRoom = new();

    /// there is a convenient large room intended for storage. value indicates the number of chest-equivalent spaces
    public static Tag<int> Structure_HasStorage = new();

    /// structure does NOT have materials that have potential to screw up progression (ex hard mode ores)
    public static Tag Structure_ProgressionSafe = new();

    /// structure has materials that have potential to screw up progression (ex hard mode ores)
    public static Tag Structure_NotProgressionSafe = new();

    public static Tag Structure_HasRoof = new();

    public static Tag Structure_HasNoRoof = new();

    /// if the structure's entry points don't represent the outer bounds of the structure. value represents the furthest extension from an entry point
    public static Tag<int> Structure_RoofExtendsPastEntryPoints = new();

    /// if the structure has landscaping built into the tilemap. value is the entry points that have landscaping attached to them, and any entry points to add in replacement
    public static Tag<(EntryPoint[] landscapedEntryPoints, EntryPoint[] newEntryPoints)> Structure_BuiltInLandscaping = new();

    /// if the structure has alternate structures that have similar tilemaps but are larger or "improved" in some other way
    public static Tag<StructureRoot[]> Structure_Upgradable = new();

    /// if the structure requires any other mods. value is the unique name of any required mods
    public static Tag<string[]> Structure_RequiredMods = new();

    /// if the structure uses Magic Storage
    public static Tag Structure_UsesMS = new();

    #endregion

    #region Theme
    
    /// structure is categorized as having an overall forest theme
    public static Tag Theme_Forest = new();

    /// structure is categorized as having an overall icy/cold theme
    public static Tag Theme_Ice = new();

    /// structure is categorized as having an overall beach theme
    public static Tag Theme_Beach = new();

    /// structure is categorized as having an overall jungle theme
    public static Tag Theme_Jungle = new();

    /// structure is categorized as having an overall cavern/underground theme
    public static Tag Theme_Cavern = new();
    
    #endregion

    #region Component General

    public static Tag External = new();

    /// use a specific predetermined sloping algorithm when filling volumes
    public static readonly Tag<SlopingAlgorithm> Component_SlopingAlgorithm = new();

    /// apply sloping algorithm with different specific predetermined contexts
    public static readonly Tag<SlopeGrouping> Component_SlopeGrouping = new();

    public static readonly Tag Component_HasCustomSloping = new();

    /// applied primarily to rooms, but could also go to hollow walls/floors
    public static Tag Component_HasDebris = new();

    #endregion

    #region Floor

    /// floor is not hollow
    public static readonly Tag Floor_Solid = new();

    public static readonly Tag Floor_Hollow = new();

    #endregion

    #region Wall

    #endregion

    #region Gap

    /// door of a horizontal gap is on the "outside" side of the gap. typically defaults to the middle of the gap, only affects wider gaps
    public static readonly Tag Gap_DoorOutside = new();

    /// door of a horizontal gap is on the "inside" side of the gap. typically defaults to the middle of the gap, only affects wider gaps
    public static readonly Tag Gap_DoorInside = new();

    #endregion

    #region Room

    public static readonly Tag Room_TypeStorage = new();

    public static readonly Tag Room_TypeLiving = new();

    public static readonly Tag Room_TypeBedroom = new();

    public static readonly Tag Room_TypeBathroom = new();

    public static readonly Tag Room_TypeStudy = new();

    public static readonly Tag Room_TypeWorkshop = new();

    /// if a room has windows that are spaced out by that room's own geometry, and the shapes that make them up
    public static readonly Tag<Shape[]> Room_HasWindows = new();

    /// if exact placement is required, the required window volumes in a room
    public static readonly Tag<Shape[]> Room_HasSpecificWindows = new();

    public static readonly Tag Room_HousingNotValid = new();

    public static readonly Tag Room_HousingValid = new();

    /// if a room has beams, what x-positions they are
    public static readonly Tag<int[]> Room_HasBeams = new();

    /// if exact placement is required, these are those beam x-positions in a room
    public static readonly Tag<int[]> Room_HasSpecificBeams = new();

    public static readonly Tag Room_BeamsAreTiles = new();

    public static readonly Tag Room_BeamsAreWalls = new();

    #endregion

    #region Stairway

    public static readonly Tag Stairway_TilesLowerX = new();

    public static readonly Tag Stairway_TilesHigherX = new();

    public static readonly Tag Stairway_RequiresJumping = new();

    public static readonly Tag Stairway_NotRequiresJumping = new();

    public static readonly Tag Stairway_ToHorizontalGap = new();

    public static readonly Tag Stairway_ToVerticalGap = new();

    #endregion

    #region Roof

    public static readonly Tag Roof_Flat = new();
    
    /// roof is tall enough that it doesn't follow the full contour of the tiles it is placed on
    public static readonly Tag Roof_Tall = new();

    /// roof is short enough to generally follow the contour of the path
    public static readonly Tag Roof_Short = new();

    public static readonly Tag<int[]> Roof_HasChimney = new();

    public static readonly Tag<int> Roof_HasLeftOverhang = new();

    public static readonly Tag<int> Roof_HasRightOverhang = new();

    #endregion

    #region Palette

    public static readonly Tag Palette_Wood = new();

    public static readonly Tag Palette_Stone = new();

    public static readonly Tag Palette_DarkGrey = new();

    public static readonly Tag Palette_LightGrey = new();

    public static readonly Tag Palette_MediumGrey = new();

    public static readonly Tag Palette_DarkBrown = new();

    public static readonly Tag Palette_LightBrown = new();

    public static readonly Tag Palette_MediumBrown = new();

    public static readonly Tag Palette_Red = new();

    public static readonly Tag Palette_Turquoise = new();

    #endregion

    /// <summary>
    ///     sets the <see cref="Tag.Name" /> field for all tags in <see cref="Tags" />
    /// </summary>
    public static void SetTagNameFields() {
        foreach (FieldInfo fieldInfo in typeof(Tags).GetFields(BindingFlags.Static | BindingFlags.Public)) ((Tag)fieldInfo.GetValue(null)!).Name = fieldInfo.Name;
    }
}