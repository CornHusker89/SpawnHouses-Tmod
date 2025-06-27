using System;
using System.Collections.Generic;
using System.Linq;
using SpawnHouses.Helpers;
using SpawnHouses.Types;
using Terraria;
using Terraria.DataStructures;
using Terraria.ID;

namespace SpawnHouses.AdvStructures.Generation;

public static class ComponentGen {
    public static readonly (ComponentTag[] possibleTags, Func<ComponentParams, object> method)[] GenMethods = [
        // ===== floors =====
        (
            [
                ComponentTag.IsFloor,
                ComponentTag.FloorSolid,
                ComponentTag.FloorGroundLevel,
                ComponentTag.FloorThin,
                ComponentTag.FloorThick
            ],
            Floor1
        ),

        (
            [
                ComponentTag.IsFloor,
                ComponentTag.FloorSolid,
                ComponentTag.FloorGroundLevel,
                ComponentTag.Elevated,
                ComponentTag.FloorThin,
                ComponentTag.FloorThick
            ],
            Floor2
        ),

        (
            [
                ComponentTag.IsFloor,
                ComponentTag.FloorSolid,
                ComponentTag.FloorGroundLevel,
                ComponentTag.Elevated,
                ComponentTag.FloorThin,
                ComponentTag.FloorThick
            ],
            Floor3
        ),

        (
            [
                ComponentTag.IsFloor,
                ComponentTag.FloorHollow,
                ComponentTag.Elevated,
                ComponentTag.FloorThick
            ],
            Floor4
        ),

        (
            [
                ComponentTag.IsFloorGap,
                ComponentTag.FloorGroundLevel,
                ComponentTag.Elevated,
                ComponentTag.FloorThin,
                ComponentTag.FloorThick
            ],
            FloorGap1
        ),


        // ===== walls =====
        (
            [
                ComponentTag.IsWall,
                ComponentTag.WallGroundLevel,
                ComponentTag.Elevated
            ],
            Wall1
        ),

        (
            [
                ComponentTag.IsWall,
                ComponentTag.WallGroundLevel,
                ComponentTag.Elevated
            ],
            Wall2
        ),

        (
            [
                ComponentTag.IsWall,
                ComponentTag.WallGroundLevel,
                ComponentTag.Elevated
            ],
            Wall3
        ),

        (
            [
                ComponentTag.IsWallGap,
                ComponentTag.WallGroundLevel,
                ComponentTag.Elevated
            ],
            WallGap1
        ),


        // ===== decor =====
        (
            [
                ComponentTag.HasDecor,
                ComponentTag.DecorGroundLevel,
                ComponentTag.DecorElevated
            ],
            Decor1
        ),


        // ===== stairways =====


        // ===== backgrounds =====
        (
            [
                ComponentTag.IsBackground
            ],
            Background1
        ),

        (
            [
                ComponentTag.IsBackground
            ],
            Background2
        ),


        // ===== roofs =====
        (
            [
                ComponentTag.IsRoof,
                ComponentTag.RoofShort,
                ComponentTag.RoofTall,
                ComponentTag.RoofSlope1To1,
                ComponentTag.RoofSlopeLessThan1,
                ComponentTag.RoofSlopeNone
            ],
            Roof2
        ),


        // ===== debug =====
        (
            [
                ComponentTag.IsDebugBlocks
            ],
            DebugBlocks1
        ),


        (
            [
                ComponentTag.IsDebugBlocks
            ],
            DebugBlocks2
        ),


        (
            [
                ComponentTag.IsDebugBlocks
            ],
            DebugBlocks3
        ),


        (
            [
                ComponentTag.IsDebugWalls
            ],
            DebugWalls1
        ),


        (
            [
                ComponentTag.IsDebugWalls
            ],
            DebugWalls2
        ),


        (
            [
                ComponentTag.IsDebugWalls
            ],
            DebugWalls3
        )
    ];

