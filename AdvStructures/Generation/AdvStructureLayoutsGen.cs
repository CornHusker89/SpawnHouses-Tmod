#nullable enable
using System;
using System.Collections.Generic;
using System.Linq;
using SpawnHouses.AdvStructures.AdvStructureParts;
using SpawnHouses.Helpers;
using SpawnHouses.Structures;
using SpawnHouses.Types;
using Terraria.DataStructures;
using Range = SpawnHouses.Structures.Range;

namespace SpawnHouses.AdvStructures;

public static class AdvStructureLayoutsGen {
    public static readonly (StructureTag[] possibleTags, Range? lengthRange, Range? volumeRange, Range? heightRange,
        Range? housingRange, Func<AdvStructure, bool> method)[] GenMethods = [
            (
                [
                    StructureTag.HasHousing
                ],
                null, null, null, null,
                StructureLayout2
            )
        ];

    public static Func<AdvStructure, bool> GetRandomMethod(StructureParams structureParams) {
        List<(StructureTag[] possibleTags, Range? lengthRange, Range? volumeRange, Range? heightRange,
            Range? housingRange, Func<AdvStructure, bool> method)> methodTuples = [];

        foreach (var tuple in GenMethods) {
            if (tuple.lengthRange?.InRange(structureParams.Length) is true) continue;

            if (tuple.volumeRange?.InRange(structureParams.Volume) is true) continue;

            if (tuple.heightRange?.InRange(structureParams.Height) is true) continue;

            if (tuple.housingRange?.InRange(structureParams.Housing) is true) continue;

            var requiredTags = structureParams.TagsRequired.ToList();
            bool valid = true;
            foreach (StructureTag possibleTag in tuple.possibleTags) {
                if (structureParams.TagBlacklist.Contains(possibleTag)) {
                    valid = false;
                    break;
                }

                requiredTags.Remove(possibleTag);
            }

            if (valid && requiredTags.Count == 0)
                methodTuples.Add(tuple);
        }

        if (methodTuples.Count == 0)
            throw new Exception("No structures found were compatible with given length and tags");
        return methodTuples[Terraria.WorldGen.genRand.Next(0, methodTuples.Count)].method;
    }

    /// <summary>
    ///     a house with a tall side, and possible extrusions on tall side. generally quite large and medieval looking
    /// </summary>
    // public static bool StructureLayout1(StructureParams structureParams, AdvStructure advStructure) {
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


    /// <summary>
    ///     a basic vertical house, expands as much as needed vertically but poor horizontal expansion
    /// </summary>
    public static bool StructureLayout2(AdvStructure advStructure) {
        // create room params first
        RoomLayoutParams roomLayoutParams = new(
            [],
            [],
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
        advStructure.Tilemap = new StructureTilemap(
            (ushort)(advStructure.Params.Length + 10),
            (ushort)(advStructure.Params.Height + 10),
            advStructure.Params.TopLeft - new Point16(5, 5)
        );

        // then create external layout
        Point16 localTilemapOffset = advStructure.Tilemap.WorldTileOffset;
        int externalWallThickness = roomLayoutParams.WallWidth.Max;
        int externalFloorThickness = roomLayoutParams.FloorWidth.Max;
        List<Floor> exteriorFloors = [];
        List<Wall> exteriorWalls = [];
        List<Gap> exteriorGaps = [];
        EntryPoint[] sortedEntryPoints = ExternalLayoutHelper.SortClockwise(advStructure.Params.EntryPoints);
        for (int i = 0; i < sortedEntryPoints.Length; i++) {
            EntryPoint thisEntryPoint = sortedEntryPoints[i];
            Point16 thisGapWidth = thisEntryPoint.Direction switch {
                Directions.Up => new Point16(0, -externalFloorThickness),
                Directions.Down => new Point16(0, externalFloorThickness),
                Directions.Left => new Point16(-externalWallThickness, 0),
                _ => new Point16(externalWallThickness, 0)
            };
            EntryPoint nextEntryPoint = sortedEntryPoints[(i + 1) % sortedEntryPoints.Length];

            // create and add corresponding external gap
            Gap gap = new(
                new Shape(
                    thisEntryPoint.Start - localTilemapOffset,
                    thisEntryPoint.End + thisGapWidth - localTilemapOffset
                ),
                null!,
                null,
                thisEntryPoint.IsHorizontal
            );
            exteriorGaps.Add(gap);

            // create floor/wall to the next gap
            Point16 thisEndPoint = thisEntryPoint.Direction is Directions.Up or Directions.Right ? thisEntryPoint.Start : thisEntryPoint.End; // end point of the entry point, with respect to clockwise motion
            Point16 nextStartPoint = nextEntryPoint.Direction is Directions.Up or Directions.Right ? nextEntryPoint.End : nextEntryPoint.Start;

            int verticalDistanceRequired = nextStartPoint.Y - thisEndPoint.Y;
            int verticalDirection = verticalDistanceRequired > 0 ? 1 : -1;

            // add in a wall (before the floor) because the entry points are too far away vertically otherwise
            if (Math.Abs(verticalDistanceRequired) > externalFloorThickness) {
                Wall wall = new(new Shape(
                    thisEndPoint + new Point16(0, verticalDirection) - localTilemapOffset,
                    new Point16(thisEndPoint.X + (externalFloorThickness - 1) * verticalDirection, nextStartPoint.Y + verticalDirection) - localTilemapOffset // exclude floor, if theres overlap
                ), true);
                exteriorWalls.Add(wall);
            }

            // add in the floor
            if (thisEndPoint.X != nextStartPoint.X) {
                Floor floor = new(new Shape(
                    new Point16(thisEndPoint.X, nextStartPoint.Y) - localTilemapOffset,
                    nextStartPoint + new Point16(0, externalFloorThickness - 1) - localTilemapOffset
                ), true);
                exteriorFloors.Add(floor);
            }
        }

        advStructure.ExternalLayout = new ExternalLayout(exteriorFloors, exteriorWalls, exteriorGaps);


        // advStructure.EvaluateTilemapData();
        // roomLayoutParams.MainVolume = Shape.FromStructureInterior(advStructure.Tilemap);
        //
        // // finally finish the room layout
        // advStructure.Layout = RoomLayoutGen.GetRandomMethod(roomLayoutParams)(roomLayoutParams);
        //
        // // assign rooms to the external gaps
        // foreach (Gap gap in advStructure.ExternalLayout.Gaps) {
        //     gap.LowerRoom = RoomLayoutHelper.GetClosestRoom(advStructure.Layout.Rooms, gap.Volume.Center);
        // }

        return true;
    }
}