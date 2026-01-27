#nullable enable

namespace SpawnHouses.Common.Palette;

/// <summary>
///     <see cref="PaintedType" />s that is related to floors
/// </summary>
public class PaintedTypeFloorSet {
    /// <summary>
    ///     represents platforms on the outside of the room (like on gaps)
    /// </summary>
    public required TilePaintedType Platform;

    /// <summary>
    ///     for large-scale tile filling
    /// </summary>
    public required TilePaintedType Primary;

    /// <summary>
    ///     redundant wall for <see cref="Primary" />
    /// </summary>
    public required WallPaintedType PrimaryBackground;

    /// <summary>
    ///     for any vertical parts of the floor
    /// </summary>
    public required TilePaintedType Vertical;
}

/// <summary>
///     <see cref="PaintedType" />s that is related to walls
/// </summary>
public class PaintedTypeWallSet {
    /// <summary>
    ///     represents any doors
    /// </summary>
    public required TilePaintedType Door;

    /// <summary>
    ///     tiles that don't tile with <see cref="Primary" /> but follow that palette, ex. <see cref="Terraria.ID.TileID.AccentSlab" />
    /// </summary>
    public required TilePaintedType NonTiling;

    /// <summary>
    ///     for most large-scale tile filling
    /// </summary>
    public required TilePaintedType Primary;

    /// <summary>
    ///     redundant wall for <see cref="Primary" />
    /// </summary>
    public required WallPaintedType PrimaryBackground;

    /// <summary>
    ///     for tiles that have unusual geometry when placed vertically, ex. <see cref="Terraria.ID.TileID.PalladiumColumn" />
    /// </summary>
    public required TilePaintedType VerticalDetail;

    /// <summary>
    ///     tile for windows
    /// </summary>
    public required TilePaintedType Windows;
}

/// <summary>
///     <see cref="PaintedType" />s that is related to roofs
/// </summary>
public class PaintedTypeRoofSet {
    /// <summary>
    ///     tile for literal beams, which can be actuated
    /// </summary>
    public required TilePaintedType BeamTile;

    /// <summary>
    ///     whether the beam tile needs to be actuated
    /// </summary>
    public required bool BeamTileActuation;

    /// <summary>
    ///     wall that is placed near the bottom to provide an accent
    /// </summary>
    public required WallPaintedType BottomBackgroundAccent;

    /// <summary>
    ///     wall that represents horizontal "beams"
    /// </summary>
    public required WallPaintedType HorizontalBeamBackground;

    /// <summary>
    ///     tiles that don't tile with <see cref="Primary" /> but follow that palette, ex. <see cref="Terraria.ID.TileID.AccentSlab" />
    /// </summary>
    public required TilePaintedType NonTiling;

    /// <summary>
    ///     for most large-scale tile filling
    /// </summary>
    public required TilePaintedType Primary;

    /// <summary>
    ///     redundant wall for <see cref="Primary" />
    /// </summary>
    public required WallPaintedType PrimaryBackground;

    /// <summary>
    ///     wall that represents vertical "beams"
    /// </summary>
    public required WallPaintedType VerticalBeamBackground;
}

/// <summary>
///     <see cref="PaintedType" />s that is related to rooms
/// </summary>
public class PaintedTypeRoomSet {
    /// <summary>
    ///     tile for literal beams, which can be actuated
    /// </summary>
    public required TilePaintedType BeamTile;

    /// <summary>
    ///     whether the beam tile needs to be actuated
    /// </summary>
    public required bool BeamTileActuation;

    /// <summary>
    ///     wall that is placed near the bottom to provide an accent
    /// </summary>
    public required WallPaintedType BottomBackgroundAccent;

    /// <summary>
    ///     wall that represents horizontal "beams"
    /// </summary>
    public required WallPaintedType HorizontalBeamBackground;

    /// <summary>
    ///     for large-scale wall filling
    /// </summary>
    public required WallPaintedType Primary;

    /// <summary>
    ///     wall that represents vertical "beams"
    /// </summary>
    public required WallPaintedType VerticalBeamBackground;

    /// <summary>
    ///     the background walls for windows
    /// </summary>
    public required WallPaintedType WindowBackground;
}

