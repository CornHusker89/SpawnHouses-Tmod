#nullable enable
using SpawnHouses.Helpers;

namespace SpawnHouses.Types.TagTypes;

public static class Tags {
    #region Structure

    public static Tag IsSymmetric = new(1);

    public static Tag<int> HasRooms = new(2);

    public static Tag<int> HasHousing = new(3);

    public static Tag HasNoRectangleRooms = new(4);

    /// has both rectangular and non-uniform rooms
    public static Tag HasSomeRectangleRooms = new(5);

    /// there is a convenient large room intended for general use
    public static Tag HasLargeRoom = new(6);

    /// there is a convenient large room intended for storage
    public static Tag HasStorage = new(7);

    /// the structure is NOT made of materials that have potential to significantly screw up progression (ex hard mode ores)
    public static Tag ProgressionSafe = new(8);

    /// structure is categorized as being below ground (typically has a no dedicated roof)
    public static Tag UnderGround = new(9);

    /// structure is categorized as having an overall forest theme
    public static Tag Forest = new(10);

    /// structure is categorized as having an overall icy/cold theme
    public static Tag Ice = new(11);

    /// structure is categorized as having an overall beach theme
    public static Tag Beach = new(12);

    /// structure is categorized as having an overall jungle theme
    public static Tag Jungle = new(13);

    /// structure is categorized as having an overall cavern/underground theme
    public static Tag Cavern = new(14);

    #endregion

    #region Component General

    public static Tag External = new(15);

    /// use sloping algorithm when filling volumes
    public static Tag<SlopingAlgorithm> ApplySloping = new(16);

    /// apply sloping algorithm with different contexts
    public static Tag<SlopeModifier> SlopingModifier = new(17);

    public static Tag HasDebris = new(18);

    #endregion

    #region Floor

    public static Tag IsFloorGap = new(19);

    /// floor is not hollow
    public static Tag FloorSolid = new(20);

    public static Tag FloorHollow = new(21);

    #endregion

    #region Wall

    public static Tag IsWallGap = new(22);

    #endregion

    #region Room

    public static Tag RoomTypeStorage = new(23);

    public static Tag RoomTypeLiving = new(24);

    public static Tag RoomTypeBedroom = new(25);

    public static Tag RoomTypeWorkshop = new(26);

    public static Tag RoomHasWindow = new(27);

    public static Tag RoomHousingNotValid = new(28);

    public static Tag RoomHousingValid = new(29);

    public static Tag RoomHasArbitraryBeams = new(30);

    public static Tag<int[]> RoomHasSpecificBeams = new(31);

    public static Tag RoomBeamsAreTiles = new(32);

    #endregion

    #region Stairway

    public static Tag StairwayTilesLowerX = new(33);

    public static Tag StairwayTilesHigherX = new(34);

    public static Tag StairwayRequiresJumping = new(35);

    public static Tag StairwayNotRequiresJumping = new(36);

    public static Tag StairwayToHorizontalGap = new(37);

    public static Tag StairwayToVerticalGap = new(38);

    #endregion

    #region Roof

    /// roof is tall enough that it doesn't follow the contour of the tiles it is placed on
    public static Tag RoofTall = new(39);

    /// roof is short enough to generally follow the contour of the path
    public static Tag RoofShort = new(40);

    public static Tag<int[]> RoofHasChimney = new(41);

    public static Tag<(int, int)> RoofHasOverhang = new(42);

    #endregion

    #region Gap

    /// door of a horizontal gap is on the "outside" side of the gap. typically defaults to the middle of the gap, only affects wide gaps
    public static Tag GapDoorOutside = new(43);

    /// door of a horizontal gap is on the "inside" side of the gap. typically defaults to the middle of the gap, only affects wide gaps
    public static Tag GapDoorInside = new(44);

    #endregion

    #region Palette

    public static Tag PaletteWood = new(45);

    public static Tag PaletteStone = new(46);

    public static Tag PaletteDarkGrey = new(47);

    public static Tag PaletteLightGrey = new(48);

    public static Tag PaletteMediumGrey = new(49);

    public static Tag PaletteDarkBrown = new(50);

    public static Tag PaletteLightBrown = new(51);

    public static Tag PaletteMediumBrown = new(52);

    public static Tag PaletteRed = new(53);

    public static Tag PaletteTurquoise = new(54);

    #endregion
}