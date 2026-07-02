using System;
using System.ComponentModel;
using SpawnHouses.Enums;
using SpawnHouses.Structures.Bridges;
using SpawnHouses.Types;

namespace SpawnHouses.Helpers;

public static class BridgeIdHelper {
    public static Type GetBridgeType(ushort id) {
        switch (id) {
            // empties
            case (ushort)BridgeType.EmptyBridgeHorizontal:
                return typeof(SingleStructureBridge.EmptyBridgeHorizontal);
            case (ushort)BridgeType.EmptyBridgeHorizontalAltGen:
                return typeof(SingleStructureBridge.EmptyBridgeHorizontalAltGen);
            case (ushort)BridgeType.EmptyBridgeVertical:
                return typeof(SingleStructureBridge.EmptyBridgeVertical);
            case (ushort)BridgeType.EmptyBridgeVerticalAltGen:
                return typeof(SingleStructureBridge.EmptyBridgeVerticalAltGen);

            // main basement
            case (ushort)BridgeType.MainBasementHallway1:
                return typeof(SingleStructureBridge.MainBasementHallway1);
            case (ushort)BridgeType.MainBasementHallway1AltGen:
                return typeof(SingleStructureBridge.MainBasementHallway1AltGen);
            case (ushort)BridgeType.MainBasementHallway2:
                return typeof(SingleStructureBridge.MainBasementHallway2);
            case (ushort)BridgeType.MainBasementHallway2AltGen:
                return typeof(SingleStructureBridge.MainBasementHallway2AltGen);
            case (ushort)BridgeType.MainBasementHallway2Reversed:
                return typeof(SingleStructureBridge.MainBasementHallway2Reversed);
            case (ushort)BridgeType.MainBasementHallway2ReversedAltGen:
                return typeof(SingleStructureBridge.MainBasementHallway2ReversedAltGen);
            case (ushort)BridgeType.MainBasementHallway3:
                return typeof(SingleStructureBridge.MainBasementHallway3);
            case (ushort)BridgeType.MainBasementHallway3AltGen:
                return typeof(SingleStructureBridge.MainBasementHallway3AltGen);
            case (ushort)BridgeType.MainBasementHallway3Reversed:
                return typeof(SingleStructureBridge.MainBasementHallway3Reversed);
            case (ushort)BridgeType.MainBasementHallway3ReversedAltGen:
                return typeof(SingleStructureBridge.MainBasementHallway3ReversedAltGen);
            case (ushort)BridgeType.MainBasementHallway6:
                return typeof(SingleStructureBridge.MainBasementHallway6);
            case (ushort)BridgeType.MainBasementHallway6AltGen:
                return typeof(SingleStructureBridge.MainBasementHallway6AltGen);
            case (ushort)BridgeType.MainBasementHallway7:
                return typeof(SingleStructureBridge.MainBasementHallway7);
            case (ushort)BridgeType.MainBasementHallway7AltGen:
                return typeof(SingleStructureBridge.MainBasementHallway7AltGen);
            case (ushort)BridgeType.MainBasementHallway8:
                return typeof(SingleStructureBridge.MainBasementHallway8);
            case (ushort)BridgeType.MainBasementHallway8AltGen:
                return typeof(SingleStructureBridge.MainBasementHallway8AltGen);

            // testing structures
            case (ushort)BridgeType.TestBridgeLarge:
                return typeof(ParabolaBridge.TestBridgeLarge);
            case (ushort)BridgeType.TestBridgeLargeAltGen:
                return typeof(ParabolaBridge.TestBridgeLargeAltGen);
            case (ushort)BridgeType.TestBridgeSmall:
                return typeof(ParabolaBridge.TestBridgeSmall);
            case (ushort)BridgeType.TestBridgeSmallAltGen:
                return typeof(ParabolaBridge.TestBridgeSmallAltGen);

            default:
                throw new InvalidEnumArgumentException();
        }
    }

    public static Bridge CreateBridge(ushort id) {
        Type bridgeType = GetBridgeType(id);
        object obj = Activator.CreateInstance(bridgeType);
        if (obj is null)
            throw new Exception("Structure ID to structure object failed");

        Bridge bridge = (Bridge)obj;
        return bridge;
    }
}