#nullable enable
using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using SpawnHouses.Content.Debug;
using SpawnHouses.Content.Palette;
using SpawnHouses.Content.Types.DataStructures;
using SpawnHouses.Content.Types.Interfaces;
using SpawnHouses.Content.Types.RootStructureTypes;
using SpawnHouses.Helpers;
using Terraria;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.Utilities;

namespace SpawnHouses.Content.Tiles;

public class StructureTilemap : IDebugDraw, IBoundingBox {
    // IDebugDraw
    public string Name => Structure.Name + "_Tilemap";
    public DebugInfoLevel DebugInfoVisibility { get; set; }

    // IBoundingBox
    /// <summary>
    ///     has tilemap position
    /// </summary>
    public TileBox BoundingBox { get; private set; }
    
    private readonly DebugLabel _label;
    private readonly StructureTile[,] _tiles;
    
    public readonly IStructureRoot Structure;
    public List<MultiTile> MultiTiles;

    /// <summary>
    ///     if there is a full structure loaded into the tilemap. not set by the tilemap itself, set outside the tilemap
    /// </summary>
    public bool IsAllTilesLoaded;

    /// <summary>
    ///     if the tilemap has been placed into the world. set by <see cref="ApplyTilemap" />
    /// </summary>
    public bool IsTilesPlaced { get; private set; }

    /// <summary>the actual global tile coordinates of the top left tile in this tilemap</summary>
    public Point16 GlobalTileOffset => BoundingBox.TopLeftPoint16;

    private UnifiedRandom OtherRandom => Structure is AdvStructure advStructure ? advStructure.OtherRandom : WorldGen._genRand;

    public int Width => BoundingBox.Width;
    public int Height => BoundingBox.Height;

    public StructureTilemap(IStructureRoot structure, ushort width, ushort height, Point16? globalTileOffset = null) {
        Structure = structure;
        _tiles = new StructureTile[width, height];
        BoundingBox = new TileBox(GlobalTileOffset.X, GlobalTileOffset.Y, width, height);
        MultiTiles = [];

        _label = new DebugLabel(GlobalTileOffset, this);
        DebugInfoVisibility = new DebugInfoLevel();
    }

    public Color GetDrawColor() => DrawHelper.GetColor(Structure.Id);

    /// <summary>
    ///     can only draw the bounding box
    /// </summary>
    /// <remarks>assumes that a world-relative batch has begun in <see cref="Main.spriteBatch" />. does not end sprite batch</remarks>
    public List<DebugLabel> DrawDebugGeometry() {
        if (DebugInfoVisibility.DisplayBounds)
            DrawHelper.DrawWorldBasedBorder(
                BoundingBox.Scale(16),
                GetDrawColor(),
                DrawHelper.DebugDrawWidth
            );

        return _label.IsVisible(BoundingBox) ? [_label] : [];
    }

    /// <summary>
    ///     uses coordinates relative to this tilemap
    /// </summary>
    /// <param name="x"></param>
    /// <param name="y"></param>
    /// <exception cref="IndexOutOfRangeException"></exception>
    public StructureTile this[int x, int y] {
        get {
            if (x < 0 || x >= Width || y < 0 || y >= Height) throw new IndexOutOfRangeException();

            // ReSharper disable once NullCoalescingConditionIsAlwaysNotNullAccordingToAPIContract
            return _tiles[x, y] ?? (_tiles[x, y] = new StructureTile());
        }
    }

    /// <summary>
    ///     uses coordinates relative to this tilemap
    /// </summary>
    /// <param name="point"></param>
    public StructureTile this[Point16 point] => this[point.X, point.Y];

    /// <summary>
    ///     uses coordinates relative to this tilemap
    /// </summary>
    /// <param name="x"></param>
    /// <param name="y"></param>
    /// <returns></returns>
    public bool InBounds(int x, int y) => x >= 0 && x < Width && y >= 0 && y < Height;

