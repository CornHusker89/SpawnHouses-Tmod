using System;
using System.Collections.Generic;
using Microsoft.Build.Utilities;
using Microsoft.Extensions.Logging;
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
        boundingBoxes.Add(rootStructure.StructureBoundingBox);
        for (byte index = 1; index < rootStructure.ConnectPoints.Length; index++)
            CalculateChildrenStructures(rootStructure.ConnectPoints[index], rootStructure.StructureXSize, 1);

        GenerateChildren(rootStructure);
        Console.WriteLine();
        Console.WriteLine();
        Console.WriteLine();
        
        // start of functions
        void GenerateChildren(CustomChainStructure structure, int branchLength = 0)
        {
            if (branchLength > maxBranchLength) return;
            structure.GenerateStructure();
            foreach (ChainConnectPoint connectPoint in structure.ConnectPoints)
            {
                if (connectPoint.ChildStructure is null) continue;
                Bridge bridge = structure.ChildBridgeType.Clone();
                
                bridge.Point1 = connectPoint;
                bridge.Point2 = connectPoint.ChildStructure.ConnectPoints[0];
                bridge.Generate();
                
                //recursive call
                GenerateChildren(connectPoint.ChildStructure, branchLength + 1);
            }
        }
        
        CustomChainStructure NewStructure(ChainConnectPoint parentConnectPoint, ushort x = 100, ushort y = 100)
        {
            byte structureIndex = (byte)Terraria.WorldGen.genRand.Next(0, structureList.Count);
            CustomChainStructure structure = structureList[structureIndex].Clone();
            structure.ParentChainConnectPoint = parentConnectPoint;
            structure.SetPosition(x, y);
            return structure;
        }

        void MoveStructureConnectPoint(CustomChainStructure structure, ushort x, ushort y)
        {
            int deltaX = structure.ConnectPoints[0].X - x;
            int deltaY = structure.ConnectPoints[0].Y - y;
            structure.SetPosition((ushort)(structure.X - deltaX), (ushort)(structure.Y - deltaY));
        }
        
        void CalculateChildrenStructures(ChainConnectPoint connectPoint, ushort structureXSize, byte currentBranchLength)
        {
            if (Terraria.WorldGen.genRand.Next(0, maxBranchLength - currentBranchLength) == 0 || currentCost + averageCost >= maxCost) return;
            
            bool validLocation = false;
            byte validLocationCount = 0;
            
            CustomChainStructure structure = NewStructure(connectPoint);
            
            while (!validLocation && validLocationCount < 20)
            {
                ushort structureX;
                if (connectPoint.FacingLeft)
                    structureX = (ushort)(connectPoint.X +
                                               structureXSize * Terraria.WorldGen.genRand.Next(7, 13) / 10.0);
                else
                    structureX = (ushort)(connectPoint.X +
                                               structureXSize * Terraria.WorldGen.genRand.Next(7, 13) / 10.0);
    
                // ensure that the bridge like actually fits right lol
                structureX += (ushort)((Math.Abs(connectPoint.X - structureX) - 1) % structure.ChildBridgeType.StructureLength);
                ushort structureY = (ushort)(connectPoint.Y + Terraria.WorldGen.genRand.Next(-100, 101) / 100.0 * structure.ChildBridgeType.MaxDeltaY);
                
                MoveStructureConnectPoint(structure, structureX, structureY);
                
                validLocation = true;
                foreach (var boundingBox in boundingBoxes)
                {
                    if (structure.StructureBoundingBox.IsBoundingBoxColliding(boundingBox))
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
            
            boundingBoxes.Add(structure.StructureBoundingBox);
            
            connectPoint.ChildStructure = structure;
            currentCost += (ushort)structure.Cost;
            
            
            
            for (byte index = 1; index < structure.ConnectPoints.Length; index++)
                // recursive
                CalculateChildrenStructures(structure.ConnectPoints[index], structure.StructureXSize, (byte)(currentBranchLength + 1));
        }
        
    }
}