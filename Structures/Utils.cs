using System;
using System.Linq;
using Terraria.ModLoader;

namespace SpawnHouses.Structures;

public static class StructureID
{
    // normal structures
    public static readonly short MainHouse = 1;
    public static readonly short BeachHouse = 2;
    public static readonly short Firepit = 3;
    public static readonly short Well = 20;
    
    // main basement
    public static readonly short MainHouseBasement_Entry1 = 4;
    public static readonly short MainHouseBasement_Entry2 = 5;
    public static readonly short MainHouseBasement_Hallway4 = 6;
    public static readonly short MainHouseBasement_Hallway5 = 7;
    public static readonly short MainHouseBasement_Hallway9 = 8;
    public static readonly short MainHouseBasement_Room1 = 9;
    public static readonly short MainHouseBasement_Room1_WithFloor = 10;
    public static readonly short MainHouseBasement_Room2 = 11;
    public static readonly short MainHouseBasement_Room2_WithRoof = 12;
    public static readonly short MainHouseBasement_Room3 = 13;
    public static readonly short MainHouseBasement_Room4 = 14;
    public static readonly short MainHouseBasement_Room5 = 15;
    public static readonly short MainHouseBasement_Room6 = 16;
    public static readonly short MainHouseBasement_Room7 = 17;
    
    // cave town 1
    public static readonly short CaveTown1_Test1 = 18;
    public static readonly short CaveTown1_Test2 = 19;
    
    // testing structures
    public static readonly short BridgeTestStructure = -1;
    public static readonly short TestChainStructure = -2;
    public static readonly short TestChainStructure2 = -3;
    


    public static readonly short[] BranchingHallwayIDs =
    [
        6, 7
    ];

    public static bool IsBranchingHallway(CustomChainStructure structure)
    {
        return BranchingHallwayIDs.Contains(structure.ID);
    }
}

public static class Directions 
{
    public const byte Up = 0;
    public const byte Down = 1;
    public const byte Left = 2;
    public const byte Right = 3;

    public static byte FlipDirection(byte direction)
    {
        if (direction == 1 || direction == 3)
            return (byte)(direction - 1);
        else
            return (byte)(direction + 1);
    }
}

public static class StructureStatus
{
    public const byte NotGenerated = 0;
    public const byte GeneratedButNotFound = 1;
    public const byte GeneratedAndFound = 2;
}

public static class GenerateChances
{
    public const byte Rejected = 0;
    public const byte Neutral = 1;
    public const byte Guaranteed = 2;
}

public static class ModInstance
{
    public static readonly Mod Mod = ModContent.GetInstance<SpawnHouses>();
}