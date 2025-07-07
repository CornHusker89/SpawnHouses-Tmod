using Terraria;
using Terraria.ID;
using Terraria.WorldBuilding;

namespace SpawnHouses.Types;

/// <summary>
///     has many of the same properties as the tML Tile, but uses direct references and has a few more properties
/// </summary>
public class StructureTile {
    // Slopes

    /// <summary>
    ///     The <see cref="Slope" /> and <see cref="IsHalfBlock" /> of this tile combined, which can be changed by hammering.
    ///     <br />
    ///     Legacy/vanilla equivalent is <see cref="blockType" />.
    /// </summary>
    public BlockType BlockType = BlockType.Solid;

    /// <summary>
    ///     Whether there is an actuator at this position.<br />
    ///     Legacy/vanilla equivalent is <see cref="actuator()" /> or <see cref="actuator(bool)" />.
    /// </summary>
    public bool HasActuator;

    /// <summary>
    ///     Whether there is a tile at this position. Check this whenever you are accessing data from a tile to avoid getting
    ///     data from an empty tile.<br />
    ///     Legacy/vanilla equivalent is <see cref="active()" /> or <see cref="active(bool)" />.
    /// </summary>
    /// <remarks>
    ///     Actuated tiles are not solid, so use <see cref="HasUnactuatedTile" /> instead of <see cref="HasTile" /> for
    ///     collision checks.<br />
    ///     This only corresponds to whether a tile exists, however, a wall can exist without a tile. To check if a wall
    ///     exists, use <c>tile.WallType != WallID.None</c>.
    /// </remarks>
    public bool HasTile;

    /// <summary>
    ///     Whether the tile at this position is actuated by an actuator.<br />
    ///     Legacy/vanilla equivalent is <see cref="inActive()" /> or <see cref="inActive(bool)" />.
    /// </summary>
    /// <remarks>
    ///     Actuated tiles are <strong>not</strong> solid.
    /// </remarks>
    public bool IsActuated;

    // Custom fields

    public bool IsExteriorComponent;

    public bool IsFloor;

    public bool IsGap;

    public bool IsInside;

    /// <summary>
    ///     If true, when the tilemap is pasted, the original tile here will remain
    /// </summary>
    public bool IsNullTile;

    /// <summary>
    ///     If true, when the tilemap is pasted, the original wall here will remain
    /// </summary>
    public bool IsNullWall;

    public bool IsOutside;

    public bool IsWall;

    // Colors

    /// <summary>
    ///     The <see cref="PaintID" /> the tile at this position is painted with. Is <see cref="PaintID.None" /> if not
    ///     painted.<br />
    ///     Legacy/vanilla equivalent is <see cref="color()" /> or <see cref="color(byte)" />.
    /// </summary>
    public byte TileColor;

    // General state

    /// <summary>
    ///     The <see cref="TileID" /> of the tile at this position.<br />
    ///     This value is only valid if <see cref="HasTile" /> is true.<br />
    ///     Legacy/vanilla equivalent is <see cref="type" />.
    /// </summary>
    public ushort TileType;

    /// <summary>
    ///     The <see cref="PaintID" /> the wall at this position is painted with. Is <see cref="PaintID.None" /> if not
    ///     painted.<br />
    ///     Legacy/vanilla equivalent is <see cref="wallColor()" /> or <see cref="wallColor(byte)" />.
    /// </summary>
    public byte WallColor;

    /// <summary>
    ///     The <see cref="WallID" /> of the wall at this position.<br />
    ///     A value of 0 indicates no wall.<br />
    ///     Legacy/vanilla equivalent is <see cref="wall" />.
    /// </summary>
    public ushort WallType;

    /// <summary>
    ///     Whether there is a tile at this position that isn't actuated.<br />
    ///     Legacy/vanilla equivalent is <see cref="nactive" />.
    /// </summary>
    /// <remarks>
    ///     Actuated tiles are not solid, so use <see cref="HasUnactuatedTile" /> instead of <see cref="HasTile" /> for
    ///     collision checks.<br />
    ///     When checking if a tile exists, use <see cref="HasTile" /> instead of <see cref="HasUnactuatedTile" />.
    /// </remarks>
    public bool HasUnactuatedTile => HasTile && !IsActuated;

    /// <summary>
    ///     Whether a tile's <see cref="Slope" /> has a solid top side (<see cref="BlockType.SlopeDownLeft" /> or
    ///     <see cref="BlockType.SlopeDownRight" />).<br />
    ///     Legacy/vanilla equivalent is <see cref="topSlope" />.
    /// </summary>
    public bool TopSlope => BlockType == BlockType.SlopeDownLeft || BlockType == BlockType.SlopeDownRight;

    /// <summary>
    ///     Whether a tile's <see cref="Slope" /> has a solid bottom side (<see cref="BlockType.SlopeUpLeft" /> or
    ///     <see cref="BlockType.SlopeUpRight" />).<br />
    ///     Legacy/vanilla equivalent is <see cref="bottomSlope" />.
    /// </summary>
    public bool BottomSlope => BlockType == BlockType.SlopeUpLeft || BlockType == BlockType.SlopeUpRight;

    /// <summary>
    ///     Whether a tile's <see cref="Slope" /> has a solid left side (<see cref="BlockType.SlopeDownRight" /> or
    ///     <see cref="BlockType.SlopeUpRight" />).<br />
    ///     Legacy/vanilla equivalent is <see cref="leftSlope" />.
    /// </summary>
    public bool LeftSlope => BlockType == BlockType.SlopeDownRight || BlockType == BlockType.SlopeUpRight;

    /// <summary>
    ///     Whether a tile's <see cref="Slope" /> has a solid right side (<see cref="BlockType.SlopeDownLeft" /> or
    ///     <see cref="BlockType.SlopeUpLeft" />).<br />
    ///     Legacy/vanilla equivalent is <see cref="rightSlope" />.
    /// </summary>
    public bool RightSlope => BlockType == BlockType.SlopeDownLeft || BlockType == BlockType.SlopeUpLeft;


    /// <summary>
    ///     Resets the tile data at this position.<br />
    ///     Sets <see cref="HasTile" /> and <see cref="IsActuated" /> to <see langword="false" /> and sets the
    ///     <see cref="BlockType" /> to <see cref="Terraria.ID.BlockType.Solid" />.
    /// </summary>
    /// <remarks>
    ///     Does not reset data related to walls, wires, or anything else. For that, use <see cref="ClearEverything" />.
    /// </remarks>
    public void ClearTile() {
        BlockType = BlockType.Solid;
        HasTile = false;
        IsActuated = false;
    }

    /// <summary>
    ///     copies all of this data to Main.tile at the given position
    /// </summary>
    /// <param name="x"></param>
    /// <param name="y"></param>
    public void CopyTile(int x, int y) {
        Tile tile = Main.tile[x, y];
        if (!IsNullTile) {
            tile.TileType = TileType;
            tile.HasTile = HasTile;
            tile.IsActuated = IsActuated;
            tile.HasActuator = HasActuator;
            tile.TileColor = TileColor;
            tile.BlockType = BlockType;
        }

        if (!IsNullWall) {
            tile.WallType = WallType;
            tile.WallColor = WallColor;
        }

        WorldUtils.TileFrame(x, y);
        Framing.WallFrame(x, y);
    }
}