    /// <summary>
    ///     uses coordinates relative to this tilemap
    /// </summary>
    /// <param name="point"></param>
    /// <returns></returns>
    public bool InBounds(Point16 point) => InBounds(point.X, point.Y);

    /// <summary>
    ///     if this point's tile <see cref="StructureTile.IsInside" /> is true. uses coordinates relative to this tilemap.
    ///     checks for being in tilemap bounds
    /// </summary>
    /// <param name="x"></param>
    /// <param name="y"></param>
    /// <returns></returns>
    public bool InInterior(int x, int y) => InBounds(x, y) && this[x, y].IsInside;

    /// <summary>
    ///     uif this point's tile <see cref="StructureTile.IsInside" /> is true. uses coordinates relative to this tilemap.
    ///     checks for being in tilemap bounds
    /// </summary>
    /// <param name="point"></param>
    /// <returns></returns>
    public bool InInterior(Point16 point) => InInterior(point.X, point.Y);

    /// <summary>
    ///     if this point's tile <see cref="StructureTile.IsInside" /> is true. uses coordinates relative to this tilemap.
    ///     does not check for being in tilemap bounds
    /// </summary>
    /// <param name="x"></param>
    /// <param name="y"></param>
    /// <returns></returns>
    public bool InInteriorUnsafe(int x, int y) => this[x, y].IsInside;

    /// <summary>
    ///     if this point's tile <see cref="StructureTile.IsInside" /> is true. uses coordinates relative to this tilemap.
    ///     does not check for being in tilemap bounds
    /// </summary>
    /// <param name="point"></param>
    /// <returns></returns>
    public bool InInteriorUnsafe(Point16 point) => InInteriorUnsafe(point.X, point.Y);

    /// <summary>
    ///     gets tile from this tilemap using global tile coordinates
    /// </summary>
    /// <param name="x"></param>
    /// <param name="y"></param>
    /// <returns></returns>
    public StructureTile GetTileByGlobalPos(int x, int y) => this[x - GlobalTileOffset.X, y - GlobalTileOffset.Y];

    /// <summary>
    ///     gets tile from this tilemap using global tile coordinates
    /// </summary>
    /// <param name="pos"></param>
    /// <returns></returns>
    public StructureTile GetTileByGlobalPos(Point16 pos) => this[pos - GlobalTileOffset];

    public int ConvertToRelative(int coordinate, bool isX) => coordinate - (isX ? GlobalTileOffset.X : GlobalTileOffset.Y);

    public Point16 ConvertToRelative(int x, int y) => new(x - GlobalTileOffset.X, y - GlobalTileOffset.Y);

    public Point16 ConvertToRelative(Point16 point) => ConvertToRelative(point.X, point.Y);

    public int ConvertToGlobal(int coordinate, bool isX) => coordinate + (isX ? GlobalTileOffset.X : GlobalTileOffset.Y);

    public Point16 ConvertToGlobal(int x, int y) => new(x + GlobalTileOffset.X, y + GlobalTileOffset.Y);

    public Point16 ConvertToGlobal(Point16 point) => ConvertToGlobal(point.X, point.Y);

    public TileBox ConvertToGlobal(TileBox tileBox) => tileBox.Offset(GlobalTileOffset);

    /// <summary>
    ///     sets top-left position of tilemap
    /// </summary>
    /// <param name="position"></param>
    public void SetPosition(Point16 position) {
        BoundingBox = BoundingBox.SetPosition(position.X, position.Y);
    }
    
    public void PlaceTile(int x, int y, TilePaintedType? paintedType, BlockType blockType = BlockType.Solid, bool actuated = false) {
        if (paintedType == null) return;

        StructureTile tile = this[x, y];
        tile.HasTile = true;
        tile.BlockType = blockType;
        (tile.TileType, tile.TileColor, tile.Style) = paintedType.GetIds(OtherRandom);
        tile.IsNullTile = false;
        tile.IsActuated = actuated;
    }