    /// <summary>
    ///     Gets a random component method that aligns with the given <see cref="ComponentParams" />
    /// </summary>
    /// <param name="componentParams"></param>
    /// <returns></returns>
    /// <exception cref="Exception">When no components can be found for the given tags</exception>
    public static Func<ComponentParams, object> GetRandomMethod(ComponentParams componentParams) {
        List<(ComponentTag[] possibleTags, Func<ComponentParams, object> method)> methodTuples = [];
        foreach (var tuple in GenMethods) {
            var requiredTags = componentParams.TagsRequired.ToList();
            bool valid = true;
            foreach (ComponentTag possibleTag in tuple.possibleTags) {
                if (componentParams.TagsBlacklist.Contains(possibleTag)) {
                    valid = false;
                    break;
                }

                requiredTags.Remove(possibleTag);
            }

            if (valid && requiredTags.Count == 0)
                methodTuples.Add(tuple);
        }

        if (methodTuples.Count == 0)
            throw new Exception("No components found were compatible with given tags");
        return methodTuples[Terraria.WorldGen.genRand.Next(0, methodTuples.Count)].method;
    }


    #region Decor Methods

    /// <summary>
    ///     Fills a volume with main walls, then has random blocks on the bottom with an accent just above
    /// </summary>
    public static object Decor1(ComponentParams componentParams) {
        int bottomY = componentParams.Volume.BoundingBox.bottomRight.Y;
        componentParams.Volume.ExecuteInArea((x, y) => {
            if (y == bottomY)
                PaintedType.PlaceWall(x, y, PaintedType.PickRandom(componentParams.TilePalette.BackgroundRoomAlt), componentParams.Tilemap);
            else if (y == bottomY - 1)
                PaintedType.PlaceWall(x, y, componentParams.TilePalette.BackgroundRoomAccent, componentParams.Tilemap);
            else
                PaintedType.PlaceWall(x, y, componentParams.TilePalette.BackgroundRoomMain, componentParams.Tilemap);

            StructureTile tile = componentParams.Tilemap[x, y];
            tile.HasTile = false;
        });

        return false;
    }

    #endregion


    #region Roof Methods

    /// <summary>
    ///     basic roof, capable of handling most roof shapes
    /// </summary>
    public static object Roof2(ComponentParams componentParams) {
        TilePalette palette = componentParams.TilePalette;

        Point16 roofStart = new(componentParams.Volume.BoundingBox.topLeft.X,
            componentParams.Volume.BoundingBox.bottomRight.Y);
        int roofLength = componentParams.Volume.BoundingBox.bottomRight.X - roofStart.X;
        var roofBottom = RaycastHelper.GetTopTilesPos(roofStart, roofLength);
        roofStart = new Point16(roofStart.X, roofBottom.pos[0]);

        var roofBottomFlats = RaycastHelper.GetFlatTiles(roofBottom);
        bool isTallRoof = roofBottomFlats.flatLengths.Sum() > roofLength / 2 &&
                          roofLength > 30 &&
                          Terraria.WorldGen.genRand.Next(0, 4) == 0;
        bool hasEndCaps = Terraria.WorldGen.genRand.Next(0, 5) != 0;

        // pass 1: make roof shape
        int index = -1;
        for (int x = roofStart.X; x < roofStart.X + roofLength; x++) {
            index++;
            if (index != 0 && index != roofLength - 1)
                PaintedType.PlaceWall(x, roofBottom.pos[index] - 1, palette.BackgroundRoofMain, componentParams.Tilemap);

            // check if we're on a slope
            if (Math.Abs(roofBottom.slope[index] + 1) < 0.05 &&
                (index == 0 ||
                 roofBottom.pos[index] !=
                 roofBottom.pos[index - 1])) // if slope is -1 (up), and if we're actually on a slope
            {
                PaintedType.PlaceTile(x, roofBottom.pos[index] - 2, palette.RoofMain, componentParams.Tilemap, BlockType.SlopeDownRight);
                if (index != 0)
                    PaintedType.PlaceTile(x, roofBottom.pos[index] - 1, palette.RoofMain, componentParams.Tilemap, BlockType.SlopeUpLeft);
            }
            else if (Math.Abs(roofBottom.slope[index] - 1) < 0.05) // if slope is 1 (down)
            {
                PaintedType.PlaceTile(x, roofBottom.pos[index] - 2, palette.RoofMain, componentParams.Tilemap, BlockType.SlopeDownLeft);
                if (index != roofLength - 1)
                    PaintedType.PlaceTile(x, roofBottom.pos[index] - 1, palette.RoofMain, componentParams.Tilemap, BlockType.SlopeUpRight);
            }
            else if
                (index != 0 &&
                 roofBottom.slope[index - 1] <
                 -0.05) // if the tile behind had a - (up) slope, don't place a solid block
            {
                PaintedType.PlaceTile(x, roofBottom.pos[index] - 2, palette.RoofMain, componentParams.Tilemap, BlockType.SlopeDownRight);
                if (index != roofLength - 1)
                    PaintedType.PlaceTile(x, roofBottom.pos[index] - 1, palette.RoofMain, componentParams.Tilemap, BlockType.SlopeUpLeft);
            }
            else {
                PaintedType.PlaceTile(x, roofBottom.pos[index] - 2, palette.RoofMain, componentParams.Tilemap);
            }
        }

