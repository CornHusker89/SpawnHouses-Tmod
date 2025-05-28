using System;
using System.ComponentModel;
using SpawnHouses.Enums;
using SpawnHouses.Structures;
using SpawnHouses.Structures.Bridges;
using SpawnHouses.Types;

namespace SpawnHouses.Helpers;

public static class BridgeIdHelper {
    public static Type GetBridgeType(BridgeType type) {
        switch (type) {
            // empties
            case BridgeType.EmptyBridgeHorizontal:
                return typeof(SingleStructureBridge.EmptyBridgeHorizontal);
            case BridgeType.EmptyBridgeHorizontalAltGen:
                return typeof(SingleStructureBridge.EmptyBridgeHorizontalAltGen);
            case BridgeType.EmptyBridgeVertical:
                return typeof(SingleStructureBridge.EmptyBridgeVertical);
            case BridgeType.EmptyBridgeVerticalAltGen:
                return typeof(SingleStructureBridge.EmptyBridgeVerticalAltGen);

            // main basement
            case BridgeType.MainBasementHallway1:
                return typeof(SingleStructureBridge.MainBasementHallway1);
            case BridgeType.MainBasementHallway1AltGen:
                return typeof(SingleStructureBridge.MainBasementHallway1AltGen);
            case BridgeType.MainBasementHallway2:
                return typeof(SingleStructureBridge.MainBasementHallway2);
            case BridgeType.MainBasementHallway2AltGen:
                return typeof(SingleStructureBridge.MainBasementHallway2AltGen);
            case BridgeType.MainBasementHallway2Reversed:
                return typeof(SingleStructureBridge.MainBasementHallway2Reversed);
            case BridgeType.MainBasementHallway2ReversedAltGen:
                return typeof(SingleStructureBridge.MainBasementHallway2ReversedAltGen);
            case BridgeType.MainBasementHallway3:
                return typeof(SingleStructureBridge.MainBasementHallway3);
            case BridgeType.MainBasementHallway3AltGen:
                return typeof(SingleStructureBridge.MainBasementHallway3AltGen);
            case BridgeType.MainBasementHallway3Reversed:
                return typeof(SingleStructureBridge.MainBasementHallway3Reversed);
            case BridgeType.MainBasementHallway3ReversedAltGen:
                return typeof(SingleStructureBridge.MainBasementHallway3ReversedAltGen);
            case BridgeType.MainBasementHallway6:
                return typeof(SingleStructureBridge.MainBasementHallway6);
            case BridgeType.MainBasementHallway6AltGen:
                return typeof(SingleStructureBridge.MainBasementHallway6AltGen);
            case BridgeType.MainBasementHallway7:
                return typeof(SingleStructureBridge.MainBasementHallway7);
            case BridgeType.MainBasementHallway7AltGen:
                return typeof(SingleStructureBridge.MainBasementHallway7AltGen);
            case BridgeType.MainBasementHallway8:
                return typeof(SingleStructureBridge.MainBasementHallway8);
            case BridgeType.MainBasementHallway8AltGen:
                return typeof(SingleStructureBridge.MainBasementHallway8AltGen);

            // testing structures
            case BridgeType.TestBridgeLarge:
                return typeof(ParabolaBridge.TestBridgeLarge);
            case BridgeType.TestBridgeLargeAltGen:
                return typeof(ParabolaBridge.TestBridgeLargeAltGen);
            case BridgeType.TestBridgeSmall:
                return typeof(ParabolaBridge.TestBridgeSmall);
            case BridgeType.TestBridgeSmallAltGen:
                return typeof(ParabolaBridge.TestBridgeSmallAltGen);

            default:
                throw new InvalidEnumArgumentException();
        }
    }

    public static Bridge CreateBridge(ushort type) {
        Type bridgeType = GetBridgeType((BridgeType)type);
        object obj = Activator.CreateInstance(bridgeType);
        if (obj is null)
            throw new Exception("Structure ID to structure object failed");

        Bridge bridge = (Bridge)obj;
        return bridge;
    }
}