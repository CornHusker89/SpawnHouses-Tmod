#nullable enable
using static SpawnHouses.Structures.AdvStructures.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using SpawnHouses.Structures.StructureParts;
using Stubble.Core.Classes;
using Terraria;
using Terraria.DataStructures;

namespace SpawnHouses.Structures.AdvStructures;

public static class AdvStructureGen
{
    public static readonly (StructureTag[] possibleTags, Range? lengthRange, Range? volumeRange, Range? heightRange, 
        Range? housingRange, Func<StructureParams, AdvStructure> method)[] GenMethods =
        [
            ([], null, null, null, null,
                Layout1)
        ];

    public static Func<StructureParams, AdvStructure> GetRandomMethod(StructureParams structureParams)
    {
        List<(StructureTag[] possibleTags, Range? lengthRange, Range? volumeRange, Range? heightRange, 
            Range? housingRange, Func<StructureParams, AdvStructure> method)> methodTuples = [];
        
        foreach (var tuple in GenMethods)
        {
            if (tuple.lengthRange?.InRange(structureParams.Length) is !true)
                continue;
            if (tuple.volumeRange?.InRange(structureParams.Volume) is !true)
                continue;
            if (tuple.heightRange?.InRange(structureParams.Height) is !true)
                continue;
            if (tuple.housingRange?.InRange(structureParams.Housing) is !true)
                continue;
            List<StructureTag> requiredTags = structureParams.StructureTagsRequired.ToList();
            foreach (var tag in tuple.possibleTags)
            {
                if (structureParams.StructureTagBlacklist.Contains(tag))
                    break;
                requiredTags.Remove(tag);
            }

            if (requiredTags.Count == 0)
                methodTuples.Add(tuple);
        }

        if (methodTuples.Count == 0)
            throw new Exception("No structures found were compatible with given length and tags");
        return methodTuples[Terraria.WorldGen.genRand.Next(0, methodTuples.Count)].method;
    }

    public static Shape GetBoundingShape(StructureParams structureParams)
    {
        Shape boundingShape = new Shape(
            new Point16(structureParams.Start.X, Math.Max(structureParams.Start.Y, structureParams.End.Y)),
            new Point16(structureParams.End.X,
                Math.Min(structureParams.Start.Y, structureParams.End.Y) + structureParams.Height + 3)
        );
        return boundingShape;
    }
    