        // 2nd pass: validate slopes
        var roofTop =
            RaycastHelper.GetTopTilesPos(roofStart + new Point16(0, -2), roofLength);
        index = 0;
        for (int x = roofStart.X + 1; x < roofStart.X + roofLength - 1; x++) {
            index++;
            if (componentParams.Tilemap[x, roofTop.pos[index]].BlockType == BlockType.SlopeDownLeft &&
                componentParams.Tilemap[x - 1, roofTop.pos[index] + 1].BlockType == BlockType.SlopeDownRight) {
                PaintedType.PlaceTile(x, roofTop.pos[index] + 1, palette.RoofMain, componentParams.Tilemap);
                componentParams.Tilemap[x, roofTop.pos[index]].ClearTile();
            }
        }

        // 3rd pass: make slopes double width
        roofTop = RaycastHelper.GetTopTilesPos(roofStart + new Point16(0, -2), roofLength);
        index = -1;
        for (int x = roofStart.X; x < roofStart.X + roofLength; x++) {
            index++;
            StructureTile tile = componentParams.Tilemap[x, roofTop.pos[index]];
            switch (tile.BlockType) {
                case BlockType.SlopeDownRight: // up
                    PaintedType.PlaceTile(x, roofTop.pos[index], palette.RoofMain, componentParams.Tilemap);
                    if (index != 0 && index != roofLength - 1)
                        PaintedType.PlaceTile(x - 1, roofTop.pos[index], palette.RoofMain, componentParams.Tilemap, BlockType.SlopeDownRight);
                    break;
                case BlockType.SlopeDownLeft: // down
                    PaintedType.PlaceTile(x, roofTop.pos[index], palette.RoofMain, componentParams.Tilemap);
                    if (index != 0 && index != +roofLength - 1)
                        PaintedType.PlaceTile(x + 1, roofTop.pos[index], palette.RoofMain, componentParams.Tilemap, BlockType.SlopeDownLeft);
                    break;
            }
        }

        // 4th pass: prep slope transitions to half blocks
        roofTop = RaycastHelper.GetTopTilesPos(roofStart + new Point16(0, -2), roofLength);
        List<(int, int)> tranitions = [];
        var roofTopFlats = RaycastHelper.GetFlatTiles(roofTop);

        // if there's a flat of length 1 at the start, add transition and manually add half slopes
        if (componentParams.Tilemap[roofStart.X, roofTop.pos[0]].BlockType == BlockType.Solid)
            if (componentParams.Tilemap[roofStart.X + 1, roofTop.pos[0] - 1].BlockType == BlockType.SlopeDownRight) {
                PaintedType.PlaceTile(roofStart.X + 1, roofTop.pos[0] - 1, palette.RoofMain, componentParams.Tilemap);
                PaintedType.PlaceTile(roofStart.X, roofTop.pos[0] - 1, palette.RoofMain, componentParams.Tilemap, BlockType.HalfBlock);
                tranitions.Add((roofStart.X + 1, roofTop.pos[0] - 1));
            }
            else if (componentParams.Tilemap[roofStart.X + 1, roofTop.pos[0]].BlockType == BlockType.SlopeDownLeft) {
                PaintedType.PlaceTile(roofStart.X + 1, roofTop.pos[0], palette.RoofMain, componentParams.Tilemap);
                PaintedType.PlaceTile(roofStart.X, roofTop.pos[0] - 1, palette.RoofMain, componentParams.Tilemap, BlockType.HalfBlock);
                tranitions.Add((roofStart.X + 1, roofTop.pos[0]));
            }

