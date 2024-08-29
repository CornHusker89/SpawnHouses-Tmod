using System;
using SpawnHouses.Structures.Bridges;
using SpawnHouses.Structures.ChainStructures;
using SpawnHouses.Structures.Structures.ChainStructures.caveTown1;
using Terraria.DataStructures;

namespace SpawnHouses.Structures.StructureChains;

public class CaveTown1Chain : StructureChain
{
    public static Bridge[] _bridgeList =
    [
        new ParabolaBridge.TestBridge(),
        new ParabolaBridge.TestBridgeAltGen()
    ];
    
    public static CustomChainStructure[] _structureList =
    [
        new CaveTown1_Test1(10, 100, _bridgeList),
        new CaveTown1_Test2(10, 100, _bridgeList)
    ];

    public CaveTown1Chain(ushort x, ushort y) :
        base(100, 10, _structureList, x, y, 1, 3, null, null, false) {}
}   