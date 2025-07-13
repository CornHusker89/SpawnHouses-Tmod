#nullable enable

using Terraria.ID;

namespace SpawnHouses.Types;

/// <summary>
///     Everything inside the struct will be initialized and valid, no matter the params to the constructor
/// </summary>
public class TilePalette {
    public readonly PaintedType BackgroundBeam;
    public readonly PaintedType BackgroundFloorAccent;
    public readonly PaintedType[] BackgroundFloorAlt;
    public readonly PaintedType BackgroundFloorMain;
    public readonly PaintedType BackgroundOutdoors;
    public readonly PaintedType BackgroundRoofAccent;
    public readonly PaintedType[] BackgroundRoofAlt;
    public readonly PaintedType BackgroundRoofMain;
    public readonly PaintedType BackgroundRoofSpecial;
    public readonly PaintedType BackgroundRoomAccent;
    public readonly PaintedType BackgroundRoomAccentElevated;
    public readonly PaintedType[] BackgroundRoomAlt;
    public readonly PaintedType[] BackgroundRoomAltElevated;
    public readonly PaintedType BackgroundRoomMain;
    public readonly PaintedType BackgroundRoomMainElevated;
    public readonly PaintedType BackgroundRoomSpecial;
    public readonly PaintedType BackgroundRoomWindow;
    public readonly PaintedType BackgroundRoomWindowFrame;
    public readonly PaintedType BackgroundWallAccent;
    public readonly PaintedType[] BackgroundWallAlt;
    public readonly PaintedType BackgroundWallMain;

    public readonly PaintedType FloorAccent;

    public readonly PaintedType FloorAccentElevated;

    public readonly PaintedType[] FloorAlt;

    public readonly PaintedType[] FloorAltElevated;

    // basic order: floor, wall, roof, background room, background roof
    // variant order: main, accent, alt, main-elv., accent-elv., alt-elv., misc
    public readonly PaintedType FloorMain;

    public readonly PaintedType FloorMainElevated;

    public readonly PaintedType FloorSpecial;
    public readonly ushort FurnitureStyle;

    public readonly PaintedType Platform;


    public readonly StructureTag[] PossibleTags;
    public readonly PaintedType RoofAccent;
    public readonly PaintedType[] RoofAlt;
    public readonly PaintedType RoofMain;
    public readonly PaintedType RoofSpecial;
    public readonly PaintedType WallAccent;
    public readonly PaintedType WallAccentElevated;
    public readonly PaintedType[] WallAlt;
    public readonly PaintedType[] WallAltElevated;
    public readonly PaintedType WallMain;
    public readonly PaintedType WallMainElevated;
    public readonly PaintedType WallSpecial;

