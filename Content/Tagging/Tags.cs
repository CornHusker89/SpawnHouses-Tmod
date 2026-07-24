#nullable enable
using System.Reflection;
using SpawnHouses.Content.Types.Geometry;
using SpawnHouses.Helpers;

namespace SpawnHouses.Content.Tagging;

// ReSharper disable InconsistentNaming

/// <summary>
///     Tag saving/loading is based on the tag's identifier, so don't change them once implemented
/// </summary>
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

    /// there is a convenient large room intended for storage
    public static Tag Structure_HasStorage = new();

    /// the structure is NOT made of materials that have potential to significantly screw up progression (ex hard mode ores)
    public static Tag Structure_ProgressionSafe = new();

    /// the structure is made of materials that have potential to significantly screw up progression (ex hard mode ores)
    public static Tag Structure_NotProgressionSafe = new();

    public static Tag Structure_HasRoof = new();

    public static Tag Structure_HasNoRoof = new();

    /// if the structure's entry points don't represent the outer bounds of the structure. value represents the furthest extension from an entry point
    public static Tag<int> Structure_ExtendsPastEntryPoints = new();

    /// structure is categorized as having an overall forest theme
    public static Tag Structure_ForestTheme = new();

    /// structure is categorized as having an overall icy/cold theme
    public static Tag Structure_IceTheme = new();

    /// structure is categorized as having an overall beach theme
    public static Tag Structure_BeachTheme = new();

    /// structure is categorized as having an overall jungle theme
    public static Tag Structure_JungleTheme = new();

    /// structure is categorized as having an overall cavern/underground theme
    public static Tag Structure_CavernTheme = new();

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

    public static void SetInternalTagNames() {
        foreach (FieldInfo fieldInfo in typeof(Tags).GetFields(BindingFlags.Static | BindingFlags.Public)) ((Tag)fieldInfo.GetValue(null)!).Name = fieldInfo.Name;
    }
}