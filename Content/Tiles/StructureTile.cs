using System.Reflection;
using SpawnHouses.Helpers;
using Terraria;
using Terraria.DataStructures;
using Terraria.ID;

namespace SpawnHouses.Content.Tiles;

/// <summary>
///     has many of the same properties as the tML Tile, but uses direct references and has a few more properties
/// </summary>
public class StructureTile {
    
    /// <summary>
    ///     Resets the tile data at this position.<br />
    ///     Sets <see cref="HasTile" /> and <see cref="IsActuated" /> to <see langword="false" /> and sets the
    ///     <see cref="BlockType" /> to <see cref="Terraria.ID.BlockType.Solid" />.
    /// </summary>
    /// <param name="nullTile">If true, when the tilemap is pasted, the original tile here will remain</param>
    /// <remarks>
    ///     Does not reset data related to walls, wires, or anything else. For that, use <see cref="ClearEverything" />.
    /// </remarks>
    public void ClearTile(bool nullTile) {
        BlockType = BlockType.Solid;
        HasTile = false;
        IsActuated = false;
        IsNullTile = nullTile;
    }

    /// <summary>
    ///     copies all of this tile's data to Main.tile at the given global position
    /// </summary>
    /// <param name="x"></param>
    /// <param name="y"></param>
    /// <remarks>does not apply tile <see cref="BlockType" /></remarks>
    public void PasteTile(int x, int y) {
        Tile tile = Main.tile[x, y];
        if (!IsNullTile && !IsFakeTile) {
            if (Style == 0) {
                tile.TileType = TileType;
                tile.HasTile = HasTile;
            }
            else {
                tile.HasTile = HasTile;
                WorldGen.PlaceTile(x, y, TileType, true, style: Style);
            }

            tile.IsActuated = IsActuated;
            tile.HasActuator = HasActuator;
            tile.TileColor = TileColor;

            if (TileFrameX != -1) tile.TileFrameX = TileFrameX;
            if (TileFrameY != -1) tile.TileFrameY = TileFrameY;
        }

        if (!IsNullWall) {
            tile.WallType = WallType;
            tile.WallColor = WallColor;
        }
    }

    /// <summary>
    ///     copies all of this tile's data to Main.tile at the given global position
    /// </summary>
    /// <param name="point"></param>
    /// <remarks>does not apply tile <see cref="BlockType" /></remarks>
    public void PasteTile(Point16 point) {
        PasteTile(point.X, point.Y);
    }

    /// <summary>
    ///     copies <see cref="BlockType" /> of this tile to Main.tile at the given global position
    /// </summary>
    public void ApplySlopes(int x, int y) {
        Tile tile = Main.tile[x, y];
        tile.BlockType = BlockType;
    }

    /// <summary>
    ///     copies <see cref="BlockType" /> of this tile to Main.tile at the given global position
    /// </summary>
    public void ApplySlopes(Point16 point) {
        ApplySlopes(point.X, point.Y);
    }

    public void CopyTo(StructureTile tile) {
        foreach (PropertyInfo property in typeof(StructureTile).GetProperties())
            if (property.CanWrite)
                property.SetValue(tile, property.GetValue(this, null), null);
    }

    #region Custom Fields

    public SlopingAlgorithm SlopingAlg;

    public SlopeGrouping SlopeGrouping = SlopeGrouping.GlobalOnlySloping;

    public int Style;

    public bool IsOutside;

    public bool IsInside;

    public bool IsExteriorComponent;

    public bool IsFloor;

    public bool IsWall;

    public bool IsGap;

    public bool IsFurniture;

    /// <summary>
    ///     if true, when the tilemap is pasted, the original tile here will remain. true by default
    /// </summary>
    public bool IsNullTile = true;

    /// <summary>
    ///     if true, when the tilemap is pasted, the original wall here will remain. true by default
    /// </summary>
    public bool IsNullWall = true;

    /// <summary>
    ///     if false, tile will be pasted like normal; otherwise it is considered a placeholder block, and will not be pasted.
    ///     used to create placeholders when making <see cref="MultiTile" />s
    /// </summary>
    public bool IsFakeTile;

    #endregion


    #region Vanilla Fields

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

    /// <summary>
    ///     The X coordinate of the top left corner of the area in the spritesheet for the <see cref="TileType" /> to be used to draw the tile at this position.
    ///     <para />
    ///     For a Framed tile, this value is set automatically according to the framing logic as the world loads or other tiles are placed or mined nearby. See <see href="https://github.com/tModLoader/tModLoader/wiki/Basic-Tile#framed-vs-frameimportant-tiles">Framed vs FrameImportant</see> for more info. For <see cref="Main.tileFrameImportant" /> tiles, this value will not change due to tile framing and will be saved and synced in Multiplayer. In either case, <see cref="TileFrameX" /> and
    ///     <see cref="TileFrameY" /> correspond to the coordinates of the top left corner of the area in the spritesheet corresponding to the <see cref="TileType" /> that should be drawn at this position. Custom drawing logic can adjust these values.
    ///     <para />
    ///     Some tiles such as Christmas Tree and Weapon Rack use the higher bits of these fields to do tile-specific behaviors. Modders should not attempt to do similar approaches, but should use <see cref="ModLoader.ModTileEntity" />s.
    ///     <para />
    ///     Legacy/vanilla equivalent is <see cref="frameX" />.
    /// </summary>
    /// <remarks>-1 means it is not set</remarks>
    public short TileFrameX = -1;

    /// <summary>
    ///     The Y coordinate of the top left corner of the area in the spritesheet for the <see cref="TileType" /> to be used to draw the tile at this position.
    ///     <para />
    ///     For a Framed tile, this value is set automatically according to the framing logic as the world loads or other tiles are placed or mined nearby. See <see href="https://github.com/tModLoader/tModLoader/wiki/Basic-Tile#framed-vs-frameimportant-tiles">Framed vs FrameImportant</see> for more info. For <see cref="Main.tileFrameImportant" /> tiles, this value will not change due to tile framing and will be saved and synced in Multiplayer. In either case, <see cref="TileFrameX" /> and
    ///     <see cref="TileFrameY" /> correspond to the coordinates of the top left corner of the area in the spritesheet corresponding to the <see cref="TileType" /> that should be drawn at this position. Custom drawing logic can adjust these values.
    ///     <para />
    ///     Some tiles such as Christmas Tree and Weapon Rack use the higher bits of these fields to do tile-specific behaviors. Modders should not attempt to do similar approaches, but should use <see cref="ModLoader.ModTileEntity" />s.
    ///     <para />
    ///     Legacy/vanilla equivalent is <see cref="frameY" />.
    /// </summary>
    /// <remarks>-1 means it is not set</remarks>
    public short TileFrameY = -1;


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

    #endregion


    #region Vanilla Properties

    /// <summary>
    ///     Whether there is a tile at this position that isn't actuated.<br />
    ///     Legacy/vanilla equivalent is <see cref="inactive" />.
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

    #endregion
}