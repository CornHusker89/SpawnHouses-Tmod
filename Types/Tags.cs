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


    // ===== wall =====
    IsWall,
    IsWallGap,
    WallGroundLevel,


    // ===== background =====
    IsBackground,
    BackgroundHasWindow,
    BackgroundGroundLevel,


    // ===== stairway =====
    IsStairway,
    StairwayRequiresJumping,
    StairwayNotRequiresJumping,


    // ===== decor =====
    IsDecor,
    DecorIsUnderGround,
    DecorGroundLevel,
    DecorElevated,


    // ===== roof =====
    IsRoof,
    RoofTall,
    RoofShort,
    RoofHasChimney,
    RoofSlope1To1,
    RoofSlopeLessThan1,
    RoofSlopeGreaterThan1,
    RoofSlopeNone,


    // ===== gap =====


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