    /// <summary>
    /// a house with a tall side, and possible extrusions on tall side. generally quite large and medieval looking
    /// </summary>
    public static AdvStructure Layout1(StructureParams structureParams)
    {
        List<Shape> roomVolumes = [];
        List<Shape> wallVolumes = [];
        List<Shape> floorVolumes = [];
        
        bool leftTall = Terraria.WorldGen.genRand.NextBool();
        int leftHeight = !leftTall? (int)Math.Round(structureParams.Height * 1.33) - 4 : (int)Math.Round(structureParams.Height * 0.66) - 4;
        int rightHeight = leftTall? (int)Math.Round(structureParams.Height * 1.33) - 4 : (int)Math.Round(structureParams.Height * 0.66) - 4;
        int flangeHeight = (int)(0.85 + Terraria.WorldGen.genRand.NextDouble() * 0.3) * structureParams.Height;
        bool leftFlange = Terraria.WorldGen.genRand.NextBool();
        int leftFlangeWidth = leftFlange ? 0 : Terraria.WorldGen.genRand.Next(4, 8);
        bool rightFlange = Terraria.WorldGen.genRand.NextBool();
        int rightFlangeWidth = rightFlange ? 0 : Terraria.WorldGen.genRand.Next(4, 8);
        int rightSideStartXPos = structureParams.Start.X + (structureParams.End.X - structureParams.Start.X) / 2;
        if (leftTall)
            rightSideStartXPos += leftFlangeWidth + rightFlangeWidth;
        else
            rightSideStartXPos -= leftFlangeWidth + rightFlangeWidth;

        int firstFloorHeight = Terraria.WorldGen.genRand.Next(6, 9);
        int nonFirstFloorHeight = firstFloorHeight >= 8 ? firstFloorHeight - 2 : firstFloorHeight - 3;
        List<int> leftSideFloorYPositions = [structureParams.Start.Y];
        List<int> rightSideFloorYPositions = [structureParams.End.Y];

        while (leftSideFloorYPositions[^1] < leftSideFloorYPositions[0] - leftHeight)
            leftSideFloorYPositions.Add(leftSideFloorYPositions[^1] - nonFirstFloorHeight - 3);
        leftHeight = structureParams.Start.Y - (leftSideFloorYPositions[^1] - nonFirstFloorHeight - 3);

        while (rightSideFloorYPositions[^1] < rightSideFloorYPositions[0] + rightHeight)
            rightSideFloorYPositions.Add(rightSideFloorYPositions[^1] + nonFirstFloorHeight + 3);
        rightHeight = structureParams.End.Y - (rightSideFloorYPositions[^1] - nonFirstFloorHeight - 3);

        // add outer wall volumes
        if (leftTall && leftFlange)
        {
            wallVolumes.Add(new Shape(
                new Point16(structureParams.Start.X - 2, structureParams.Start.Y - 4),
                new Point16(structureParams.Start.X, structureParams.Start.Y - flangeHeight)
            ));
            wallVolumes.Add(new Shape(
                new Point16(structureParams.Start.X + leftFlangeWidth + 1, structureParams.Start.Y - flangeHeight + 1),
                new Point16(structureParams.Start.X + leftFlangeWidth + 3, structureParams.Start.Y - leftHeight)
            ));
        }
        else
        {
            wallVolumes.Add(new Shape(
                new Point16(structureParams.Start.X - 2, structureParams.Start.Y - 4),
                new Point16(structureParams.Start.X, structureParams.Start.Y - leftHeight)
            ));
        }

        if (leftTall && rightFlange)
        {
            wallVolumes.Add(new Shape(
                new Point16(rightSideStartXPos - 3 - rightFlangeWidth - 2, structureParams.Start.Y - flangeHeight + 1),
                new Point16(rightSideStartXPos - 3 - rightFlangeWidth, structureParams.Start.Y - leftHeight)
            ));
            wallVolumes.Add(new Shape(
                new Point16(rightSideStartXPos - 3, structureParams.Start.Y - 4),
                new Point16(rightSideStartXPos - 1, structureParams.Start.Y - flangeHeight)
            ));
        }
        else if (!leftTall && leftFlange)
        {
            wallVolumes.Add(new Shape(
                new Point16(rightSideStartXPos, structureParams.End.Y - 4),
                new Point16(rightSideStartXPos + 2, structureParams.End.Y - flangeHeight)
            ));
            wallVolumes.Add(new Shape(
                new Point16(rightSideStartXPos + leftFlangeWidth + 1, structureParams.End.Y - flangeHeight - 1),
                new Point16(rightSideStartXPos + leftFlangeWidth + 3, structureParams.End.Y - rightHeight)
            ));
        }
        else
        {
            wallVolumes.Add(new Shape(
                new Point16(rightSideStartXPos - 3, structureParams.Start.Y - 4),
                new Point16(rightSideStartXPos - 1, structureParams.Start.Y - leftHeight)
            ));
        }

        if (!leftTall && rightFlange)
        {
            wallVolumes.Add(new Shape(
                new Point16(structureParams.End.X - rightFlangeWidth - 2, structureParams.End.Y - flangeHeight + 1),
                new Point16(structureParams.End.X - rightFlangeWidth, structureParams.End.Y - rightHeight)
            ));
            wallVolumes.Add(new Shape(
                new Point16(structureParams.End.X, structureParams.End.Y - 4),
                new Point16(structureParams.End.X + 2, structureParams.End.Y - flangeHeight)
            ));
        }
        else
        {
            wallVolumes.Add(new Shape(
                new Point16(structureParams.End.X, structureParams.End.Y - 4),
                new Point16(structureParams.End.X + 2, structureParams.End.Y - rightHeight)
            ));
        }

        Console.WriteLine(
            $"leftTall: {leftTall}, leftFlange: {leftFlange}, rightFlange: {rightFlange}, leftHeight: {leftHeight}, rightHeight: {rightHeight}, rightSideStartXPos: {rightSideStartXPos}, approxHeight: {structureParams.Height}");
        Shape.CreateOutline(wallVolumes.ToArray());

        return new AdvStructure();
    }

    
    
