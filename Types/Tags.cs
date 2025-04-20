namespace SpawnHouses.Types;

public enum StructureTag {
    // ===== structureLayout =====
    IsStructure,
    IsSymmetric,
    StructureHasLargeRoom,
    StructureHasNoLargeRoom,
    StructureHasFlatFloors,
    StructureHasNoFlatFloors,
    StructureHasSomeFlatFloors,
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
    Cavern,


    // ===== roomLayout =====
    // these are specific to each roomLayout, and not necessarily the whole structure
    HasRoomLayout,
    HasHousing,
    RoomLayoutHasFlatFloors,
    RoomLayoutHasNoFlatFloors,
    RoomLayoutHasSomeFlatFloors,


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

    /// stairway is 4 or 5 tiles wide
    StairwayMedium,

    /// stairway is 6 or greater tiles wide
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
    RoofSlopeNone,


    // ===== debug =====
    DebugBlocks,
    DebugWalls
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