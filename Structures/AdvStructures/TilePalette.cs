#nullable enable

using Terraria;
using Terraria.ID;

namespace SpawnHouses.Structures.AdvStructures;

/// <summary>
/// Everything inside the struct will be initialized and valid, no matter the params to the constructor
/// </summary>
public class TilePalette
{
    public readonly StructureTag[] PossibleTags;
    
    // basic order: floor, wall, roof, background room, background roof
    // variant order: main, accent, alt, main-elv., accent-elv., alt-elv., misc
    public readonly PaintedType FloorMain;
    public readonly PaintedType WallMain;
    public readonly PaintedType RoofMain;
    public readonly PaintedType BackgroundFloorMain;
    public readonly PaintedType BackgroundWallMain;
    public readonly PaintedType BackgroundRoomMain;
    public readonly PaintedType BackgroundRoofMain;
    
    public readonly PaintedType FloorAccent;
    public readonly PaintedType WallAccent;
    public readonly PaintedType RoofAccent;
    public readonly PaintedType BackgroundFloorAccent;
    public readonly PaintedType BackgroundWallAccent;
    public readonly PaintedType BackgroundRoomAccent;
    public readonly PaintedType BackgroundRoofAccent;
    
    public readonly PaintedType[] FloorAlt;
    public readonly PaintedType[] WallAlt;
    public readonly PaintedType[] RoofAlt;
    public readonly PaintedType[] BackgroundFloorAlt;
    public readonly PaintedType[] BackgroundWallAlt;
    public readonly PaintedType[] BackgroundRoomAlt;
    public readonly PaintedType[] BackgroundRoofAlt;

    public readonly PaintedType FloorSpecial;
    public readonly PaintedType WallSpecial;
    public readonly PaintedType RoofSpecial;
    public readonly PaintedType BackgroundRoomSpecial;
    public readonly PaintedType BackgroundRoofSpecial;
    
    public readonly PaintedType FloorMainElevated;
    public readonly PaintedType WallMainElevated;
    public readonly PaintedType BackgroundRoomMainElevated;
    
    public readonly PaintedType FloorAccentElevated;
    public readonly PaintedType WallAccentElevated;
    public readonly PaintedType BackgroundRoomAccentElevated;
    
    public readonly PaintedType[] FloorAltElevated;
    public readonly PaintedType[] WallAltElevated;
    public readonly PaintedType[] BackgroundRoomAltElevated;

    public readonly PaintedType Platform;
    public readonly PaintedType BackgroundRoomWindow;
    public readonly PaintedType BackgroundRoomWindowFrame;
    public readonly PaintedType BackgroundOutdoors;

    public readonly ushort FurnitureStyle;
   
