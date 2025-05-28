namespace SpawnHouses.Enums;


// NOTE: keep numbers consistent for backwards compatability
public enum StructureType : ushort {
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