    /// <summary>
    /// a basic vertical house, expands as much as needed vertically but poor horizontal expansion
    /// </summary>
    public static AdvStructure Layout2(StructureParams structureParams)
    {
        List<Shape> roomVolumes = [];
        List<Shape> wallVolumes = [];
        List<Shape> floorVolumes = [];

        int externalWallThickness = Terraria.WorldGen.genRand.Next(1, 3);

        // set floor y positions & floor count
        List<int> floorYPositions = [];
        int floorVolumeHeight = Terraria.WorldGen.genRand.Next(1, 3);
        int roomVolumeHeight = Terraria.WorldGen.genRand.Next(4, 7);
        structureParams.Height -= structureParams.Height % (floorVolumeHeight + roomVolumeHeight);
        for (int y = structureParams.Start.Y; y >= structureParams.Start.Y - structureParams.Height - (floorVolumeHeight + roomVolumeHeight); y -= floorVolumeHeight + roomVolumeHeight)
            floorYPositions.Add(y);
        
        // determine floor slope
        bool flatFloor = structureParams.Start.Y == structureParams.End.Y;
        int floorSlopeStartX = !flatFloor? 0 : Terraria.WorldGen.genRand.Next(
            structureParams.Start.X + 2, structureParams.Start.X + 2 + (structureParams.Length / 2));
        int floorSlopeLength = Math.Abs(structureParams.Start.Y - structureParams.End.Y);
        int floorSlopeEndX = !flatFloor? 0 : Terraria.WorldGen.genRand.Next(
            structureParams.Start.X + 2 + (structureParams.Length / 2) + floorSlopeLength, structureParams.End.X - 2);
        
        // set first floor volumes
        if (flatFloor)
        {
            floorVolumes.Add(new Shape(
                new Point16(structureParams.Start.X, structureParams.Start.Y + 1),
                new Point16(structureParams.End.X, structureParams.End.Y + 3)
            ));
            roomVolumes.Add(new Shape(
                new Point16(structureParams.Start.X + 1, structureParams.Start.Y),
                new Point16(structureParams.End.X - 1, floorYPositions.Count == 1? structureParams.Start.Y - roomVolumeHeight : floorYPositions[1])
            ));
        }
        else
        {
            floorVolumes.Add(new Shape(
                new Point16(structureParams.Start.X, structureParams.Start.Y + 1),
                new Point16(floorSlopeStartX, structureParams.Start.Y + 1),
                new Point16(floorSlopeEndX, structureParams.End.Y + 1),
                new Point16(structureParams.End.X, structureParams.End.Y + 1),
                new Point16(structureParams.Start.X, structureParams.Start.Y + 4),
                new Point16(structureParams.End.X, structureParams.End.Y + 4)
            ));
            roomVolumes.Add(new Shape(
                new Point16(structureParams.Start.X + 1, structureParams.Start.Y),
                new Point16(floorSlopeStartX, structureParams.Start.Y),
                new Point16(floorSlopeEndX, structureParams.End.Y),
                new Point16(structureParams.End.X - 1, structureParams.End.Y),
                new Point16(structureParams.Start.X + 1, floorYPositions.Count == 1? structureParams.Start.Y + roomVolumeHeight : floorYPositions[1]),
                new Point16(structureParams.End.X - 1, floorYPositions.Count == 1? structureParams.Start.Y + roomVolumeHeight : floorYPositions[1])
            ));
        }
        
        wallVolumes.Add(new Shape(
            new Point16(structureParams.Start.X + 1 - externalWallThickness, structureParams.Start.Y - 3),
            new Point16(structureParams.Start.X, structureParams.Start.Y - structureParams.Height)
        ));
        wallVolumes.Add(new Shape(
            new Point16(structureParams.End.X, structureParams.End.Y - 3),
            new Point16(structureParams.End.X - 1 + externalWallThickness, structureParams.End.Y - structureParams.Height)
        ));
        wallVolumes.Add(new Shape(
            new Point16(structureParams.End.X + 10, structureParams.End.Y - 3),
            new Point16(structureParams.End.X + 9 + externalWallThickness, structureParams.End.Y - structureParams.Height)
        ));
        
        for (int floorNumber = 1; floorNumber < floorYPositions.Count; floorNumber++)
        {
            floorVolumes.Add(new Shape(
                new Point16(structureParams.Start.X + 1, floorYPositions[floorNumber] + floorVolumeHeight),
                new Point16(structureParams.End.X - 1, floorYPositions[floorNumber])
            ));
            roomVolumes.Add(new Shape(
                new Point16(structureParams.Start.X + 1, floorYPositions[floorNumber] - 1),
                new Point16(structureParams.End.X - 1, floorYPositions[floorNumber] - 1 - roomVolumeHeight)
            ));
        }
        
        // Shape.CreateOutline(roomVolumes.ToArray());
        Shape.CreateOutline(wallVolumes.ToArray());
        // Shape.CreateOutline(floorVolumes.ToArray());
        // Shape.CreateOutline([new Shape(
        //     new Point16(structureParams.End.X, structureParams.End.Y - 3),
        //     new Point16(structureParams.End.X - 1 + externalWallThickness, structureParams.End.Y - structureParams.Height)
        // )]);
        
        
        
        return new AdvStructure();
    }
}