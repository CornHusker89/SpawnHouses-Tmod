#nullable enable
using System;
using System.Collections.Generic;
using SpawnHouses.Common.Modules;
using SpawnHouses.Common.Modules.Components;
using SpawnHouses.Common.Parameters;
using SpawnHouses.Common.Tagging;
using SpawnHouses.Common.Tiles;
using SpawnHouses.Common.Types;
using SpawnHouses.Helpers;
using SpawnHouses.Helpers.Complex;
using SpawnHouses.Structures;
using Terraria;
using Terraria.DataStructures;
using Range = SpawnHouses.Structures.Range;

namespace SpawnHouses.Common.Generation;

public static class StructureLayoutGen {
    /// <summary>
    ///     a square, possibly with square vertical extrusions. can only have 2 entry points
    /// </summary>
    [InstanceGenerator(typeof(StructureLayout))]
    public class StructureLayoutGenerator1 : StructureLayoutGenerator {
        public override HashSet<Tag> PossibleTags { get; } = TagMap.NewTagSet(
            [
                Tags.HasRooms,
                Tags.HasHousing,
                Tags.HasStorage,
                Tags.HasRoof,
                Tags.HasOnlyRectangleRooms
            ],
            StructureLayoutHelper.SubdivideRoom.PossibleTags,
            StructureLayoutHelper.CreateStairways.PossibleTags
        );

