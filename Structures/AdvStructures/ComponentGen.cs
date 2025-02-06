using static SpawnHouses.Structures.AdvStructures.Data;
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
            ([StructureTag.HasRoof, 
                StructureTag.RoofShort,
                StructureTag.RoofTall,
                StructureTag.RoofSlantCenterHigh, 
                StructureTag.RoofSlantLeftHigh, 
                StructureTag.RoofSlantRightHigh,
                StructureTag.RoofSlope1To1,
                StructureTag.RoofSlopeLessThan1,
                StructureTag.RoofSlopeNone], 
            Roof2)
        ];
    
    public static Func<ComponentParams, object> GetRandomMethod(ComponentParams componentParams)
    {
        List<(StructureTag[] possibleTags, Func<ComponentParams, object> method)> methodTuples = [];
        foreach (var tuple in GenMethods)
        {
            List<StructureTag> requiredTags = componentParams.TagsRequired.ToList();
            foreach (var tag in tuple.possibleTags)
            {
                if (componentParams.TagsBlacklist.Contains(tag))
                    break;
                requiredTags.Remove(tag);
            }
            if (requiredTags.Count == 0)
                methodTuples.Add(tuple);
        }
        if (methodTuples.Count == 0)
            throw new Exception("No components found were compatible with given tags");
        return methodTuples[Terraria.WorldGen.genRand.Next(0, methodTuples.Count)].method;
    }
    
    private static void PlaceTile(Tile tile, BlockType blockType, byte paintId = PaintID.None)
    {
        tile.HasTile = true;
        tile.TileType = TileID.BorealWood;
        tile.BlockType = blockType;
        tile.TileColor = paintId;
    }
    
    private static void PlaceWall(Tile tile, byte paintId = PaintID.None)
    {
        tile.WallType = WallID.Planked;
        tile.WallColor = paintId;
    }
    

    #region Roof Methods
    
    /// <summary>
    /// basic roof, capable of handling most roof shapes
    /// </summary>
    public static object Roof2(ComponentParams componentParams)
    {
        var roofBottom = StructureGenHelper.GetTopTilesPos(componentParams.Start, componentParams.Length);
        componentParams.Start = new Point16(componentParams.Start.X, roofBottom.pos[0]);
             
        var roofBottomFlats = StructureGenHelper.GetFlatTiles(roofBottom);
        bool isTallRoof = roofBottomFlats.flatLengths.Sum() > componentParams.Length / 2 && componentParams.Length > 30 &&
            Terraria.WorldGen.genRand.Next(0, 4) == 0;
        bool hasEndCaps = Terraria.WorldGen.genRand.Next(0, 5) != 0;

        // pass 1: make roof shape
        int index = -1;
        for (int x = componentParams.Start.X; x < componentParams.Start.X + componentParams.Length; x++)
        {
            index++;
            if (index != 0 && index != componentParams.Length - 1)
                PlaceWall(Main.tile[x, roofBottom.pos[index] - 1]);
            
            // check if we're on a slope
            if (Math.Abs(roofBottom.slope[index] + 1) < 0.05 && (index == 0 || roofBottom.pos[index] != roofBottom.pos[index - 1])) // if slope is -1 (up), and if we're actually on a slope
            {
                PlaceTile(Main.tile[x, roofBottom.pos[index] - 2], BlockType.SlopeDownRight);
                if (index != 0)
                    PlaceTile(Main.tile[x, roofBottom.pos[index] - 1], BlockType.SlopeUpLeft);
            }
            else if (Math.Abs(roofBottom.slope[index] - 1) < 0.05) // if slope is 1 (down)
            {
                PlaceTile(Main.tile[x, roofBottom.pos[index] - 2], BlockType.SlopeDownLeft);
                if (index != componentParams.Length - 1)
                    PlaceTile(Main.tile[x, roofBottom.pos[index] - 1], BlockType.SlopeUpRight);
            }
            else if (index != 0 && roofBottom.slope[index - 1] < -0.05) // if the tile behind had a - (up) slope, don't place a solid block
            {
                PlaceTile(Main.tile[x, roofBottom.pos[index] - 2], BlockType.SlopeDownRight);
                if (index != componentParams.Length - 1)
                    PlaceTile(Main.tile[x, roofBottom.pos[index] - 1], BlockType.SlopeUpLeft);
            }
            else
                PlaceTile(Main.tile[x, roofBottom.pos[index] - 2], BlockType.Solid);
        }
        
        // 2nd pass: validate slopes
        var roofTop = StructureGenHelper.GetTopTilesPos(componentParams.Start + new Point16(0, -2), componentParams.Length);        
        index = 0;
        for (int x = componentParams.Start.X + 1; x < componentParams.Start.X + componentParams.Length - 1; x++)
        {
            index++;
            if (Main.tile[x, roofTop.pos[index]].BlockType == BlockType.SlopeDownLeft &&
                Main.tile[x - 1, roofTop.pos[index] + 1].BlockType == BlockType.SlopeDownRight)
            {
                PlaceTile(Main.tile[x, roofTop.pos[index] + 1], BlockType.Solid);
                Main.tile[x, roofTop.pos[index]].ClearTile();
            }
        }

        // 3rd pass: make slopes double width
        roofTop = StructureGenHelper.GetTopTilesPos(componentParams.Start + new Point16(0, -2), componentParams.Length);
        index = -1;
        for (int x = componentParams.Start.X; x < componentParams.Start.X + componentParams.Length; x++)
        {            
            index++;
            Tile tile = Main.tile[x, roofTop.pos[index]];
            switch (tile.BlockType)
            {
                case BlockType.SlopeDownRight: // up
                    PlaceTile(tile, BlockType.Solid);
                    if (index != 0 && index != componentParams.Length - 1)
                        PlaceTile(Main.tile[x - 1, roofTop.pos[index]], BlockType.SlopeDownRight);
                    break;
                case BlockType.SlopeDownLeft: // down
                    PlaceTile(tile, BlockType.Solid);
                    if (index != 0 && index != + componentParams.Length - 1)
                        PlaceTile(Main.tile[x + 1, roofTop.pos[index]], BlockType.SlopeDownLeft);
                    break;
            }
        }
        
        // 4th pass: prep slope transitions to half blocks
        roofTop = StructureGenHelper.GetTopTilesPos(componentParams.Start + new Point16(0, -2), componentParams.Length);
        List<(int, int)> tranitions = [];
        var roofTopFlats = StructureGenHelper.GetFlatTiles(roofTop);
        
        // if there's a flat of length 1 at the start, add transition and manually add half slopes
        if (Main.tile[componentParams.Start.X, roofTop.pos[0]].BlockType == BlockType.Solid)
            if (Main.tile[componentParams.Start.X + 1, roofTop.pos[0] - 1].BlockType == BlockType.SlopeDownRight)
            {
                PlaceTile(Main.tile[componentParams.Start.X + 1, roofTop.pos[0] - 1], BlockType.Solid);
                PlaceTile(Main.tile[componentParams.Start.X, roofTop.pos[0] - 1], BlockType.HalfBlock);
                tranitions.Add((componentParams.Start.X + 1, roofTop.pos[0] - 1));
            }
            else if (Main.tile[componentParams.Start.X + 1, roofTop.pos[0]].BlockType == BlockType.SlopeDownLeft)
            {
                PlaceTile(Main.tile[componentParams.Start.X + 1, roofTop.pos[0]], BlockType.Solid);
                PlaceTile(Main.tile[componentParams.Start.X, roofTop.pos[0] - 1], BlockType.HalfBlock);
                tranitions.Add((componentParams.Start.X + 1, roofTop.pos[0]));
            }
        
        for (int flatIndex = 0; flatIndex < roofTopFlats.flatStartIndexes.Count; flatIndex++)
        {
            // if the correct slope to left or right of flat, create transition
            int furthestLeftIndex = roofTopFlats.flatStartIndexes[flatIndex];
            Tile leftTile = Main.tile[componentParams.Start.X + furthestLeftIndex - 1, roofTop.pos[furthestLeftIndex] - 1];
            if (leftTile is { HasTile: true, BlockType: BlockType.SlopeDownLeft })
            {
                leftTile.BlockType = BlockType.Solid;
                tranitions.Add((componentParams.Start.X + furthestLeftIndex - 1, roofTop.pos[furthestLeftIndex] - 1));
            }
            int furthestRightIndex = roofTopFlats.flatStartIndexes[flatIndex] + roofTopFlats.flatLengths[flatIndex] - 1;
            Tile rightTile = Main.tile[componentParams.Start.X + furthestRightIndex + 1, roofTop.pos[furthestRightIndex] - 1];
            if (rightTile is { HasTile: true, BlockType: BlockType.SlopeDownRight })
            {
                rightTile.BlockType = BlockType.Solid;
                tranitions.Add((componentParams.Start.X + furthestRightIndex + 1, roofTop.pos[furthestRightIndex] - 1));
            }
        }
        
        // 5th pass: actually add the half blocks
        for (int flatIndex = 0; flatIndex < roofTopFlats.flatStartIndexes.Count; flatIndex++)
            for (int xIndex = roofTopFlats.flatStartIndexes[flatIndex]; xIndex < roofTopFlats.flatStartIndexes[flatIndex] + roofTopFlats.flatLengths[flatIndex]; xIndex++)
                // ensure the tile underneath is solid (and not a transition tile)
                if (Main.tile[componentParams.Start.X + xIndex, roofTop.pos[xIndex]].BlockType == BlockType.Solid &&
                    !tranitions.Contains((componentParams.Start.X + xIndex, roofTop.pos[xIndex])))
                {
                    PlaceTile(Main.tile[componentParams.Start.X + xIndex, roofTop.pos[xIndex] - 1], BlockType.HalfBlock);
                }
        
        // 6th pass: create endcaps
        bool anySideEndsWithSlope = Math.Abs(roofBottom.slope[0]) > 0.05 || Math.Abs(roofBottom.slope[^1]) > 0.05;
        bool hasTallEndCaps = anySideEndsWithSlope || Terraria.WorldGen.genRand.NextBool();
        if (hasEndCaps)
        {
            // left cap
            Tile leftTestTile = Main.tile[componentParams.Start.X - 1, componentParams.Start.Y - 2];
            if (!leftTestTile.HasTile || leftTestTile.BlockType != BlockType.Solid)
            {
                if (hasTallEndCaps)
                {
                    PlaceTile(Main.tile[componentParams.Start.X - 1, componentParams.Start.Y - 2], BlockType.SlopeUpRight);
                    PlaceTile(Main.tile[componentParams.Start.X - 1, componentParams.Start.Y - 3], BlockType.HalfBlock);
                    if (Main.tile[componentParams.Start.X, roofTop.pos[0]].BlockType is 
                        BlockType.SlopeDownRight or BlockType.SlopeDownLeft)
                    {
                        PlaceTile(Main.tile[componentParams.Start.X, roofTop.pos[0]], BlockType.Solid);
                        PlaceWall(Main.tile[componentParams.Start.X + 1, componentParams.Start.Y - 1]);
                    }
                }
                else
                {
                    PlaceTile(Main.tile[componentParams.Start.X + 1, componentParams.Start.Y - 1], BlockType.SlopeUpLeft);
                    PlaceTile(Main.tile[componentParams.Start.X, componentParams.Start.Y - 1], BlockType.Solid);
                    PlaceTile(Main.tile[componentParams.Start.X - 1, componentParams.Start.Y - 1], BlockType.Solid);
                    PlaceTile(Main.tile[componentParams.Start.X - 2, componentParams.Start.Y - 1], BlockType.SlopeUpRight);
                    PlaceTile(Main.tile[componentParams.Start.X, componentParams.Start.Y - 2], BlockType.Solid);
                    PlaceTile(Main.tile[componentParams.Start.X - 1, componentParams.Start.Y - 2], BlockType.HalfBlock);
                    PlaceTile(Main.tile[componentParams.Start.X - 2, componentParams.Start.Y - 2], BlockType.HalfBlock);
                    if (Main.tile[componentParams.Start.X, componentParams.Start.Y - 3].BlockType == BlockType.HalfBlock)
                        Main.tile[componentParams.Start.X, componentParams.Start.Y - 3].ClearTile();
                }
            }
            
            // right cap
            int rightPosX = componentParams.Start.X + componentParams.Length - 1;
            Tile rightTestTile = Main.tile[rightPosX + 1, roofBottom.pos[^1] - 2];
            if (!rightTestTile.HasTile || rightTestTile.BlockType != BlockType.Solid)
            {
                if (hasTallEndCaps)
                {
                    PlaceTile(Main.tile[rightPosX + 1, roofBottom.pos[^1] - 2], BlockType.SlopeUpLeft);
                    PlaceTile(Main.tile[rightPosX + 1, roofBottom.pos[^1] - 3], BlockType.HalfBlock);
                    if (Main.tile[componentParams.Start.X + componentParams.Length - 1, roofTop.pos[^1]].BlockType is 
                        BlockType.SlopeDownRight or BlockType.SlopeDownLeft)
                    {
                        PlaceTile(Main.tile[componentParams.Start.X + componentParams.Length - 1, roofTop.pos[^1]], BlockType.Solid);
                        PlaceWall(Main.tile[componentParams.Start.X + componentParams.Length - 2, roofTop.pos[^1] + 2]);
                    }
                }
                else
                {
                    PlaceTile(Main.tile[rightPosX - 1, roofBottom.pos[^1] - 1], BlockType.SlopeUpRight);
                    PlaceTile(Main.tile[rightPosX, roofBottom.pos[^1] - 1], BlockType.Solid);
                    PlaceTile(Main.tile[rightPosX + 1, roofBottom.pos[^1] - 1], BlockType.Solid);
                    PlaceTile(Main.tile[rightPosX + 2, roofBottom.pos[^1] - 1], BlockType.SlopeUpLeft);
                    PlaceTile(Main.tile[rightPosX, roofBottom.pos[^1] - 2], BlockType.Solid);
                    PlaceTile(Main.tile[rightPosX + 1, roofBottom.pos[^1] - 2], BlockType.HalfBlock);
                    PlaceTile(Main.tile[rightPosX + 2, roofBottom.pos[^1] - 2], BlockType.HalfBlock);
                    if (Main.tile[rightPosX, roofBottom.pos[^1] - 3].BlockType == BlockType.HalfBlock)
                        Main.tile[rightPosX, roofBottom.pos[^1] - 3].ClearTile();
                }
            }
        }
        return false;
    }
    
    #endregion
    
    
    #region Volume Methods
    
    /// <summary>
    /// Creates a volume with random blocks, but the top block consistent. great for base-layer floors
    /// </summary>
    public static void Volume1(ComponentParams componentParams)
    {
    }
    
    #endregion
    
    
    #region Room Methods
    #endregion
}