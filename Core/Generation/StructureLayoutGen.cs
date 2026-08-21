#nullable enable
using System;
using System.Collections.Generic;
using SpawnHouses.Core.AdvGeneratables;
using SpawnHouses.Core.AdvGeneratables.Components;
using SpawnHouses.Core.Attributes;
using SpawnHouses.Core.DataStructures;
using SpawnHouses.Core.Enums;
using SpawnHouses.Core.Palette;
using SpawnHouses.Core.Parameters;
using SpawnHouses.Core.Tagging;
using SpawnHouses.Core.Tiles;
using SpawnHouses.Helpers;
using SpawnHouses.Helpers.Complex;
using Terraria;
using Terraria.DataStructures;
using Terraria.Utilities;

namespace SpawnHouses.Core.Generation;

public static class StructureLayoutGen {
    /// <summary>
    ///     a square, possibly with square vertical extrusions. can only have 2 entry points
    /// </summary>
    [AdvGeneratorLoadable(typeof(StructureLayout), false)]
    public class StructureLayoutAdvGenerator1 : StructureLayoutAdvGenerator {
        public override HashSet<Tag> PossibleTags { get; } = TagMap.NewTagSet(
            [
                Tags.Structure_HasRooms,
                Tags.Structure_HasHousing,
                Tags.Structure_HasStorage,
                Tags.Structure_HasRoof,
                Tags.Structure_HasOnlyRectangleRooms
            ],
            StructureLayoutHelper.SubdivideRoom.PossibleTags,
            StructureLayoutHelper.CreateStairways.PossibleTags,
            StructureLayoutHelper.CreateRoof.PossibleTags
        );

        public override bool CanGenerate(StructureLayout structureLayout, StructureLayoutParams param, UnifiedRandom random) {
            if (param.EntryPoints.Length != 2) return false;

            EntryPoint lower, upper;
            if (param.EntryPoints[0].Center.X < param.EntryPoints[1].Center.X) {
                lower = param.EntryPoints[0];
                upper = param.EntryPoints[1];
            }
            else {
                lower = param.EntryPoints[1];
                upper = param.EntryPoints[0];
            }

            return lower.EntryDirection == Direction.Right && upper.EntryDirection == Direction.Left;
        }

