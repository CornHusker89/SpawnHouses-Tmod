using System;
using System.Collections.Generic;
using SpawnHouses.Structures.StructureParts;
using Terraria;
using Terraria.DataStructures;

namespace SpawnHouses.Structures;

public class StructureChain
{
    /*
     * @summary makes a chain of CustomStructures and Bridges using NodeTree
     * 
     * @param totalCost the total structure cost for the whole chain
     * @param costList a list of the costs for every possible structure
     * @param bridges a list of the possible bridges to pick from when generating
     */
    public StructureChain(ushort maxCost, List<CustomChainStructure> costList, Point16 entryPos, byte maxBranchLength)
    {
        foreach (var structure in costList)
            if (structure.Cost == -1)
                throw new Exception("invalid item in costList");

        if (costList.Count == 0)
            throw new Exception("costList had no valid options");
        
        ushort currentCost = 0;
        ushort averageCost = 1;
        foreach (var structure in costList)
            averageCost += (ushort)structure.Cost;
        averageCost /= (ushort)costList.Count;
        
        List<BoundingBox> boundingBoxes = new List<BoundingBox>();
        
        CustomChainStructure rootStructure = NewStructure((ushort)entryPos.X, (ushort)entryPos.Y);
        boundingBoxes.Add(rootStructure.StructureBoundingBox);
        for (byte index = 1; index < rootStructure.ConnectPoints.Length; index++)
            CalculateChildrenStructures(rootStructure.ConnectPoints[index], rootStructure.StructureXSize, 1);
        
        //generate the structures and bridges
        void GenerateChild(CustomChainStructure structure)
        {
            structure.GenerateStructure();
            foreach (ChainConnectPoint connectPoint in structure.ConnectPoints)
            {
                if (connectPoint.ChildStructure == null) continue;
                ChainConnectPoint endPoint = connectPoint.ChildStructure.ConnectPoints[0];
                Bridge bridge = structure.ChildBridgeType.Clone();
                bridge.Point1 = connectPoint;
                bridge.Point2 = endPoint;
                bridge.Generate();
            }
        }
        
        // start of functions
        CustomChainStructure NewStructure(ChainConnectPoint connectPoint, ushort x = 100, ushort y = 100)
        {
            byte structureIndex = (byte)Terraria.WorldGen.genRand.Next(0, costList.Count);
            CustomChainStructure structure = costList[structureIndex].Clone();
            structure.ParentChainConnectPoint = connectPoint;
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
            if (Terraria.WorldGen.genRand.Next(0, maxBranchLength - currentBranchLength) == 0 || averageCost + currentCost >= maxCost) return;
            
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
                                               structureXSize * Terraria.WorldGen.genRand.Next(10, 10) / 10.0);
    
                // ensure that the bridge like actually fits right lol
                structureX -= (ushort)((Math.Abs(connectPoint.X - structureX) - 1) % structure.ChildBridgeType.StructureLength);
                ushort structureY = (ushort)(connectPoint.Y + Terraria.WorldGen.genRand.Next(0, 101) / 100.0 * structure.ChildBridgeType.MaxDeltaY);
                
                MoveStructureConnectPoint(structure, structureX, structureY);
                
                foreach (var boundingBox in boundingBoxes)
                {
                    if (structure.StructureBoundingBox.IsBoundingBoxColliding(boundingBox))
                    {
                        Main.NewText("retrying house gen cuz of bounding box collision");
                        continue;
                    }; 
                    boundingBoxes.Add(structure.StructureBoundingBox);
                    validLocation = true;
                    break;
                }
                validLocationCount++;
            }

            if (!validLocation)
            {
                Main.NewText("child failed becuase it couldnt find valid location");
                return;
            }

            connectPoint.ChildStructure = structure;
            currentCost += (ushort)structure.Cost;
            for (byte index = 1; index < structure.ConnectPoints.Length; index++)
                GenerateChildren(structure.ConnectPoints[index], structure.StructureXSize, (byte)(currentBranchLength + 1));
        }
        
    }
}