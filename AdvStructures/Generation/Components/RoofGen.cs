using System;
using System.Collections.Generic;
using System.Linq;
using SpawnHouses.Helpers;
using SpawnHouses.Types;
using Terraria;
using Terraria.DataStructures;
using Terraria.ID;

namespace SpawnHouses.AdvStructures.Generation.Components;

public static class RoofGen {
    public class RoofGenerator1 : IComponentGenerator {
        public ComponentTag[] GetPossibleTags() {
            return [
                ComponentTag.IsRoof,
                ComponentTag.External,
                ComponentTag.RoofShort,
                ComponentTag.RoofTall,
                ComponentTag.RoofSlope1To1,
                ComponentTag.RoofSlopeLessThan1,
                ComponentTag.RoofSlopeNone
            ];
        }

        public bool Generate(ComponentParams componentParams) {
            TilePalette p = componentParams.TilePalette;
            Point16 roofStart = new Point16(componentParams.Volume.BoundingBox.topLeft.X, componentParams.Volume.BoundingBox.bottomRight.Y);
            int roofLength = componentParams.Volume.BoundingBox.bottomRight.X - roofStart.X + 1;
            var roofBottom = RaycastHelper.GetTopTilesPos(roofStart, roofLength, tilemap: componentParams.Tilemap);

            roofStart = new Point16(roofStart.X, roofBottom.pos[0]);

            var roofBottomFlats = RaycastHelper.GetFlatTiles(roofBottom);
            bool isTallRoof = roofBottomFlats.flatLengths.Sum() > roofLength / 2 &&
                roofLength >= 22 &&
                Terraria.WorldGen.genRand.Next(0, 4) == 0;
            bool hasEndCaps = Terraria.WorldGen.genRand.Next(0, 5) != 0;

            // pass 1: make roof shape
            int index = -1;
            for (int x = roofStart.X; x < roofStart.X + roofLength; x++) {
                index++;
                if (index != 0 && index != roofLength - 1)
                    PaintedType.PlaceWall(x, roofBottom.pos[index] - 1, p.BackgroundRoofMain, componentParams.Tilemap);

                // check if we're on a slope
                if (Math.Abs(roofBottom.slope[index] + 1) < 0.05 &&
                    (index == 0 || roofBottom.pos[index] != roofBottom.pos[index - 1])) // if slope is -1 (up), and if we're actually on a slope
                {
                    PaintedType.PlaceTile(x, roofBottom.pos[index] - 2, p.RoofMain, componentParams.Tilemap, BlockType.SlopeDownRight);
                    if (index != 0)
                        PaintedType.PlaceTile(x, roofBottom.pos[index] - 1, p.RoofMain, componentParams.Tilemap, BlockType.SlopeUpLeft);
                }
                else if (Math.Abs(roofBottom.slope[index] - 1) < 0.05) // if slope is 1 (down)
                {
                    PaintedType.PlaceTile(x, roofBottom.pos[index] - 2, p.RoofMain, componentParams.Tilemap, BlockType.SlopeDownLeft);
                    if (index != roofLength - 1)
                        PaintedType.PlaceTile(x, roofBottom.pos[index] - 1, p.RoofMain, componentParams.Tilemap, BlockType.SlopeUpRight);
                }
                else if
                    (index != 0 &&
                     roofBottom.slope[index - 1] <
                     -0.05) // if the tile behind had a - (up) slope, don't place a solid block
                {
                    PaintedType.PlaceTile(x, roofBottom.pos[index] - 2, p.RoofMain, componentParams.Tilemap, BlockType.SlopeDownRight);
                    if (index != roofLength - 1)
                        PaintedType.PlaceTile(x, roofBottom.pos[index] - 1, p.RoofMain, componentParams.Tilemap, BlockType.SlopeUpLeft);
                }
                else {
                    PaintedType.PlaceTile(x, roofBottom.pos[index] - 2, p.RoofMain, componentParams.Tilemap);
                }
            }

            // 2nd pass: validate slopes
            var roofTop =
                RaycastHelper.GetTopTilesPos(roofStart + new Point16(0, -2), roofLength, tilemap: componentParams.Tilemap);
            index = 0;
            for (int x = roofStart.X + 1; x < roofStart.X + roofLength - 1; x++) {
                index++;
                if (componentParams.Tilemap[x, roofTop.pos[index]].BlockType == BlockType.SlopeDownLeft &&
                    componentParams.Tilemap[x - 1, roofTop.pos[index] + 1].BlockType == BlockType.SlopeDownRight) {
                    PaintedType.PlaceTile(x, roofTop.pos[index] + 1, p.RoofMain, componentParams.Tilemap);
                    componentParams.Tilemap[x, roofTop.pos[index]].ClearTile();
                }
            }

            // 3rd pass: make slopes double width
            roofTop = RaycastHelper.GetTopTilesPos(roofStart + new Point16(0, -2), roofLength, tilemap: componentParams.Tilemap);
            index = -1;
            for (int x = roofStart.X; x < roofStart.X + roofLength; x++) {
                index++;
                StructureTile tile = componentParams.Tilemap[x, roofTop.pos[index]];
                switch (tile.BlockType) {
                    case BlockType.SlopeDownRight: // up
                        PaintedType.PlaceTile(x, roofTop.pos[index], p.RoofMain, componentParams.Tilemap);
                        if (index != 0 && index != roofLength - 1)
                            PaintedType.PlaceTile(x - 1, roofTop.pos[index], p.RoofMain, componentParams.Tilemap, BlockType.SlopeDownRight);
                        break;
                    case BlockType.SlopeDownLeft: // down
                        PaintedType.PlaceTile(x, roofTop.pos[index], p.RoofMain, componentParams.Tilemap);
                        if (index != 0 && index != +roofLength - 1)
                            PaintedType.PlaceTile(x + 1, roofTop.pos[index], p.RoofMain, componentParams.Tilemap, BlockType.SlopeDownLeft);
                        break;
                }
            }

            // 4th pass: prep slope transitions to half blocks
            roofTop = RaycastHelper.GetTopTilesPos(roofStart + new Point16(0, -2), roofLength, tilemap: componentParams.Tilemap);
            List<(int, int)> tranitions = [];
            var roofTopFlats = RaycastHelper.GetFlatTiles(roofTop);

            // if there's a flat of length 1 at the start, add transition and manually add half slopes
            if (componentParams.Tilemap[roofStart.X, roofTop.pos[0]].BlockType == BlockType.Solid)
                if (componentParams.Tilemap[roofStart.X + 1, roofTop.pos[0] - 1].BlockType == BlockType.SlopeDownRight) {
                    PaintedType.PlaceTile(roofStart.X + 1, roofTop.pos[0] - 1, p.RoofMain, componentParams.Tilemap);
                    PaintedType.PlaceTile(roofStart.X, roofTop.pos[0] - 1, p.RoofMain, componentParams.Tilemap, BlockType.HalfBlock);
                    tranitions.Add((roofStart.X + 1, roofTop.pos[0] - 1));
                }
                else if (componentParams.Tilemap[roofStart.X + 1, roofTop.pos[0]].BlockType == BlockType.SlopeDownLeft) {
                    PaintedType.PlaceTile(roofStart.X + 1, roofTop.pos[0], p.RoofMain, componentParams.Tilemap);
                    PaintedType.PlaceTile(roofStart.X, roofTop.pos[0] - 1, p.RoofMain, componentParams.Tilemap, BlockType.HalfBlock);
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
                    PaintedType.PlaceTile(roofStart.X + xIndex, roofTop.pos[xIndex] - 1, p.RoofMain, componentParams.Tilemap, BlockType.HalfBlock);

            // 6th pass: create endcaps
            bool anySideEndsWithSlope = Math.Abs(roofBottom.slope[0]) > 0.05 || Math.Abs(roofBottom.slope[^1]) > 0.05;
            bool hasTallEndCaps = anySideEndsWithSlope || Terraria.WorldGen.genRand.NextBool();
            if (hasEndCaps) {
                // left cap
                StructureTile leftTestTile = componentParams.Tilemap[roofStart.X - 1, roofStart.Y - 2];
                if (!leftTestTile.HasTile || leftTestTile.BlockType != BlockType.Solid) {
                    if (hasTallEndCaps) {
                        PaintedType.PlaceTile(roofStart.X - 1, roofStart.Y - 2, p.RoofMain, componentParams.Tilemap, BlockType.SlopeUpRight);
                        PaintedType.PlaceTile(roofStart.X - 1, roofStart.Y - 3, p.RoofMain, componentParams.Tilemap, BlockType.HalfBlock);
                        if (componentParams.Tilemap[roofStart.X, roofTop.pos[0]].BlockType is
                            BlockType.SlopeDownRight or BlockType.SlopeDownLeft) {
                            PaintedType.PlaceTile(roofStart.X, roofTop.pos[0], p.RoofMain, componentParams.Tilemap);
                            PaintedType.PlaceWall(roofStart.X + 1, roofStart.Y - 1, p.BackgroundRoofMain, componentParams.Tilemap);
                        }
                    }
                    else {
                        PaintedType.PlaceTile(roofStart.X + 1, roofStart.Y - 1, p.RoofMain, componentParams.Tilemap, BlockType.SlopeUpLeft);
                        PaintedType.PlaceTile(roofStart.X, roofStart.Y - 1, p.RoofMain, componentParams.Tilemap);
                        PaintedType.PlaceTile(roofStart.X - 1, roofStart.Y - 1, p.RoofMain, componentParams.Tilemap);
                        PaintedType.PlaceTile(roofStart.X - 2, roofStart.Y - 1, p.RoofMain, componentParams.Tilemap, BlockType.SlopeUpRight);
                        PaintedType.PlaceTile(roofStart.X, roofStart.Y - 2, p.RoofMain, componentParams.Tilemap);
                        PaintedType.PlaceTile(roofStart.X - 1, roofStart.Y - 2, p.RoofMain, componentParams.Tilemap, BlockType.HalfBlock);
                        PaintedType.PlaceTile(roofStart.X - 2, roofStart.Y - 2, p.RoofMain, componentParams.Tilemap, BlockType.HalfBlock);
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
                        PaintedType.PlaceTile(rightPosX + 1, roofBottom.pos[^1] - 2, p.RoofMain, componentParams.Tilemap, BlockType.SlopeUpLeft);
                        PaintedType.PlaceTile(rightPosX + 1, roofBottom.pos[^1] - 3, p.RoofMain, componentParams.Tilemap, BlockType.HalfBlock);
                        if (componentParams.Tilemap[roofStart.X + roofLength - 1, roofTop.pos[^1]].BlockType is
                            BlockType.SlopeDownRight or BlockType.SlopeDownLeft) {
                            PaintedType.PlaceTile(roofStart.X + roofLength - 1, roofTop.pos[^1], p.RoofMain, componentParams.Tilemap);
                            PaintedType.PlaceWall(roofStart.X + roofLength - 2, roofTop.pos[^1] + 2, p.BackgroundRoofMain, componentParams.Tilemap);
                        }
                    }
                    else {
                        PaintedType.PlaceTile(rightPosX - 1, roofBottom.pos[^1] - 1, p.RoofMain, componentParams.Tilemap, BlockType.SlopeUpRight);
                        PaintedType.PlaceTile(rightPosX, roofBottom.pos[^1] - 1, p.RoofMain, componentParams.Tilemap);
                        PaintedType.PlaceTile(rightPosX + 1, roofBottom.pos[^1] - 1, p.RoofMain, componentParams.Tilemap);
                        PaintedType.PlaceTile(rightPosX + 2, roofBottom.pos[^1] - 1, p.RoofMain, componentParams.Tilemap, BlockType.SlopeUpLeft);
                        PaintedType.PlaceTile(rightPosX, roofBottom.pos[^1] - 2, p.RoofMain, componentParams.Tilemap);
                        PaintedType.PlaceTile(rightPosX + 1, roofBottom.pos[^1] - 2, p.RoofMain, componentParams.Tilemap, BlockType.HalfBlock);
                        PaintedType.PlaceTile(rightPosX + 2, roofBottom.pos[^1] - 2, p.RoofMain, componentParams.Tilemap, BlockType.HalfBlock);
                        if (componentParams.Tilemap[rightPosX, roofBottom.pos[^1] - 3].BlockType == BlockType.HalfBlock)
                            componentParams.Tilemap[rightPosX, roofBottom.pos[^1] - 3].ClearTile();
                    }
                }
            }

            return true;
        }
    }
}