        for (int flatIndex = 0; flatIndex < roofTopFlats.flatStartIndexes.Count; flatIndex++) {
            // if the correct slope to left or right of flat, create transition
            int furthestLeftIndex = roofTopFlats.flatStartIndexes[flatIndex];
            StructureTile leftTile = componentParams.Tilemap[roofStart.X + furthestLeftIndex - 1,
                roofTop.pos[furthestLeftIndex] - 1];
            if (leftTile is { HasTile: true, BlockType: BlockType.SlopeDownLeft }) {
                leftTile.BlockType = BlockType.Solid;
                tranitions.Add((roofStart.X + furthestLeftIndex - 1, roofTop.pos[furthestLeftIndex] - 1));
            }

            int furthestRightIndex = roofTopFlats.flatStartIndexes[flatIndex] + roofTopFlats.flatLengths[flatIndex] - 1;
            StructureTile rightTile = componentParams.Tilemap[roofStart.X + furthestRightIndex + 1,
                roofTop.pos[furthestRightIndex] - 1];
            if (rightTile is { HasTile: true, BlockType: BlockType.SlopeDownRight }) {
                rightTile.BlockType = BlockType.Solid;
                tranitions.Add((roofStart.X + furthestRightIndex + 1, roofTop.pos[furthestRightIndex] - 1));
            }
        }

        // 5th pass: actually add the half blocks
        for (int flatIndex = 0; flatIndex < roofTopFlats.flatStartIndexes.Count; flatIndex++)
        for (int xIndex = roofTopFlats.flatStartIndexes[flatIndex];
             xIndex < roofTopFlats.flatStartIndexes[flatIndex] + roofTopFlats.flatLengths[flatIndex];
             xIndex++)
            // ensure the tile underneath is solid (and not a transition tile)
            if (componentParams.Tilemap[roofStart.X + xIndex, roofTop.pos[xIndex]].BlockType == BlockType.Solid &&
                !tranitions.Contains((roofStart.X + xIndex, roofTop.pos[xIndex])))
                PaintedType.PlaceTile(roofStart.X + xIndex, roofTop.pos[xIndex] - 1, palette.RoofMain, componentParams.Tilemap, BlockType.HalfBlock);

