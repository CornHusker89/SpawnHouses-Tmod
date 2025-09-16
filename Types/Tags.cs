namespace SpawnHouses.Types;

public enum StructureTag {
    // ===== structureLayout =====
    IsSymmetric = 1,
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
    
    /// when filling volumes, use <see cref="Helpers.SlopeHelper.SimpleSlopes"/>
    UseSimpleSloping = 25,
    /// when filling volumes, use <see cref="Helpers.SlopeHelper.GothicSlopes"/>
    UseGothicSloping = 26,
    /// when filling volumes, use <see cref="Helpers.SlopeHelper.HalfSlopes"/>
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
    
    RoofShort = 16,
    RoofHasChimney = 17,
    RoofSlope1To1 = 18,
    RoofSlopeLessThan1 = 19,
    RoofSlopeGreaterThan1 = 20,
    RoofSlopeNone = 21,
    
    /// roof has an overhand of more than 1 tile
    RoofHasLargeOverhang = 22,


    // ===== gap =====


    // ===== debug =====
    IsDebugBlocks = 23,
    IsDebugWalls = 24
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