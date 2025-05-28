using System;
using System.ComponentModel;
using System.Linq;
using SpawnHouses.Enums;
using SpawnHouses.Structures;
using SpawnHouses.Structures.ChainStructures;
using SpawnHouses.Structures.Structures;
using SpawnHouses.Structures.Structures.ChainStructures;

namespace SpawnHouses.Helpers;

public static class StructureIdHelper {
    public static readonly StructureType[] BranchingHallwayIds = [
        StructureType.MainBasementHallway4,
        StructureType.MainBasementHallway5,
        StructureType.MainBasementHallway9
    ];

    public static Type GetStructureType(StructureType type) {
        switch (type) {
            // normal structures
            case StructureType.MainHouse:
                return typeof(MainHouse);
            case StructureType.BeachHouse:
                return typeof(BeachHouse);
            case StructureType.Firepit:
                return typeof(Firepit);
            case StructureType.Mineshaft:
                return typeof(Mineshaft);

            // main basement
            case StructureType.MainBasementEntry1:
                return typeof(MainBasement_Entry1);
            case StructureType.MainBasementEntry2:
                return typeof(MainBasement_Entry2);
            case StructureType.MainBasementHallway4:
                return typeof(MainBasement_Hallway4);
            case StructureType.MainBasementHallway5:
                return typeof(MainBasement_Hallway5);
            case StructureType.MainBasementHallway9:
                return typeof(MainBasement_Hallway9);
            case StructureType.MainBasementRoom1:
                return typeof(MainBasement_Room1);
            case StructureType.MainBasementRoom1WithFloor:
                return typeof(MainBasement_Room1_WithFloor);
            case StructureType.MainBasementRoom2:
                return typeof(MainBasement_Room2);
            case StructureType.MainBasementRoom2WithRoof:
                return typeof(MainBasement_Room2_WithRoof);
            case StructureType.MainBasementRoom3:
                return typeof(MainBasement_Room3);
            case StructureType.MainBasementRoom4:
                return typeof(MainBasement_Room4);
            case StructureType.MainBasementRoom5:
                return typeof(MainBasement_Room5);
            case StructureType.MainBasementRoom6:
                return typeof(MainBasement_Room6);
            case StructureType.MainBasementRoom7:
                return typeof(MainBasement_Room7);


            case StructureType.CaveTown1Test1:
                return typeof(CaveTown1_Test1);
            case StructureType.CaveTown1Test2:
                return typeof(CaveTown1_Test2);


            case StructureType.BridgeTest:
                return typeof(BridgeTest);
            case StructureType.TestChainStructure:
                return typeof(TestChainStructure);
            case StructureType.TestChainStructure2:
                return typeof(TestChainStructure2);

            default:
                throw new InvalidEnumArgumentException();
        }
    }

    public static CustomStructure CreateStructure(ushort type, ushort x, ushort y, byte status) {
        Type structureType = GetStructureType((StructureType)type);
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