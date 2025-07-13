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
        public StructureTag[] GetPossibleTags() => [
            StructureTag.HasHousing,
            StructureTag.HasFlatFloors,
            StructureTag.HasLargeRoom,
            StructureTag.HasStorage,
            StructureTag.MainFloorConnected,
            StructureTag.AboveGround,
            StructureTag.UnderGround
        ];

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
            StructureParams p = advStructure.Params;
            RoomLayoutParams roomLayoutParams = new(
                null,
                p.EntryPoints,
                p.Palette,
                p.Housing,
                new Range(4, 18),
                new Range(7, p.Length),
                new Range(1, 1),
                new Range(1, 1),
                0.3f
            );

            // setup external layout
            int externalWallThickness = roomLayoutParams.WallWidth.Max;
            int externalFloorThickness = roomLayoutParams.FloorWidth.Max;

            EntryPoint upper, lower, left, right;
            if (p.EntryPoints[0].Center.Y < p.EntryPoints[1].Center.Y) {
                lower = p.EntryPoints[0];
                upper = p.EntryPoints[1];
            }
            else {
                lower = p.EntryPoints[1];
                upper = p.EntryPoints[0];
            }

            if (p.EntryPoints[0].Center.X < p.EntryPoints[1].Center.X) {
                left = p.EntryPoints[0];
                right = p.EntryPoints[1];
            }
            else {
                left = p.EntryPoints[1];
                right = p.EntryPoints[0];
            }

            const int tilemapMargin = 5;
            const int roofMargin = 12;

            int entryPointDistance = upper.Start.Y - lower.Start.Y;
            bool hasBasement = Terraria.WorldGen.genRand.NextDouble() < 0.4 && p.Height - entryPointDistance > 10; //40% if conditions are met
            int verticalOffset = hasBasement ? 7 : 0;
            int floorTopY = upper.End.Y + 1 + verticalOffset;

            // roof setup
            bool unevenRoof = true;//Terraria.WorldGen.genRand.NextDouble() < 0.6;
            bool leftRoofHigher = Terraria.WorldGen.genRand.NextDouble() < 0.5;
            int unevenRoofStartX = Terraria.WorldGen.genRand.Next(p.StartEntryPointX + (int)(p.Length * 0.4), p.StartEntryPointX + (int)(p.Length * 0.6));
            int roofHeightModifier = p.Height / 5 + 1;
            int upperRoofBottomY = floorTopY - p.Height + 1;
            if (unevenRoof && upperRoofBottomY + roofHeightModifier >= (leftRoofHigher ? right.Start.Y : left.Start.Y)) { // check that an uneven roof won't cause collision with entry points
                unevenRoof = false;
            }
            int lowerRoofBottomY = upperRoofBottomY + (unevenRoof ? roofHeightModifier : 0);
            if (unevenRoof) {
                upperRoofBottomY -= roofHeightModifier;
            }

            // create the tilemap around it
            advStructure.Tilemap = new StructureTilemap(
                (ushort)(p.Length + 2 * (tilemapMargin + externalWallThickness)),
                (ushort)(p.Height + 2 * (tilemapMargin + externalFloorThickness) + roofMargin),
                new Point16(p.StartEntryPointX - externalWallThickness - tilemapMargin, upperRoofBottomY + 1 - externalFloorThickness - roofMargin - tilemapMargin)
            );
            Point16 localTilemapOffset = advStructure.Tilemap.WorldTileOffset;

            // actually fill the external layout
            List<Floor> exteriorFloors = [];
            List<Wall> exteriorWalls = [];
            List<Roof> roofs = [];
            exteriorFloors.Add(ExternalLayoutHelper.CreateFloor(floorTopY, p.StartEntryPointX + 1 - (hasBasement ? 0 : externalWallThickness),
                p.EndEntryPointX - 1 + (hasBasement ? 0 : externalWallThickness), true, externalFloorThickness));

            if (unevenRoof) {
                (List<Floor> floors, List<Wall> walls, List<Roof> roofs) topComponents = ExternalLayoutHelper.CreateTopFloorsAndWalls(
                    [
                        new Point16(p.StartEntryPointX + 1 - externalWallThickness, leftRoofHigher ? upperRoofBottomY : lowerRoofBottomY),
                        new Point16(unevenRoofStartX, leftRoofHigher ? upperRoofBottomY : lowerRoofBottomY),
                        new Point16(unevenRoofStartX, !leftRoofHigher ? upperRoofBottomY : lowerRoofBottomY),
                        new Point16(p.EndEntryPointX - 1 + externalWallThickness, !leftRoofHigher ? upperRoofBottomY : lowerRoofBottomY)
                    ],
                    externalFloorThickness,
                    true,
                    externalWallThickness
                );
                exteriorFloors.AddRange(topComponents.floors);
                exteriorWalls.AddRange(topComponents.walls);
                roofs.AddRange(topComponents.roofs);
            }
            else {
                exteriorFloors.Add(ExternalLayoutHelper.CreateFloor(upperRoofBottomY, p.StartEntryPointX + 1 - externalWallThickness, p.EndEntryPointX - 1 + externalWallThickness, false, externalFloorThickness));

                roofs.Add(
                    new Roof(new Shape(
                        new Point16(p.StartEntryPointX + 1 - externalWallThickness, upperRoofBottomY - externalFloorThickness - 3),
                        new Point16(p.EndEntryPointX - 1 + externalWallThickness, upperRoofBottomY - externalFloorThickness)
                    ))
                );
            }

            // if the top of either entry point is NOT flush with the roof
            if (left.Start.Y - 1 != (leftRoofHigher ? upperRoofBottomY : lowerRoofBottomY)) exteriorWalls.Add(ExternalLayoutHelper.CreateWall(left.Start.X, left.Start.Y - 1,
                (leftRoofHigher ? upperRoofBottomY : lowerRoofBottomY) + 1, false, externalWallThickness));
            if (right.Start.Y - 1 != (!leftRoofHigher ? upperRoofBottomY : lowerRoofBottomY)) exteriorWalls.Add(ExternalLayoutHelper.CreateWall(right.Start.X, right.Start.Y - 1,
                (!leftRoofHigher ? upperRoofBottomY : lowerRoofBottomY) + 1, true, externalWallThickness));

            // if the bottom of either entry point is NOT flush with the floor
            if (left.End.Y + 1 != floorTopY) exteriorWalls.Add(ExternalLayoutHelper.CreateWall(left.Start.X, left.End.Y + 1, floorTopY - 1 + externalFloorThickness, false, externalWallThickness));
            if (right.End.Y + 1 != floorTopY) exteriorWalls.Add(ExternalLayoutHelper.CreateWall(right.Start.X, right.End.Y + 1, floorTopY - 1 + externalFloorThickness, true, externalWallThickness));

            advStructure.ExternalLayout = new ExternalLayout(
                exteriorFloors,
                exteriorWalls,
                RoomLayoutHelper.GapsFromEntryPoints(p.EntryPoints, externalFloorThickness, externalWallThickness).ToList(),
                roofs
            );

            advStructure.Tilemap.OffsetExternalLayout(advStructure.ExternalLayout);
            advStructure.EvaluateTilemapData();

            // finally finish the room layout
            advStructure.Layout = new RoomLayout([], [], [], [
                new Room(
                    new Shape(
                        new Point16(left.Start.X + 1, upperRoofBottomY + 1) - localTilemapOffset,
                        new Point16(right.Start.X - 1, floorTopY - 1) - localTilemapOffset
                    ),
                    advStructure.ExternalLayout.Gaps
                )
            ]);

            advStructure.CompleteExternalGaps();
            RoomLayoutHelper.SubdivideRoom(advStructure.Layout, advStructure.Layout.Rooms[0], roomLayoutParams, prioritySplitYs: [left.End.Y + 1, right.Start.Y + 1]);

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