using System;
using System.Collections.Generic;
using SpawnHouses.Structures.StructureParts;
using Terraria;
using Terraria.DataStructures;

namespace SpawnHouses.Structures;

public class StructureChain
{
    public NodeTree<CustomChainStructure> Chain;
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
        Chain = new NodeTree<CustomChainStructure>(rootStructure);
        TreeVisitor<CustomChainStructure> generateStructure = GenerateStructure;
        for (byte index = 1; index < rootStructure.ConnectPoints.Length; index++)
            GenerateChildren(rootStructure.ConnectPoints[index], rootStructure.StructureXSize, 1);
        
        Chain.Traverse(Chain, generateStructure);
        
        // start of functions
        void GenerateStructure(CustomChainStructure structure, LinkedList<NodeTree<CustomChainStructure>> children)
        {
            structure.GenerateStructure();
            for (byte i = 1; i < children.Count; i++)
            {
                Bridge bridge = structure.ChildBridge.Clone();
                bridge.Point1 = structure.ConnectPoints[i].Clone();
                CustomChainStructure childStructure = null;
                foreach (NodeTree<CustomChainStructure> n in children)
                    if (--i == 0)
                        childStructure = n.data;
                bridge.Point2 = childStructure.ConnectPoints[i].Clone();
            }
        }
        
        CustomChainStructure NewStructure(ushort x = 100, ushort y = 100)
        {
            int structureIndex = Terraria.WorldGen.genRand.Next(0, costList.Count);
            CustomChainStructure structure = costList[structureIndex].Clone();
            structure.SetPosition(x, y);
            currentCost += (ushort)structure.Cost;
            return structure;
        }

        void MoveStructureConnectPoint(CustomChainStructure structure, ushort x, ushort y)
        {
            int deltaX = structure.ConnectPoints[0].X - x;
            int deltaY = structure.ConnectPoints[0].Y - y;
            structure.SetPosition((ushort)(structure.X - deltaX), (ushort)(structure.Y - deltaY));
            
        }
        
        void GenerateChildren(ConnectPoint connectPoint, ushort structureXSize, byte currentBranchLength)
        {
            if (Terraria.WorldGen.genRand.Next(0, maxBranchLength - currentBranchLength) == 0 || averageCost + currentCost >= maxCost) return;
            
            bool validLocation = false;
            byte validLocationCount = 0;
            CustomChainStructure structure = NewStructure();

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
                structureX -= (ushort)((Math.Abs(connectPoint.X - structureX) - 1) % structure.ChildBridge.StructureLength);
                ushort structureY = (ushort)(connectPoint.Y + Terraria.WorldGen.genRand.Next(0, 101) / 100.0 * structure.ChildBridge.MaxDeltaY);
                
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
            
            currentCost += (ushort)structure.Cost;
            Chain.AddChild(structure);

            for (byte index = 1; index < structure.ConnectPoints.Length; index++)
                GenerateChildren(structure.ConnectPoints[index], structure.StructureXSize, (byte)(currentBranchLength + 1));
        }
        
    }
}