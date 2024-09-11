using System;
using SpawnHouses.Structures.Bridges;
using SpawnHouses.Structures.ChainStructures;
using SpawnHouses.Structures.StructureParts;
using SpawnHouses.Structures.Structures.ChainStructures.caveTown1;
using Terraria.DataStructures;

namespace SpawnHouses.Structures.StructureChains;

public class CaveTown1 : StructureChain
{
    public static Bridge[] _bridgeListLarge =
    [
        new ParabolaBridge.TestBridgeLarge(),
        new ParabolaBridge.TestBridgeLargeAltGen()
    ];
    
    public static Bridge[] _bridgeListSmall =
    [
        new ParabolaBridge.TestBridgeSmall(),
        new ParabolaBridge.TestBridgeSmallAltGen()
    ];
    
    public static CustomChainStructure[] _structureList =
    [
        new CaveTown1_Test1(10, 100, _bridgeListSmall),
        new CaveTown1_Test2(10, 25, _bridgeListLarge)
    ];

    public CaveTown1(ushort x, ushort y) :
        base(100, 40, _structureList, x, y, 2, 5) {}

    // Only lets 1 structure to the left and right of the root structure
    protected override bool IsConnectPointValid(ChainConnectPoint connectPoint)
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