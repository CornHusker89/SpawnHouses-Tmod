using System;
using SpawnHouses.Structures.StructureParts;

namespace SpawnHouses.Structures.Structures.ChainStructures.caveTown1;

public class CaveTown1_CustomChainStructure : CustomChainStructure
{
    public CaveTown1_CustomChainStructure(string _filePath, ushort _structureXSize, ushort _structureYSize,
        Floor[] _floors, ChainConnectPoint[][] _connectPoints, 
        Bridge[] childBridgeType, ushort x = 1000, ushort y = 1000, sbyte cost = -1, ushort weight = 0) :
        base(_filePath, _structureXSize, _structureYSize, CopyFloors(_floors),
            CopyChainConnectPoints(_connectPoints), childBridgeType, x, y, cost, weight) {}

    
    public override bool IsConnectPointValid(ChainConnectPoint connectPoint)
    {
        int netSideDistance = 0;
        foreach (byte direction in connectPoint.ParentStructure.BridgeDirectionHistory)
        {
            if (direction == Directions.Left) netSideDistance--;
            if (direction == Directions.Right) netSideDistance++;
        }
        
        if (connectPoint.Direction is Directions.Left)
            return netSideDistance >= 0;
        else
            return netSideDistance <= 0;
    }
}