    public TilePalette(
        StructureTag[] possibleTags,
        PaintedType floorMain, PaintedType wallMain, PaintedType roofMain, PaintedType backgroundFloorMain, PaintedType backgroundWallMain, PaintedType backgroundRoomMain, PaintedType backgroundRoofMain,
        PaintedType? floorAccent = null, PaintedType? wallAccent = null, PaintedType? roofAccent = null, PaintedType? backgroundFloorAccent = null, PaintedType? backgroundWallAccent = null, PaintedType? backgroundRoomAccent = null, PaintedType? backgroundRoofAccent = null,
        PaintedType[]? floorAlt = null, PaintedType[]? wallAlt = null, PaintedType[]? roofAlt = null, PaintedType[]? backgroundFloorAlt = null, PaintedType[]? backgroundWallAlt = null, PaintedType[]? backgroundRoomAlt = null, PaintedType[]? backgroundRoofAlt = null,
        PaintedType? floorSpecial = null, PaintedType? wallSpecial = null, PaintedType? roofSpecial = null, PaintedType? backgroundRoomSpecial = null, PaintedType? backgroundRoofSpecial = null, 
        PaintedType? floorMainElevated = null, PaintedType? wallMainElevated = null, PaintedType? backgroundRoomMainElevated = null,
        PaintedType? floorAccentElevated = null, PaintedType? wallAccentElevated = null, PaintedType? backgroundRoomAccentElevated = null,
        PaintedType[]? floorAltElevated = null, PaintedType[]? wallAltElevated = null, PaintedType[]? backgroundRoomAltElevated = null,
        PaintedType? platform = null, PaintedType? backgroundRoomWindow = null, PaintedType? backgroundRoomWindowFrame = null, PaintedType? backgroundOutdoors = null,
        ushort furnitureStyle = 1
        )
    {
        PossibleTags = possibleTags;
        
        FloorMain = floorMain;
        WallMain = wallMain;
        RoofMain = roofMain;
        BackgroundFloorMain = backgroundFloorMain;
        BackgroundWallMain = backgroundWallMain;
        BackgroundRoomMain = backgroundRoomMain;
        BackgroundRoofMain = backgroundRoofMain;
        
        FloorAccent = floorAccent ?? floorMain;
        WallAccent = wallAccent ?? wallMain;
        RoofAccent = roofAccent ?? roofMain;
        BackgroundFloorAccent = backgroundFloorAccent ?? backgroundFloorMain;
        BackgroundWallAccent = backgroundWallAccent ?? backgroundWallMain;
        BackgroundRoomAccent = backgroundRoomAccent ?? backgroundRoomMain;
        BackgroundRoofAccent = backgroundRoofAccent ?? backgroundRoofMain;
        
        FloorAlt = floorAlt ?? [floorMain];
        WallAlt = wallAlt ?? [wallMain];
        RoofAlt = roofAlt ?? [roofMain];
        BackgroundFloorAlt = backgroundFloorAlt ?? [backgroundFloorMain];
        BackgroundWallAlt = backgroundWallAlt ?? [backgroundWallMain];
        BackgroundRoomAlt = backgroundRoomAlt ?? [backgroundRoomMain];
        BackgroundRoofAlt = backgroundRoofAlt ?? [backgroundRoofMain];

        FloorSpecial = floorSpecial ?? FloorAccent;
        WallSpecial = wallSpecial ?? WallAccent;
        RoofSpecial = roofSpecial ?? RoofAccent;
        BackgroundRoomSpecial = backgroundRoomSpecial ?? BackgroundRoomAccent;
        BackgroundRoofSpecial = backgroundRoofSpecial ?? BackgroundRoofAccent;
        
        FloorMainElevated = floorMainElevated ?? floorMain;
        WallMainElevated = wallMainElevated ?? wallMain;
        BackgroundRoomMainElevated = backgroundRoomMainElevated ?? backgroundRoomMain;
        
        FloorAccentElevated = floorAccentElevated ?? FloorAccent;
        WallAccentElevated = wallAccentElevated ?? WallAccent;
        BackgroundRoomAccentElevated = backgroundRoomAccentElevated ?? BackgroundRoomAccent;
        
        FloorAltElevated = floorAltElevated ?? FloorAlt;
        WallAltElevated = wallAltElevated ?? WallAlt;
        BackgroundRoomAltElevated = backgroundRoomAltElevated ?? BackgroundRoomAlt;
        
        Platform = platform ?? new PaintedType(TileID.Platforms);
        BackgroundRoomWindow = backgroundRoomWindow ?? backgroundRoomMain;
        BackgroundRoomWindowFrame = backgroundRoomWindowFrame ?? backgroundRoomMain;
        BackgroundOutdoors = backgroundOutdoors ?? new PaintedType(WallID.Grass);

        FurnitureStyle = furnitureStyle;
    }
    
    
    // for easy copy-pasting
    // new PaintedType(TileID, PaintID),
    
    /// <summary>
    /// A palette with dark stone and wood, medieval looking
    /// </summary>
    public static TilePalette Palette1()
    {
        return new TilePalette(
            [StructureTag.Forest, StructureTag.Ice, StructureTag.Cavern, StructureTag.AboveGround, StructureTag.UnderGround],
            new PaintedType(TileID.StoneSlab, PaintID.None),
            new PaintedType(TileID.GrayBrick, PaintID.None),
            new PaintedType(TileID.RedDynastyShingles, PaintID.YellowPaint),
            new PaintedType(WallID.Wood, PaintID.None),
            new PaintedType(WallID.GrayBrick, PaintID.None),
            new PaintedType(WallID.BlueDungeon, PaintID.GrayPaint),
            new PaintedType(WallID.Wood, PaintID.None),
            null,
            new PaintedType(TileID.DynastyWood, PaintID.BrownPaint),
            null,
            null,
            null,
            new PaintedType(WallID.Wood, PaintID.None),
            null,
            [new PaintedType(TileID.GrayBrick, PaintID.None), new PaintedType(TileID.Stone, PaintID.None), new PaintedType(TileID.StoneSlab, PaintID.None)],
            [new PaintedType(TileID.GrayBrick, PaintID.None), new PaintedType(TileID.Stone, PaintID.None), new PaintedType(TileID.StoneSlab, PaintID.None)],
            null,
            null,
            [new PaintedType(WallID.GrayBrick, PaintID.None), new PaintedType(WallID.Stone, PaintID.None), new PaintedType(WallID.StoneSlab, PaintID.None), new PaintedType(WallID.BlueDungeon, PaintID.GrayPaint)],
            [new PaintedType(WallID.GrayBrick, PaintID.None), new PaintedType(WallID.Stone, PaintID.None), new PaintedType(WallID.StoneSlab, PaintID.None), new PaintedType(WallID.BlueDungeon, PaintID.GrayPaint)],
            null,
            null,
            new PaintedType(TileID.PalladiumColumn, PaintID.GrayPaint),
            null,
            null,
            null,
            new PaintedType(TileID.DynastyWood, PaintID.BrownPaint),
            null,
            null,
            new PaintedType(TileID.DynastyWood, PaintID.BrownPaint),
            null,
            new PaintedType(WallID.WhiteDynasty, PaintID.None),
            null,
            null,
            null,
            new PaintedType(TileID.Platforms, PaintID.BrownPaint, 1),
            new PaintedType(WallID.WoodenFence, PaintID.BrownPaint),
            null,
            new PaintedType(WallID.Grass, PaintID.None),
            1
        );
    }
}