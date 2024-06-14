using System;
using System.Collections.Generic;
using System.Linq;
using SpawnHouses.Structures.StructureParts;
using Terraria;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;

namespace SpawnHouses.Structures;

public class StructureChain
{
    /*
     * @summary makes a chain of CustomStructures and Bridges
     * 
     */
    public StructureChain(ushort maxCost, List<CustomChainStructure> structureList, Point16 entryPos, byte maxBranchLength, bool keepRootPointClear = true)
    {
        foreach (var structure in structureList)
            if (structure.Cost == -1)
                throw new Exception("invalid item in structureList");

        if (structureList.Count == 0)
            throw new Exception("structureList had no valid options");
        
        int currentCost = 0;
        int averageCost = 1;
        foreach (var structure in structureList)
            averageCost += structure.Cost;
        averageCost /= structureList.Count;

        List<BoundingBox> boundingBoxes = new List<BoundingBox>();
        
        CustomChainStructure rootStructure = NewStructure(null, entryPos.X, entryPos.Y);
        
        ChainConnectPoint rootConnectPoint = rootStructure.GetRootConnectPoint();
        if (rootConnectPoint != null)
            MoveConnectPointAndStructure(rootStructure, rootConnectPoint, entryPos.X, entryPos.Y);
        
        boundingBoxes.AddRange(rootStructure.StructureBoundingBoxes);
        for (byte direction = 0; direction < 4; direction++)
        {
            for (byte index = 0; index < rootStructure.ConnectPoints[direction].Length; index++)
            {
                if (keepRootPointClear && rootStructure.ConnectPoints[direction][index] == rootConnectPoint)
                    continue;
                
                CalculateChildrenStructures(rootStructure.ConnectPoints[direction][index], rootStructure, direction, 1);
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

                    int bridgeIndex = Terraria.WorldGen.genRand.Next(0, structure.ChildBridgeTypes.Length);
                    Bridge bridge = structure.ChildBridgeTypes[bridgeIndex].Clone();
                    
                    bridge.Point1 = connectPoint;
                    bridge.Point2 = connectPoint.ChildConnectPoint;
                    bridge.Generate();
                    
                    //recursive call
                    GenerateChildren(connectPoint.ChildStructure, branchLength + 1);
                }
            }
        }
        
        CustomChainStructure NewStructure(ChainConnectPoint parentConnectPoint, int x = 500, int y = 500)
        {
            int structureIndex = Terraria.WorldGen.genRand.Next(0, structureList.Count);
            CustomChainStructure structure = structureList[structureIndex].Clone();
            structure.ParentChainConnectPoint = parentConnectPoint;
            structure.SetPosition(x, y);
            return structure;
        }

        void MoveConnectPointAndStructure(CustomChainStructure structure, ChainConnectPoint connectPoint, int x, int y)
        {
            int deltaX = connectPoint.X - x;
            int deltaY = connectPoint.Y - y;
            structure.SetPosition(structure.X - deltaX, structure.Y - deltaY);
        }

        Bridge GetBridgeOfDirection(Bridge[] bridges, byte direction)
        {
            for (ushort i = 0; i < 5000; i++)
            {
                int index = Terraria.WorldGen.genRand.Next(0, bridges.Length);
                if (bridges[index].InputDirections.Contains(direction))
                    return bridges[index];
            }

            throw new Exception($"bridge of direction {direction} was not found in this structure's ChildBridgeTypes");
        }
        
