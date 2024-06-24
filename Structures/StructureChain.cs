using System;
using System.Collections.Generic;
using System.Linq;
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
    public StructureChain(ushort maxCost, CustomChainStructure[] structureList, Point16 entryPos, byte maxBranchLength, CustomChainStructure rootStructure = null, bool keepRootPointClear = true, bool ignoreInvalidDirections = false)
    {
        foreach (var structure in structureList)
            if (structure.Cost == -1)
                throw new Exception("invalid item in structureList");

        if (structureList.Length == 0)
            throw new Exception("structureList had no valid options");
        
        int currentCost = 0;

        List<BoundingBox> boundingBoxes = new List<BoundingBox>();

        if (rootStructure == null)
        {
            rootStructure = NewStructure(null, entryPos.X, entryPos.Y);
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

        List<Bridge> bridgeList = new List<Bridge>();
        GenerateChildren(rootStructure);

        foreach (var bridge in bridgeList)
        {
            bridge.Generate();
        }
            
        
        
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

                    Bridge bridge = connectPoint.ChildBridge;
                    
                    bridge.Point1 = connectPoint;
                    bridge.Point2 = connectPoint.ChildConnectPoint;
                    bridgeList.Add(bridge.Clone());
                    
                    //recursive call
                    GenerateChildren(connectPoint.ChildStructure, branchLength + 1);
                }
            }
        }
        
        CustomChainStructure NewStructure(ChainConnectPoint parentConnectPoint, int x = 500, int y = 500)
        {
            int structureIndex = Terraria.WorldGen.genRand.Next(0, structureList.Length);
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
                if (bridges[index].InputDirections[0] == direction)
                    return bridges[index];
            }

            if (ignoreInvalidDirections)
                return null;
            else
                throw new Exception($"bridge of direction {direction} was not found in this structure's ChildBridgeTypes");
        }
        
        void CalculateChildrenStructures(ChainConnectPoint connectPoint, CustomChainStructure connectPointParentStructure, 
            byte targetDirection, int currentBranchLength)
        {
            if (Terraria.WorldGen.genRand.Next(0, maxBranchLength - currentBranchLength) == 0) return;
            
            bool validLocation = false;
            byte findLocationAttempts = 0;

            // try to find a structure that wont result in a cost overrun
            bool foundStructure = false;
            CustomChainStructure newStructure = null;
            for (byte attempts = 0; attempts < 20; attempts++)
            {
                newStructure = NewStructure(connectPoint);
                if (currentCost + newStructure.Cost <= maxCost)
                {
                    foundStructure = true;
                    break;
                }
            }
            
            if (!foundStructure) return;

            ChainConnectPoint targetConnectPoint = null;
            Bridge connectPointBridge = null;
            
            
            while (!validLocation && findLocationAttempts < 1)
            {
                
                // randomly pick either the top or bottom side
                int randomSide = 0; //Terraria.WorldGen.genRand.Next(0, 2);
                int newStructureConnectPointX, newStructureConnectPointY;
                
                connectPointBridge = GetBridgeOfDirection(connectPointParentStructure.ChildBridgeTypes, targetDirection);
                if (connectPointBridge == null)
                    return;
                
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
    
    public class TestStructureChain
    {
        public TestStructureChain(Point16 point)
        {
            Bridge bridge = new ParabolaBridge.TestBridge();

            CustomChainStructure[] structureList =
            [
                new TestChainStructure(10, [bridge])
            ];
			
            StructureChain chain = new StructureChain(100, structureList, point, 7, null, false);
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
                new SingleStructureBridge.MainHouseBasementHallway3ReversedAltGen()
            ];
            
            CustomChainStructure rootStructure = new MainBasement_Entry1(10, bridgeList);
            
            CustomChainStructure[] structureList = 
            [
                new MainBasement_Room1(10, bridgeList),
                new MainBasement_Room1(10, bridgeList),
                //new MainBasement_Hallway4(6, bridgeList),
                new MainBasement_Hallway5(8, bridgeList)
            ];
            
            StructureChain chain = new StructureChain(120, structureList, point, 4, rootStructure, true, true);
        }
    }
}






















