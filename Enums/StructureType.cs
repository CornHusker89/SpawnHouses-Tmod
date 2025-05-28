namespace SpawnHouses.Enums;

// NOTE: keep numbers consistent for backwards compatability
public enum StructureType : ushort {
    // normal structures
    MainHouse = 1,
    BeachHouse = 2,
    Firepit = 3,
    Mineshaft = 20,

    // main basement
    MainBasementEntry1 = 4,
    MainBasementEntry2 = 5,
    MainBasementHallway4 = 6,
    MainBasementHallway5 = 7,
    MainBasementHallway9 = 8,
    MainBasementRoom1 = 9,
    MainBasementRoom1WithFloor = 10,
    MainBasementRoom2 = 11,
    MainBasementRoom2WithRoof = 12,
    MainBasementRoom3 = 13,
    MainBasementRoom4 = 14,
    MainBasementRoom5 = 15,
    MainBasementRoom6 = 16,
    MainBasementRoom7 = 17,

    // cave town 1
    CaveTown1Test1 = 18,
    CaveTown1Test2 = 19,

    // testing structures
    BridgeTest = 10000,
    TestChainStructure = 10001,
    TestChainStructure2 = 10002
}