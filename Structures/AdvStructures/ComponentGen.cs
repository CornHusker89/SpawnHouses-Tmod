using System;
using System.Collections.Generic;
using System.Linq;
using Terraria;
using Terraria.DataStructures;
using Terraria.ID;

namespace SpawnHouses.Structures.AdvStructures;

public static class ComponentGen
{
    public static readonly (StructureTag[] possibleTags, Func<ComponentParams, object> method)[] GenMethods =
    [
        (
            [
                StructureTag.HasRoof,
                StructureTag.RoofShort,
                StructureTag.RoofTall,
                StructureTag.RoofSlantCenterHigh,
                StructureTag.RoofSlantLeftHigh,
                StructureTag.RoofSlantRightHigh,
                StructureTag.RoofSlope1To1,
                StructureTag.RoofSlopeLessThan1,
                StructureTag.RoofSlopeNone
            ],
            Roof2
        ),

            
            
        (
            [
                StructureTag.HasFloor, 
                StructureTag.FloorSolid,
                StructureTag.FloorGroundLevel,
                StructureTag.FloorThin,
                StructureTag.FloorThick,
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
                StructureTag.FloorGroundLevel,
                StructureTag.FloorElevated,
                StructureTag.FloorThick
            ],
            Floor4
        ),
            
            
            
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
        )
    ];

    public static Func<ComponentParams, object> GetRandomMethod(ComponentParams componentParams)
    {
        List<(StructureTag[] possibleTags, Func<ComponentParams, object> method)> methodTuples = [];
        foreach (var tuple in GenMethods)
        {
            List<StructureTag> requiredTags = componentParams.TagsRequired.ToList();
            foreach (var possibleTag in tuple.possibleTags)
            {
                if (componentParams.TagsBlacklist.Contains(possibleTag))
                    break;
                requiredTags.Remove(possibleTag);
            }

            if (requiredTags.Count == 0)
                methodTuples.Add(tuple);
        }

        if (methodTuples.Count == 0)
            throw new Exception("No components found were compatible with given tags");
        return methodTuples[Terraria.WorldGen.genRand.Next(0, methodTuples.Count)].method;
    }

    #region Roof Methods

    /// <summary>
    /// basic roof, capable of handling most roof shapes
    /// </summary>
    public static object Roof2(ComponentParams componentParams)
    {
        TilePalette palette = componentParams.TilePalette;

        Point16 roofStart = new Point16(componentParams.MainVolume.BoundingBox.topLeft.X, componentParams.MainVolume.BoundingBox.bottomRight.Y);
        int roofLength = componentParams.MainVolume.BoundingBox.bottomRight.X - roofStart.X;
        var roofBottom = StructureGenHelper.GetTopTilesPos(roofStart, roofLength);
        roofStart = new Point16(roofStart.X, roofBottom.pos[0]);

        var roofBottomFlats = StructureGenHelper.GetFlatTiles(roofBottom);
        bool isTallRoof = roofBottomFlats.flatLengths.Sum() > roofLength / 2 &&
                          roofLength > 30 &&
                          Terraria.WorldGen.genRand.Next(0, 4) == 0;
        bool hasEndCaps = Terraria.WorldGen.genRand.Next(0, 5) != 0;

        // pass 1: make roof shape
        int index = -1;
        for (int x = roofStart.X; x < roofStart.X + roofLength; x++)
        {
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
            else
                PaintedType.PlaceTile(x, roofBottom.pos[index] - 2, palette.RoofMain, BlockType.Solid);
        }

        // 2nd pass: validate slopes
        var roofTop =
            StructureGenHelper.GetTopTilesPos(roofStart + new Point16(0, -2), roofLength);
        index = 0;
        for (int x = roofStart.X + 1; x < roofStart.X + roofLength - 1; x++)
        {
            index++;
            if (Main.tile[x, roofTop.pos[index]].BlockType == BlockType.SlopeDownLeft &&
                Main.tile[x - 1, roofTop.pos[index] + 1].BlockType == BlockType.SlopeDownRight)
            {
                PaintedType.PlaceTile(x, roofTop.pos[index] + 1, palette.RoofMain, BlockType.Solid);
                Main.tile[x, roofTop.pos[index]].ClearTile();
            }
        }

        // 3rd pass: make slopes double width
        roofTop = StructureGenHelper.GetTopTilesPos(roofStart + new Point16(0, -2), roofLength);
        index = -1;
        for (int x = roofStart.X; x < roofStart.X + roofLength; x++)
        {
            index++;
            Tile tile = Main.tile[x, roofTop.pos[index]];
            switch (tile.BlockType)
            {
                case BlockType.SlopeDownRight: // up
                    PaintedType.PlaceTile(x, roofTop.pos[index], palette.RoofMain, BlockType.Solid);
                    if (index != 0 && index != roofLength - 1)
                        PaintedType.PlaceTile(x - 1, roofTop.pos[index], palette.RoofMain, BlockType.SlopeDownRight);
                    break;
                case BlockType.SlopeDownLeft: // down
                    PaintedType.PlaceTile(x, roofTop.pos[index], palette.RoofMain, BlockType.Solid);
                    if (index != 0 && index != +roofLength - 1)
                        PaintedType.PlaceTile(x + 1, roofTop.pos[index], palette.RoofMain, BlockType.SlopeDownLeft);
                    break;
            }
        }

        // 4th pass: prep slope transitions to half blocks
        roofTop = StructureGenHelper.GetTopTilesPos(roofStart + new Point16(0, -2), roofLength);
        List<(int, int)> tranitions = [];
        var roofTopFlats = StructureGenHelper.GetFlatTiles(roofTop);

        // if there's a flat of length 1 at the start, add transition and manually add half slopes
        if (Main.tile[roofStart.X, roofTop.pos[0]].BlockType == BlockType.Solid)
            if (Main.tile[roofStart.X + 1, roofTop.pos[0] - 1].BlockType == BlockType.SlopeDownRight)
            {
                PaintedType.PlaceTile(roofStart.X + 1, roofTop.pos[0] - 1, palette.RoofMain,
                    BlockType.Solid);
                PaintedType.PlaceTile(roofStart.X, roofTop.pos[0] - 1, palette.RoofMain,
                    BlockType.HalfBlock);
                tranitions.Add((roofStart.X + 1, roofTop.pos[0] - 1));
            }
            else if (Main.tile[roofStart.X + 1, roofTop.pos[0]].BlockType == BlockType.SlopeDownLeft)
            {
                PaintedType.PlaceTile(roofStart.X + 1, roofTop.pos[0], palette.RoofMain, BlockType.Solid);
                PaintedType.PlaceTile(roofStart.X, roofTop.pos[0] - 1, palette.RoofMain,
                    BlockType.HalfBlock);
                tranitions.Add((roofStart.X + 1, roofTop.pos[0]));
            }

        for (int flatIndex = 0; flatIndex < roofTopFlats.flatStartIndexes.Count; flatIndex++)
        {
            // if the correct slope to left or right of flat, create transition
            int furthestLeftIndex = roofTopFlats.flatStartIndexes[flatIndex];
            Tile leftTile = Main.tile[roofStart.X + furthestLeftIndex - 1,
                roofTop.pos[furthestLeftIndex] - 1];
            if (leftTile is { HasTile: true, BlockType: BlockType.SlopeDownLeft })
            {
                leftTile.BlockType = BlockType.Solid;
                tranitions.Add((roofStart.X + furthestLeftIndex - 1, roofTop.pos[furthestLeftIndex] - 1));
            }

            int furthestRightIndex = roofTopFlats.flatStartIndexes[flatIndex] + roofTopFlats.flatLengths[flatIndex] - 1;
            Tile rightTile = Main.tile[roofStart.X + furthestRightIndex + 1,
                roofTop.pos[furthestRightIndex] - 1];
            if (rightTile is { HasTile: true, BlockType: BlockType.SlopeDownRight })
            {
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
            if (Main.tile[roofStart.X + xIndex, roofTop.pos[xIndex]].BlockType == BlockType.Solid &&
                !tranitions.Contains((roofStart.X + xIndex, roofTop.pos[xIndex])))
            {
                PaintedType.PlaceTile(roofStart.X + xIndex, roofTop.pos[xIndex] - 1, palette.RoofMain,
                    BlockType.HalfBlock);
            }

        // 6th pass: create endcaps
        bool anySideEndsWithSlope = Math.Abs(roofBottom.slope[0]) > 0.05 || Math.Abs(roofBottom.slope[^1]) > 0.05;
        bool hasTallEndCaps = anySideEndsWithSlope || Terraria.WorldGen.genRand.NextBool();
        if (hasEndCaps)
        {
            // left cap
            Tile leftTestTile = Main.tile[roofStart.X - 1, roofStart.Y - 2];
            if (!leftTestTile.HasTile || leftTestTile.BlockType != BlockType.Solid)
            {
                if (hasTallEndCaps)
                {
                    PaintedType.PlaceTile(roofStart.X - 1, roofStart.Y - 2, palette.RoofMain,
                        BlockType.SlopeUpRight);
                    PaintedType.PlaceTile(roofStart.X - 1, roofStart.Y - 3, palette.RoofMain,
                        BlockType.HalfBlock);
                    if (Main.tile[roofStart.X, roofTop.pos[0]].BlockType is
                        BlockType.SlopeDownRight or BlockType.SlopeDownLeft)
                    {
                        PaintedType.PlaceTile(roofStart.X, roofTop.pos[0], palette.RoofMain,
                            BlockType.Solid);
                        PaintedType.PlaceWall(roofStart.X + 1, roofStart.Y - 1,
                            palette.BackgroundRoofMain);
                    }
                }
                else
                {
                    PaintedType.PlaceTile(roofStart.X + 1, roofStart.Y - 1, palette.RoofMain,
                        BlockType.SlopeUpLeft);
                    PaintedType.PlaceTile(roofStart.X, roofStart.Y - 1, palette.RoofMain,
                        BlockType.Solid);
                    PaintedType.PlaceTile(roofStart.X - 1, roofStart.Y - 1, palette.RoofMain,
                        BlockType.Solid);
                    PaintedType.PlaceTile(roofStart.X - 2, roofStart.Y - 1, palette.RoofMain,
                        BlockType.SlopeUpRight);
                    PaintedType.PlaceTile(roofStart.X, roofStart.Y - 2, palette.RoofMain,
                        BlockType.Solid);
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
            int rightPosX = roofStart.X + roofLength - 1;
            Tile rightTestTile = Main.tile[rightPosX + 1, roofBottom.pos[^1] - 2];
            if (!rightTestTile.HasTile || rightTestTile.BlockType != BlockType.Solid)
            {
                if (hasTallEndCaps)
                {
                    PaintedType.PlaceTile(rightPosX + 1, roofBottom.pos[^1] - 2, palette.RoofMain,
                        BlockType.SlopeUpLeft);
                    PaintedType.PlaceTile(rightPosX + 1, roofBottom.pos[^1] - 3, palette.RoofMain, BlockType.HalfBlock);
                    if (Main.tile[roofStart.X + roofLength - 1, roofTop.pos[^1]].BlockType is
                        BlockType.SlopeDownRight or BlockType.SlopeDownLeft)
                    {
                        PaintedType.PlaceTile(roofStart.X + roofLength - 1, roofTop.pos[^1],
                            palette.RoofMain, BlockType.Solid);
                        PaintedType.PlaceWall(roofStart.X + roofLength - 2, roofTop.pos[^1] + 2,
                            palette.BackgroundRoofMain);
                    }
                }
                else
                {
                    PaintedType.PlaceTile(rightPosX - 1, roofBottom.pos[^1] - 1, palette.RoofMain,
                        BlockType.SlopeUpRight);
                    PaintedType.PlaceTile(rightPosX, roofBottom.pos[^1] - 1, palette.RoofMain, BlockType.Solid);
                    PaintedType.PlaceTile(rightPosX + 1, roofBottom.pos[^1] - 1, palette.RoofMain, BlockType.Solid);
                    PaintedType.PlaceTile(rightPosX + 2, roofBottom.pos[^1] - 1, palette.RoofMain,
                        BlockType.SlopeUpLeft);
                    PaintedType.PlaceTile(rightPosX, roofBottom.pos[^1] - 2, palette.RoofMain, BlockType.Solid);
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
    /// Fills a volume with the same floor blocks
    /// </summary>
    public static object Floor1(ComponentParams componentParams)
    {
        componentParams.MainVolume.ExecuteInArea((x, y) =>
        {
            PaintedType.PlaceTile(x, y, componentParams.TilePalette.FloorMain);
        });
    
        return false;
    }

    /// <summary>
    /// Fills a volume with random floor blocks
    /// </summary>
    public static object Floor2(ComponentParams componentParams)
    {
        componentParams.MainVolume.ExecuteInArea((x, y) =>
        {
            PaintedType.PlaceTile(x, y, PaintedType.PickRandom(componentParams.TilePalette.FloorAlt));
        });

        return false;
    }

    /// <summary>
    /// Fills a volume with random blocks, but the top block consistent. great for base-layer floors
    /// </summary>
    public static object Floor3(ComponentParams componentParams)
    {
        bool elevated = componentParams.TagsRequired.Contains(StructureTag.FloorElevated);
        int xStart = componentParams.MainVolume.BoundingBox.topLeft.X;
        int xSize = componentParams.MainVolume.BoundingBox.bottomRight.X - xStart + 1;
        int[] topY = new int[xSize];
    
        componentParams.MainVolume.ExecuteInArea((x, y) =>
        {
            PaintedType.PlaceTile(x, y, 
                PaintedType.PickRandom(elevated? componentParams.TilePalette.FloorAlt : componentParams.TilePalette.FloorAltElevated));
    
            if (topY[x - xStart] == 0)
                topY[x - xStart] = y;
            
            if (y < topY[x - xStart])
                topY[x - xStart] = y;
        });
    
        for (int index = 0; index < topY.Length; index++)
            PaintedType.PlaceTile(xStart + index, topY[index], 
                elevated? componentParams.TilePalette.FloorMainElevated : componentParams.TilePalette.FloorMain);
    
        return false;
    }
    
    /// <summary>
    /// Fills top and bottom of volume, adds support struts in the middle
    /// </summary>
    public static object Floor4(ComponentParams componentParams)
    {
        bool elevated = componentParams.TagsRequired.Contains(StructureTag.FloorElevated);
        int xStart = componentParams.MainVolume.BoundingBox.topLeft.X;
        int xSize = componentParams.MainVolume.BoundingBox.bottomRight.X - xStart + 1;
        int[] topY = new int[xSize];
        int[] bottomY = new int[xSize];
        int supportInterval = Terraria.WorldGen.genRand.Next(3, 5);
    
        componentParams.MainVolume.ExecuteInArea((x, y) =>
        {
            if ((x - xStart + 1) % supportInterval == 0)
                PaintedType.PlaceTile(x, y, elevated? componentParams.TilePalette.FloorMainElevated : componentParams.TilePalette.FloorMain);
            else
            {
                Tile tile = Main.tile[x, y];
                tile.HasTile = false;
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
    
        for (int index = 0; index < topY.Length; index++)
        {
            PaintedType.PlaceTile(xStart + index, topY[index], 
                elevated? componentParams.TilePalette.FloorMainElevated : componentParams.TilePalette.FloorMain);
            PaintedType.PlaceTile(xStart + index, bottomY[index], 
                elevated? componentParams.TilePalette.FloorMainElevated : componentParams.TilePalette.FloorMain);
        }
    
        return false;
    }
    
    /// <summary>
    /// A gap floor, places platforms 
    /// </summary>
    public static object Floor5(ComponentParams componentParams)
    {
        return false;
    }

    #endregion


    #region Wall Methods

    /// <summary>
    /// Fills a volume with the same wall blocks, with special blocks at regular intervals
    /// </summary>
    public static object Wall1(ComponentParams componentParams)
    {
        int yStart = componentParams.MainVolume.BoundingBox.topLeft.Y;
        int ySize = componentParams.MainVolume.BoundingBox.bottomRight.Y - yStart + 1;
        int[] lowX = new int[ySize];
        int[] highX = new int[ySize];
        componentParams.MainVolume.ExecuteInArea((x, y) =>
        {
            PaintedType.PlaceTile(x, y, componentParams.TilePalette.WallMain);
            
            if (lowX[y - yStart] == 0)
                lowX[y - yStart] = x;
            if (highX[y - yStart] == 0)
                highX[y - yStart] = x;
    
            if (x < lowX[y - yStart])
                lowX[y - yStart] = x;
            if (x > highX[y - yStart])
                highX[y - yStart] = x;
        });
        
        for (int index = 0; index < lowX.Length; index++)
        {
            PaintedType.PlaceTile(yStart + index, lowX[index], componentParams.TilePalette.WallSpecial);
            PaintedType.PlaceTile(yStart + index, highX[index], componentParams.TilePalette.WallSpecial);
        }
    
        return false;
    }

    /// <summary>
    /// Fills a volume with random wall blocks, with special blocks at regular intervals
    /// </summary>
    public static object Wall2(ComponentParams componentParams)
    {
        int yStart = componentParams.MainVolume.BoundingBox.topLeft.Y;
        int ySize = componentParams.MainVolume.BoundingBox.bottomRight.Y - yStart + 1;
        int[] lowX = new int[ySize];
        int[] highX = new int[ySize];
        componentParams.MainVolume.ExecuteInArea((x, y) =>
        {
            PaintedType.PlaceTile(x, y, PaintedType.PickRandom(componentParams.TilePalette.WallAlt));
                        
            if (lowX[y - yStart] == 0)
                lowX[y - yStart] = x;
            if (highX[y - yStart] == 0)
                highX[y - yStart] = x;
    
            if (x < lowX[y - yStart])
                lowX[y - yStart] = x;
            if (x > highX[y - yStart])
                highX[y - yStart] = x;
        });
        
        for (int index = 0; index < lowX.Length; index++)
        {
            PaintedType.PlaceTile(yStart + index, lowX[index], componentParams.TilePalette.WallSpecial);
            PaintedType.PlaceTile(yStart + index, highX[index], componentParams.TilePalette.WallSpecial);
        }

        return false;
    }

    /// <summary>
    /// Fills a volume with random blocks, but the bottom block consistent
    /// </summary>
    public static object Wall3(ComponentParams componentParams){
        int xStart = componentParams.MainVolume.BoundingBox.topLeft.X;
        int xSize = componentParams.MainVolume.BoundingBox.bottomRight.X - xStart + 1;
        int[] bottomY = new int[xSize];
    
        componentParams.MainVolume.ExecuteInArea((x, y) =>
        {
            PaintedType.PlaceTile(x, y, PaintedType.PickRandom(componentParams.TilePalette.WallAlt));
    
            if (bottomY[x - xStart] == 0)
                bottomY[x - xStart] = y;
            
            if (y > bottomY[x - xStart])
                bottomY[x - xStart] = y;
        });
    
        for (int index = 0; index < bottomY.Length; index++)
            PaintedType.PlaceTile(xStart + index, bottomY[index], componentParams.TilePalette.WallAccent);
    
        return false;
    }

    #endregion
    
    #region Room Methods
    
    public static object Room1(ComponentParams componentParams)
    {
        componentParams.MainVolume.ExecuteInArea((x, y) =>
        {
            PaintedType.PlaceTile(x, y, PaintedType.PickRandom(componentParams.TilePalette.WallAlt));
        });
        
        return false;
    }
    
    #endregion
}