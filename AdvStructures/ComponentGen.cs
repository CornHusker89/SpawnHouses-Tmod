using System;
using System.Collections.Generic;
using System.Linq;
using SpawnHouses.Helpers;
using SpawnHouses.Types;
using Terraria;
using Terraria.DataStructures;
using Terraria.ID;

namespace SpawnHouses.AdvStructures;

public static class ComponentGen {
    public static readonly (StructureTag[] possibleTags, Func<ComponentParams, object> method)[] GenMethods = [
        // ===== floors =====
        (
            [
                StructureTag.HasFloor,
                StructureTag.FloorSolid,
                StructureTag.FloorGroundLevel,
                StructureTag.FloorThin,
                StructureTag.FloorThick
            ],
            Floor1
        ),

        (
            [
                StructureTag.HasFloor,
                StructureTag.FloorSolid,
                StructureTag.FloorGroundLevel,
                StructureTag.FloorThin,
                StructureTag.FloorThick
            ],
            Floor2
        ),

        (
            [
                StructureTag.HasFloor,
                StructureTag.FloorSolid,
                StructureTag.FloorGroundLevel,
                StructureTag.FloorElevated,
                StructureTag.FloorThin,
                StructureTag.FloorThick
            ],
            Floor3
        ),

        (
            [
                StructureTag.HasFloor,
                StructureTag.FloorHollow,
                StructureTag.FloorElevated,
                StructureTag.FloorThick
            ],
            Floor4
        ),

        (
            [
                StructureTag.IsFloorGap,
                StructureTag.FloorGroundLevel,
                StructureTag.FloorElevated,
                StructureTag.FloorThin,
                StructureTag.FloorThick
            ],
            FloorGap1
        ),


        // ===== walls =====
        (
            [
                StructureTag.HasWall,
                StructureTag.WallGroundLevel,
                StructureTag.WallElevated
            ],
            Wall1
        ),

        (
            [
                StructureTag.HasWall,
                StructureTag.WallGroundLevel,
                StructureTag.WallElevated
            ],
            Wall2
        ),

        (
            [
                StructureTag.HasWall,
                StructureTag.WallGroundLevel,
                StructureTag.WallElevated
            ],
            Wall3
        ),

        (
            [
                StructureTag.IsWallGap,
                StructureTag.WallGroundLevel,
                StructureTag.WallElevated
            ],
            WallGap1
        ),


        // ===== decor =====
        (
            [
                StructureTag.HasDecor,
                StructureTag.DecorGroundLevel,
                StructureTag.DecorElevated
            ],
            Decor1
        ),


        // ===== stairways =====


        // ===== backgrounds =====
        (
            [
                StructureTag.HasBackground
            ],
            Background1
        ),

        (
            [
                StructureTag.HasBackground
            ],
            Background2
        ),


        // ===== roofs =====
        (
            [
                StructureTag.HasRoof,
                StructureTag.RoofShort,
                StructureTag.RoofTall,
                StructureTag.RoofSlope1To1,
                StructureTag.RoofSlopeLessThan1,
                StructureTag.RoofSlopeNone
            ],
            Roof2
        )
    ];

