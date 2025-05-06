using System;
using System.ComponentModel;
using System.Linq;
using SpawnHouses.Structures.Bridges;
using SpawnHouses.Structures.ChainStructures;
using SpawnHouses.Structures.Structures;
using SpawnHouses.Structures.Structures.ChainStructures;
using Terraria;
using Terraria.ModLoader;

namespace SpawnHouses.Structures;

// ReSharper disable InconsistentNaming
public enum StructureID : ushort {
    // normal structures
    MainHouse = 1,
    BeachHouse = 2,
    Firepit = 3,
    Mineshaft = 20,

    // main basement
    MainBasement_Entry1 = 4,
    MainBasement_Entry2 = 5,
    MainBasement_Hallway4 = 6,
    MainBasement_Hallway5 = 7,
    MainBasement_Hallway9 = 8,
    MainBasement_Room1 = 9,
    MainBasement_Room1_WithFloor = 10,
    MainBasement_Room2 = 11,
    MainBasement_Room2_WithRoof = 12,
    MainBasement_Room3 = 13,
    MainBasement_Room4 = 14,
    MainBasement_Room5 = 15,
    MainBasement_Room6 = 16,
    MainBasement_Room7 = 17,

    // cave town 1
    CaveTown1_Test1 = 18,
    CaveTown1_Test2 = 19,

    // testing structures
    BridgeTest = 10000,
    TestChainStructure = 10001,
    TestChainStructure2 = 10002
}

public static class StructureIDUtils {
    public static readonly ushort[] BranchingHallwayIDs = [
        6, 7, 8
    ];

    public static Type GetStructureType(StructureID type) {
        switch (type) {
            // normal structures
            case StructureID.MainHouse:
                return typeof(MainHouse);
            case StructureID.BeachHouse:
                return typeof(BeachHouse);
            case StructureID.Firepit:
                return typeof(Firepit);
            case StructureID.Mineshaft:
                return typeof(Mineshaft);

            // main basement
            case StructureID.MainBasement_Entry1:
                return typeof(MainBasement_Entry1);
            case StructureID.MainBasement_Entry2:
                return typeof(MainBasement_Entry2);
            case StructureID.MainBasement_Hallway4:
                return typeof(MainBasement_Hallway4);
            case StructureID.MainBasement_Hallway5:
                return typeof(MainBasement_Hallway5);
            case StructureID.MainBasement_Hallway9:
                return typeof(MainBasement_Hallway9);
            case StructureID.MainBasement_Room1:
                return typeof(MainBasement_Room1);
            case StructureID.MainBasement_Room1_WithFloor:
                return typeof(MainBasement_Room1_WithFloor);
            case StructureID.MainBasement_Room2:
                return typeof(MainBasement_Room2);
            case StructureID.MainBasement_Room2_WithRoof:
                return typeof(MainBasement_Room2_WithRoof);
            case StructureID.MainBasement_Room3:
                return typeof(MainBasement_Room3);
            case StructureID.MainBasement_Room4:
                return typeof(MainBasement_Room4);
            case StructureID.MainBasement_Room5:
                return typeof(MainBasement_Room5);
            case StructureID.MainBasement_Room6:
                return typeof(MainBasement_Room6);
            case StructureID.MainBasement_Room7:
                return typeof(MainBasement_Room7);


            case StructureID.CaveTown1_Test1:
                return typeof(CaveTown1_Test1);
            case StructureID.CaveTown1_Test2:
                return typeof(CaveTown1_Test2);


            case StructureID.BridgeTest:
                return typeof(BridgeTest);
            case StructureID.TestChainStructure:
                return typeof(TestChainStructure);
            case StructureID.TestChainStructure2:
                return typeof(TestChainStructure2);

            default:
                throw new InvalidEnumArgumentException();
        }
    }

    public static CustomStructure CreateStructure(ushort type, ushort x, ushort y, byte status) {
        var structureType = GetStructureType((StructureID)type);
        object obj;
        if (structureType.BaseType == null)
            obj = Activator.CreateInstance(structureType, x, y, status);
        else
            obj = Activator.CreateInstance(structureType, x, y, status, (sbyte)-1, (ushort)10);

        if (obj is null)
            throw new Exception("Structure ID to structure object failed");

        var structure = (CustomStructure)obj;
        return structure;
    }

    public static bool IsBranchingHallway(CustomChainStructure structure) {
        return BranchingHallwayIDs.Contains(structure.ID);
    }
}

public enum BridgeID : ushort {
    // empties
    EmptyBridgeHorizontal = 1,
    EmptyBridgeHorizontalAltGen = 2,
    EmptyBridgeVertical = 3,
    EmptyBridgeVerticalAltGen = 4,

    // main basement
    MainBasementHallway1 = 5,
    MainBasementHallway1AltGen = 6,
    MainBasementHallway2 = 7,
    MainBasementHallway2AltGen = 8,
    MainBasementHallway2Reversed = 9,
    MainBasementHallway2ReversedAltGen = 10,
    MainBasementHallway3 = 11,
    MainBasementHallway3AltGen = 12,
    MainBasementHallway3Reversed = 13,
    MainBasementHallway3ReversedAltGen = 14,
    MainBasementHallway6 = 15,
    MainBasementHallway6AltGen = 16,
    MainBasementHallway7 = 17,
    MainBasementHallway7AltGen = 18,
    MainBasementHallway8 = 19,
    MainBasementHallway8AltGen = 20,

