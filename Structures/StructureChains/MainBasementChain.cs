using SpawnHouses.Structures.Bridges;
using SpawnHouses.Structures.ChainStructures.MainBasement;
using Terraria.DataStructures;

namespace SpawnHouses.Structures.StructureChains;

public class MainBasementChain : StructureChain
{
    // constants
    public static Bridge[] _bridgeList =
    [
        new SingleStructureBridge.MainHouseBasementHallway1(),
        new SingleStructureBridge.MainHouseBasementHallway1AltGen(),
        
        new SingleStructureBridge.MainHouseBasementHallway2(),
        new SingleStructureBridge.MainHouseBasementHallway2AltGen(),
        
        new SingleStructureBridge.MainHouseBasementHallway2Reversed(),
        new SingleStructureBridge.MainHouseBasementHallway2ReversedAltGen(),
        
        new SingleStructureBridge.MainHouseBasementHallway3(),
        new SingleStructureBridge.MainHouseBasementHallway3AltGen(),
        
        new SingleStructureBridge.MainHouseBasementHallway3Reversed(),
        new SingleStructureBridge.MainHouseBasementHallway3ReversedAltGen(),
        
        new SingleStructureBridge.MainHouseBasementHallway6(),
        new SingleStructureBridge.MainHouseBasementHallway6AltGen(),
        
        new SingleStructureBridge.MainHouseBasementHallway7(),
        new SingleStructureBridge.MainHouseBasementHallway7AltGen(),
        
        new SingleStructureBridge.MainHouseBasementHallway8(),
        new SingleStructureBridge.MainHouseBasementHallway8AltGen()
    ];
        
    public static CustomChainStructure _rootStructure = new MainBasement_Entry1(10, 100, _bridgeList);
        
    public static CustomChainStructure[] _structureList = 
    [
        new MainBasement_Room1            (12, 40, _bridgeList),
        new MainBasement_Room1_WithFloor  (14, 90, _bridgeList),
        new MainBasement_Room2            (13, 40, _bridgeList),
        new MainBasement_Room2_WithRoof   (15, 90, _bridgeList),
        new MainBasement_Room3            (8, 100, _bridgeList),
        new MainBasement_Room4            (11, 8, _bridgeList),
        new MainBasement_Room5            (10, 100, _bridgeList),
        new MainBasement_Room6            (14, 100, _bridgeList),
        new MainBasement_Hallway4         (7, 90, _bridgeList),
        new MainBasement_Hallway5         (9, 90, _bridgeList),
        new MainBasement_Hallway9         (7, 90, _bridgeList)
    ];


    public MainBasementChain(ushort x, ushort y, int seed = -1) :
        base(75, 40, _structureList, x, y, 1, 3, _rootStructure, true, true, seed) {}

    public override void Generate()
    {
        base.Generate();
        Status = StructureStatus.GeneratedAndFound;
    }
}