    public void PlaceTile(int x, int y, TilePaintedType? paintedType,
        SlopingAlgorithm? slopingAlgorithm, SlopeGrouping slopeGrouping = SlopeGrouping.GlobalSloping, bool actuated = false) {
        if (paintedType == null) return;

        StructureTile tile = this[x, y];
        tile.HasTile = true;
        (tile.TileType, tile.TileColor, tile.Style) = paintedType.GetIds(OtherRandom);
        tile.IsNullTile = false;
        tile.SlopingAlg = slopingAlgorithm;
        tile.SlopeGrouping = slopeGrouping;
        tile.IsActuated = actuated;
    }

    public void PlaceMultiTile(Point16 topLeftPos, Point16 size, TilePaintedType tilePaintedType, bool isFurniture,
        Point16? origin = null, bool facingRight = true) {
        MultiTile multiTile = new(topLeftPos, size, tilePaintedType, OtherRandom, origin, facingRight);
        PlaceMultiTile(multiTile, isFurniture);
    }

    public void PlaceMultiTile(MultiTile multiTile, bool isFurniture) {
        multiTile.Volume.ExecuteInArea((x, y) => {
            StructureTile tile = this[x, y];
            tile.HasTile = true;
            tile.IsNullTile = false;
            tile.TileType = multiTile.TileType;
            tile.TileColor = multiTile.PaintType;
            tile.IsFurniture = isFurniture;
            tile.IsFakeTile = true;
        });
        MultiTiles.Add(multiTile);
    }

    /// <summary>
    ///     changes the tile at this position to be the <paramref name="paintedType" />,
    ///     does not change <see cref="StructureTile.BlockType" />, <see cref="StructureTile.HasTile" />, or <see cref="StructureTile.IsNullTile" />
    /// </summary>
    /// <param name="x"></param>
    /// <param name="y"></param>
    /// <param name="paintedType"></param>
    public void SoftPlaceTile(int x, int y, TilePaintedType paintedType) {
        StructureTile tile = this[x, y];
        (tile.TileType, tile.TileColor, tile.Style) = paintedType.GetIds(OtherRandom);
    }

    public void PlaceWall(int x, int y, WallPaintedType paintedType) {
        StructureTile tile = this[x, y];
        (tile.WallType, tile.WallColor, tile.Style) = paintedType.GetIds(OtherRandom);
        tile.IsNullWall = false;
    }

    /// <summary>
    /// </summary>
    /// <param name="x"></param>
    /// <param name="y"></param>
    /// <param name="nullTile">If true, when the tilemap is pasted, the original tile here will remain</param>
    public void ClearTile(int x, int y, bool nullTile) {
        this[x, y].ClearTile(nullTile);
    }

    /// <summary>
    ///     places a structure helper file into the tilemap (not world) at a specific LOCAL position
    /// </summary>
    /// <param name="pos">top left of placed structure</param>
    /// <param name="filepath"></param>
    public void PlaceFile(Point16 pos, string filepath) => CompatabilityHelper.PlaceShStructure(this, filepath, pos);