    public TilePalette(
        StructureTag[] possibleTags,
        PaintedType floorMain, PaintedType wallMain, PaintedType roofMain, PaintedType backgroundFloorMain,
        PaintedType backgroundWallMain, PaintedType backgroundRoomMain, PaintedType backgroundRoofMain,
        PaintedType? floorAccent = null, PaintedType? wallAccent = null, PaintedType? roofAccent = null,
        PaintedType? backgroundFloorAccent = null, PaintedType? backgroundWallAccent = null,
        PaintedType? backgroundRoomAccent = null, PaintedType? backgroundRoofAccent = null,
        PaintedType[]? floorAlt = null, PaintedType[]? wallAlt = null, PaintedType[]? roofAlt = null,
        PaintedType[]? backgroundFloorAlt = null, PaintedType[]? backgroundWallAlt = null,
        PaintedType[]? backgroundRoomAlt = null, PaintedType[]? backgroundRoofAlt = null,
        PaintedType? floorSpecial = null, PaintedType? wallSpecial = null, PaintedType? roofSpecial = null,
        PaintedType? backgroundRoomSpecial = null, PaintedType? backgroundRoofSpecial = null,
        PaintedType? floorMainElevated = null, PaintedType? wallMainElevated = null,
        PaintedType? backgroundRoomMainElevated = null,
        PaintedType? floorAccentElevated = null, PaintedType? wallAccentElevated = null,
        PaintedType? backgroundRoomAccentElevated = null,
        PaintedType[]? floorAltElevated = null, PaintedType[]? wallAltElevated = null,
        PaintedType[]? backgroundRoomAltElevated = null,
        PaintedType? platform = null, PaintedType? backgroundRoomWindow = null,
        PaintedType? backgroundRoomWindowFrame = null, PaintedType? backgroundOutdoors = null,
        PaintedType? backgroundBeam = null, ushort furnitureStyle = 1
    ) {
        PossibleTags = possibleTags;

        FloorMain = floorMain;
        WallMain = wallMain;
        RoofMain = roofMain;
        BackgroundFloorMain = backgroundFloorMain;
        BackgroundWallMain = backgroundWallMain;
        BackgroundRoomMain = backgroundRoomMain;
        BackgroundRoofMain = backgroundRoofMain;

        FloorAccent = floorAccent ?? FloorMain;
        WallAccent = wallAccent ?? WallMain;
        RoofAccent = roofAccent ?? RoofMain;
        BackgroundFloorAccent = backgroundFloorAccent ?? BackgroundFloorMain;
        BackgroundWallAccent = backgroundWallAccent ?? BackgroundWallMain;
        BackgroundRoomAccent = backgroundRoomAccent ?? BackgroundRoomMain;
        BackgroundRoofAccent = backgroundRoofAccent ?? BackgroundRoofMain;

        FloorAlt = floorAlt ?? [FloorMain];
        WallAlt = wallAlt ?? [WallMain];
        RoofAlt = roofAlt ?? [RoofMain];
        BackgroundFloorAlt = backgroundFloorAlt ?? [BackgroundFloorMain];
        BackgroundWallAlt = backgroundWallAlt ?? [BackgroundWallMain];
        BackgroundRoomAlt = backgroundRoomAlt ?? [BackgroundRoomMain];
        BackgroundRoofAlt = backgroundRoofAlt ?? [BackgroundRoofMain];

        FloorSpecial = floorSpecial ?? FloorAccent;
        WallSpecial = wallSpecial ?? WallAccent;
        RoofSpecial = roofSpecial ?? RoofAccent;
        BackgroundRoomSpecial = backgroundRoomSpecial ?? BackgroundRoomAccent;
        BackgroundRoofSpecial = backgroundRoofSpecial ?? BackgroundRoofAccent;

        FloorMainElevated = floorMainElevated ?? floorMain;
        WallMainElevated = wallMainElevated ?? wallMain;
        BackgroundRoomMainElevated = backgroundRoomMainElevated ?? BackgroundRoomMain;

        FloorAccentElevated = floorAccentElevated ?? FloorAccent;
        WallAccentElevated = wallAccentElevated ?? WallAccent;
        BackgroundRoomAccentElevated = backgroundRoomAccentElevated ?? BackgroundRoomAccent;

        FloorAltElevated = floorAltElevated ?? FloorAlt;
        WallAltElevated = wallAltElevated ?? WallAlt;
        BackgroundRoomAltElevated = backgroundRoomAltElevated ?? BackgroundRoomAlt;

        Platform = platform ?? new PaintedType(TileID.Platforms);
        BackgroundRoomWindow = backgroundRoomWindow ?? BackgroundRoomMain;
        BackgroundRoomWindowFrame = backgroundRoomWindowFrame ?? BackgroundRoomAccent;
        BackgroundOutdoors = backgroundOutdoors ?? new PaintedType(WallID.Grass);

        BackgroundBeam = backgroundBeam ?? BackgroundRoomMain;
        FurnitureStyle = furnitureStyle;
    }

    /// <summary>
    ///     A palette with dark stone and wood, medieval looking
    /// </summary>
    public static TilePalette Palette1 => new(
        [
            StructureTag.Forest, StructureTag.Ice, StructureTag.Cavern, StructureTag.AboveGround,
            StructureTag.UnderGround
        ],
        new PaintedType(TileID.RichMahogany, PaintID.BrownPaint),
        new PaintedType(TileID.GrayBrick),
        new PaintedType(TileID.RedDynastyShingles),
        new PaintedType(WallID.Wood),
        new PaintedType(WallID.GrayBrick),
        new PaintedType(WallID.BlueDungeonSlab, PaintID.GrayPaint),
        new PaintedType(WallID.Wood),
        new PaintedType(TileID.LivingMahogany, PaintID.BrownPaint),
        new PaintedType(TileID.LivingWood),
        null,
        null,
        null,
        new PaintedType(WallID.Wood),
        null,
        [
            new PaintedType(TileID.LivingWood),
            new PaintedType(TileID.WoodBlock)
        ],
        [
            new PaintedType(TileID.GrayBrick),
            new PaintedType(TileID.Stone),
            new PaintedType(TileID.StoneSlab)
        ],
        null,
        null,
        [
            new PaintedType(WallID.GrayBrick), new PaintedType(WallID.Stone), new PaintedType(WallID.StoneSlab),
            new PaintedType(WallID.BlueDungeonSlab, PaintID.GrayPaint)
        ],
        [
            new PaintedType(WallID.GrayBrick), new PaintedType(WallID.Stone), new PaintedType(WallID.StoneSlab),
            new PaintedType(WallID.BlueDungeonSlab, PaintID.GrayPaint)
        ],
        null,
        null,
        new PaintedType(TileID.PalladiumColumn, PaintID.GrayPaint),
        null,
        null,
        null,
        null,
        null,
        null,
        null,
        null,
        new PaintedType(WallID.WhiteDynasty),
        null,
        null,
        null,
        new PaintedType(TileID.Platforms, PaintID.BrownPaint, 1),
        new PaintedType(WallID.WoodenFence, PaintID.BrownPaint),
        null,
        new PaintedType(WallID.Grass),
        new PaintedType(WallID.PalmWood, PaintID.BrownPaint)
    );
}