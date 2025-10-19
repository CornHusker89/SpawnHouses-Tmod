using System.Collections.Generic;
using System.Linq;
using SpawnHouses.AdvStructures.AdvStructureParts;
using SpawnHouses.Helpers;
using SpawnHouses.Types;
using Terraria;
using Terraria.DataStructures;

namespace SpawnHouses.AdvStructures.Generation.Components;

public static class RoofGen {
    // public class RoofGenerator1 : IPathComponentGenerator {
    //     public HashSet<ComponentTag> GetPossibleTags() {
    //         return [
    //             ComponentTag.External,
    //             ComponentTag.RoofShort,
    //             ComponentTag.RoofTall,
    //             ComponentTag.RoofSlope1To1,
    //             ComponentTag.RoofSlopeLessThan1,
    //             ComponentTag.RoofSlopeNone
    //         ];
    //     }
    //
    //     public bool CanGenerate(PathComponentParams componentParams) {
    //         return false;
    //     }
    //
    //     public bool Generate(PathComponentParams param) {
    //         TilePalette p = param.Palette;
    //         Point16 roofStart = new(param.Component.Volume.BoundingBox.topLeft.X, param.Component.Volume.BoundingBox.bottomRight.Y);
    //         int roofLength = param.Component.Volume.BoundingBox.bottomRight.X - roofStart.X + 1;
    //         var roofBottom = RaycastHelper.GetTopTilesPos(roofStart, roofLength, tilemap: param.Tilemap);
    //
    //         roofStart = new Point16(roofStart.X, roofBottom.pos[0]);
    //         int rightPosX = roofStart.X + roofLength - 1;
    //
    //         var roofBottomFlats = RaycastHelper.GetFlatTiles(roofBottom);
    //         bool isTallRoof = roofBottomFlats.flatLengths.Sum() > roofLength / 2 &&
    //                           roofLength >= 22 &&
    //                           Terraria.WorldGen.genRand.Next(0, 4) == 0;
    //
    //         bool isLeftFlushWithWall = param.Tilemap[roofStart.X - 1, roofStart.Y - 2].HasTile;
    //         bool isRightFlushWithWall = param.Tilemap[rightPosX + 1, roofBottom.pos[^1] - 2].HasTile;
    //
    //         // pass 1: make roof shape
    //         int index = -1;
    //         for (int x = roofStart.X; x < roofStart.X + roofLength; x++) {
    //             index++;
    //             if ((index != 0 || isLeftFlushWithWall) && index != roofLength - 1)
    //                 param.Tilemap.PlaceWall(x, roofBottom.pos[index] - 1, p.BackgroundRoofMain);
    //
    //             // check if we're on a slope
    //             if (Math.Abs(roofBottom.slope[index] + 1) < 0.05 &&
    //                 (index == 0 || roofBottom.pos[index] != roofBottom.pos[index - 1])) // if slope is -1 (up), and if we're actually on a slope
    //             {
    //                 param.Tilemap.PlaceTile(x, roofBottom.pos[index] - 2, p.RoofMain, BlockType.SlopeDownRight);
    //                 if (index != 0)
    //                     param.Tilemap.PlaceTile(x, roofBottom.pos[index] - 1, p.RoofMain, BlockType.SlopeUpLeft);
    //             }
    //             else if (Math.Abs(roofBottom.slope[index] - 1) < 0.05) // if slope is 1 (down)
    //             {
    //                 param.Tilemap.PlaceTile(x, roofBottom.pos[index] - 2, p.RoofMain, BlockType.SlopeDownLeft);
    //                 if (index != roofLength - 1)
    //                     param.Tilemap.PlaceTile(x, roofBottom.pos[index] - 1, p.RoofMain, BlockType.SlopeUpRight);
    //             }
    //             else if
    //                 (index != 0 &&
    //                  roofBottom.slope[index - 1] <
    //                  -0.05) // if the tile behind had a - (up) slope, don't place a solid block
    //             {
    //                 param.Tilemap.PlaceTile(x, roofBottom.pos[index] - 2, p.RoofMain, BlockType.SlopeDownRight);
    //                 if (index != roofLength - 1)
    //                     param.Tilemap.PlaceTile(x, roofBottom.pos[index] - 1, p.RoofMain, BlockType.SlopeUpLeft);
    //             }
    //             else {
    //                 param.Tilemap.PlaceTile(x, roofBottom.pos[index] - 2, p.RoofMain);
    //             }
    //         }
    //
    //         // 2nd pass: validate slopes
    //         var roofTop =
    //             RaycastHelper.GetTopTilesPos(roofStart + new Point16(0, -2), roofLength, tilemap: param.Tilemap);
    //         index = 0;
    //         for (int x = roofStart.X + 1; x < rightPosX; x++) {
    //             index++;
    //             if (param.Tilemap[x, roofTop.pos[index]].BlockType == BlockType.SlopeDownLeft &&
    //                 param.Tilemap[x - 1, roofTop.pos[index] + 1].BlockType == BlockType.SlopeDownRight) {
    //                 param.Tilemap.PlaceTile(x, roofTop.pos[index] + 1, p.RoofMain);
    //                 param.Tilemap[x, roofTop.pos[index]].ClearTile();
    //             }
    //         }
    //
    //         // 3rd pass: make slopes double width
    //         roofTop = RaycastHelper.GetTopTilesPos(roofStart + new Point16(0, -2), roofLength, tilemap: param.Tilemap);
    //         index = -1;
    //         for (int x = roofStart.X; x < roofStart.X + roofLength; x++) {
    //             index++;
    //             StructureTile tile = param.Tilemap[x, roofTop.pos[index]];
    //             switch (tile.BlockType) {
    //                 case BlockType.SlopeDownRight: // up
    //                     param.Tilemap.PlaceTile(x, roofTop.pos[index], p.RoofMain);
    //                     if (index != 0 && index != roofLength - 1)
    //                         param.Tilemap.PlaceTile(x - 1, roofTop.pos[index], p.RoofMain, BlockType.SlopeDownRight);
    //                     break;
    //                 case BlockType.SlopeDownLeft: // down
    //                     param.Tilemap.PlaceTile(x, roofTop.pos[index], p.RoofMain);
    //                     if (index != 0 && index != +roofLength - 1)
    //                         param.Tilemap.PlaceTile(x + 1, roofTop.pos[index], p.RoofMain, BlockType.SlopeDownLeft);
    //                     break;
    //             }
    //         }
    //
    //         // 4th pass: prep slope transitions to half blocks
    //         roofTop = RaycastHelper.GetTopTilesPos(roofStart + new Point16(0, -2), roofLength, tilemap: param.Tilemap);
    //         List<(int, int)> tranitions = [];
    //         var roofTopFlats = RaycastHelper.GetFlatTiles(roofTop);
    //
    //         // if there's a flat of length 1 at the start, add transition and manually add half slopes
    //         if (param.Tilemap[roofStart.X, roofTop.pos[0]].BlockType == BlockType.Solid)
    //             if (param.Tilemap[roofStart.X + 1, roofTop.pos[0] - 1].BlockType == BlockType.SlopeDownRight) {
    //                 param.Tilemap.PlaceTile(roofStart.X + 1, roofTop.pos[0] - 1, p.RoofMain);
    //                 param.Tilemap.PlaceTile(roofStart.X, roofTop.pos[0] - 1, p.RoofMain, BlockType.HalfBlock);
    //                 tranitions.Add((roofStart.X + 1, roofTop.pos[0] - 1));
    //             }
    //             else if (param.Tilemap[roofStart.X + 1, roofTop.pos[0]].BlockType == BlockType.SlopeDownLeft) {
    //                 param.Tilemap.PlaceTile(roofStart.X + 1, roofTop.pos[0], p.RoofMain);
    //                 param.Tilemap.PlaceTile(roofStart.X, roofTop.pos[0] - 1, p.RoofMain, BlockType.HalfBlock);
    //                 tranitions.Add((roofStart.X + 1, roofTop.pos[0]));
    //             }
    //
    //         for (int flatIndex = 0; flatIndex < roofTopFlats.flatStartIndexes.Count; flatIndex++) {
    //             // if the correct slope to left or right of flat, create transition
    //             int furthestLeftIndex = roofTopFlats.flatStartIndexes[flatIndex];
    //             StructureTile leftTile = param.Tilemap[roofStart.X + furthestLeftIndex - 1,
    //                 roofTop.pos[furthestLeftIndex] - 1];
    //             if (leftTile is { HasTile: true, BlockType: BlockType.SlopeDownLeft }) {
    //                 leftTile.BlockType = BlockType.Solid;
    //                 tranitions.Add((roofStart.X + furthestLeftIndex - 1, roofTop.pos[furthestLeftIndex] - 1));
    //             }
    //
    //             int furthestRightIndex = roofTopFlats.flatStartIndexes[flatIndex] + roofTopFlats.flatLengths[flatIndex] - 1;
    //             StructureTile rightTile = param.Tilemap[roofStart.X + furthestRightIndex + 1,
    //                 roofTop.pos[furthestRightIndex] - 1];
    //             if (rightTile is { HasTile: true, BlockType: BlockType.SlopeDownRight }) {
    //                 rightTile.BlockType = BlockType.Solid;
    //                 tranitions.Add((roofStart.X + furthestRightIndex + 1, roofTop.pos[furthestRightIndex] - 1));
    //             }
    //         }
    //
    //         // 5th pass: actually add the half blocks
    //         for (int flatIndex = 0; flatIndex < roofTopFlats.flatStartIndexes.Count; flatIndex++)
    //         for (int xIndex = roofTopFlats.flatStartIndexes[flatIndex];
    //              xIndex < roofTopFlats.flatStartIndexes[flatIndex] + roofTopFlats.flatLengths[flatIndex];
    //              xIndex++)
    //             // ensure the tile underneath is solid (and not a transition tile)
    //             if (param.Tilemap[roofStart.X + xIndex, roofTop.pos[xIndex]].BlockType == BlockType.Solid &&
    //                 !tranitions.Contains((roofStart.X + xIndex, roofTop.pos[xIndex])))
    //                 param.Tilemap.PlaceTile(roofStart.X + xIndex, roofTop.pos[xIndex] - 1, p.RoofMain, BlockType.HalfBlock);
    //
    //         // 6th pass: create endcaps
    //         bool anySideEndsWithSlope = Math.Abs(roofBottom.slope[0]) > 0.05 || Math.Abs(roofBottom.slope[^1]) > 0.05;
    //         bool hasTallEndCaps = (anySideEndsWithSlope || Terraria.WorldGen.genRand.NextDouble() < 0.35)
    //                               && !param.Component.TagsRequired.Contains(ComponentTag.RoofHasLargeOverhang);
    //
    //         // left cap
    //         if (!isLeftFlushWithWall) {
    //             if (hasTallEndCaps) {
    //                 param.Tilemap.PlaceTile(roofStart.X - 1, roofStart.Y - 2, p.RoofMain, BlockType.SlopeUpRight);
    //                 param.Tilemap.PlaceTile(roofStart.X - 1, roofStart.Y - 3, p.RoofMain, BlockType.HalfBlock);
    //                 if (param.Tilemap[roofStart.X, roofTop.pos[0]].BlockType is
    //                     BlockType.SlopeDownRight or BlockType.SlopeDownLeft) {
    //                     param.Tilemap.PlaceTile(roofStart.X, roofTop.pos[0], p.RoofMain);
    //                     param.Tilemap.PlaceWall(roofStart.X + 1, roofStart.Y - 1, p.BackgroundRoofMain);
    //                 }
    //             }
    //             else {
    //                 param.Tilemap.PlaceTile(roofStart.X + 1, roofStart.Y - 1, p.RoofMain, BlockType.SlopeUpLeft);
    //                 param.Tilemap.PlaceTile(roofStart.X, roofStart.Y - 1, p.RoofMain);
    //                 param.Tilemap.PlaceTile(roofStart.X - 1, roofStart.Y - 1, p.RoofMain);
    //                 param.Tilemap.PlaceTile(roofStart.X - 2, roofStart.Y - 1, p.RoofMain, BlockType.SlopeUpRight);
    //                 param.Tilemap.PlaceTile(roofStart.X, roofStart.Y - 2, p.RoofMain);
    //                 param.Tilemap.PlaceTile(roofStart.X - 1, roofStart.Y - 2, p.RoofMain, BlockType.HalfBlock);
    //                 param.Tilemap.PlaceTile(roofStart.X - 2, roofStart.Y - 2, p.RoofMain, BlockType.HalfBlock);
    //                 if (param.Tilemap[roofStart.X, roofStart.Y - 3].BlockType ==
    //                     BlockType.HalfBlock)
    //                     param.Tilemap[roofStart.X, roofStart.Y - 3].ClearTile();
    //             }
    //         }
    //
    //         // right cap
    //         if (!isRightFlushWithWall) {
    //             if (hasTallEndCaps) {
    //                 param.Tilemap.PlaceTile(rightPosX + 1, roofBottom.pos[^1] - 2, p.RoofMain, BlockType.SlopeUpLeft);
    //                 param.Tilemap.PlaceTile(rightPosX + 1, roofBottom.pos[^1] - 3, p.RoofMain, BlockType.HalfBlock);
    //                 if (param.Tilemap[rightPosX, roofTop.pos[^1]].BlockType is
    //                     BlockType.SlopeDownRight or BlockType.SlopeDownLeft) {
    //                     param.Tilemap.PlaceTile(rightPosX, roofTop.pos[^1], p.RoofMain);
    //                     param.Tilemap.PlaceWall(rightPosX - 1, roofTop.pos[^1] + 2, p.BackgroundRoofMain);
    //                 }
    //             }
    //             else {
    //                 param.Tilemap.PlaceTile(rightPosX - 1, roofBottom.pos[^1] - 1, p.RoofMain, BlockType.SlopeUpRight);
    //                 param.Tilemap.PlaceTile(rightPosX, roofBottom.pos[^1] - 1, p.RoofMain);
    //                 param.Tilemap.PlaceTile(rightPosX + 1, roofBottom.pos[^1] - 1, p.RoofMain);
    //                 param.Tilemap.PlaceTile(rightPosX + 2, roofBottom.pos[^1] - 1, p.RoofMain, BlockType.SlopeUpLeft);
    //                 param.Tilemap.PlaceTile(rightPosX, roofBottom.pos[^1] - 2, p.RoofMain);
    //                 param.Tilemap.PlaceTile(rightPosX + 1, roofBottom.pos[^1] - 2, p.RoofMain, BlockType.HalfBlock);
    //                 param.Tilemap.PlaceTile(rightPosX + 2, roofBottom.pos[^1] - 2, p.RoofMain, BlockType.HalfBlock);
    //                 if (param.Tilemap[rightPosX, roofBottom.pos[^1] - 3].BlockType == BlockType.HalfBlock)
    //                     param.Tilemap[rightPosX, roofBottom.pos[^1] - 3].ClearTile();
    //             }
    //         }
    //
    //         return true;
    //     }
    // }

