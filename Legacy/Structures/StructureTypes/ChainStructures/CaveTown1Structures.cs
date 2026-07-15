using SpawnHouses.Legacy.Helpers;
using SpawnHouses.Legacy.Structures.StructureParts;

namespace SpawnHouses.Legacy.Structures.StructureTypes.ChainStructures;

// ReSharper disable InconsistentNaming
public class CaveTown1_Test1 : LegacyChainStructure {
    public CaveTown1_Test1(ushort x = 0, ushort y = 0, byte status = StructureStatus.NotGenerated, sbyte cost = -1,
        ushort weight = 10) :
        base("Assets/StructureFiles/mainBasement/caveTown1_Test1.shstruct",
            30,
            16,
            [
                // top
                [],

                // bottom
                [],

                // left
                [
                    new ChainConnectPoint(0, 7, LegacyDirections.Left),
                    new ChainConnectPoint(0, 15, LegacyDirections.Left, rootPoint: true)
                ],

                // right
                [
                    new ChainConnectPoint(29, 15, LegacyDirections.Right)
                ]
            ],
            x, y, status, cost, weight) {
    }
}

public class CaveTown1_Test2 : LegacyChainStructure {
    public CaveTown1_Test2(ushort x = 0, ushort y = 0, byte status = StructureStatus.NotGenerated, sbyte cost = -1,
        ushort weight = 10) :
        base("Assets/StructureFiles/mainBasement/caveTown1_Test2.shstruct",
            25,
            13,
            [
                // top
                [],

                // bottom
                [],

                // left
                [
                    new ChainConnectPoint(0, 12, LegacyDirections.Left, rootPoint: true)
                ],

                // right
                [
                    new ChainConnectPoint(24, 12, LegacyDirections.Right)
                ]
            ],
            x, y, status, cost, weight) {
    }
}