    // testing structures
    TestBridgeLarge = 10000,
    TestBridgeLargeAltGen = 10001,
    TestBridgeSmall = 10002,
    TestBridgeSmallAltGen = 10003
}

public static class BridgeIDUtils {
    public static readonly ushort[] BranchingHallwayIDs = [
        6, 7, 8
    ];

    public static Type GetBridgeType(BridgeID type) {
        switch (type) {
            // empties
            case BridgeID.EmptyBridgeHorizontal:
                return typeof(SingleStructureBridge.EmptyBridgeHorizontal);
            case BridgeID.EmptyBridgeHorizontalAltGen:
                return typeof(SingleStructureBridge.EmptyBridgeHorizontalAltGen);
            case BridgeID.EmptyBridgeVertical:
                return typeof(SingleStructureBridge.EmptyBridgeVertical);
            case BridgeID.EmptyBridgeVerticalAltGen:
                return typeof(SingleStructureBridge.EmptyBridgeVerticalAltGen);

            // main basement
            case BridgeID.MainBasementHallway1:
                return typeof(SingleStructureBridge.MainBasementHallway1);
            case BridgeID.MainBasementHallway1AltGen:
                return typeof(SingleStructureBridge.MainBasementHallway1AltGen);
            case BridgeID.MainBasementHallway2:
                return typeof(SingleStructureBridge.MainBasementHallway2);
            case BridgeID.MainBasementHallway2AltGen:
                return typeof(SingleStructureBridge.MainBasementHallway2AltGen);
            case BridgeID.MainBasementHallway2Reversed:
                return typeof(SingleStructureBridge.MainBasementHallway2Reversed);
            case BridgeID.MainBasementHallway2ReversedAltGen:
                return typeof(SingleStructureBridge.MainBasementHallway2ReversedAltGen);
            case BridgeID.MainBasementHallway3:
                return typeof(SingleStructureBridge.MainBasementHallway3);
            case BridgeID.MainBasementHallway3AltGen:
                return typeof(SingleStructureBridge.MainBasementHallway3AltGen);
            case BridgeID.MainBasementHallway3Reversed:
                return typeof(SingleStructureBridge.MainBasementHallway3Reversed);
            case BridgeID.MainBasementHallway3ReversedAltGen:
                return typeof(SingleStructureBridge.MainBasementHallway3ReversedAltGen);
            case BridgeID.MainBasementHallway6:
                return typeof(SingleStructureBridge.MainBasementHallway6);
            case BridgeID.MainBasementHallway6AltGen:
                return typeof(SingleStructureBridge.MainBasementHallway6AltGen);
            case BridgeID.MainBasementHallway7:
                return typeof(SingleStructureBridge.MainBasementHallway7);
            case BridgeID.MainBasementHallway7AltGen:
                return typeof(SingleStructureBridge.MainBasementHallway7AltGen);
            case BridgeID.MainBasementHallway8:
                return typeof(SingleStructureBridge.MainBasementHallway8);
            case BridgeID.MainBasementHallway8AltGen:
                return typeof(SingleStructureBridge.MainBasementHallway8AltGen);

            // testing structures
            case BridgeID.TestBridgeLarge:
                return typeof(ParabolaBridge.TestBridgeLarge);
            case BridgeID.TestBridgeLargeAltGen:
                return typeof(ParabolaBridge.TestBridgeLargeAltGen);
            case BridgeID.TestBridgeSmall:
                return typeof(ParabolaBridge.TestBridgeSmall);
            case BridgeID.TestBridgeSmallAltGen:
                return typeof(ParabolaBridge.TestBridgeSmallAltGen);

            default:
                throw new InvalidEnumArgumentException();
        }
    }

    public static Bridge CreateBridge(ushort type) {
        var bridgeType = GetBridgeType((BridgeID)type);
        var obj = Activator.CreateInstance(bridgeType);
        if (obj is null)
            throw new Exception("Structure ID to structure object failed");

        var bridge = (Bridge)obj;
        return bridge;
    }

    public static bool IsBranchingHallway(CustomChainStructure structure) {
        return BranchingHallwayIDs.Contains(structure.ID);
    }
}

public static class Directions {
    public const byte Up = 0;
    public const byte Down = 1;
    public const byte Left = 2;
    public const byte Right = 3;
    public const byte None = 4;

    public static byte FlipDirection(byte direction) {
        if (direction is 1 or 3)
            return (byte)(direction - 1);
        return (byte)(direction + 1);
    }
}

public static class StructureStatus {
    public const byte NotGenerated = 0;
    public const byte GeneratedButNotFound = 1;
    public const byte GeneratedAndFound = 2;
}

public static class GenerateChances {
    public const byte Rejected = 0;
    public const byte Neutral = 1;
    public const byte Guaranteed = 2;
}

public static class ModInstance {
    public static readonly Mod Mod = ModContent.GetInstance<SpawnHouses>();
}

public static class WebClientInstance {
    public static readonly WebHelper WebClient = new();
}