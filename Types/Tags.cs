namespace SpawnHouses.Types;

public enum StructureTag {
    // ===== structureLayout =====
    IsSymmetric,
    HasHousing,
    HasLargeRoom,
    HasNoLargeRoom,
    HasFlatFloors,
    HasNoFlatFloors,
    HasSomeFlatFloors,
    FirstFloorStorage,

    /// the first floor has horizontal gaps wherever possible
    FirstFloorConnected,

    /// structure is categorized as being above ground (typically has a roof)
    AboveGround,

    /// structure is categorized as being below ground (typically has a no dedicated roof)
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
    Cavern
}

public enum RoomLayoutTag {
    // these are specific to each roomLayout, and not necessarily the whole structure
    HasHousing,
    HasFlatFloors,
    NoFlatFloors,
    SomeFlatFloors,

    /// room layout has entry points that go left/right (ex. is a door)
    HorizontalEntryPoints,

    /// room layout has entry points that go up/down (ex. is a platform)
    HasVerticalEntryPoints,

    /// will reliably generate a large, convenient room intended for storage
    FirstFloorStorage,

    /// the first floor has horizontal gaps wherever possible
    FirstFloorConnected
}

public enum ComponentTag {
    // ===== all =====
    Elevated,
    External,

    // ===== floor =====
    IsFloor,
    IsFloorGap,
    FloorSolid,
    FloorHollow,
    FloorGroundLevel,

    /// floor is either 1 or 2 tiles thick
    FloorThin,

    /// floor is larger than 2 tiles thick
    FloorThick,


    // ===== wall =====
    IsWall,
    IsWallGap,
    WallGroundLevel,


    // ===== decor =====
    HasDecor,
    DecorIsUnderGround,
    DecorGroundLevel,
    DecorElevated,


    // ===== stairway =====
    IsStairway,

    /// stairway is 3 or fewer tiles wide
    StairwayNarrow,

    /// stairway is 4 or 5 tiles wide
    StairwayMedium,

    /// stairway is 6 or greater tiles wide
    StairwayWide,


    // ===== background =====
    IsBackground,
    BackgroundHasWindow,
    BackgroundGroundLevel,


    // ===== roof =====
    IsRoof,
    RoofTall,
    RoofShort,
    RoofHasChimney,
    RoofSlope1To1,
    RoofSlopeLessThan1,
    RoofSlopeGreaterThan1,
    RoofSlopeNone,


    // ===== debug =====
    IsDebugBlocks,
    IsDebugWalls
}

public enum PaletteTag {
    Wood,
    Stone,
    DarkGrey,
    LightGrey,
    MediumGrey,
    DarkBrown,
    LightBrown,
    MediumBrown,
    Red,
    Turquoise
}