        // 6th pass: create endcaps
        bool anySideEndsWithSlope = Math.Abs(roofBottom.slope[0]) > 0.05 || Math.Abs(roofBottom.slope[^1]) > 0.05;
        bool hasTallEndCaps = anySideEndsWithSlope || Terraria.WorldGen.genRand.NextBool();
        if (hasEndCaps) {
            // left cap
            StructureTile leftTestTile = componentParams.Tilemap[roofStart.X - 1, roofStart.Y - 2];
            if (!leftTestTile.HasTile || leftTestTile.BlockType != BlockType.Solid) {
                if (hasTallEndCaps) {
                    PaintedType.PlaceTile(roofStart.X - 1, roofStart.Y - 2, palette.RoofMain, componentParams.Tilemap, BlockType.SlopeUpRight);
                    PaintedType.PlaceTile(roofStart.X - 1, roofStart.Y - 3, palette.RoofMain, componentParams.Tilemap, BlockType.HalfBlock);
                    if (componentParams.Tilemap[roofStart.X, roofTop.pos[0]].BlockType is
                        BlockType.SlopeDownRight or BlockType.SlopeDownLeft) {
                        PaintedType.PlaceTile(roofStart.X, roofTop.pos[0], palette.RoofMain, componentParams.Tilemap);
                        PaintedType.PlaceWall(roofStart.X + 1, roofStart.Y - 1, palette.BackgroundRoofMain, componentParams.Tilemap);
                    }
                }
                else {
                    PaintedType.PlaceTile(roofStart.X + 1, roofStart.Y - 1, palette.RoofMain, componentParams.Tilemap, BlockType.SlopeUpLeft);
                    PaintedType.PlaceTile(roofStart.X, roofStart.Y - 1, palette.RoofMain, componentParams.Tilemap);
                    PaintedType.PlaceTile(roofStart.X - 1, roofStart.Y - 1, palette.RoofMain, componentParams.Tilemap);
                    PaintedType.PlaceTile(roofStart.X - 2, roofStart.Y - 1, palette.RoofMain, componentParams.Tilemap, BlockType.SlopeUpRight);
                    PaintedType.PlaceTile(roofStart.X, roofStart.Y - 2, palette.RoofMain, componentParams.Tilemap);
                    PaintedType.PlaceTile(roofStart.X - 1, roofStart.Y - 2, palette.RoofMain, componentParams.Tilemap, BlockType.HalfBlock);
                    PaintedType.PlaceTile(roofStart.X - 2, roofStart.Y - 2, palette.RoofMain, componentParams.Tilemap, BlockType.HalfBlock);
                    if (componentParams.Tilemap[roofStart.X, roofStart.Y - 3].BlockType ==
                        BlockType.HalfBlock)
                        componentParams.Tilemap[roofStart.X, roofStart.Y - 3].ClearTile();
                }
            }

            // right cap
            int rightPosX = roofStart.X + roofLength - 1;
            StructureTile rightTestTile = componentParams.Tilemap[rightPosX + 1, roofBottom.pos[^1] - 2];
            if (!rightTestTile.HasTile || rightTestTile.BlockType != BlockType.Solid) {
                if (hasTallEndCaps) {
                    PaintedType.PlaceTile(rightPosX + 1, roofBottom.pos[^1] - 2, palette.RoofMain, componentParams.Tilemap, BlockType.SlopeUpLeft);
                    PaintedType.PlaceTile(rightPosX + 1, roofBottom.pos[^1] - 3, palette.RoofMain, componentParams.Tilemap, BlockType.HalfBlock);
                    if (componentParams.Tilemap[roofStart.X + roofLength - 1, roofTop.pos[^1]].BlockType is
                        BlockType.SlopeDownRight or BlockType.SlopeDownLeft) {
                        PaintedType.PlaceTile(roofStart.X + roofLength - 1, roofTop.pos[^1], palette.RoofMain, componentParams.Tilemap);
                        PaintedType.PlaceWall(roofStart.X + roofLength - 2, roofTop.pos[^1] + 2, palette.BackgroundRoofMain, componentParams.Tilemap);
                    }
                }
                else {
                    PaintedType.PlaceTile(rightPosX - 1, roofBottom.pos[^1] - 1, palette.RoofMain, componentParams.Tilemap, BlockType.SlopeUpRight);
                    PaintedType.PlaceTile(rightPosX, roofBottom.pos[^1] - 1, palette.RoofMain, componentParams.Tilemap);
                    PaintedType.PlaceTile(rightPosX + 1, roofBottom.pos[^1] - 1, palette.RoofMain, componentParams.Tilemap);
                    PaintedType.PlaceTile(rightPosX + 2, roofBottom.pos[^1] - 1, palette.RoofMain, componentParams.Tilemap, BlockType.SlopeUpLeft);
                    PaintedType.PlaceTile(rightPosX, roofBottom.pos[^1] - 2, palette.RoofMain, componentParams.Tilemap);
                    PaintedType.PlaceTile(rightPosX + 1, roofBottom.pos[^1] - 2, palette.RoofMain, componentParams.Tilemap, BlockType.HalfBlock);
                    PaintedType.PlaceTile(rightPosX + 2, roofBottom.pos[^1] - 2, palette.RoofMain, componentParams.Tilemap, BlockType.HalfBlock);
                    if (componentParams.Tilemap[rightPosX, roofBottom.pos[^1] - 3].BlockType == BlockType.HalfBlock)
                        componentParams.Tilemap[rightPosX, roofBottom.pos[^1] - 3].ClearTile();
                }
            }
        }

        return false;
    }

    #endregion


    #region Floor Methods

    /// <summary>
    ///     Fills a volume with the same floor blocks
    /// </summary>
    public static object Floor1(ComponentParams componentParams) {
        componentParams.Volume.ExecuteInArea((x, y) => { PaintedType.PlaceTile(x, y, componentParams.TilePalette.FloorMain, componentParams.Tilemap); });

        return false;
    }

    /// <summary>
    ///     Fills a volume with random floor blocks
    /// </summary>
    public static object Floor2(ComponentParams componentParams) {
        componentParams.Volume.ExecuteInArea((x, y) => { PaintedType.PlaceTile(x, y, PaintedType.PickRandom(componentParams.TilePalette.FloorAlt), componentParams.Tilemap); });

        return false;
    }

    /// <summary>
    ///     Fills a volume with random blocks, but the top block consistent
    /// </summary>
    public static object Floor3(ComponentParams componentParams) {
        bool elevated = componentParams.TagsRequired.Contains(ComponentTag.Elevated);
        int xStart = componentParams.Volume.BoundingBox.topLeft.X;
        int[] topY = new int[componentParams.Volume.Size.X];

