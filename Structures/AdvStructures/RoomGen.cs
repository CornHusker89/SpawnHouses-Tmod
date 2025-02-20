using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using SpawnHouses.Structures.StructureParts;
using Terraria;
using Terraria.DataStructures;

namespace SpawnHouses.Structures.AdvStructures;

public class RoomGen
{
    public static readonly (StructureTag[] possibleTags, Func<RoomParams, RoomLayout> method)[] GenMethods =
    [
        (
            [
                StructureTag.HasRoom,
                StructureTag.DecorGroundLevel,
                StructureTag.DecorElevated
            ],
            Room1
        )
    ];
    
    public static Func<RoomParams, RoomLayout> GetRandomMethod(RoomParams componentParams)
    {
        List<(StructureTag[] possibleTags, Func<RoomParams, RoomLayout> method)> methodTuples = [];
        foreach (var tuple in GenMethods)
        {
            List<StructureTag> requiredTags = componentParams.TagsRequired.ToList();
            foreach (var possibleTag in tuple.possibleTags)
            {
                if (componentParams.TagsBlacklist.Contains(possibleTag))
                    break;
                requiredTags.Remove(possibleTag);
            }

            if (requiredTags.Count == 0)
                methodTuples.Add(tuple);
        }

        if (methodTuples.Count == 0)
            throw new Exception("No components found were compatible with given tags");
        return methodTuples[Terraria.WorldGen.genRand.Next(0, methodTuples.Count)].method;
    }    
    
    
    /// <summary>
    /// procedural BSP algorithm to split rooms
    /// </summary>
    public static RoomLayout Room1(RoomParams roomParams)
    {
        List<Shape> floorVolumes = [];
        List<Shape> floorGapVolumes = [];
        List<Shape> wallVolumes = [];
        List<Shape> wallGapVolumes = [];

        List<int> potentialXSplits = [];
        List<int> potentialYSplits = [];
        
        Queue<Shape> roomQueue = new Queue<Shape>([roomParams.MainVolume]);
        
        foreach (var point in roomParams.MainVolume.Points)
        {
            if (point.X != roomParams.MainVolume.BoundingBox.topLeft.X &&
                point.X != roomParams.MainVolume.BoundingBox.bottomRight.X && !potentialXSplits.Contains(point.X))
            {
                potentialXSplits.Add(point.X);
            }
            if (point.Y != roomParams.MainVolume.BoundingBox.topLeft.Y &&
                point.Y != roomParams.MainVolume.BoundingBox.bottomRight.Y && !potentialYSplits.Contains(point.Y))
            {
                potentialYSplits.Add(point.Y);
            }
        }
        
        for (int curHousing = 0; curHousing < roomParams.Housing; curHousing++)
        {
            Shape roomVolume = roomQueue.Dequeue();
            bool canSplitAlongX = roomVolume.BoundingBox.bottomRight.Y - roomVolume.BoundingBox.topLeft.Y > 2 * (5 + roomParams.FloorWidth);
            bool canSplitAlongY = roomVolume.BoundingBox.bottomRight.X - roomVolume.BoundingBox.topLeft.X > 2 * (7 + roomParams.WallWidth);
            bool splitAlongX;
            if (canSplitAlongX && canSplitAlongY)
                splitAlongX = Terraria.WorldGen.genRand.NextBool();
            else if (!canSplitAlongX && canSplitAlongY)
                splitAlongX = false;
            else if (canSplitAlongX)
                splitAlongX = true;
            else
                continue; // and don't add this room back to the queue
                
            int outerBoundaryWidth = splitAlongX? 5 + roomParams.FloorWidth : 7 + roomParams.WallWidth;
            Range range = new Range(
                (splitAlongX ? roomVolume.BoundingBox.topLeft.Y : roomVolume.BoundingBox.topLeft.X) + outerBoundaryWidth, 
                (splitAlongX? roomVolume.BoundingBox.bottomRight.Y : roomVolume.BoundingBox.bottomRight.X) - 
                    outerBoundaryWidth - (splitAlongX? roomParams.FloorWidth : roomParams.WallWidth)
            );

            List<int> predeterminedSplits = [];
            foreach (int predeterminedSplit in splitAlongX? potentialXSplits : potentialYSplits)
                if (range.InRange(predeterminedSplit))
                    predeterminedSplits.Add(predeterminedSplit);

            int splitStart = predeterminedSplits.Count == 0 ? 
                Terraria.WorldGen.genRand.Next(range.Min, range.Max + 1) : 
                predeterminedSplits[Terraria.WorldGen.genRand.Next(predeterminedSplits.Count)];

            Shape splitShape;
            if (splitAlongX)
                splitShape = new Shape(
                    new Point16(roomVolume.BoundingBox.topLeft.X - 1, splitStart),
                    new Point16(roomVolume.BoundingBox.bottomRight.X + 1, splitStart + roomParams.FloorWidth - 1)
                );
            else
                splitShape = new Shape(
                    new Point16(splitStart, roomVolume.BoundingBox.topLeft.Y - 1),
                    new Point16(splitStart + roomParams.WallWidth - 1, roomVolume.BoundingBox.bottomRight.X + 1)
                );

            foreach(Shape room in roomVolume.Difference(splitShape))
                roomQueue.Enqueue(room);
            
            if (splitAlongX)
                floorVolumes.AddRange(roomVolume.Intersect(splitShape));
            else
                wallVolumes.AddRange(roomVolume.Intersect(splitShape));
        }
        
        return new RoomLayout(
            floorVolumes,
            null,
            wallVolumes,
            null,
            null,
            null,
            roomQueue.ToList()
        );
    }
}