        public override bool Generate(StructureLayout structureLayout, StructureLayoutParams param, UnifiedRandom random, TilePalette palette, StructureTilemap tilemap) {
            const int tilemapMargin = 7;

            // TODO: compensate structure volume and roofMargin for the non-square volume at the top

            // structure parameters that aren't dependent on tilemap position
            bool forceFlatRoof = param.TagsRequired.HasTag(Tags.Structure_HasOnlyRectangleRooms);
            int entryPointVerticalDistance = Math.Abs(param.EntryPoints[0].End.Y - param.EntryPoints[1].End.Y);
            bool hasBasement = param.Structure.OtherRandom.NextBool(3, 10) && param.Height - entryPointVerticalDistance > 12; //40% if conditions are met
            NumRange externalFloorThicknessRange = new(1, 2);
            NumRange externalWallThicknessRange = new(1, 2);
            int externalFloorThickness = externalFloorThicknessRange.Max;
            int externalWallThickness = externalWallThicknessRange.Max;

            int basementVerticalOffset = hasBasement ? 7 : 0;
            bool hasHigherSide = random.NextBool(4, 5) && !forceFlatRoof;
            bool leftRoofHigher = random.NextBool();
            EntryPoint upper = param.EntryPoints[0].Center.Y < param.EntryPoints[1].Center.Y ? param.EntryPoints[1] : param.EntryPoints[0];
            EntryPoint left, right;
            if (param.EntryPoints[0].Center.X < param.EntryPoints[1].Center.X) {
                left = param.EntryPoints[0];
                right = param.EntryPoints[1];
            }
            else {
                left = param.EntryPoints[1];
                right = param.EntryPoints[0];
            }

            // create constants
            int floorTopY = upper.End.Y + 1 + basementVerticalOffset;
            int roofHeightModifier = (int)((param.Height / 9.0 + param.Length / 9.0 - 3) * random.NextFloat(1, 1.35f)); // if uneven roof, adjust each side by this much
            int upperRoofBottomY = floorTopY - param.Height + 1;
            if (hasHigherSide && upperRoofBottomY + roofHeightModifier >= (leftRoofHigher ? right.Start.Y : left.Start.Y)) // check that an uneven roof won't cause collision with entry points
                hasHigherSide = false;
            int lowerRoofBottomY = upperRoofBottomY + (hasHigherSide ? roofHeightModifier : 0);
            if (hasHigherSide) upperRoofBottomY -= roofHeightModifier;

            // create roof, to size the tilemap
            var (exteriorFloors, exteriorWalls, roofs) = StructureLayoutHelper.CreateRoof.Action(
                param,
                new Point16(param.LeftEntryPointX + 1 - externalWallThickness, leftRoofHigher ? upperRoofBottomY : lowerRoofBottomY),
                new Point16(param.RightEntryPointX - 1 + externalWallThickness, !leftRoofHigher ? upperRoofBottomY : lowerRoofBottomY),
                externalFloorThickness,
                externalWallThickness
            );

            // create the tilemap
            int roofTopY = ExternalLayoutHelper.GetHighestRoofPoint(roofs);
            param.Structure.Tilemap = new StructureTilemap(
                param.Structure,
                (ushort)(param.Length + 2 * (tilemapMargin + externalWallThickness)),
                (ushort)(int.Max(param.EntryPoints[0].Start.Y, param.EntryPoints[1].Start.Y) + 5 + basementVerticalOffset - roofTopY + 2 * tilemapMargin),
                new Point16(
                    param.LeftEntryPointX - externalWallThickness - tilemapMargin,
                    roofTopY - tilemapMargin
                )
            );

            // move entry points and roof components to be relative to the tilemap
            left.SetOffset(param.Structure.Tilemap.GlobalTileOffset * Point16.NegativeOne);
            right.SetOffset(param.Structure.Tilemap.GlobalTileOffset * Point16.NegativeOne);
            floorTopY -= param.Structure.Tilemap.GlobalTileOffset.Y;
            upperRoofBottomY -= param.Structure.Tilemap.GlobalTileOffset.Y;
            lowerRoofBottomY -= param.Structure.Tilemap.GlobalTileOffset.Y;
            foreach (Floor floor in exteriorFloors)
                floor.Geometry.Move(param.Structure.Tilemap.GlobalTileOffset * Point16.NegativeOne);
            foreach (Wall wall in exteriorWalls)
                wall.Geometry.Move(param.Structure.Tilemap.GlobalTileOffset * Point16.NegativeOne);
            foreach (Roof roof in roofs)
                roof.Geometry.Move(param.Structure.Tilemap.GlobalTileOffset * Point16.NegativeOne);

            // create exterior components
            exteriorFloors.Add(ExternalLayoutHelper.CreateFloor(param.Structure, floorTopY, param.LeftEntryPointX + 1 - (hasBasement ? 0 : externalWallThickness),
                param.RightEntryPointX - 1 + (hasBasement ? 0 : externalWallThickness), true, externalFloorThickness, "F_Main"));

            // if the top of either entry point is NOT flush with the roof
            if (left.Start.Y - 1 != (leftRoofHigher ? upperRoofBottomY : lowerRoofBottomY))
                exteriorWalls.Add(ExternalLayoutHelper.CreateWall(param.Structure, left.Start.X, left.Start.Y - 1,
                    (leftRoofHigher ? upperRoofBottomY : lowerRoofBottomY) + 1, false, externalWallThickness, "W_TopSeal_LeftEntryPoint"));
            if (right.Start.Y - 1 != (!leftRoofHigher ? upperRoofBottomY : lowerRoofBottomY))
                exteriorWalls.Add(ExternalLayoutHelper.CreateWall(param.Structure, right.Start.X, right.Start.Y - 1,
                    (!leftRoofHigher ? upperRoofBottomY : lowerRoofBottomY) + 1, true, externalWallThickness, "W_TopSeal_RightEntryPoint"));

            // if the bottom of either entry point is NOT flush with the floor
            if (left.End.Y + 1 != floorTopY)
                exteriorWalls.Add(ExternalLayoutHelper.CreateWall(param.Structure, left.Start.X,
                    left.End.Y + 1, floorTopY - 1 + externalFloorThickness, false, externalWallThickness, "W_BottomSeal_LeftEntryPoint"));
            if (right.End.Y + 1 != floorTopY)
                exteriorWalls.Add(ExternalLayoutHelper.CreateWall(param.Structure, right.Start.X,
                    right.End.Y + 1, floorTopY - 1 + externalFloorThickness, true, externalWallThickness, "W_BottomSeal_RightEntryPoint"));

            structureLayout.SetExternalComponents(exteriorFloors, exteriorWalls, roofs);
            Room internalRoom = StructureLayoutHelper.InitializeStructureInterior.Action(param, structureLayout, exteriorFloors, exteriorWalls, externalFloorThickness, externalWallThickness);

            RoomLayoutParams roomLayoutParams = new(
                param.Structure,
                new NumRange(1, 1),
                new NumRange(1, 1),
                new NumRange(4, 13),
                new NumRange(7, param.Length),
                param.TagsRequired,
                0.3f
            );

            RoomLayout roomLayout = StructureLayoutHelper.SubdivideRoom.Action(internalRoom, roomLayoutParams);
            param.TagsRequired.GetValueSafe(Tags.Structure_HasHousing, out int housingCount);
            structureLayout.TagsCurrent.Add(Tags.Structure_HasHousing, housingCount);
            structureLayout.TagsCurrent.Add(Tags.Structure_HasRooms, roomLayout.Rooms.Count);
            structureLayout.SetInternalComponents([roomLayout]);
            
            foreach (Room room in param.Structure.StructureLayout.Rooms) {
                //StructureLayoutHelper.CreateStairways.Action(param, room);
                room.Params.TagsRequired.Add(param.Structure.LayoutRandom.NextBool() ? Tags.Room_TypeLiving : Tags.Room_TypeBedroom);
            }

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
    //     bool leftTall = random.NextBool();
    //     int leftHeight = !leftTall
    //         ? (int)Math.Round(structureParams.Height * 1.33) - 4
    //         : (int)Math.Round(structureParams.Height * 0.66) - 4;
    //     int rightHeight = leftTall
    //         ? (int)Math.Round(structureParams.Height * 1.33) - 4
    //         : (int)Math.Round(structureParams.Height * 0.66) - 4;
    //     int flangeHeight = (int)(0.85 + random.NextDouble() * 0.3) * structureParams.Height;
    //     bool leftFlange = random.NextBool();
    //     int leftFlangeWidth = leftFlange ? 0 : random.Next(4, 8);
    //     bool rightFlange = random.NextBool();
    //     int rightFlangeWidth = rightFlange ? 0 : random.Next(4, 8);
    //     int rightSideStartXPos = structureParams.Start.X + (structureParams.End.X - structureParams.Start.X) / 2;
    //     if (leftTall)
    //         rightSideStartXPos += leftFlangeWidth + rightFlangeWidth;
    //     else
    //         rightSideStartXPos -= leftFlangeWidth + rightFlangeWidth;
    //
    //     int firstFloorHeight = random.Next(6, 9);
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