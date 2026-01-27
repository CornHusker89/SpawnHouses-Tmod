#nullable enable
using SpawnHouses.Helpers;

namespace SpawnHouses.Common.Tagging;

#pragma warning disable CA2211

/// <summary>
///     Tag saving/loading is based on the tag's name, so don't change them once implemented
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

    /// there is a convenient large room intended for general use
    public static Tag HasLargeRoom = new();

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

    /// use sloping algorithm when filling volumes
    public static Tag<SlopingAlgorithm> ApplySloping = new();

    /// apply sloping algorithm with different contexts
    public static Tag<SlopeModifier> SlopingModifier = new();

    public static Tag HasDebris = new();

    #endregion

    #region Floor

    /// floor is not hollow
    public static Tag FloorSolid = new();

    public static Tag FloorHollow = new();

    #endregion

    #region Wall

    #endregion

    #region Room

    public static Tag RoomTypeStorage = new();

    public static Tag RoomTypeLiving = new();

    public static Tag RoomTypeBedroom = new();

    public static Tag RoomTypeWorkshop = new();

    public static Tag RoomHasWindow = new();

    public static Tag RoomHousingNotValid = new();

    public static Tag RoomHousingValid = new();

    public static Tag RoomHasArbitraryBeams = new();

    public static Tag<int[]> RoomHasSpecificBeams = new();

    public static Tag RoomBeamsAreTiles = new();

    #endregion

    #region Stairway

    public static Tag StairwayTilesLowerX = new();

    public static Tag StairwayTilesHigherX = new();

    public static Tag StairwayRequiresJumping = new();

    public static Tag StairwayNotRequiresJumping = new();

    public static Tag StairwayToHorizontalGap = new();

    public static Tag StairwayToVerticalGap = new();

    #endregion

    #region Roof

    /// roof is tall enough that it doesn't follow the full contour of the tiles it is placed on
    public static Tag RoofTall = new();

    /// roof is short enough to generally follow the contour of the path
    public static Tag RoofShort = new();

    public static Tag<int[]> RoofHasChimney = new();

    public static Tag<(int, int)> RoofHasOverhang = new();

    #endregion

    #region Gap

    /// door of a horizontal gap is on the "outside" side of the gap. typically defaults to the middle of the gap, only affects wide gaps
    public static Tag GapDoorOutside = new();

    /// door of a horizontal gap is on the "inside" side of the gap. typically defaults to the middle of the gap, only affects wide gaps
    public static Tag GapDoorInside = new();

    #endregion

    #region Palette

    public static Tag PaletteWood = new();

    public static Tag PaletteStone = new();

    public static Tag PaletteDarkGrey = new();

    public static Tag PaletteLightGrey = new();

    public static Tag PaletteMediumGrey = new();

    public static Tag PaletteDarkBrown = new();

    public static Tag PaletteLightBrown = new();

    public static Tag PaletteMediumBrown = new();

    public static Tag PaletteRed = new();

    public static Tag PaletteTurquoise = new();

    #endregion
}