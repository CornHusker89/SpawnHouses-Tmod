using System;
using System.Collections.Generic;
using SpawnHouses.Structures.StructureParts;
using Terraria;
using Terraria.DataStructures;
using Terraria.ModLoader;

namespace SpawnHouses.Structures;

public class StructureChain
{
    /*
     * @summary makes a chain of CustomStructures and Bridges
     * 
     */
    public StructureChain(ushort maxCost, List<CustomChainStructure> structureList, Point16 entryPos, byte maxBranchLength, bool keepEntryPointClear = true)
    {
        foreach (var structure in structureList)
            if (structure.Cost == -1)
                throw new Exception("invalid item in structureList");

        if (structureList.Count == 0)
            throw new Exception("structureList had no valid options");
        
        ushort currentCost = 0;
        ushort averageCost = 1;
        foreach (var structure in structureList)
            averageCost += (ushort)structure.Cost;
        averageCost /= (ushort)structureList.Count;

        List<BoundingBox> boundingBoxes = new List<BoundingBox>();
        
        CustomChainStructure rootStructure = NewStructure(null, (ushort)entryPos.X, (ushort)entryPos.Y);
        
        boundingBoxes.AddRange(rootStructure.StructureBoundingBoxes);
        for (byte direction = 0; direction < 4; direction++)
        {
            for (byte index = 0; index < rootStructure.ConnectPoints[direction].Length; index++)
            {
                if (keepEntryPointClear && rootStructure.ConnectPoints[direction][index].X == entryPos.X &&
                    rootStructure.ConnectPoints[direction][index].Y == entryPos.Y) continue;

                CalculateChildrenStructures(rootStructure.ConnectPoints[direction][index],
                    rootStructure.StructureXSize, direction, 1);
            }
        }
        
        GenerateChildren(rootStructure);
        
        // functions
        void GenerateChildren(CustomChainStructure structure, int branchLength = 0)
        {
            if (branchLength > maxBranchLength) return;
            structure.Generate();
            for (byte direction = 0; direction < 4; direction++)
            {
                foreach (ChainConnectPoint connectPoint in structure.ConnectPoints[direction])
                {
                    if (connectPoint.ChildStructure is null) continue;
                    
                    Bridge bridge = structure.ChildBridgeType.Clone();
                    
                    bridge.Point1 = connectPoint;
                    bridge.Point2 = connectPoint.ChildConnectPoint;
                    bridge.Generate();
                    
                    //recursive call
                    GenerateChildren(connectPoint.ChildStructure, branchLength + 1);
                }
            }
        }
        
        CustomChainStructure NewStructure(ChainConnectPoint parentConnectPoint, ushort x = 500, ushort y = 500)
        {
            byte structureIndex = (byte)Terraria.WorldGen.genRand.Next(0, structureList.Count);
            CustomChainStructure structure = structureList[structureIndex].Clone();
            structure.ParentChainConnectPoint = parentConnectPoint;
            structure.SetPosition(x, y);
            return structure;
        }

        void MoveStructureConnectPoint(CustomChainStructure structure, ChainConnectPoint connectPoint, ushort x, ushort y)
        {
            int deltaX = connectPoint.X - x;
            int deltaY = connectPoint.Y - y;
            structure.SetPosition((ushort)(structure.X - deltaX), (ushort)(structure.Y - deltaY));
        }
        
        void CalculateChildrenStructures(ChainConnectPoint connectPoint, ushort structureXSize, byte targetDirection, byte currentBranchLength)
        {
            if (Terraria.WorldGen.genRand.Next(0, maxBranchLength - currentBranchLength) == 0 ||
                currentCost + averageCost >= maxCost) return;
            
            bool validLocation = false;
            byte findLocationAttempts = 0;
            
            CustomChainStructure newStructure = NewStructure(connectPoint);
            ChainConnectPoint targetConnectPoint = null;
            
            while (!validLocation && findLocationAttempts < 40)
            {
                // randomly pick either the top or bottom side
                byte randomSide = (byte)Terraria.WorldGen.genRand.Next(0, 2);
                ushort newStructureConnectPointX, newStructureConnectPointY;
                if (targetDirection == CPDirection.Left || targetDirection == CPDirection.Right)
                {
                    if (targetDirection == CPDirection.Right)
                        newStructureConnectPointX = (ushort)(connectPoint.X + 
                                                structureXSize * Terraria.WorldGen.genRand.Next(7, 13) / 10.0);
                    else
                        newStructureConnectPointX = (ushort)(connectPoint.X - structureXSize -
                                                structureXSize * Terraria.WorldGen.genRand.Next(7, 13) / 10.0);
        
                    // ensure that the bridge like actually fits right lol
                    newStructureConnectPointX += (ushort)((Math.Abs(connectPoint.X - newStructureConnectPointX) - 1) % newStructure.ChildBridgeType.StructureLength);

                    newStructureConnectPointY = (ushort)(connectPoint.Y + Terraria.WorldGen.genRand.Next(-100, 101) / 100.0 * newStructure.ChildBridgeType.MaxDeltaY);
                }
                else
                {
                    newStructureConnectPointX = connectPoint.X;

                    if (targetDirection == CPDirection.Up)
                        newStructureConnectPointY = (ushort)(connectPoint.Y - 1);
                    else
                        newStructureConnectPointY = (ushort)(connectPoint.Y + 1);
                }

                ChainConnectPoint[] targetConnectPointList = newStructure.ConnectPoints[CPDirection.flipDirection(targetDirection)];
                if (targetConnectPointList.Length != 0)
                    targetConnectPoint = targetConnectPointList[(targetConnectPointList.Length - 1) * randomSide];
                else
                    return;
                
                MoveStructureConnectPoint(newStructure, targetConnectPoint, newStructureConnectPointX, newStructureConnectPointY);
                
                validLocation = true;
                if (BoundingBox.IsAnyBoundingBoxesColliding(newStructure.StructureBoundingBoxes, boundingBoxes))
                {
                    findLocationAttempts++;
                    validLocation = false;
                }
            }

            if (!validLocation)
                return;
            
            boundingBoxes.AddRange(newStructure.StructureBoundingBoxes);
            
            connectPoint.ChildStructure = newStructure;
            connectPoint.ChildConnectPoint = targetConnectPoint;
            
            currentCost += (ushort)newStructure.Cost;
            
            for (byte direction = 0; direction < 4; direction++)
                foreach (var nextConnectPoint in newStructure.ConnectPoints[direction])
                {
                    // if the point already has a bridge on it
                    if (connectPoint.ChildConnectPoint == nextConnectPoint)
                        continue;
                    
                    // recursive
                    CalculateChildrenStructures(nextConnectPoint, newStructure.StructureXSize,
                        direction, (byte)(currentBranchLength + 1));
                }
        }
    }
}