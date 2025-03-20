namespace SpawnHouses.Types;

public enum StructureTag
{
    // ===== structureLayout =====
    HasHousing,
    IsSymmetric,
    /// structure is categorized as being above ground (typically has a roof)
    AboveGround,
    /// structure is categorized as being below ground (typically has a no roof)
    UnderGround,
    /// structure is categorized as having an overall forest theme
    Forest,
    /// structure is categorized as having an overall icy/cold theme
    Ice,
    /// structure is categorized as having an overall beach theme
    Beach,
    /// structure is categorized as having an overall jungle theme
    Jungle,
    /// structure is categorized as having an overall cavern/underground theme
    Cavern,


    // ===== roomLayout =====
    HasRoomLayout,
    PrebuiltRoomLayout,
    ProceduralRoomLayout,
    HasStorageRoom,
    HasNoStorageRoom,
    HasLargeRoom,
    HasNoLargeRoom,
    HasFlatFloors,
    HasNoFlatFloors,
    HasSomeFlatFloors,


    // ===== floor =====
    HasFloor,
    IsFloorGap,
    FloorSolid,
    FloorHollow,
    FloorGroundLevel,
    FloorElevated,
    /// floor is either 1 or 2 tiles thick
    FloorThin,
    /// floor is larger than 2 tiles thick
    FloorThick,


    // ===== wall =====
    HasWall,
    IsWallGap,
    WallGroundLevel,
    WallElevated,


    // ===== decor =====
    HasDecor,
    DecorIsUnderGround,
    DecorGroundLevel,
    DecorElevated,


    // ===== stairway =====
    HasStairway,
    /// stairway is 3 or fewer tiles wide
    StairwayNarrow,
    /// stairway is greater than 3 tiles wide
    StairwayWide,


    // ===== background =====
    HasBackground,
    BackgroundHasWindow,
    BackgroundGroundLevel,
    BackgroundElevated,


    // ===== roof =====
    HasRoof,
    RoofTall,
    RoofShort,
    HasChimney,
    RoofSlope1To1,
    RoofSlopeLessThan1,
    RoofSlopeGreaterThan1,
    RoofSlopeNone
}

public enum PaletteTag
{
    Wood,
    Stone,
    DarkGrey,
    LightGrey,
    MediumGrey,
    DarkBrown,
    LightBrown,
    MediumBrown,
    Red,
    Turquoise,
}