        componentParams.Volume.ExecuteInArea((x, y) => {
            PaintedType.PlaceTile(x, y,
                PaintedType.PickRandom(elevated
                    ? componentParams.TilePalette.FloorAlt
                    : componentParams.TilePalette.FloorAltElevated),
                componentParams.Tilemap);

            if (topY[x - xStart] == 0)
                topY[x - xStart] = y;

            if (y < topY[x - xStart])
                topY[x - xStart] = y;
        });

        for (int index = 0; index < topY.Length; index++)
            PaintedType.PlaceTile(xStart + index, topY[index],
                elevated ? componentParams.TilePalette.FloorMainElevated : componentParams.TilePalette.FloorMain,
                componentParams.Tilemap);

        return false;
    }

    /// <summary>
    ///     Fills top and bottom of volume, adds support struts in the middle
    /// </summary>
    public static object Floor4(ComponentParams componentParams) {
        bool elevated = componentParams.TagsRequired.Contains(ComponentTag.Elevated);
        int xStart = componentParams.Volume.BoundingBox.topLeft.X;
        int[] topY = new int[componentParams.Volume.Size.X];
        int[] bottomY = new int[componentParams.Volume.Size.X];
        int supportInterval = Terraria.WorldGen.genRand.Next(3, 5);

        componentParams.Volume.ExecuteInArea((x, y) => {
            if ((x - xStart - 2) % supportInterval == 0 || x == xStart ||
                x == componentParams.Volume.BoundingBox.bottomRight.X) {
                PaintedType.PlaceTile(x, y,
                    elevated ? componentParams.TilePalette.FloorMainElevated : componentParams.TilePalette.FloorMain,
                    componentParams.Tilemap);
            }
            else {
                PaintedType.PlaceWall(x, y, componentParams.TilePalette.BackgroundFloorMain, componentParams.Tilemap);
                StructureTile tile = componentParams.Tilemap[x, y];
                if (Terraria.WorldGen.genRand.Next(0, 3) == 0) {
                    tile.HasTile = true;
                    tile.TileType = TileID.Cobweb;
                }
                else {
                    tile.HasTile = false;
                }
            }

            if (topY[x - xStart] == 0)
                topY[x - xStart] = y;
            if (bottomY[x - xStart] == 0)
                bottomY[x - xStart] = y;

            if (y < topY[x - xStart])
                topY[x - xStart] = y;
            if (y > bottomY[x - xStart])
                bottomY[x - xStart] = y;
        });

        for (int index = 0; index < topY.Length; index++) {
            PaintedType.PlaceTile(xStart + index, topY[index],
                elevated ? componentParams.TilePalette.FloorMainElevated : componentParams.TilePalette.FloorMain,
                componentParams.Tilemap);
            PaintedType.PlaceTile(xStart + index, bottomY[index],
                elevated ? componentParams.TilePalette.FloorMainElevated : componentParams.TilePalette.FloorMain,
                componentParams.Tilemap);
        }

        return false;
    }

    /// <summary>
    ///     A gap floor, places platforms
    /// </summary>
    public static object FloorGap1(ComponentParams componentParams) {
        int xStart = componentParams.Volume.BoundingBox.topLeft.X;
        int[] topY = new int[componentParams.Volume.Size.X];
        int[] bottomY = new int[componentParams.Volume.Size.X];

        componentParams.Volume.ExecuteInArea((x, y) => {
            PaintedType.PlaceWall(x, y, componentParams.TilePalette.BackgroundFloorMain, componentParams.Tilemap);
            StructureTile tile = componentParams.Tilemap[x, y];
            tile.HasTile = false;

            if (topY[x - xStart] == 0)
                topY[x - xStart] = y;
            if (bottomY[x - xStart] == 0)
                bottomY[x - xStart] = y;

            if (y < topY[x - xStart])
                topY[x - xStart] = y;
            if (y > bottomY[x - xStart])
                bottomY[x - xStart] = y;
        });

        for (int index = 0; index < topY.Length; index++) {
            PaintedType.PlaceTile(xStart + index, topY[index], componentParams.TilePalette.Platform, componentParams.Tilemap);
            PaintedType.PlaceTile(xStart + index, bottomY[index], componentParams.TilePalette.Platform, componentParams.Tilemap);
        }

        return false;
    }

    #endregion


    #region Wall Methods

