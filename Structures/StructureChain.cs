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
    public StructureChain(ushort maxCost, List<CustomChainStructure> costList, List<Bridge> bridgeList, Point16 entryPos, byte maxBranchLength)
    {
        foreach (var structure in costList)
            if (structure.Cost == -1)
                throw new Exception("invalid item in costList");

        if (costList.Count == 0)
            throw new Exception("costList had no valid options");

        if (bridgeList.Count == 0)
            throw new Exception("bridgeList was empty");
        
        ushort currentCost = 0;
        ushort averageCost = 1;
        foreach (var structure in costList)
            averageCost += (ushort)structure.Cost;
        averageCost /= (ushort)costList.Count;
        
        List<Bridge> bridges = new List<Bridge>();
        List<BoundingBox> boundingBoxes = new List<BoundingBox>();
        
        CustomChainStructure rootStructure = NewStructure((ushort)entryPos.X, (ushort)entryPos.Y);
        boundingBoxes.Add(rootStructure.StructureBoundingBox);
        Chain = new NodeTree<CustomChainStructure>(rootStructure);
        TreeVisitor<CustomChainStructure> generateStructure = GenerateStructure;
        for (byte index = 1; index < rootStructure.ConnectPoints.Length; index++)
            GenerateChildren(rootStructure.ConnectPoints[index], rootStructure.StructureXSize, 1);
        
        Chain.Traverse(Chain, generateStructure);

        foreach (var bridge in bridges)
        {
            //Main.NewText($"({bridge.Point1.X}, {bridge.Point1.Y}), ({bridge.Point2.X}, {bridge.Point2.Y})");
            bridge.Generate();
        }
        
        
        
        // start of functions
        void GenerateStructure(CustomChainStructure structure)
            {
                structure.GenerateStructure();
            }
        
        CustomChainStructure NewStructure(ushort x, ushort y, bool d = false)
        {
            int structureIndex = Terraria.WorldGen.genRand.Next(0, costList.Count);
            CustomChainStructure structure = costList[structureIndex].Clone();
            structure.SetPosition(x, y);
                
            int deltaX = structure.ConnectPoints[0].X - x;
            int deltaY = structure.ConnectPoints[0].Y - y;
            
            structure.SetPosition((ushort)(structure.X - deltaX), (ushort)(structure.Y - deltaY));
            currentCost += (ushort)structure.Cost;
            
            return structure;
        }
        
        void GenerateChildren(ConnectPoint connectPoint, ushort structureXSize, byte currentBranchLength)
        {
            if (Terraria.WorldGen.genRand.Next(0, maxBranchLength - currentBranchLength) == 0 || averageCost + currentCost >= maxCost) return;
            
            byte bridgeIndex = (byte)Terraria.WorldGen.genRand.Next(0, bridgeList.Count);
            Bridge bridge = bridgeList[bridgeIndex].Clone();

            bool validLocation = false;
            byte validLocationCount = 0;
            CustomChainStructure structure = null;

            while (!validLocation && validLocationCount < 20)
            {
                ushort bridgeEndpointX;
                if (connectPoint.FacingLeft)
                    bridgeEndpointX = (ushort)(connectPoint.X +
                                               structureXSize * Terraria.WorldGen.genRand.Next(7, 13) / 10.0);
                else
                    bridgeEndpointX = (ushort)(connectPoint.X +
                                               structureXSize * Terraria.WorldGen.genRand.Next(10, 10) / 10.0);

                // ensure that the bridge like actually fits right lol
                bridgeEndpointX -= (ushort)((Math.Abs(connectPoint.X - bridgeEndpointX) - 1) % bridge.StructureLength);
                //bridgeEndpointX++;
                
                //this will be lower cuz the structure generates top-down   
                ushort bridgeEndpointY = (ushort)(connectPoint.Y + Terraria.WorldGen.genRand.Next(0, 101) / 100.0 * bridge.MaxDeltaY);
                
                structure = NewStructure(bridgeEndpointX, bridgeEndpointY); 
                
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
            
            bridge.Point1 = connectPoint;
            bridge.Point2 = structure.ConnectPoints[0];
            Main.NewText($"{bridge.Point1.X}, {bridge.Point2.X}");
            bridges.Add(bridge);
            
            currentCost += (ushort)structure.Cost;
            Chain.AddChild(structure);

            for (byte index = 1; index < rootStructure.ConnectPoints.Length; index++)
                GenerateChildren(rootStructure.ConnectPoints[index], rootStructure.StructureXSize, (byte)(currentBranchLength + 1));
        }
        
    }
}