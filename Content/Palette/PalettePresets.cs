using Terraria.ID;

namespace SpawnHouses.Content.Palette;

public class PalettePresets {
    #region Set Presets

    private static readonly PaintedTypeFloorSet FloorWoodRough = new() {
        Primary = new TilePaintedType([TileID.LivingMahogany, TileID.WoodBlock], [PaintID.BrownPaint, PaintID.None]),
        Vertical = new TilePaintedType(TileID.WoodBlock),
        PrimaryBackground = new WallPaintedType(WallID.LivingWood),
        Platform = new TilePaintedType(TileID.Platforms, PaintID.BrownPaint)
    };

    private static readonly PaintedTypeFloorSet FloorWoodSmooth = new() {
        Primary = new TilePaintedType([TileID.DynastyWood, TileID.WoodBlock, TileID.LivingWood], [PaintID.BrownPaint, PaintID.None, PaintID.None]),
        Vertical = new TilePaintedType(TileID.WoodBlock),
        PrimaryBackground = new WallPaintedType(WallID.LivingWood),
        Platform = new TilePaintedType(TileID.Platforms, PaintID.BrownPaint)
    };

    private static readonly PaintedTypeWallSet WallMixedStone = new() {
        Primary = new TilePaintedType([TileID.GrayBrick, TileID.Stone, TileID.StoneSlab]),
        VerticalDetail = new TilePaintedType(TileID.PalladiumColumn, PaintID.GrayPaint),
        NonTiling = new TilePaintedType(TileID.AccentSlab),
        Windows = new TilePaintedType(TileID.Glass, PaintID.GrayPaint),
        PrimaryBackground = new WallPaintedType(WallID.Lava2Echo, PaintID.GrayPaint),
        Door = new TilePaintedType(TileID.ClosedDoor)
    };

    private static readonly PaintedTypeWallSet WallWoodSmooth = new() {
        Primary = new TilePaintedType([TileID.DynastyWood, TileID.WoodBlock, TileID.LivingWood], [PaintID.BrownPaint, PaintID.None, PaintID.None]),
        VerticalDetail = new TilePaintedType(TileID.LivingWood, PaintID.BrownPaint),
        NonTiling = new TilePaintedType(TileID.DynastyWood),
        Windows = new TilePaintedType(TileID.Glass, PaintID.GrayPaint),
        PrimaryBackground = new WallPaintedType(WallID.LivingWood),
        Door = new TilePaintedType(TileID.ClosedDoor)
    };

    private static readonly PaintedTypeRoofSet RoofShinglesDynastyWood = new() {
        Primary = new TilePaintedType(TileID.BlueDynastyShingles, PaintID.BrownPaint),
        NonTiling = new TilePaintedType(TileID.RedDynastyShingles, PaintID.BrownPaint),
        BottomBackgroundAccent = new WallPaintedType(WallID.Lava2Echo, PaintID.GrayPaint),
        HorizontalBeamBackground = new WallPaintedType(WallID.Wood),
        VerticalBeamBackground = new WallPaintedType(WallID.LargeBambooBlockWall, PaintID.BrownPaint),
        BeamTile = new TilePaintedType(TileID.WoodenBeam),
        BeamTileActuation = false,
        PrimaryRoofBackground = new WallPaintedType(WallID.CopperBrick, PaintID.BrownPaint),
        PrimaryInteriorBackground = new WallPaintedType(WallID.GrayBrick)
    };

    private static readonly PaintedTypeRoomSet RoomBedroomMedievalWhite = new() {
        Primary = new WallPaintedType(WallID.GrinchFingerWallpaper, PaintID.WhitePaint),
        BottomBackgroundAccent = new WallPaintedType(WallID.Lava2Echo, PaintID.GrayPaint),
        HorizontalBeamBackground = new WallPaintedType(WallID.Wood),
        VerticalBeamBackground = new WallPaintedType(WallID.LargeBambooBlockWall, PaintID.BrownPaint),
        BeamTile = new TilePaintedType(TileID.WoodenBeam),
        BeamTileActuation = false,
        WindowBackground = new WallPaintedType(WallID.Glass)
    };

    private static readonly PaintedTypeRoomSet RoomLivingMedieval = new() {
        Primary = new WallPaintedType(WallID.GrinchFingerWallpaper, PaintID.WhitePaint),
        BottomBackgroundAccent = new WallPaintedType([WallID.Lava2Echo, WallID.GrayBrick, WallID.Stone, WallID.StoneSlab], [PaintID.GrayPaint, PaintID.None, PaintID.None, PaintID.None]),
        HorizontalBeamBackground = new WallPaintedType(WallID.Wood),
        VerticalBeamBackground = new WallPaintedType(WallID.LargeBambooBlockWall, PaintID.BrownPaint),
        BeamTile = new TilePaintedType(TileID.WoodenBeam),
        BeamTileActuation = false,
        WindowBackground = new WallPaintedType(WallID.Glass)
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

    #region Palette Presets

    public static readonly TilePalette Medieval = new() {
        ExternalFloor = FloorWoodRough,
        InternalFloor = FloorWoodSmooth,
        ExternalWall = WallMixedStone,
        InternalWall = WallWoodSmooth,
        Roof = RoofShinglesDynastyWood,

        LivingRoom = RoomBedroomMedievalWhite,
        LivingDecor = DecorLivingMedieval,
        BedroomRoom = RoomBedroomMedievalWhite,
        BedroomDecor = DecorLivingMedieval,
        StorageRoom = RoomBedroomMedievalWhite,
        StorageDecor = DecorLivingMedieval,
        WorkshopRoom = RoomBedroomMedievalWhite,
        WorkshopDecor = DecorLivingMedieval,

        Debris1X1 = new TilePaintedType(TileID.Cobweb)
    };

    #endregion
}