    [ComponentGenerator(typeof(Roof))]
    public class RoofGenerator2 : PathComponentGenerator {
        public override HashSet<ComponentTag> GetPossibleTags() {
            return [
                ComponentTag.External,
                ComponentTag.RoofShort,
                ComponentTag.RoofTall,
                ComponentTag.RoofHasLargeOverhang,
                ComponentTag.RoofSlopeNone,
                ComponentTag.RoofSlopeLessThan1,
                ComponentTag.RoofSlope1To1,
                ComponentTag.RoofSlopeGreaterThan1
            ];
        }

        public override bool Generate(PathComponentParams param) {
            bool isPathFlat = param.Component.Line.Points.All(p => p.Y == param.Component.Line.Points[0].Y);

            // extend roof endcaps if necessary
            (Point16 left, Point16 right) endpoints = param.Component.Line.SortEndpoints();
            bool bigEndCaps = param.Component.TagsRequired.ContainsKey(ComponentTag.RoofHasLargeOverhang);
            if (param.Component.Line.StartExtendable) {
                
            }

            Path upperMiddlePath = param.Component.Line.Clone();
            upperMiddlePath.OffsetEven(new Point16(0, -1));
            Path topPath = param.Component.Line.Clone();
            topPath.OffsetEven(new Point16(0, isPathFlat ? -2 : -3));
            topPath.Reverse();

            Shape offsetShape = upperMiddlePath.ToShape(topPath);
            offsetShape.ExecuteInArea((x, y, blockType) => { param.Tilemap.PlaceTile(x, y, param.Palette.RoofMain, blockType); }, SlopeHelper.SmoothTop);

            // add large roof parts if necessary
            if (param.Component.TagsRequired.ContainsKey(ComponentTag.RoofTall)) {
                bool tallLeftSide = Terraria.WorldGen.genRand.NextBool(3, 4);
                bool tallRightSide = !tallLeftSide || Terraria.WorldGen.genRand.NextBool(3, 4);
                if (tallLeftSide) {
                    Shape topLeftShape = topPath.FillFromCorner(new Point16(0, 0));
                    topLeftShape.ExecuteInArea((x, y) => { param.Tilemap.PlaceWall(x, y, param.Palette.BackgroundRoofMain); });
                }

                if (tallRightSide) {
                    Shape topLeftShape = topPath.FillFromCorner(new Point16(1, 0));
                    topLeftShape.ExecuteInArea((x, y) => { param.Tilemap.PlaceWall(x, y, param.Palette.BackgroundRoofMain); });
                }
            }

            // create bottom wall section
            Shape wallsShape = upperMiddlePath.ToShape(param.Component.Line.Points.Length);
            wallsShape.ExecuteInArea((x, y) => {
                if (x != wallsShape.BoundingBox.topLeft.X && x != wallsShape.BoundingBox.bottomRight.X) param.Tilemap.PlaceWall(x, y, param.Palette.BackgroundRoofMain);
            });
            
            return true;
        }
    }
}