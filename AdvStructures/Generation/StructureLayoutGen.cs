#nullable enable
using System.Collections.Generic;
using System.Linq;
using SpawnHouses.AdvStructures.AdvStructureParts;
using SpawnHouses.Helpers;
using SpawnHouses.Structures;
using SpawnHouses.Types;
using Terraria.DataStructures;
using Range = SpawnHouses.Structures.Range;

namespace SpawnHouses.AdvStructures.Generation;

public static class StructureLayoutGen {
    /// <summary>
    ///     literally just a square. can only have 2 entry points
    /// </summary>
    public class StructureLayoutGenerator1 : IStructureLayoutGenerator {
        public StructureTag[] GetPossibleTags() {
            return [
                StructureTag.HasHousing
            ];
        }

        public bool CanGenerate(StructureParams structureParams) {
            if (structureParams.EntryPoints.Length != 2) return false;

            EntryPoint lower, upper;
            if (structureParams.EntryPoints[0].Center.X < structureParams.EntryPoints[1].Center.X) {
                lower = structureParams.EntryPoints[0];
                upper = structureParams.EntryPoints[1];
            }
            else {
                lower = structureParams.EntryPoints[1];
                upper = structureParams.EntryPoints[0];
            }

            return lower.Direction == Directions.Right && upper.Direction == Directions.Left;
        }

        public bool Generate(AdvStructure advStructure) {
            RoomLayoutParams roomLayoutParams = new(
                null,
                advStructure.Params.EntryPoints,
                advStructure.Params.Palette,
                advStructure.Params.Housing,
                new Range(4, 18),
                new Range(7, advStructure.Params.Length),
                new Range(1, 2),
                new Range(1, 2),
                0.3f
            );

            // setup external layout
            int externalWallThickness = roomLayoutParams.WallWidth.Max;
            int externalFloorThickness = roomLayoutParams.FloorWidth.Max;

            EntryPoint upper, lower, left, right;
            if (advStructure.Params.EntryPoints[0].Center.Y < advStructure.Params.EntryPoints[1].Center.Y) {
                lower = advStructure.Params.EntryPoints[0];
                upper = advStructure.Params.EntryPoints[1];
            }
            else {
                lower = advStructure.Params.EntryPoints[1];
                upper = advStructure.Params.EntryPoints[0];
            }

            if (advStructure.Params.EntryPoints[0].Center.X < advStructure.Params.EntryPoints[1].Center.X) {
                left = advStructure.Params.EntryPoints[0];
                right = advStructure.Params.EntryPoints[1];
            }
            else {
                left = advStructure.Params.EntryPoints[1];
                right = advStructure.Params.EntryPoints[0];
            }

            int entryPointDistance = upper.Start.Y - lower.Start.Y;
            bool hasBasement = Terraria.WorldGen.genRand.NextDouble() < 0.4 && advStructure.Params.Height - entryPointDistance > 10; //40% if conditions are met
            int verticalOffset = hasBasement ? 9 : 0;
            int topFloorY = upper.End.Y + 1 + verticalOffset;
            int bottomRoofY = topFloorY - advStructure.Params.Height + 1;
            const int tilemapMargin = 5;
            const int roofMargin = 12;

            // create the tilemap around it
            advStructure.Tilemap = new StructureTilemap(
                (ushort)(advStructure.Params.Length + 2 * (tilemapMargin + externalWallThickness)),
                (ushort)(advStructure.Params.Height + 2 * (tilemapMargin + externalFloorThickness) + roofMargin),
                new Point16(advStructure.Params.StartEntryPointX - externalWallThickness - tilemapMargin, bottomRoofY + 1 - externalFloorThickness - roofMargin - tilemapMargin)
            );
            Point16 localTilemapOffset = advStructure.Tilemap.WorldTileOffset;

            // actually fill the external layout
            List<Wall> exteriorWalls = [];

            // if the bottom of either entry point is NOT flush with the floor
            if (left.End.Y + 1 != topFloorY) exteriorWalls.Add(ExternalLayoutHelper.CreateWall(left.Start.X, left.End.Y + 1, topFloorY - 1 + externalFloorThickness, false, externalWallThickness, true));
            if (right.End.Y + 1 != topFloorY) exteriorWalls.Add(ExternalLayoutHelper.CreateWall(right.Start.X, right.End.Y + 1, topFloorY - 1 + externalFloorThickness, true, externalWallThickness, true));

            // if the top of either entry point is NOT flush with the roof
            if (left.Start.Y - 1 != bottomRoofY) exteriorWalls.Add(ExternalLayoutHelper.CreateWall(left.Start.X, left.Start.Y - 1, bottomRoofY + 1 - externalFloorThickness, false, externalWallThickness, true));
            if (right.Start.Y - 1 != bottomRoofY) exteriorWalls.Add(ExternalLayoutHelper.CreateWall(right.Start.X, right.Start.Y - 1, bottomRoofY + 1 - externalFloorThickness, true, externalWallThickness, true));

            advStructure.ExternalLayout = new ExternalLayout(
                [
                    ExternalLayoutHelper.CreateFloor(topFloorY, advStructure.Params.StartEntryPointX + 1, advStructure.Params.EndEntryPointX - 1, true, externalFloorThickness, true),
                    ExternalLayoutHelper.CreateFloor(bottomRoofY, advStructure.Params.StartEntryPointX + 1, advStructure.Params.EndEntryPointX - 1, false, externalFloorThickness, true)
                ],
                exteriorWalls,
                RoomLayoutHelper.GapsFromEntryPoints(advStructure.Params.EntryPoints, externalFloorThickness, externalWallThickness).ToList()
            );

            advStructure.Tilemap.OffsetExternalLayout(advStructure.ExternalLayout);
            advStructure.EvaluateTilemapData();
            roomLayoutParams.MainVolume = new Shape(
                new Point16(left.Start.X + 1, bottomRoofY + 1) - localTilemapOffset,
                new Point16(right.Start.X - 1, topFloorY - 1) - localTilemapOffset
            );

            // finally finish the room layout
            advStructure.Layout = RoomLayoutHelper.CreateRoomLayout(roomLayoutParams);

            // assign rooms to the external gaps
            advStructure.CompleteExternalGaps();

            return true;
        }
    }

