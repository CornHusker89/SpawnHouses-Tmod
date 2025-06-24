using System;
using System.ComponentModel;
using System.Linq;
using SpawnHouses.Enums;
using SpawnHouses.Structures.Structures;
using SpawnHouses.Structures.Structures.ChainStructures;
using SpawnHouses.Types;

namespace SpawnHouses.Helpers;

public static class StructureIdHelper {
    public static readonly StructureType[] BranchingHallwayIds = [
        StructureType.MainBasementHallway4,
        StructureType.MainBasementHallway5,
        StructureType.MainBasementHallway9
    ];

    public static Type GetStructureType(ushort id) {
        switch (id) {
            // normal structures
            case (ushort)StructureType.MainHouse:
                return typeof(MainHouse);
            case (ushort)StructureType.BeachHouse:
                return typeof(BeachHouse);
            case (ushort)StructureType.Firepit:
                return typeof(Firepit);
            case (ushort)StructureType.Mineshaft:
                return typeof(Mineshaft);

            // main basement
            case (ushort)StructureType.MainBasementEntry1:
                return typeof(MainBasementEntry1);
            case (ushort)StructureType.MainBasementEntry2:
                return typeof(MainBasementEntry2);
            case (ushort)StructureType.MainBasementHallway4:
                return typeof(MainBasementHallway4);
            case (ushort)StructureType.MainBasementHallway5:
                return typeof(MainBasementHallway5);
            case (ushort)StructureType.MainBasementHallway9:
                return typeof(MainBasementHallway9);
            case (ushort)StructureType.MainBasementRoom1:
                return typeof(MainBasementRoom1);
            case (ushort)StructureType.MainBasementRoom1WithFloor:
                return typeof(MainBasementRoom1WithFloor);
            case (ushort)StructureType.MainBasementRoom2:
                return typeof(MainBasementRoom2);
            case (ushort)StructureType.MainBasementRoom2WithRoof:
                return typeof(MainBasementRoom2WithRoof);
            case (ushort)StructureType.MainBasementRoom3:
                return typeof(MainBasementRoom3);
            case (ushort)StructureType.MainBasementRoom4:
                return typeof(MainBasementRoom4);
            case (ushort)StructureType.MainBasementRoom5:
                return typeof(MainBasementRoom5);
            case (ushort)StructureType.MainBasementRoom6:
                return typeof(MainBasementRoom6);
            case (ushort)StructureType.MainBasementRoom7:
                return typeof(MainBasementRoom7);
            case (ushort)StructureType.MainBasementRoom8:
                return typeof(MainBasementRoom8);


            case (ushort)StructureType.CaveTown1Test1:
                return typeof(CaveTown1_Test1);
            case (ushort)StructureType.CaveTown1Test2:
                return typeof(CaveTown1_Test2);


            case (ushort)StructureType.BridgeTest:
                return typeof(BridgeTest);
            case (ushort)StructureType.TestChainStructure:
                return typeof(TestChainStructure);
            case (ushort)StructureType.TestChainStructure2:
                return typeof(TestChainStructure2);

            default:
                throw new InvalidEnumArgumentException();
        }
    }

    public static CustomStructure CreateStructure(ushort id, ushort x, ushort y, byte status) {
        Type structureType = GetStructureType(id);
        object obj;
        if (structureType.BaseType == null)
            obj = Activator.CreateInstance(structureType, x, y, status);
        else
            obj = Activator.CreateInstance(structureType, x, y, status, (sbyte)-1, (ushort)10);

        if (obj is null)
            throw new Exception("Structure ID to structure object failed");

        CustomStructure structure = (CustomStructure)obj;
        return structure;
    }

    public static bool IsBranchingHallway(CustomChainStructure structure) {
        return BranchingHallwayIds.Contains(structure.Id);
    }
}