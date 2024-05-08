using System;
using System.Collections.Generic;

namespace SpawnHouses.Structures;

public class StructureChain
{
    /*
     * @summary makes a chain of CustomStructures using NodeTree
     * 
     * @param totalCost the total structure cost for the whole chain
     * @param costList a list of the costs for every possible structure
     * @param possibleBridges a list of the possible bridges to pick from when generating
     */
    public StructureChain(ushort maxCost, List<short> costList, List<Bridge> possibleBridges)
    {

        List<byte> possibleStructureIndexes = new List<byte>();
        bool possible = false;
        for (byte index = 0; index < costList.Count; index++)
        {
            if (costList[index] != -1)
            {
                possible = true;
                possibleStructureIndexes.Add(index);
            }
        }

        if (!possible)
            throw new Exception("costList had no valid options");

        if (possibleBridges.Count == 0)
            throw new Exception("possibleBridges was empty");
        
        ushort currentCost = 0;

        int structureIndex = Terraria.WorldGen.genRand.Next(0, possibleStructureIndexes.Count - 1);
    }
}