    /// <summary>
    ///     Gets a random component method that aligns with the given <see cref="ComponentParams" />
    /// </summary>
    /// <param name="componentParams"></param>
    /// <returns></returns>
    /// <exception cref="Exception">When no components can be found for the given tags</exception>
    public static Func<ComponentParams, object> GetRandomMethod(ComponentParams componentParams) {
        List<(StructureTag[] possibleTags, Func<ComponentParams, object> method)> methodTuples = [];
        foreach (var tuple in GenMethods) {
            var requiredTags = componentParams.TagsRequired.ToList();
            var valid = true;
            foreach (var possibleTag in tuple.possibleTags) {
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
                PaintedType.PlaceWall(x, y, PaintedType.PickRandom(componentParams.TilePalette.BackgroundRoomAlt));
            else if (y == bottomY - 1)
                PaintedType.PlaceWall(x, y, componentParams.TilePalette.BackgroundRoomAccent);
            else
                PaintedType.PlaceWall(x, y, componentParams.TilePalette.BackgroundRoomMain);

            var tile = Main.tile[x, y];
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
        var palette = componentParams.TilePalette;

        var roofStart = new Point16(componentParams.Volume.BoundingBox.topLeft.X,
            componentParams.Volume.BoundingBox.bottomRight.Y);
        var roofLength = componentParams.Volume.BoundingBox.bottomRight.X - roofStart.X;
        var roofBottom = RaycastHelper.GetTopTilesPos(roofStart, roofLength);
        roofStart = new Point16(roofStart.X, roofBottom.pos[0]);

        var roofBottomFlats = RaycastHelper.GetFlatTiles(roofBottom);
        var isTallRoof = roofBottomFlats.flatLengths.Sum() > roofLength / 2 &&
                         roofLength > 30 &&
                         Terraria.WorldGen.genRand.Next(0, 4) == 0;
        var hasEndCaps = Terraria.WorldGen.genRand.Next(0, 5) != 0;

        // pass 1: make roof shape
        var index = -1;
        for (int x = roofStart.X; x < roofStart.X + roofLength; x++) {
            index++;
            if (index != 0 && index != roofLength - 1)
                PaintedType.PlaceWall(x, roofBottom.pos[index] - 1, palette.BackgroundRoofMain);

            // check if we're on a slope
            if (Math.Abs(roofBottom.slope[index] + 1) < 0.05 &&
                (index == 0 ||
                 roofBottom.pos[index] !=
                 roofBottom.pos[index - 1])) // if slope is -1 (up), and if we're actually on a slope
            {
                PaintedType.PlaceTile(x, roofBottom.pos[index] - 2, palette.RoofMain, BlockType.SlopeDownRight);
                if (index != 0)
                    PaintedType.PlaceTile(x, roofBottom.pos[index] - 1, palette.RoofMain, BlockType.SlopeUpLeft);
            }
            else if (Math.Abs(roofBottom.slope[index] - 1) < 0.05) // if slope is 1 (down)
            {
                PaintedType.PlaceTile(x, roofBottom.pos[index] - 2, palette.RoofMain, BlockType.SlopeDownLeft);
                if (index != roofLength - 1)
                    PaintedType.PlaceTile(x, roofBottom.pos[index] - 1, palette.RoofMain, BlockType.SlopeUpRight);
            }
            else if
                (index != 0 &&
                 roofBottom.slope[index - 1] <
                 -0.05) // if the tile behind had a - (up) slope, don't place a solid block
            {
                PaintedType.PlaceTile(x, roofBottom.pos[index] - 2, palette.RoofMain, BlockType.SlopeDownRight);
                if (index != roofLength - 1)
                    PaintedType.PlaceTile(x, roofBottom.pos[index] - 1, palette.RoofMain, BlockType.SlopeUpLeft);
            }
            else {
                PaintedType.PlaceTile(x, roofBottom.pos[index] - 2, palette.RoofMain);
            }
        }

        // 2nd pass: validate slopes
        var roofTop =
            RaycastHelper.GetTopTilesPos(roofStart + new Point16(0, -2), roofLength);
        index = 0;
        for (var x = roofStart.X + 1; x < roofStart.X + roofLength - 1; x++) {
            index++;
            if (Main.tile[x, roofTop.pos[index]].BlockType == BlockType.SlopeDownLeft &&
                Main.tile[x - 1, roofTop.pos[index] + 1].BlockType == BlockType.SlopeDownRight) {
                PaintedType.PlaceTile(x, roofTop.pos[index] + 1, palette.RoofMain);
                Main.tile[x, roofTop.pos[index]].ClearTile();
            }
        }

        // 3rd pass: make slopes double width
        roofTop = RaycastHelper.GetTopTilesPos(roofStart + new Point16(0, -2), roofLength);
        index = -1;
        for (int x = roofStart.X; x < roofStart.X + roofLength; x++) {
            index++;
            var tile = Main.tile[x, roofTop.pos[index]];
            switch (tile.BlockType) {
                case BlockType.SlopeDownRight: // up
                    PaintedType.PlaceTile(x, roofTop.pos[index], palette.RoofMain);
                    if (index != 0 && index != roofLength - 1)
                        PaintedType.PlaceTile(x - 1, roofTop.pos[index], palette.RoofMain, BlockType.SlopeDownRight);
                    break;
                case BlockType.SlopeDownLeft: // down
                    PaintedType.PlaceTile(x, roofTop.pos[index], palette.RoofMain);
                    if (index != 0 && index != +roofLength - 1)
                        PaintedType.PlaceTile(x + 1, roofTop.pos[index], palette.RoofMain, BlockType.SlopeDownLeft);
                    break;
            }
        }

        // 4th pass: prep slope transitions to half blocks
        roofTop = RaycastHelper.GetTopTilesPos(roofStart + new Point16(0, -2), roofLength);
        List<(int, int)> tranitions = [];
        var roofTopFlats = RaycastHelper.GetFlatTiles(roofTop);

        // if there's a flat of length 1 at the start, add transition and manually add half slopes
        if (Main.tile[roofStart.X, roofTop.pos[0]].BlockType == BlockType.Solid)
            if (Main.tile[roofStart.X + 1, roofTop.pos[0] - 1].BlockType == BlockType.SlopeDownRight) {
                PaintedType.PlaceTile(roofStart.X + 1, roofTop.pos[0] - 1, palette.RoofMain);
                PaintedType.PlaceTile(roofStart.X, roofTop.pos[0] - 1, palette.RoofMain,
                    BlockType.HalfBlock);
                tranitions.Add((roofStart.X + 1, roofTop.pos[0] - 1));
            }
            else if (Main.tile[roofStart.X + 1, roofTop.pos[0]].BlockType == BlockType.SlopeDownLeft) {
                PaintedType.PlaceTile(roofStart.X + 1, roofTop.pos[0], palette.RoofMain);
                PaintedType.PlaceTile(roofStart.X, roofTop.pos[0] - 1, palette.RoofMain,
                    BlockType.HalfBlock);
                tranitions.Add((roofStart.X + 1, roofTop.pos[0]));
            }

        for (var flatIndex = 0; flatIndex < roofTopFlats.flatStartIndexes.Count; flatIndex++) {
            // if the correct slope to left or right of flat, create transition
            var furthestLeftIndex = roofTopFlats.flatStartIndexes[flatIndex];
            var leftTile = Main.tile[roofStart.X + furthestLeftIndex - 1,
                roofTop.pos[furthestLeftIndex] - 1];
            if (leftTile is { HasTile: true, BlockType: BlockType.SlopeDownLeft }) {
                leftTile.BlockType = BlockType.Solid;
                tranitions.Add((roofStart.X + furthestLeftIndex - 1, roofTop.pos[furthestLeftIndex] - 1));
            }

            var furthestRightIndex = roofTopFlats.flatStartIndexes[flatIndex] + roofTopFlats.flatLengths[flatIndex] - 1;
            var rightTile = Main.tile[roofStart.X + furthestRightIndex + 1,
                roofTop.pos[furthestRightIndex] - 1];
            if (rightTile is { HasTile: true, BlockType: BlockType.SlopeDownRight }) {
                rightTile.BlockType = BlockType.Solid;
                tranitions.Add((roofStart.X + furthestRightIndex + 1, roofTop.pos[furthestRightIndex] - 1));
            }
        }

        // 5th pass: actually add the half blocks
        for (var flatIndex = 0; flatIndex < roofTopFlats.flatStartIndexes.Count; flatIndex++)
        for (var xIndex = roofTopFlats.flatStartIndexes[flatIndex];
             xIndex < roofTopFlats.flatStartIndexes[flatIndex] + roofTopFlats.flatLengths[flatIndex];
             xIndex++)
            // ensure the tile underneath is solid (and not a transition tile)
            if (Main.tile[roofStart.X + xIndex, roofTop.pos[xIndex]].BlockType == BlockType.Solid &&
                !tranitions.Contains((roofStart.X + xIndex, roofTop.pos[xIndex])))
                PaintedType.PlaceTile(roofStart.X + xIndex, roofTop.pos[xIndex] - 1, palette.RoofMain,
                    BlockType.HalfBlock);

        // 6th pass: create endcaps
        var anySideEndsWithSlope = Math.Abs(roofBottom.slope[0]) > 0.05 || Math.Abs(roofBottom.slope[^1]) > 0.05;
        var hasTallEndCaps = anySideEndsWithSlope || Terraria.WorldGen.genRand.NextBool();
        if (hasEndCaps) {
            // left cap
            var leftTestTile = Main.tile[roofStart.X - 1, roofStart.Y - 2];
            if (!leftTestTile.HasTile || leftTestTile.BlockType != BlockType.Solid) {
                if (hasTallEndCaps) {
                    PaintedType.PlaceTile(roofStart.X - 1, roofStart.Y - 2, palette.RoofMain,
                        BlockType.SlopeUpRight);
                    PaintedType.PlaceTile(roofStart.X - 1, roofStart.Y - 3, palette.RoofMain,
                        BlockType.HalfBlock);
                    if (Main.tile[roofStart.X, roofTop.pos[0]].BlockType is
                        BlockType.SlopeDownRight or BlockType.SlopeDownLeft) {
                        PaintedType.PlaceTile(roofStart.X, roofTop.pos[0], palette.RoofMain);
                        PaintedType.PlaceWall(roofStart.X + 1, roofStart.Y - 1,
                            palette.BackgroundRoofMain);
                    }
                }
                else {
                    PaintedType.PlaceTile(roofStart.X + 1, roofStart.Y - 1, palette.RoofMain,
                        BlockType.SlopeUpLeft);
                    PaintedType.PlaceTile(roofStart.X, roofStart.Y - 1, palette.RoofMain);
                    PaintedType.PlaceTile(roofStart.X - 1, roofStart.Y - 1, palette.RoofMain);
                    PaintedType.PlaceTile(roofStart.X - 2, roofStart.Y - 1, palette.RoofMain,
                        BlockType.SlopeUpRight);
                    PaintedType.PlaceTile(roofStart.X, roofStart.Y - 2, palette.RoofMain);
                    PaintedType.PlaceTile(roofStart.X - 1, roofStart.Y - 2, palette.RoofMain,
                        BlockType.HalfBlock);
                    PaintedType.PlaceTile(roofStart.X - 2, roofStart.Y - 2, palette.RoofMain,
                        BlockType.HalfBlock);
                    if (Main.tile[roofStart.X, roofStart.Y - 3].BlockType ==
                        BlockType.HalfBlock)
                        Main.tile[roofStart.X, roofStart.Y - 3].ClearTile();
                }
            }

            // right cap
            var rightPosX = roofStart.X + roofLength - 1;
            var rightTestTile = Main.tile[rightPosX + 1, roofBottom.pos[^1] - 2];
            if (!rightTestTile.HasTile || rightTestTile.BlockType != BlockType.Solid) {
                if (hasTallEndCaps) {
                    PaintedType.PlaceTile(rightPosX + 1, roofBottom.pos[^1] - 2, palette.RoofMain,
                        BlockType.SlopeUpLeft);
                    PaintedType.PlaceTile(rightPosX + 1, roofBottom.pos[^1] - 3, palette.RoofMain, BlockType.HalfBlock);
                    if (Main.tile[roofStart.X + roofLength - 1, roofTop.pos[^1]].BlockType is
                        BlockType.SlopeDownRight or BlockType.SlopeDownLeft) {
                        PaintedType.PlaceTile(roofStart.X + roofLength - 1, roofTop.pos[^1],
                            palette.RoofMain);
                        PaintedType.PlaceWall(roofStart.X + roofLength - 2, roofTop.pos[^1] + 2,
                            palette.BackgroundRoofMain);
                    }
                }
                else {
                    PaintedType.PlaceTile(rightPosX - 1, roofBottom.pos[^1] - 1, palette.RoofMain,
                        BlockType.SlopeUpRight);
                    PaintedType.PlaceTile(rightPosX, roofBottom.pos[^1] - 1, palette.RoofMain);
                    PaintedType.PlaceTile(rightPosX + 1, roofBottom.pos[^1] - 1, palette.RoofMain);
                    PaintedType.PlaceTile(rightPosX + 2, roofBottom.pos[^1] - 1, palette.RoofMain,
                        BlockType.SlopeUpLeft);
                    PaintedType.PlaceTile(rightPosX, roofBottom.pos[^1] - 2, palette.RoofMain);
                    PaintedType.PlaceTile(rightPosX + 1, roofBottom.pos[^1] - 2, palette.RoofMain, BlockType.HalfBlock);
                    PaintedType.PlaceTile(rightPosX + 2, roofBottom.pos[^1] - 2, palette.RoofMain, BlockType.HalfBlock);
                    if (Main.tile[rightPosX, roofBottom.pos[^1] - 3].BlockType == BlockType.HalfBlock)
                        Main.tile[rightPosX, roofBottom.pos[^1] - 3].ClearTile();
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
        componentParams.Volume.ExecuteInArea((x, y) => {
            PaintedType.PlaceTile(x, y, componentParams.TilePalette.FloorMain);
        });

        return false;
    }

    /// <summary>
    ///     Fills a volume with random floor blocks
    /// </summary>
    public static object Floor2(ComponentParams componentParams) {
        componentParams.Volume.ExecuteInArea((x, y) => {
            PaintedType.PlaceTile(x, y, PaintedType.PickRandom(componentParams.TilePalette.FloorAlt));
        });

        return false;
    }

    /// <summary>
    ///     Fills a volume with random blocks, but the top block consistent. great for base-layer floors
    /// </summary>
    public static object Floor3(ComponentParams componentParams) {
        var elevated = componentParams.TagsRequired.Contains(StructureTag.FloorElevated);
        int xStart = componentParams.Volume.BoundingBox.topLeft.X;
        var xSize = componentParams.Volume.BoundingBox.bottomRight.X - xStart + 1;
        var topY = new int[xSize];

        componentParams.Volume.ExecuteInArea((x, y) => {
            PaintedType.PlaceTile(x, y,
                PaintedType.PickRandom(elevated
                    ? componentParams.TilePalette.FloorAlt
                    : componentParams.TilePalette.FloorAltElevated));

            if (topY[x - xStart] == 0)
                topY[x - xStart] = y;

            if (y < topY[x - xStart])
                topY[x - xStart] = y;
        });

        for (var index = 0; index < topY.Length; index++)
            PaintedType.PlaceTile(xStart + index, topY[index],
                elevated ? componentParams.TilePalette.FloorMainElevated : componentParams.TilePalette.FloorMain);

        return false;
    }

    /// <summary>
    ///     Fills top and bottom of volume, adds support struts in the middle
    /// </summary>
    public static object Floor4(ComponentParams componentParams) {
        var elevated = componentParams.TagsRequired.Contains(StructureTag.FloorElevated);
        int xStart = componentParams.Volume.BoundingBox.topLeft.X;
        var xSize = componentParams.Volume.BoundingBox.bottomRight.X - xStart + 1;
        var topY = new int[xSize];
        var bottomY = new int[xSize];
        var supportInterval = Terraria.WorldGen.genRand.Next(3, 5);

        componentParams.Volume.ExecuteInArea((x, y) => {
            if ((x - xStart - 2) % supportInterval == 0 || x == xStart ||
                x == componentParams.Volume.BoundingBox.bottomRight.X) {
                PaintedType.PlaceTile(x, y,
                    elevated ? componentParams.TilePalette.FloorMainElevated : componentParams.TilePalette.FloorMain);
            }
            else {
                PaintedType.PlaceWall(x, y, componentParams.TilePalette.BackgroundFloorMain);
                var tile = Main.tile[x, y];
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

        for (var index = 0; index < topY.Length; index++) {
            PaintedType.PlaceTile(xStart + index, topY[index],
                elevated ? componentParams.TilePalette.FloorMainElevated : componentParams.TilePalette.FloorMain);
            PaintedType.PlaceTile(xStart + index, bottomY[index],
                elevated ? componentParams.TilePalette.FloorMainElevated : componentParams.TilePalette.FloorMain);
        }

        return false;
    }

    /// <summary>
    ///     A gap floor, places platforms
    /// </summary>
    public static object FloorGap1(ComponentParams componentParams) {
        int xStart = componentParams.Volume.BoundingBox.topLeft.X;
        var xSize = componentParams.Volume.BoundingBox.bottomRight.X - xStart + 1;
        var topY = new int[xSize];
        var bottomY = new int[xSize];

        componentParams.Volume.ExecuteInArea((x, y) => {
            PaintedType.PlaceWall(x, y, componentParams.TilePalette.BackgroundFloorMain);
            var tile = Main.tile[x, y];
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

        for (var index = 0; index < topY.Length; index++) {
            PaintedType.PlaceTile(xStart + index, topY[index], componentParams.TilePalette.Platform);
            PaintedType.PlaceTile(xStart + index, bottomY[index], componentParams.TilePalette.Platform);
        }

        return false;
    }

    #endregion


    #region Wall Methods

    /// <summary>
    ///     Fills a volume with the same wall blocks, with special blocks at regular intervals
    /// </summary>
    public static object Wall1(ComponentParams componentParams) {
        var elevated = componentParams.TagsRequired.Contains(StructureTag.WallElevated);
        int yStart = componentParams.Volume.BoundingBox.topLeft.Y;
        var ySize = componentParams.Volume.BoundingBox.bottomRight.Y - yStart + 1;
        var lowX = new int[ySize];
        var highX = new int[ySize];
        componentParams.Volume.ExecuteInArea((x, y) => {
            PaintedType.PlaceTile(x, y,
                elevated ? componentParams.TilePalette.WallMainElevated : componentParams.TilePalette.WallMain);

            if (lowX[y - yStart] == 0)
                lowX[y - yStart] = x;
            if (highX[y - yStart] == 0)
                highX[y - yStart] = x;

            if (x < lowX[y - yStart])
                lowX[y - yStart] = x;
            if (x > highX[y - yStart])
                highX[y - yStart] = x;
        });

        for (var index = 0; index < lowX.Length; index++) {
            PaintedType.PlaceTile(yStart + index, lowX[index], componentParams.TilePalette.WallSpecial);
            PaintedType.PlaceTile(yStart + index, highX[index], componentParams.TilePalette.WallSpecial);
        }

        return false;
    }

    /// <summary>
    ///     Fills a volume with random wall blocks, with special blocks at regular intervals
    /// </summary>
    public static object Wall2(ComponentParams componentParams) {
        var elevated = componentParams.TagsRequired.Contains(StructureTag.WallElevated);
        int yStart = componentParams.Volume.BoundingBox.topLeft.Y;
        var ySize = componentParams.Volume.BoundingBox.bottomRight.Y - yStart + 1;
        var lowX = new int[ySize];
        var highX = new int[ySize];
        componentParams.Volume.ExecuteInArea((x, y) => {
            PaintedType.PlaceTile(x, y, PaintedType.PickRandom(
                elevated ? componentParams.TilePalette.WallAltElevated : componentParams.TilePalette.WallAlt));

            if (lowX[y - yStart] == 0)
                lowX[y - yStart] = x;
            if (highX[y - yStart] == 0)
                highX[y - yStart] = x;

            if (x < lowX[y - yStart])
                lowX[y - yStart] = x;
            if (x > highX[y - yStart])
                highX[y - yStart] = x;
        });

        for (var index = 0; index < lowX.Length; index++) {
            PaintedType.PlaceTile(yStart + index, lowX[index], componentParams.TilePalette.WallSpecial);
            PaintedType.PlaceTile(yStart + index, highX[index], componentParams.TilePalette.WallSpecial);
        }

        return false;
    }

    /// <summary>
    ///     Fills a volume with random blocks, but the bottom block consistent
    /// </summary>
    public static object Wall3(ComponentParams componentParams) {
        var elevated = componentParams.TagsRequired.Contains(StructureTag.WallElevated);
        int xStart = componentParams.Volume.BoundingBox.topLeft.X;
        var xSize = componentParams.Volume.BoundingBox.bottomRight.X - xStart + 1;
        var bottomY = new int[xSize];

        componentParams.Volume.ExecuteInArea((x, y) => {
            PaintedType.PlaceTile(x, y, PaintedType.PickRandom(
                elevated ? componentParams.TilePalette.WallAltElevated : componentParams.TilePalette.WallAlt));

            if (bottomY[x - xStart] == 0)
                bottomY[x - xStart] = y;

            if (y > bottomY[x - xStart])
                bottomY[x - xStart] = y;
        });

        for (var index = 0; index < bottomY.Length; index++)
            PaintedType.PlaceTile(xStart + index, bottomY[index],
                elevated ? componentParams.TilePalette.WallAccentElevated : componentParams.TilePalette.WallAccent);

        return false;
    }

    /// <summary>
    ///     Fills a volume with random background walls
    /// </summary>
    public static object WallGap1(ComponentParams componentParams) {
        componentParams.Volume.ExecuteInArea((x, y) => {
            PaintedType.PlaceWall(x, y, PaintedType.PickRandom(componentParams.TilePalette.BackgroundWallAlt));
        });

        return false;
    }

    #endregion


    #region Stairway Methods

    #endregion


    #region Background Methods

    public static object Background1(ComponentParams componentParams) {
        componentParams.Volume.ExecuteInArea((x, y) => {
            if (y == componentParams.Volume.BoundingBox.bottomRight.Y)
                PaintedType.PlaceWall(x, y, PaintedType.PickRandom(componentParams.TilePalette.BackgroundRoomAlt));
            else if (y == componentParams.Volume.BoundingBox.bottomRight.Y - 1)
                PaintedType.PlaceWall(x, y, componentParams.TilePalette.BackgroundRoomAccent);
            else
                PaintedType.PlaceWall(x, y, componentParams.TilePalette.BackgroundRoomMain);
        });

        return false;
    }

    public static object Background2(ComponentParams componentParams) {
        int bottomY = componentParams.Volume.BoundingBox.bottomRight.Y;
        componentParams.Volume.ExecuteInArea((x, y) => {
            if (y == bottomY || y == bottomY - 1 || y == bottomY - 2)
                PaintedType.PlaceWall(x, y, PaintedType.PickRandom(componentParams.TilePalette.BackgroundRoomAlt));
            else
                PaintedType.PlaceWall(x, y, componentParams.TilePalette.BackgroundRoomMain);
        });

        return false;
    }

    #endregion
}