/// <summary>
///     <see cref="PaintedType" />s that is related to decorating rooms
/// </summary>
public class PaintedTypeDecorSet {
    /// <summary>
    ///     banner which provides an accent color to the room, ex <see cref="Terraria.ID.TileID.Banners" />
    /// </summary>
    public required TilePaintedType? AccentBanner;

    /// <summary>
    ///     bed, ex <see cref="Terraria.ID.TileID.Beds" />
    /// </summary>
    public required TilePaintedType? Bed;

    /// <summary>
    ///     bookcase, ex <see cref="Terraria.ID.TileID.Bookcases" />
    /// </summary>
    public required TilePaintedType? Bookcase;

    /// <summary>
    ///     chair, can be either 1x1 or 1x2, ex <see cref="Terraria.ID.TileID.Chairs" />
    /// </summary>
    public required TilePaintedType? Chair;

    /// <summary>
    ///     if the fireplace is the normal fireplace tile, or if it's another 3-wide tile, ex <see cref="Terraria.ID.TileID.Campfire" />
    /// </summary>
    public required bool ConventionalFireplace;

    /// <summary>
    ///     generic 1-wide, 1-tall decoration, ex <see cref="Terraria.ID.TileID.Books" />
    /// </summary>
    public required TilePaintedType? Decor1X1;

    /// <summary>
    ///     generic 2-wide, 2-tall decoration, ex <see cref="Terraria.ID.TileID.MusicBoxes" />
    /// </summary>
    public required TilePaintedType? Decor2X2;

    /// <summary>
    ///     fireplace, ex <see cref="Terraria.ID.TileID.Fireplace" />
    /// </summary>
    public required TilePaintedType? Fireplace;

    /// <summary>
    ///     generic 2-wide, 1-tall furniture, ex. <see cref="Terraria.ID.TileID.Anvils" />
    /// </summary>
    public required TilePaintedType? Furniture2X1;

    /// <summary>
    ///     generic 2-wide, 3-tall furniture, ex. <see cref="Terraria.ID.TileID.HatRack" />
    /// </summary>
    public required TilePaintedType? Furniture2X3;

    /// <summary>
    ///     generic 3-wide, 2-tall furniture, ex. <see cref="Terraria.ID.TileID.Dressers" />
    /// </summary>
    public required TilePaintedType? Furniture3X2;

    /// <summary>
    ///     generic 3-wide, 3-wide furniture, ex <see cref="Terraria.ID.TileID.Furnaces" />
    /// </summary>
    public required TilePaintedType? Furniture3X3;

    /// <summary>
    ///     generic 1-wide, 2-tall light source, hung from ceilings, ex <see cref="Terraria.ID.TileID.HangingLanterns" />
    /// </summary>
    public required TilePaintedType? HangingLight1X2;

    /// <summary>
    ///     platform that functions as a shelf for holding decor
    /// </summary>
    public required TilePaintedType? Shelf;

    /// <summary>
    ///     generic 2-wide, 2-tall storage, ex <see cref="Terraria.ID.TileID.Containers" />
    /// </summary>
    public required TilePaintedType? Storage2X2;

    /// <summary>
    ///     table, different from <see cref="Workbench" />, ex <see cref="Terraria.ID.TileID.Tables" />
    /// </summary>
    public required TilePaintedType? Table;

    /// <summary>
    ///     generic 1-wide, 1-tall light source placed on tabletops and shelves, ex <see cref="Terraria.ID.TileID.Candles" />
    /// </summary>
    public required TilePaintedType? TableLight1X1;

    /// <summary>
    ///     any 2-wide, 3-tall wall mounted decoration, ex <see cref="Terraria.ID.TileID.Painting2X3" />
    /// </summary>
    public required TilePaintedType? WallDecor2X3;

    /// <summary>
    ///     any 3-wide, 4-tall wall mounted decoration, ex <see cref="Terraria.ID.TileID.WeaponsRack" />
    /// </summary>
    public required TilePaintedType? WallDecor3X4;

    /// <summary>
    ///     workbench, different from <see cref="Table" />, ex <see cref="Terraria.ID.TileID.WorkBenches" />
    /// </summary>
    public required TilePaintedType? Workbench;
}