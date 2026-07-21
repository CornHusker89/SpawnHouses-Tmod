#nullable enable
using System.Reflection;
using SpawnHouses.Content.Types.Geometry;
using SpawnHouses.Helpers;

namespace SpawnHouses.Content.Tagging;

#pragma warning disable CA2211

/// <summary>
///     Tag saving/loading is based on the tag's identifier, so don't change them once implemented
/// </summary>
public static class Tags {
    
    #region Structure

    public static Tag IsSymmetric = new();

    public static Tag<int> HasRooms = new();

    public static Tag<int> HasHousing = new();

    public static Tag HasOnlyRectangleRooms = new();

    public static Tag HasNoRectangleRooms = new();

    /// has both rectangular and non-uniform rooms
    public static Tag HasSomeRectangleRooms = new();

    /// there are a number of convenient large rooms intended for general use
    public static Tag<int> HasLargeRoom = new();

    /// there is a convenient large room intended for storage
    public static Tag HasStorage = new();

    /// the structure is NOT made of materials that have potential to significantly screw up progression (ex hard mode ores)
    public static Tag ProgressionSafe = new();

    /// the structure is made of materials that have potential to significantly screw up progression (ex hard mode ores)
    public static Tag NotProgressionSafe = new();

    public static Tag HasRoof = new();

    public static Tag HasNoRoof = new();

    /// structure is categorized as having an overall forest theme
    public static Tag ForestTheme = new();

    /// structure is categorized as having an overall icy/cold theme
    public static Tag IceTheme = new();

    /// structure is categorized as having an overall beach theme
    public static Tag BeachTheme = new();

    /// structure is categorized as having an overall jungle theme
    public static Tag JungleTheme = new();

    /// structure is categorized as having an overall cavern/underground theme
    public static Tag CavernTheme = new();

    #endregion

    #region Component General

    public static Tag External = new();

    /// use a specific predetermined sloping algorithm when filling volumes
    public static readonly Tag<SlopingAlgorithm> SlopingAlgorithm = new();

    /// apply sloping algorithm with different specific predetermined contexts
    public static readonly Tag<SlopeGrouping> SlopeGrouping = new();

    public static readonly Tag HasCustomSloping = new();

    public static Tag HasDebris = new();

    #endregion

    #region Floor

    /// floor is not hollow
    public static readonly Tag FloorSolid = new();

    public static readonly Tag FloorHollow = new();

    #endregion

    #region Wall

    #endregion

    #region Gap

    /// door of a horizontal gap is on the "outside" side of the gap. typically defaults to the middle of the gap, only affects wider gaps
    public static readonly Tag GapDoorOutside = new();

    /// door of a horizontal gap is on the "inside" side of the gap. typically defaults to the middle of the gap, only affects wider gaps
    public static readonly Tag GapDoorInside = new();

    #endregion

    #region Room

    public static readonly Tag RoomTypeStorage = new();

    public static readonly Tag RoomTypeLiving = new();

    public static readonly Tag RoomTypeBedroom = new();

    public static readonly Tag RoomTypeBathroom = new();

    public static readonly Tag RoomTypeStudy = new();

    public static readonly Tag RoomTypeWorkshop = new();

    /// if a room has windows that are spaced out by that room's own geometry, and the shapes that make them up
    public static readonly Tag<Shape[]> RoomHasWindows = new();

    /// if exact placement is required, the required window volumes in a room
    public static readonly Tag<Shape[]> RoomHasSpecificWindows = new();

    public static readonly Tag RoomHousingNotValid = new();

    public static readonly Tag RoomHousingValid = new();

    /// if a room has beams, what x-positions they are
    public static readonly Tag<int[]> RoomHasBeams = new();

    /// if exact placement is required, these are those beam x-positions in a room
    public static readonly Tag<int[]> RoomHasSpecificBeams = new();

    public static readonly Tag RoomBeamsAreTiles = new();

    public static readonly Tag RoomBeamsAreWalls = new();

    #endregion

    #region Stairway

    public static readonly Tag StairwayTilesLowerX = new();

    public static readonly Tag StairwayTilesHigherX = new();

    public static readonly Tag StairwayRequiresJumping = new();

    public static readonly Tag StairwayNotRequiresJumping = new();

    public static readonly Tag StairwayToHorizontalGap = new();

    public static readonly Tag StairwayToVerticalGap = new();

    #endregion

    #region Roof

    public static readonly Tag RoofFlat = new();
    
    /// roof is tall enough that it doesn't follow the full contour of the tiles it is placed on
    public static readonly Tag RoofTall = new();

    /// roof is short enough to generally follow the contour of the path
    public static readonly Tag RoofShort = new();

    public static readonly Tag<int[]> RoofHasChimney = new();

    public static readonly Tag<(int leftOverhang, int rightOverhang)> RoofHasOverhang = new();

    #endregion

    #region Palette

    public static readonly Tag PaletteWood = new();

    public static readonly Tag PaletteStone = new();

    public static readonly Tag PaletteDarkGrey = new();

    public static readonly Tag PaletteLightGrey = new();

    public static readonly Tag PaletteMediumGrey = new();

    public static readonly Tag PaletteDarkBrown = new();

    public static readonly Tag PaletteLightBrown = new();

    public static readonly Tag PaletteMediumBrown = new();

    public static readonly Tag PaletteRed = new();

    public static readonly Tag PaletteTurquoise = new();

    #endregion

    public static void SetInternalTagNames() {
        foreach (FieldInfo fieldInfo in typeof(Tags).GetFields(BindingFlags.Static | BindingFlags.Public)) ((Tag)fieldInfo.GetValue(null)!).Name = fieldInfo.Name;
    }
}