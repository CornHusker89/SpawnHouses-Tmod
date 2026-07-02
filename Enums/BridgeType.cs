namespace SpawnHouses.Enums;

// NOTE: keep numbers consistent for backwards compatability
public enum BridgeType : ushort {
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