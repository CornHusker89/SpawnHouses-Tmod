using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;
using SpawnHouses.Structures.StructureParts;
using SpawnHouses.Structures.Bridges;
using SpawnHouses.Structures.ChainStructures;
using SpawnHouses.Structures.ChainStructures.MainBasement;
using BoundingBox = SpawnHouses.Structures.StructureParts.BoundingBox;

namespace SpawnHouses.Structures;

public class StructureChain
{
    /*
     * @summary makes a chain of CustomStructures and Bridges
     * 
     */
    public StructureChain(ushort maxCost, ushort minCost, CustomChainStructure[] structureList, Point16 entryPos, 
        byte minBranchLength, byte maxBranchLength, 
        CustomChainStructure rootStructure = null, bool keepRootPointClear = true, bool ignoreInvalidDirections = false)
    {
        foreach (var structure in structureList)
            if (structure.Cost < 0)
                throw new Exception("invalid item cost in structureList");

        if (structureList.Length == 0)
            throw new Exception("structureList had no valid options");
        
        int currentCost = 0;
        
        //make the weights in structureList cumulative, and make the starting weight 0
        for (byte i = 1; i < structureList.Length; i++)
            structureList[i].Weight = (ushort)(structureList[i].Weight + structureList[i - 1].Weight);

        for (byte i = 0; i < structureList.Length; i++)
            structureList[i].Weight -= structureList[0].Weight;
        ushort structureListWeightSum = structureList[^1].Weight;
        

        List<BoundingBox> boundingBoxes = new List<BoundingBox>();

        if (rootStructure == null)
        {
            rootStructure = NewStructure(null, false, entryPos.X, entryPos.Y);
            currentCost += rootStructure.Cost;  
        }
        else
        {
            rootStructure = rootStructure.Clone();
            rootStructure.SetPosition(entryPos.X, entryPos.Y);
            currentCost += rootStructure.Cost; 
        }
        
        ChainConnectPoint rootConnectPoint = rootStructure.GetRootConnectPoint();
        if (rootConnectPoint != null)
            MoveConnectPointAndStructure(rootStructure, rootConnectPoint, entryPos.X, entryPos.Y);
        
        
        List<ChainConnectPoint> queuedConnectPoints = [];
        List<Bridge> bridgeList = [];
        List<ChainConnectPoint> failedConnectPointList = [];

        bool foundValidStructureChain = false;
        // try to find a configuration that satisfies the minCost
        for (byte attempts = 0; attempts < 10; attempts++)
        {
            currentCost = 0;
            queuedConnectPoints = [];
            bridgeList = [];
            failedConnectPointList = [];
            boundingBoxes = [];
            boundingBoxes.AddRange(rootStructure.StructureBoundingBoxes);
            
            for (byte direction = 0; direction < 4; direction++)
            {
                for (byte index = 0; index < rootStructure.ConnectPoints[direction].Length; index++)
                {
                    if (keepRootPointClear && rootStructure.ConnectPoints[direction][index] == rootConnectPoint)
                        continue;

                    rootStructure.ConnectPoints[direction][index].BranchLength = 0;
                    queuedConnectPoints.Add(rootStructure.ConnectPoints[direction][index]);
                }
            }
            
            while (queuedConnectPoints.Count > 0)
            {
                CalculateChildrenStructures(queuedConnectPoints[0], rootStructure);
                queuedConnectPoints.RemoveAt(0);
            }

            if (currentCost >= minCost)
            {
                foundValidStructureChain = true;
                break;
            }
        }

        // if we didn't find what we needed in 10 tries, abort
        if (!foundValidStructureChain)
            return;
        
        GenerateChildren(rootStructure);

        foreach (var bridge in bridgeList)
            bridge.Generate();

        foreach (var connectPoint in failedConnectPointList)
            connectPoint.GenerateSeal();
        
        
        // functions
        void GenerateChildren(CustomChainStructure structure)
        {
            structure.Generate();
            for (byte direction = 0; direction < 4; direction++)
            {
                foreach (ChainConnectPoint connectPoint in structure.ConnectPoints[direction])
                {
                    //if (connectPoint.GenerateChance != GenerateChances.Guaranteed && connectPoint.BranchLength > maxBranchLength) continue;
                    if (connectPoint.ChildStructure is null) continue;

                    Bridge bridge = connectPoint.ChildBridge;
                    
                    bridge.Point1 = connectPoint;
                    bridge.Point2 = connectPoint.ChildConnectPoint;
                    bridgeList.Add(bridge.Clone());
                    
                    //recursive call
                    GenerateChildren(connectPoint.ChildStructure);
                }
            }
        }
        
        CustomChainStructure NewStructure(ChainConnectPoint parentConnectPoint, bool closeToMaxBranchLength = false, int x = 500, int y = 500)
        {
            CustomChainStructure structure = null;
            for (int i = 0; i < 5000; i++)
            {
                double value = Terraria.WorldGen.genRand.NextDouble() * structureListWeightSum;
                structure = structureList.Last(curStructure => curStructure.Weight <= value).Clone();

                // don't generate a branching hallway right after another one :)
                if (parentConnectPoint.GenerateChance == GenerateChances.Guaranteed &&
                    CustomChainStructure.BranchingHallwayIDs.Contains(structure.FilePath))
                {
                    structure = null;
                    continue;
                }
                
                // don't generate a branching hallway if it means going over the max branch count
                if (closeToMaxBranchLength && CustomChainStructure.BranchingHallwayIDs.Contains(structure.FilePath))
                {
                    structure = null;
                    continue;
                }
                
                structure.ParentChainConnectPoint = parentConnectPoint;
                structure.SetPosition(x, y);
                break;
            }
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
                if (bridges[index].InputDirections[0] == direction)
                    return bridges[index];
            }

            if (ignoreInvalidDirections)
                return null;
            else
                throw new Exception($"bridge of direction {direction} was not found in this structure's ChildBridgeTypes");
        }
        