    /// <summary>
    ///     applies this tilemap (with it's offset) onto main game tilemap
    /// </summary>
    public void ApplyTilemap() {
        bool[,] globalTilemap = new bool[Width, Height];
        bool[,] globalNonLocalTilemap = new bool[Width, Height];

        // place normal tiles
        for (int x = 0; x < Width; x++)
        for (int y = 0; y < Height; y++) {
            StructureTile tile = this[x, y];
            tile.PasteTile(ConvertToGlobal(x, y));
            globalTilemap[x, y] = tile.HasTile;
            globalNonLocalTilemap[x, y] = tile.HasTile && tile.SlopeGrouping != SlopeGrouping.LocalSloping;
        }

        // slope tiles
        for (int x = 0; x < Width; x++)
        for (int y = 0; y < Height; y++) {
            StructureTile tile = this[x, y];
            if (tile.SlopingAlg != null) {
                bool[,] tilemap;
                switch (tile.SlopeGrouping) {
                    case SlopeGrouping.GlobalSloping:
                        tilemap = globalTilemap;
                        tile.BlockType = tile.SlopingAlg(x, y, tilemap);
                        break;
                    case SlopeGrouping.GlobalOnlySloping:
                        tilemap = globalNonLocalTilemap;
                        tile.BlockType = tile.SlopingAlg(x, y, tilemap);
                        break;
                    case SlopeGrouping.LocalSloping:
                        break;
                    default:
                        throw new Exception($"tile has unsupported slope modifier {tile.SlopeGrouping} for standalone placement");
                }
            }

            if (tile.HasTile)
                tile.ApplySlopes(ConvertToGlobal(x, y));
        }

        // place MultiTiles
        foreach (MultiTile multiTile in MultiTiles) {
            Point16 originPoint = ConvertToGlobal(multiTile.Volume.BoundingBox.TopLeftPoint16 + multiTile.Origin);
            WorldGen.PlaceObject(originPoint.X, originPoint.Y, multiTile.TileType, true, multiTile.Style, direction: multiTile.FacingRight ? 1 : -1);
            multiTile.Volume.ExecuteInArea((x, y) => {
                Tile tile = Main.tile[ConvertToGlobal(x, y)];
                tile.TileColor = multiTile.PaintType;
            });
        }

        // If we're not in worldgen, set frames
        // and then sync if we're in multiplayer
        if (!WorldGen.generatingWorld) {
            for (int x = BoundingBox.Left; x < BoundingBox.Left + Width; x++)
            for (int y = BoundingBox.Top; y < BoundingBox.Top + Height; y++) {
                WorldGen.TileFrame(x, y);
                WorldGen.SquareWallFrame(x, y);
            }

            if (Main.netMode != NetmodeID.SinglePlayer)
                NetMessage.SendTileSquare(-1, BoundingBox.Left, BoundingBox.Top, BoundingBox.Left + Width, BoundingBox.Top + Height);
        }

        IsTilesPlaced = true;
    }

    /// <summary>
    ///     creates a representation of the tilemap with <see cref="SolidDebugTile" /> and <see cref="NonSolidDebugTile" />
    /// </summary>
    /// <returns></returns>
    public IDebugTile?[,] CreateDebugTilemap() {
        // set base tiles
        var debugTilemap = new IDebugTile?[Width, Height];
        for (int x = 0; x < Width; x++) {
            for (int y = 0; y < Height; y++) {
                StructureTile tile = this[x, y];
                if (tile.IsExteriorComponent || tile.IsFloor || tile.IsWall || tile.IsGap) {
                    SolidDebugTile debugTile = new();
                    tile.CopyTo(debugTile);
                    debugTilemap[x, y] = debugTile;
                }
                else if (tile.IsInside) {
                    NonSolidDebugTile debugTile = new();
                    tile.CopyTo(debugTile);
                    debugTilemap[x, y] = debugTile;
                }
                else {
                    debugTilemap[x, y] = null;
                }
            }
        }

        // link tiles to their components
        if (Structure is AdvStructure advStructure) {
            List<IComponent>? components = null;
            if (advStructure.StructureLayout.AllComponents != null)
                components = advStructure.StructureLayout.AllComponents;
            else if (advStructure.StructureLayout.ExternalComponents != null)
                components = advStructure.StructureLayout.ExternalComponents;

            if (components != null)
                foreach (IComponent component in components)
                    if (component is VolumeComponent volumeComponent)
                        volumeComponent.Geometry.ExecuteInArea((x, y) => {
                            if (InBounds(x, y) && debugTilemap[x, y] != null) debugTilemap[x, y]!.SetComponent(volumeComponent);
                        });
        }
        
        return debugTilemap;
    }
}