#nullable enable

using Terraria.ID;

namespace SpawnHouses.Types.Palette;

public class TilePalette {
    public StructureTag[] PossibleTags;

    public required PaintedTypeFloorSet ExteriorFloor;
    public required PaintedTypeFloorSet InteriorFloor;

    public required PaintedTypeWallSet ExteriorWall;
    public required PaintedTypeWallSet InteriorWall;

    public required PaintedTypeRoomSet LivingRoom;
    public required PaintedTypeDecorSet LivingDecor;
    public required PaintedTypeRoomSet BedroomRoom;
    public required PaintedTypeDecorSet BedroomDecor;
    public required PaintedTypeRoomSet StorageRoom;
    public required PaintedTypeDecorSet StorageDecor;
    public required PaintedTypeRoomSet WorkshopRoom;
    public required PaintedTypeDecorSet WorkshopDecor;

    #region Sets

    private static readonly PaintedTypeFloorSet FloorWoodRough = new() {
        Primary = new TilePaintedType([TileID.LivingMahogany, TileID.WoodBlock], [PaintID.BrownPaint, PaintID.None]),
        Vertical = new TilePaintedType(TileID.WoodBlock),
        PrimaryBackground = new WallPaintedType(WallID.LivingWood)
    };

    private static readonly PaintedTypeFloorSet FloorWoodSmooth = new() {
        Primary = new TilePaintedType([TileID.RichMahogany, TileID.LivingWood], [PaintID.BrownPaint, PaintID.None]),
        Vertical = new TilePaintedType(TileID.WoodBlock),
        PrimaryBackground = new WallPaintedType(WallID.LivingWood)
    };

    private static readonly PaintedTypeWallSet WallMixedStone = new() {
        Primary = new TilePaintedType([TileID.GrayBrick, TileID.Stone, TileID.StoneSlab]),
        VerticalDetail = new TilePaintedType(TileID.PalladiumColumn, PaintID.GrayPaint),
        NonTiling = new TilePaintedType(TileID.AccentSlab),
        Windows = new TilePaintedType(TileID.Glass, PaintID.GrayPaint),
        PrimaryBackground = new WallPaintedType(WallID.BlueDungeonSlab, PaintID.GrayPaint)
    };

    private static readonly PaintedTypeWallSet WallWoodSmooth = new() {
        Primary = new TilePaintedType([TileID.RichMahogany, TileID.LivingWood], [PaintID.BrownPaint, PaintID.None]),
        VerticalDetail = new TilePaintedType(TileID.LivingWood, PaintID.BrownPaint),
        NonTiling = new TilePaintedType(TileID.DynastyWood),
        Windows = new TilePaintedType(TileID.Glass, PaintID.GrayPaint),
        PrimaryBackground = new WallPaintedType(WallID.LivingWood)
    };

    private static readonly PaintedTypeRoomSet RoomLivingMedieval = new() {
        Primary = new WallPaintedType(WallID.WhiteDynasty),
        BottomAccent = new WallPaintedType(WallID.BlueDungeonSlab, PaintID.GrayPaint),
        HorizontalBeamBackground = new WallPaintedType(WallID.Wood),
        VerticalBeamBackground = new WallPaintedType(WallID.SpookyWood),
        BeamTile = new TilePaintedType(TileID.WoodenBeam),
        BeamTileActuation = false,
        WindowBackground = new WallPaintedType(WallID.Glass),
        Platform = new TilePaintedType(TileID.Platforms, PaintID.BrownPaint),
        Door = new TilePaintedType(TileID.ClosedDoor)
    };

    private static readonly PaintedTypeDecorSet DecorLivingMedieval = new() {
        Furniture2X1 = null,
        Furniture2X3 = new TilePaintedType(TileID.Pots),
        Furniture3X2 = new TilePaintedType(TileID.Dressers),
        Furniture3X3 = null, // new TilePaintedType(TileID.HeavyWorkBench),
        Bed = null,
        Workbench = new TilePaintedType(TileID.WorkBenches),
        Table = new TilePaintedType(TileID.Tables),
        Chair = new TilePaintedType(TileID.Chairs),
        Shelf = new TilePaintedType(TileID.Platforms),
        Fireplace = new TilePaintedType(TileID.Fireplace),
        ConventionalFireplace = true,
        Bookcase = new TilePaintedType(TileID.Bookcases),
        Decor1X1 = new TilePaintedType([TileID.Books, TileID.ShellPile, TileID.Bottles]),
        Decor2X2 = new TilePaintedType([TileID.MusicBoxes, TileID.ShipInABottle]),
        TableLight1X1 = new TilePaintedType(TileID.Candles),
        HangingLight1X2 = new TilePaintedType(TileID.HangingLanterns),
        Storage2X2 = null,
        WallDecor2X3 = new TilePaintedType(TileID.Painting2X3),
        WallDecor3X4 = new TilePaintedType(TileID.WeaponsRack2),
        AccentBanner = new TilePaintedType(TileID.Banners)
    };

    #endregion


    #region Palettes

    public static readonly TilePalette Stone1 = new() {
        ExteriorFloor = FloorWoodRough,
        InteriorFloor = FloorWoodSmooth,
        ExteriorWall = WallMixedStone,
        InteriorWall = WallWoodSmooth,

        LivingRoom = RoomLivingMedieval,
        LivingDecor = DecorLivingMedieval,
        BedroomRoom = RoomLivingMedieval,
        BedroomDecor = DecorLivingMedieval,
        StorageRoom = RoomLivingMedieval,
        StorageDecor = DecorLivingMedieval,
        WorkshopRoom = RoomLivingMedieval,
        WorkshopDecor = DecorLivingMedieval
    };

    #endregion
}