        void CalculateChildrenStructures(ChainConnectPoint connectPoint, CustomChainStructure connectPointParentStructure)
        {
            byte currentBranchLength = connectPoint.BranchLength;
            byte targetDirection = connectPoint.Direction;
            
            if (connectPoint.GenerateChance != GenerateChances.Guaranteed)
                if ((Terraria.WorldGen.genRand.Next(0, maxBranchLength - currentBranchLength) == 0 || currentBranchLength >= maxBranchLength) && currentBranchLength >= minBranchLength)
                {
                    failedConnectPointList.Add(connectPoint);
                    return;
                }
            
            if (connectPoint.GenerateChance == GenerateChances.Rejected) return;
            
            // try to find a structure that wont result in a cost overrun
            bool foundStructure = false;
            CustomChainStructure newStructure = null;
            for (int attempts = 0; attempts < 20; attempts++)
            {
                bool closeToMaxBranchLen = connectPoint.BranchLength >= maxBranchLength - 1;
                newStructure = NewStructure(connectPoint, closeToMaxBranchLen);
                if (currentCost + newStructure.Cost <= maxCost)
                {
                    foundStructure = true;
                    break;
                }
            }
            
            if (!foundStructure)
            {
                failedConnectPointList.Add(connectPoint);
                return;
            }

            ChainConnectPoint targetConnectPoint = null;
            Bridge connectPointBridge = null;
            bool validLocation = false;
            
            for (int findLocationAttempts = 0; findLocationAttempts < 20; findLocationAttempts++)
            {
                
                // randomly pick either the top or bottom side
                int randomSide = 0; //Terraria.WorldGen.genRand.Next(0, 2);
                int newStructureConnectPointX, newStructureConnectPointY;
                
                connectPointBridge = GetBridgeOfDirection(connectPointParentStructure.ChildBridgeTypes, targetDirection);
                if (connectPointBridge == null)
                {
                    failedConnectPointList.Add(connectPoint);
                    return;
                }
                
                int deltaX, deltaY;
                if (connectPointBridge.DeltaXMultiple != 0)
                {
                    int minRangeX = connectPointBridge.MinDeltaX / connectPointBridge.DeltaXMultiple;
                    int maxRangeX = connectPointBridge.MaxDeltaX / connectPointBridge.DeltaXMultiple;
                    deltaX = Terraria.WorldGen.genRand.Next(minRangeX, maxRangeX + 1) * connectPointBridge.DeltaXMultiple;
                }
                else
                    deltaX = 0;

                if (connectPointBridge.DeltaYMultiple != 0)
                {
                    int minRangeY = connectPointBridge.MinDeltaY / connectPointBridge.DeltaYMultiple;
                    int maxRangeY = connectPointBridge.MaxDeltaY / connectPointBridge.DeltaYMultiple;
                    deltaY = Terraria.WorldGen.genRand.Next(minRangeY, maxRangeY + 1) * connectPointBridge.DeltaYMultiple;
                }
                else
                    deltaY = 0;
                
                // note: the +/-1 is because connect points have no space between them even when the deltaX is 1
                
                newStructureConnectPointX = connectPoint.X + deltaX + 1;
                newStructureConnectPointY = connectPoint.Y + deltaY + 1;
                
                byte secondConnectPointDirection;
                if (connectPointBridge.InputDirections[0] == targetDirection)
                    secondConnectPointDirection = connectPointBridge.InputDirections[1];
                else
                    secondConnectPointDirection = connectPointBridge.InputDirections[0];
                    
                ChainConnectPoint[] targetConnectPointList = newStructure.ConnectPoints[secondConnectPointDirection];
                
                
                if (targetConnectPointList.Length != 0)
                    targetConnectPoint = targetConnectPointList[(targetConnectPointList.Length - 1) * randomSide];
                else
                    continue; // try another bridge
                
                MoveConnectPointAndStructure(newStructure, targetConnectPoint, newStructureConnectPointX, newStructureConnectPointY);
                connectPointBridge.SetPoints(connectPoint, targetConnectPoint);


                if (
                    !BoundingBox.IsAnyBoundingBoxesColliding(newStructure.StructureBoundingBoxes, boundingBoxes) &&
                    !BoundingBox.IsAnyBoundingBoxesColliding(connectPointBridge.BoundingBoxes, boundingBoxes)
                ) {
                    validLocation = true;
                    break;
                }
            }

            if (!validLocation)
            {
                failedConnectPointList.Add(connectPoint);
                return;
            }
            
            boundingBoxes.AddRange(newStructure.StructureBoundingBoxes);
            boundingBoxes.AddRange(connectPointBridge.BoundingBoxes);
            
            connectPoint.ChildStructure = newStructure;
            connectPoint.ChildConnectPoint = targetConnectPoint;
            connectPoint.ChildBridge = connectPointBridge;
            
            currentCost += newStructure.Cost;
            
            for (byte direction = 0; direction < 4; direction++)
                foreach (ChainConnectPoint nextConnectPoint in newStructure.ConnectPoints[direction])
                {
                    // if the point already has the bridge on it
                    if (connectPoint.ChildConnectPoint == nextConnectPoint)
                        continue;
                    
                    // """recursive"""
                    nextConnectPoint.BranchLength = (byte)(currentBranchLength + 1);
                    queuedConnectPoints.Add(nextConnectPoint);
                }
        }
    }
    
    public class TestStructureChain
    {
        public TestStructureChain(Point16 point)
        {
            Bridge bridge = new ParabolaBridge.TestBridge();

            CustomChainStructure[] structureList =
            [
                new TestChainStructure(10, 100, [bridge])
            ];
			
            StructureChain chain = new StructureChain(100, 60, structureList, point, 3, 7, null, false);
        }
    }

    public class MainBasementChain
    {
        public MainBasementChain(Point16 point)
        {
            Bridge[] bridgeList =
            [
                new SingleStructureBridge.MainHouseBasementHallway1(),
                new SingleStructureBridge.MainHouseBasementHallway1AltGen(),
                
                new SingleStructureBridge.MainHouseBasementHallway2(),
                new SingleStructureBridge.MainHouseBasementHallway2AltGen(),
                
                new SingleStructureBridge.MainHouseBasementHallway2Reversed(),
                new SingleStructureBridge.MainHouseBasementHallway2ReversedAltGen(),
                
                new SingleStructureBridge.MainHouseBasementHallway3(),
                new SingleStructureBridge.MainHouseBasementHallway3AltGen(),
                
                new SingleStructureBridge.MainHouseBasementHallway3Reversed(),
                new SingleStructureBridge.MainHouseBasementHallway3ReversedAltGen(),
                
                new SingleStructureBridge.MainHouseBasementHallway6(),
                new SingleStructureBridge.MainHouseBasementHallway6AltGen(),
                
                new SingleStructureBridge.MainHouseBasementHallway7(),
                new SingleStructureBridge.MainHouseBasementHallway7AltGen(),
                
                new SingleStructureBridge.MainHouseBasementHallway8(),
                new SingleStructureBridge.MainHouseBasementHallway8AltGen()
            ];
            
            CustomChainStructure rootStructure = new MainBasement_Entry1(10, 100, bridgeList);
            
            CustomChainStructure[] structureList = 
            [
                new MainBasement_Room1            (12, 40, bridgeList),
                new MainBasement_Room1_WithFloor  (14, 90, bridgeList),
                new MainBasement_Room2            (13, 40, bridgeList),
                new MainBasement_Room2_WithRoof   (15, 90, bridgeList),
                new MainBasement_Room3            (8, 100, bridgeList),
                new MainBasement_Room4            (11, 8, bridgeList),
                new MainBasement_Room5            (10, 100, bridgeList),
                new MainBasement_Room6            (14, 100, bridgeList),
                new MainBasement_Hallway4         (7, 90, bridgeList),
                new MainBasement_Hallway5         (9, 90, bridgeList),
                new MainBasement_Hallway9         (7, 90, bridgeList)
            ];
            
            StructureChain chain = new StructureChain(75, 40, structureList, point, 1, 3, rootStructure, true, true);
        }
    }
}






