        void CalculateChildrenStructures(ChainConnectPoint connectPoint, CustomChainStructure connectPointParentStructure, 
            byte targetDirection, int currentBranchLength)
        {
            if (Terraria.WorldGen.genRand.Next(0, maxBranchLength - currentBranchLength) == 0 ||
                currentCost + averageCost >= maxCost) return;
            
            bool validLocation = false;
            byte findLocationAttempts = 0;
            
            CustomChainStructure newStructure = NewStructure(connectPoint);
            ChainConnectPoint targetConnectPoint = null;
            Bridge connectPointBridge = null;
            
            
            while (!validLocation && findLocationAttempts < 50)
            {
                // randomly pick either the top or bottom side
                int randomSide = Terraria.WorldGen.genRand.Next(0, 2);
                int newStructureConnectPointX, newStructureConnectPointY;
                
                connectPointBridge = GetBridgeOfDirection(connectPointParentStructure.ChildBridgeTypes, targetDirection);
                if (targetDirection == Directions.Left || targetDirection == Directions.Right)
                {
                    int deltaX, deltaY;
                    int minRangeX = connectPointBridge.MinDeltaX / connectPointBridge.DeltaXMultiple;
                    int maxRangeX = connectPointBridge.MaxDeltaX / connectPointBridge.DeltaXMultiple;
                    deltaX = Terraria.WorldGen.genRand.Next(minRangeX, maxRangeX + 1) * connectPointBridge.DeltaXMultiple;
                    int minRangeY = connectPointBridge.MinDeltaY / connectPointBridge.DeltaYMultiple;
                    int maxRangeY = connectPointBridge.MaxDeltaY / connectPointBridge.DeltaYMultiple;
                    deltaY = Terraria.WorldGen.genRand.Next(minRangeY, maxRangeY + 1) * connectPointBridge.DeltaYMultiple;
                    
                    // note: the +/-1 is because connect points have no space between them even when the deltaX is 1
                    if (targetDirection == Directions.Right)
                        newStructureConnectPointX = connectPoint.X + deltaX + 1;
                    else
                        newStructureConnectPointX = connectPoint.X - deltaX - 1;

                    if (Terraria.WorldGen.genRand.NextBool())
                        newStructureConnectPointY = connectPoint.Y + deltaY + 1;
                    else
                        newStructureConnectPointY = connectPoint.Y - deltaY - 1;
                }
                else
                {
                    newStructureConnectPointX = connectPoint.X;

                    if (targetDirection == Directions.Up)
                        newStructureConnectPointY = connectPoint.Y - 1;
                    else
                        newStructureConnectPointY = connectPoint.Y + 1;
                }

                ChainConnectPoint[] targetConnectPointList = newStructure.ConnectPoints[Directions.flipDirection(targetDirection)];
                if (targetConnectPointList.Length != 0)
                    targetConnectPoint = targetConnectPointList[(targetConnectPointList.Length - 1) * randomSide];
                else
                    return;
                
                MoveConnectPointAndStructure(newStructure, targetConnectPoint, newStructureConnectPointX, newStructureConnectPointY);
                connectPointBridge.SetPoints(connectPoint, targetConnectPoint);
                
                validLocation = true;
                if (BoundingBox.IsAnyBoundingBoxesColliding(newStructure.StructureBoundingBoxes, boundingBoxes) || 
                    BoundingBox.IsAnyBoundingBoxesColliding(connectPointBridge.BoundingBoxes, boundingBoxes))
                {
                    findLocationAttempts++;
                    validLocation = false;
                }
            }

            if (!validLocation)
                return;
            
            boundingBoxes.AddRange(newStructure.StructureBoundingBoxes);
            boundingBoxes.AddRange(connectPointBridge.BoundingBoxes);
            
            connectPoint.ChildStructure = newStructure;
            connectPoint.ChildConnectPoint = targetConnectPoint;
            connectPoint.ChildBridge = connectPointBridge;
            
            currentCost += newStructure.Cost;
            
            for (byte direction = 0; direction < 4; direction++)
                foreach (var nextConnectPoint in newStructure.ConnectPoints[direction])
                {
                    // if the point already has a bridge on it
                    if (connectPoint.ChildConnectPoint == nextConnectPoint)
                        continue;
                    
                    // recursive
                    CalculateChildrenStructures(nextConnectPoint, newStructure,
                        direction, currentBranchLength + 1);
                }
        }
    }
}