    /// <summary>
    ///     a house with a tall side, and possible extrusions on tall side. generally quite large and medieval looking
    /// </summary>
    // public static bool StructureLayout2(StructureParams structureParams, AdvStructure advStructure) {
    //     List<Shape> roomVolumes = [];
    //     List<Shape> wallVolumes = [];
    //     List<Shape> floorVolumes = [];
    //
    //     bool leftTall = Terraria.WorldGen.genRand.NextBool();
    //     int leftHeight = !leftTall
    //         ? (int)Math.Round(structureParams.Height * 1.33) - 4
    //         : (int)Math.Round(structureParams.Height * 0.66) - 4;
    //     int rightHeight = leftTall
    //         ? (int)Math.Round(structureParams.Height * 1.33) - 4
    //         : (int)Math.Round(structureParams.Height * 0.66) - 4;
    //     int flangeHeight = (int)(0.85 + Terraria.WorldGen.genRand.NextDouble() * 0.3) * structureParams.Height;
    //     bool leftFlange = Terraria.WorldGen.genRand.NextBool();
    //     int leftFlangeWidth = leftFlange ? 0 : Terraria.WorldGen.genRand.Next(4, 8);
    //     bool rightFlange = Terraria.WorldGen.genRand.NextBool();
    //     int rightFlangeWidth = rightFlange ? 0 : Terraria.WorldGen.genRand.Next(4, 8);
    //     int rightSideStartXPos = structureParams.Start.X + (structureParams.End.X - structureParams.Start.X) / 2;
    //     if (leftTall)
    //         rightSideStartXPos += leftFlangeWidth + rightFlangeWidth;
    //     else
    //         rightSideStartXPos -= leftFlangeWidth + rightFlangeWidth;
    //
    //     int firstFloorHeight = Terraria.WorldGen.genRand.Next(6, 9);
    //     int nonFirstFloorHeight = firstFloorHeight >= 8 ? firstFloorHeight - 2 : firstFloorHeight - 3;
    //     List<int> leftSideFloorYPositions = [structureParams.Start.Y];
    //     List<int> rightSideFloorYPositions = [structureParams.End.Y];
    //
    //     while (leftSideFloorYPositions[^1] < leftSideFloorYPositions[0] - leftHeight)
    //         leftSideFloorYPositions.Add(leftSideFloorYPositions[^1] - nonFirstFloorHeight - 3);
    //     leftHeight = structureParams.Start.Y - (leftSideFloorYPositions[^1] - nonFirstFloorHeight - 3);
    //
    //     while (rightSideFloorYPositions[^1] < rightSideFloorYPositions[0] + rightHeight)
    //         rightSideFloorYPositions.Add(rightSideFloorYPositions[^1] + nonFirstFloorHeight + 3);
    //     rightHeight = structureParams.End.Y - (rightSideFloorYPositions[^1] - nonFirstFloorHeight - 3);
    //
    //     // add outer wall volumes
    //     if (leftTall && leftFlange) {
    //         wallVolumes.Add(new Shape(
    //             new Point16(structureParams.Start.X - 2, structureParams.Start.Y - 4),
    //             new Point16(structureParams.Start.X, structureParams.Start.Y - flangeHeight)
    //         ));
    //         wallVolumes.Add(new Shape(
    //             new Point16(structureParams.Start.X + leftFlangeWidth + 1, structureParams.Start.Y - flangeHeight + 1),
    //             new Point16(structureParams.Start.X + leftFlangeWidth + 3, structureParams.Start.Y - leftHeight)
    //         ));
    //     }
    //     else {
    //         wallVolumes.Add(new Shape(
    //             new Point16(structureParams.Start.X - 2, structureParams.Start.Y - 4),
    //             new Point16(structureParams.Start.X, structureParams.Start.Y - leftHeight)
    //         ));
    //     }
    //
    //     if (leftTall && rightFlange) {
    //         wallVolumes.Add(new Shape(
    //             new Point16(rightSideStartXPos - 3 - rightFlangeWidth - 2, structureParams.Start.Y - flangeHeight + 1),
    //             new Point16(rightSideStartXPos - 3 - rightFlangeWidth, structureParams.Start.Y - leftHeight)
    //         ));
    //         wallVolumes.Add(new Shape(
    //             new Point16(rightSideStartXPos - 3, structureParams.Start.Y - 4),
    //             new Point16(rightSideStartXPos - 1, structureParams.Start.Y - flangeHeight)
    //         ));
    //     }
    //     else if (!leftTall && leftFlange) {
    //         wallVolumes.Add(new Shape(
    //             new Point16(rightSideStartXPos, structureParams.End.Y - 4),
    //             new Point16(rightSideStartXPos + 2, structureParams.End.Y - flangeHeight)
    //         ));
    //         wallVolumes.Add(new Shape(
    //             new Point16(rightSideStartXPos + leftFlangeWidth + 1, structureParams.End.Y - flangeHeight - 1),
    //             new Point16(rightSideStartXPos + leftFlangeWidth + 3, structureParams.End.Y - rightHeight)
    //         ));
    //     }
    //     else {
    //         wallVolumes.Add(new Shape(
    //             new Point16(rightSideStartXPos - 3, structureParams.Start.Y - 4),
    //             new Point16(rightSideStartXPos - 1, structureParams.Start.Y - leftHeight)
    //         ));
    //     }
    //
    //     if (!leftTall && rightFlange) {
    //         wallVolumes.Add(new Shape(
    //             new Point16(structureParams.End.X - rightFlangeWidth - 2, structureParams.End.Y - flangeHeight + 1),
    //             new Point16(structureParams.End.X - rightFlangeWidth, structureParams.End.Y - rightHeight)
    //         ));
    //         wallVolumes.Add(new Shape(
    //             new Point16(structureParams.End.X, structureParams.End.Y - 4),
    //             new Point16(structureParams.End.X + 2, structureParams.End.Y - flangeHeight)
    //         ));
    //     }
    //     else {
    //         wallVolumes.Add(new Shape(
    //             new Point16(structureParams.End.X, structureParams.End.Y - 4),
    //             new Point16(structureParams.End.X + 2, structureParams.End.Y - rightHeight)
    //         ));
    //     }
    //
    //     Console.WriteLine(
    //         $"leftTall: {leftTall}, leftFlange: {leftFlange}, rightFlange: {rightFlange}, leftHeight: {leftHeight}, rightHeight: {rightHeight}, rightSideStartXPos: {rightSideStartXPos}, approxHeight: {structureParams.Height}");
    //     Shape.CreateOutline(wallVolumes.ToArray());
    //
    //     return false;
    // }
}