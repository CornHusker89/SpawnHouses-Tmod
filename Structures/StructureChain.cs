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
    public StructureChain(ushort maxCost, List<CustomChainStructure> structureList, Point16 entryPos, byte maxBranchLength)
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
        Console.WriteLine(rootStructure.X + ", " + rootStructure.Y);
        boundingBoxes.Add(rootStructure.StructureBoundingBox);
        for (byte direction = 0; direction < 4; direction++)
        {
            for (byte index = 1; index < rootStructure.ConnectPoints[direction].Length; index++)
                CalculateChildrenStructures(rootStructure.ConnectPoints[direction][index], rootStructure.StructureXSize, direction, 1);
        }
        
        GenerateChildren(rootStructure);
        Console.WriteLine();
        Console.WriteLine();
        Console.WriteLine();
        
        // functions
        void GenerateChildren(CustomChainStructure structure, int branchLength = 0)
        {
            Console.WriteLine("generating struct");
            structure.Generate();
            for (byte direction = 0; direction < 4; direction++)
            {
                foreach (ChainConnectPoint connectPoint in structure.ConnectPoints[direction])
                {
                    if (connectPoint.ChildStructure is null) continue;
                    Bridge bridge = structure.ChildBridgeType.Clone();
                    
                    bridge.Point1 = connectPoint;
                    bridge.Point2 = connectPoint.ChildConnectPoint;
                    Console.WriteLine("bridge point X's: " + connectPoint.X + ", " + connectPoint.ChildConnectPoint.X);
                    //bridge.Generate();
                    
                    //recursive call
                    GenerateChildren(connectPoint.ChildStructure, branchLength + 1);
                }
            }
        }
        
        CustomChainStructure NewStructure(ChainConnectPoint parentConnectPoint, ushort x = 300, ushort y = 300)
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
            byte validLocationCount = 0;
            
            CustomChainStructure newStructure = NewStructure(connectPoint);
            ChainConnectPoint targetConnectPoint = null;
            
            while (!validLocation && validLocationCount < 20)
            {
                // randomly pick either the top or bottom side
                byte randomSide = (byte)Terraria.WorldGen.genRand.Next(0, 2);
                ushort newStructureConnectPointX, newStructureConnectPointY;
                if (targetDirection == CPDirection.Left || targetDirection == CPDirection.Right)
                {
                    if (connectPoint.FacingLeft)
                        newStructureConnectPointX = (ushort)(connectPoint.X +
                                                structureXSize * Terraria.WorldGen.genRand.Next(7, 13) / 10.0);
                    else
                        newStructureConnectPointX = (ushort)(connectPoint.X -
                                                structureXSize * Terraria.WorldGen.genRand.Next(7, 13) / 10.0);
        
                    // ensure that the bridge like actually fits right lol
                    newStructureConnectPointX += 12; //(ushort)((Math.Abs(connectPoint.X - structureX) - 1) % newStructure.ChildBridgeType.StructureLength);

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
                
                Console.WriteLine(newStructure.X + ", " + newStructure.Y);
                
                validLocation = true;
                foreach (var boundingBox in boundingBoxes)
                {
                    // test for the first bounding box that does collide
                    if (newStructure.StructureBoundingBox.IsBoundingBoxColliding(boundingBox))
                    {
                        validLocationCount++;
                        validLocation = false;
                        break;
                    }
                }
            }

            if (!validLocation)
            {
                Console.WriteLine("child failed because it couldn't find valid location");
                return;
            }
            
            boundingBoxes.Add(newStructure.StructureBoundingBox);
            
            connectPoint.ChildStructure = newStructure;
            connectPoint.ChildConnectPoint = targetConnectPoint;
            
            currentCost += (ushort)newStructure.Cost;
            
            for (byte direction = 0; direction < 4; direction++)
                foreach (var nextConnectPoint in newStructure.ConnectPoints[direction])
                {
                    // recursive
                    CalculateChildrenStructures(nextConnectPoint, newStructure.StructureXSize,
                        direction, (byte)(currentBranchLength + 1));
                }
        }
    }
}