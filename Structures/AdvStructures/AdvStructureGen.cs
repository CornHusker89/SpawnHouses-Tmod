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
    public static readonly (StructureTag[] possibleTags, int minLength, int maxLength, Func<StructureParams, AdvStructure> method)[] GenMethods = 
    [
        ([], 
        34, 70,
        Layout1)
    ];
    
    public static Func<StructureParams, AdvStructure> GetRandomMethod(StructureParams structureParams)
    {
        List<(StructureTag[] possibleTags, int minLength, int maxLength, Func<StructureParams, AdvStructure> method)> methodTuples = [];
        foreach (var tuple in GenMethods)
        {
            int length = structureParams.End.X - structureParams.Start.X;
            if (length < tuple.minLength || length > tuple.maxLength)
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
    
    
    public static (int volume, int housing, int approxHeight, Shape boundingShape) GetBasicAttributes(StructureParams structureParams)
    {
        double scale = Terraria.WorldGen.genRand.NextDouble();
        int volume = (int)(structureParams.MinVolume + (structureParams.MaxVolume - structureParams.MinVolume) * scale);
        int approxHeight = volume / (structureParams.End.X - structureParams.Start.X);
        if (approxHeight <= 4)
            throw new ArgumentException("Volume is too small compared to the length of the structure");
        Shape boundingShape = new Shape(
            new Point16(structureParams.Start.X, Math.Max(structureParams.Start.Y, structureParams.End.Y)),
            new Point16(structureParams.End.X, Math.Min(structureParams.Start.Y, structureParams.End.Y) + approxHeight + 3)
        );
        
        if (structureParams.MinHousing < 0)
            throw new ArgumentException("Min housing cannot be less than 0");
        if (structureParams.MaxHousing < 0)
            throw new ArgumentException("Max housing cannot be less than 0");
        if (structureParams.MaxHousing < structureParams.MinHousing)
            throw new  ArgumentException("Max Housing is less than min housing");
        if (structureParams.MaxHousing == 0 && structureParams.ComponentTagBlacklist.Contains(ComponentTag.RoomHasHousing))
            throw new ArgumentException("Adv structure cannot have a max housing of 0 while blacklisting components with housing");
        int housing = (int)(structureParams.MinHousing + (structureParams.MaxHousing - structureParams.MinHousing) * scale);
        return (volume, housing, approxHeight, boundingShape);
    }
    
    
    public static AdvStructure Layout1(StructureParams structureParams)
    {
        var basicAttributes = GetBasicAttributes(structureParams);
        bool leftTall = Terraria.WorldGen.genRand.NextBool();
        int leftHeight = leftTall? (int)Math.Round(basicAttributes.approxHeight * 1.33) : (int)Math.Round(basicAttributes.approxHeight * 0.66) - 4;
        int rightHeight = !leftTall? (int)Math.Round(basicAttributes.approxHeight * 1.33) : (int)Math.Round(basicAttributes.approxHeight * 0.66) - 4;
        int flangeHeight = (int)(0.85 + Terraria.WorldGen.genRand.NextDouble() * 0.3) * basicAttributes.approxHeight;
        bool leftFlange = Terraria.WorldGen.genRand.NextBool();
        int leftFlangeWidth = leftFlange? Terraria.WorldGen.genRand.Next(4, 8) : 0;
        bool rightFlange = Terraria.WorldGen.genRand.NextBool();
        int rightFlangeWidth = rightFlange? Terraria.WorldGen.genRand.Next(4, 8) : 0;
        int rightSideStartXPos = structureParams.Start.X + (structureParams.End.X - structureParams.Start.X) / 2;
        if (leftTall)
            rightSideStartXPos += leftFlangeWidth + rightFlangeWidth;
        else
            rightSideStartXPos -= leftFlangeWidth + rightFlangeWidth;
        
        int firstFloorHeight = Terraria.WorldGen.genRand.Next(6, 9);
        int nonFirstFloorHeight = firstFloorHeight >= 8? firstFloorHeight - 3 : firstFloorHeight - 2;
        List<int> leftSideFloorYPositions = [structureParams.Start.Y];
        List<int> rightSideFloorYPositions = [structureParams.End.Y];
        
        Shape boundingShape;
        List<Shape> roomVolumes = [];
        List<Shape> wallVolumes = [];
        List<Shape> floorVolumes = [];
        
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
        
        Shape.CreateOutline(wallVolumes.ToArray());
        
        return new AdvStructure();
    }
}