    /// <summary>
    ///     Fills a volume with the same wall blocks, with special blocks at regular intervals
    /// </summary>
    public static object Wall1(ComponentParams componentParams) {
        bool elevated = componentParams.TagsRequired.Contains(ComponentTag.Elevated);
        int yStart = componentParams.Volume.BoundingBox.topLeft.Y;
        int[] lowX = new int[componentParams.Volume.Size.Y];
        int[] highX = new int[componentParams.Volume.Size.Y];
        componentParams.Volume.ExecuteInArea((x, y) => {
            PaintedType.PlaceTile(x, y,
                elevated ? componentParams.TilePalette.WallMainElevated : componentParams.TilePalette.WallMain,
                componentParams.Tilemap);

            if (lowX[y - yStart] == 0)
                lowX[y - yStart] = x;
            if (highX[y - yStart] == 0)
                highX[y - yStart] = x;

            if (x < lowX[y - yStart])
                lowX[y - yStart] = x;
            if (x > highX[y - yStart])
                highX[y - yStart] = x;
        });

        for (int index = 0; index < lowX.Length; index++) {
            PaintedType.PlaceTile(yStart + index, lowX[index], componentParams.TilePalette.WallSpecial, componentParams.Tilemap);
            PaintedType.PlaceTile(yStart + index, highX[index], componentParams.TilePalette.WallSpecial, componentParams.Tilemap);
        }

        return false;
    }

    /// <summary>
    ///     Fills a volume with random wall blocks, with special blocks at regular intervals
    /// </summary>
    public static object Wall2(ComponentParams componentParams) {
        bool elevated = componentParams.TagsRequired.Contains(ComponentTag.Elevated);
        int yStart = componentParams.Volume.BoundingBox.topLeft.Y;
        int[] lowX = new int[componentParams.Volume.Size.Y];
        int[] highX = new int[componentParams.Volume.Size.Y];
        componentParams.Volume.ExecuteInArea((x, y) => {
            PaintedType.PlaceTile(x, y, PaintedType.PickRandom(
                    elevated ? componentParams.TilePalette.WallAltElevated : componentParams.TilePalette.WallAlt),
                componentParams.Tilemap);

            if (lowX[y - yStart] == 0) lowX[y - yStart] = x;
            if (highX[y - yStart] == 0) highX[y - yStart] = x;

            if (x < lowX[y - yStart]) lowX[y - yStart] = x;
            if (x > highX[y - yStart]) highX[y - yStart] = x;
        });

        for (int index = 0; index < lowX.Length; index++) {
            PaintedType.PlaceTile(yStart + index, lowX[index], componentParams.TilePalette.WallSpecial, componentParams.Tilemap);
            PaintedType.PlaceTile(yStart + index, highX[index], componentParams.TilePalette.WallSpecial, componentParams.Tilemap);
        }

        return false;
    }

    /// <summary>
    ///     Fills a volume with random blocks, but the bottom block consistent
    /// </summary>
    public static object Wall3(ComponentParams componentParams) {
        bool elevated = componentParams.TagsRequired.Contains(ComponentTag.Elevated);
        int xStart = componentParams.Volume.BoundingBox.topLeft.X;
        int[] bottomY = new int[componentParams.Volume.Size.X];

        componentParams.Volume.ExecuteInArea((x, y) => {
            PaintedType.PlaceTile(x, y, PaintedType.PickRandom(
                    elevated ? componentParams.TilePalette.WallAltElevated : componentParams.TilePalette.WallAlt),
                componentParams.Tilemap);

            if (bottomY[x - xStart] == 0)
                bottomY[x - xStart] = y;

            if (y > bottomY[x - xStart])
                bottomY[x - xStart] = y;
        });

        for (int index = 0; index < bottomY.Length; index++)
            PaintedType.PlaceTile(xStart + index, bottomY[index],
                elevated ? componentParams.TilePalette.WallAccentElevated : componentParams.TilePalette.WallAccent,
                componentParams.Tilemap);

        return false;
    }

    /// <summary>
    ///     Fills a volume with random background walls
    /// </summary>
    public static object WallGap1(ComponentParams componentParams) {
        componentParams.Volume.ExecuteInArea((x, y) => { PaintedType.PlaceWall(x, y, PaintedType.PickRandom(componentParams.TilePalette.BackgroundWallAlt), componentParams.Tilemap); });

        return false;
    }