        public override bool CanGenerate(StructureLayoutParams structureParams) {
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

        public override StructureLayout Generate(StructureLayoutParams p) {
            const int tilemapMargin = 7;

            // TODO: compensate structure volume and roofMargin for the non-square volume at the top

            // structure parameters that aren't dependent on tilemap position
            bool forceFlatRoof = p.TagsRequired.HasTag(Tags.HasOnlyRectangleRooms);
            int entryPointVerticalDistance = Math.Abs(p.EntryPoints[0].End.Y - p.EntryPoints[1].End.Y);
            bool hasBasement = p.Structure.RandomGen.NextBool(3, 10) && p.Height - entryPointVerticalDistance > 12; //40% if conditions are met
            Range externalFloorThicknessRange = new(1, 1);
            Range externalWallThicknessRange = new(1, 1);
            int externalFloorThickness = externalFloorThicknessRange.Max;
            int externalWallThickness = externalWallThicknessRange.Max;
            
            int verticalOffset = hasBasement ? 7 : 0;
            bool hasHigherSide = Terraria.WorldGen.genRand.NextBool(4, 5) && !forceFlatRoof;
            bool leftRoofHigher = Terraria.WorldGen.genRand.NextBool();
            EntryPoint upper = p.EntryPoints[0].Center.Y < p.EntryPoints[1].Center.Y ? p.EntryPoints[1] : p.EntryPoints[0];
            EntryPoint left, right;
            if (p.EntryPoints[0].Center.X < p.EntryPoints[1].Center.X) {
                left = p.EntryPoints[0];
                right = p.EntryPoints[1];
            }
            else {
                left = p.EntryPoints[1];
                right = p.EntryPoints[0];
            }

            int floorTopY = upper.End.Y + 1 + verticalOffset;
            int roofHeightModifier = (int)((p.Height / 6.3 + 2) * Terraria.WorldGen.genRand.NextFloat(1, 1.35f)); // if uneven roof, adjust each side by this much
            int upperRoofBottomY = floorTopY - p.Height + 1;
            if (hasHigherSide && upperRoofBottomY + roofHeightModifier >= (leftRoofHigher ? right.Start.Y : left.Start.Y)) // check that an uneven roof won't cause collision with entry points
                hasHigherSide = false;
            int lowerRoofBottomY = upperRoofBottomY + (hasHigherSide ? roofHeightModifier : 0);
            if (hasHigherSide) upperRoofBottomY -= roofHeightModifier;

            // create external components
            var (exteriorFloors, exteriorWalls, roofs) = ExternalLayoutHelper.CreateBasicRoof(
                p.Structure,
                new Point16(p.LeftEntryPointX + 1 - externalWallThickness, leftRoofHigher ? upperRoofBottomY : lowerRoofBottomY),
                new Point16(p.RightEntryPointX - 1 + externalWallThickness, !leftRoofHigher ? upperRoofBottomY : lowerRoofBottomY),
                externalFloorThickness,
                externalWallThickness,
                forceFlatRoof,
                false //hasHigherSide && Terraria.WorldGen.genRand.NextBool(2, 3)
            );

            exteriorFloors.Add(ExternalLayoutHelper.CreateFloor(p.Structure, floorTopY, p.LeftEntryPointX + 1 - (hasBasement ? 0 : externalWallThickness),
                p.RightEntryPointX - 1 + (hasBasement ? 0 : externalWallThickness), true, externalFloorThickness));

            // if the top of either entry point is NOT flush with the roof
            if (left.Start.Y - 1 != (leftRoofHigher ? upperRoofBottomY : lowerRoofBottomY))
                exteriorWalls.Add(ExternalLayoutHelper.CreateWall(p.Structure, left.Start.X, left.Start.Y - 1,
                    (leftRoofHigher ? upperRoofBottomY : lowerRoofBottomY) + 1, false, externalWallThickness));
            if (right.Start.Y - 1 != (!leftRoofHigher ? upperRoofBottomY : lowerRoofBottomY))
                exteriorWalls.Add(ExternalLayoutHelper.CreateWall(p.Structure, right.Start.X, right.Start.Y - 1,
                    (!leftRoofHigher ? upperRoofBottomY : lowerRoofBottomY) + 1, true, externalWallThickness));

            // if the bottom of either entry point is NOT flush with the floor
            if (left.End.Y + 1 != floorTopY)
                exteriorWalls.Add(ExternalLayoutHelper.CreateWall(p.Structure, left.Start.X,
                    left.End.Y + 1, floorTopY - 1 + externalFloorThickness, false, externalWallThickness));
            if (right.End.Y + 1 != floorTopY)
                exteriorWalls.Add(ExternalLayoutHelper.CreateWall(p.Structure, right.Start.X,
                    right.End.Y + 1, floorTopY - 1 + externalFloorThickness, true, externalWallThickness));

            // create the tilemap
            int roofTopY = ExternalLayoutHelper.GetHighestRoofPoint(roofs);
            p.Structure.Tilemap = new StructureTilemap(
                p.Structure,
                (ushort)(p.Length + 2 * (tilemapMargin + externalWallThickness)),
                (ushort)(int.Max(p.EntryPoints[0].Start.Y, p.EntryPoints[1].Start.Y) + 5 + verticalOffset - roofTopY),
                new Point16(
                    p.LeftEntryPointX - externalWallThickness - tilemapMargin,
                    roofTopY - tilemapMargin
                )
            );
            left.SetOffset(p.Structure.Tilemap.WorldTileOffset * Point16.NegativeOne);
            right.SetOffset(p.Structure.Tilemap.WorldTileOffset * Point16.NegativeOne);


            // finish the room e
            Room internalRoom = StructureLayoutHelper.InitializeStructureInterior.Action(p, externalFloorThickness, externalWallThickness);

            RoomLayoutParams roomLayoutParams = new(
                p.Structure,
                new Range(1, 1),
                new Range(1, 1),
                new Range(4, 13),
                new Range(7, p.Length),
                p.TagsRequired,
                0.3f
            );

            RoomLayout roomLayout = StructureLayoutHelper.SubdivideRoom.Action(internalRoom, roomLayoutParams);

            StructureLayout structureLayout = new(p);
            structureLayout.SetComponents(exteriorFloors, exteriorWalls, internalRoom.Gaps, roofs, [roomLayout]);
            structureLayout.SetTilesExternalStatus();

            foreach (Room room in p.Structure.StructureLayout.Rooms) {
                StructureLayoutHelper.CreateStairways.Action(p, room);
                room.Params.TagsRequired.Add(Tags.RoomTypeLiving);
            }

            return structureLayout;
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