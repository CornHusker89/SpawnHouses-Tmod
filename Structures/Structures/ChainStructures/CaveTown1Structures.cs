using Microsoft.Xna.Framework;
using SpawnHouses.Structures.StructureParts;
using Terraria.DataStructures;

namespace SpawnHouses.Structures.Structures.ChainStructures;

// ReSharper disable InconsistentNaming



public class CaveTown1_Test1 : CustomChainStructure
{
    public CaveTown1_Test1(ushort x = 0, ushort y = 0, byte status = StructureStatus.NotGenerated, sbyte cost = -1, ushort weight = 10) :
        base("Structures/StructureFiles/mainBasement/caveTown1_Test1",
            30,
            16,
            [
                // top
                [],
        
                // bottom
                [],
        
                // left
                [
                    new ChainConnectPoint(0, 7, Directions.Left),
                    new ChainConnectPoint(0, 15, Directions.Left, rootPoint: true)
                ],
        
                // right
                [
                    new ChainConnectPoint(29, 15, Directions.Right)
                ]
            ],
            x, y, status, cost, weight)
    {}
}

public class CaveTown1_Test2 : CustomChainStructure
{
    public CaveTown1_Test2(ushort x = 0, ushort y = 0, byte status = StructureStatus.NotGenerated, sbyte cost = -1, ushort weight = 10) :
        base("Structures/StructureFiles/mainBasement/caveTown1_Test2",
            25,
            13,
            [
                // top
                [],
        
                // bottom
                [],
        
                // left
                [
                    new ChainConnectPoint(0, 12, Directions.Left, rootPoint: true)
                ],
        
                // right
                [
                    new ChainConnectPoint(24, 12, Directions.Right)
                ]
            ],
            x, y, status, cost, weight)
    {}
}