    #endregion


    #region Stairway Methods

    #endregion


    #region Background Methods

    /// <summary>
    ///     Fills mostly with random walls, but has specific walls on bottom edge
    /// </summary>
    public static object Background1(ComponentParams componentParams) {
        componentParams.Volume.ExecuteInArea((x, y) => {
            if (y == componentParams.Volume.BoundingBox.bottomRight.Y)
                PaintedType.PlaceWall(x, y, PaintedType.PickRandom(componentParams.TilePalette.BackgroundRoomAlt), componentParams.Tilemap);
            else if (y == componentParams.Volume.BoundingBox.bottomRight.Y - 1)
                PaintedType.PlaceWall(x, y, componentParams.TilePalette.BackgroundRoomAccent, componentParams.Tilemap);
            else
                PaintedType.PlaceWall(x, y, componentParams.TilePalette.BackgroundRoomMain, componentParams.Tilemap);
        });

        return false;
    }

    /// <summary>
    ///     Fills mostly with random walls, but places main walls on bottom 3
    /// </summary>
    public static object Background2(ComponentParams componentParams) {
        int bottomY = componentParams.Volume.BoundingBox.bottomRight.Y;
        componentParams.Volume.ExecuteInArea((x, y) => {
            if (y == bottomY || y == bottomY - 1 || y == bottomY - 2)
                PaintedType.PlaceWall(x, y, PaintedType.PickRandom(componentParams.TilePalette.BackgroundRoomAlt), componentParams.Tilemap);
            else
                PaintedType.PlaceWall(x, y, componentParams.TilePalette.BackgroundRoomMain, componentParams.Tilemap);
        });

        return false;
    }

    #endregion


    #region Debug Methods

    /// <summary>
    ///     Fills with emerald gem spark
    /// </summary>
    public static object DebugBlocks1(ComponentParams componentParams) {
        componentParams.Volume.ExecuteInArea((x, y) => {
            StructureTile tile = componentParams.Tilemap[x, y];
            tile.HasTile = true;
            tile.BlockType = BlockType.Solid;
            tile.TileType = TileID.EmeraldGemspark;
            tile.TileColor = PaintID.None;
        });
        return false;
    }

    /// <summary>
    ///     Fills with sapphire gem spark
    /// </summary>
    public static object DebugBlocks2(ComponentParams componentParams) {
        componentParams.Volume.ExecuteInArea((x, y) => {
            StructureTile tile = componentParams.Tilemap[x, y];
            tile.HasTile = true;
            tile.BlockType = BlockType.Solid;
            tile.TileType = TileID.SapphireGemspark;
            tile.TileColor = PaintID.None;
        });
        return false;
    }

    /// <summary>
    ///     Fills with ruby gem spark
    /// </summary>
    public static object DebugBlocks3(ComponentParams componentParams) {
        componentParams.Volume.ExecuteInArea((x, y) => {
            StructureTile tile = componentParams.Tilemap[x, y];
            tile.HasTile = true;
            tile.BlockType = BlockType.Solid;
            tile.TileType = TileID.RubyGemspark;
            tile.TileColor = PaintID.None;
        });
        return false;
    }

    /// <summary>
    ///     Fills with emerald gem spark
    /// </summary>
    public static object DebugWalls1(ComponentParams componentParams) {
        componentParams.Volume.ExecuteInArea((x, y) => {
            StructureTile tile = componentParams.Tilemap[x, y];
            tile.WallType = WallID.EmeraldGemspark;
            tile.WallColor = PaintID.None;
        });
        return false;
    }

    /// <summary>
    ///     Fills with sapphire gem spark
    /// </summary>
    public static object DebugWalls2(ComponentParams componentParams) {
        componentParams.Volume.ExecuteInArea((x, y) => {
            StructureTile tile = componentParams.Tilemap[x, y];
            tile.WallType = WallID.SapphireGemspark;
            tile.WallColor = PaintID.None;
        });
        return false;
    }

    /// <summary>
    ///     Fills with ruby gem spark
    /// </summary>
    public static object DebugWalls3(ComponentParams componentParams) {
        componentParams.Volume.ExecuteInArea((x, y) => {
            StructureTile tile = componentParams.Tilemap[x, y];
            tile.WallType = WallID.RubyGemspark;
            tile.WallColor = PaintID.None;
        });
        return false;
    }

    #endregion
}