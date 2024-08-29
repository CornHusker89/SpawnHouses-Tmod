using System;
using SpawnHouses.Structures.Bridges;
using SpawnHouses.Structures.Structures.ChainStructures.MainBasement;
using Terraria;
using Terraria.ModLoader;



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

    public static CustomChainStructure[] _rootStructures =
    [
        new MainBasement_Entry2(10, 100, _bridgeList),
        new MainBasement_Entry1(10, 100, _bridgeList)
    ];
        
    public static CustomChainStructure[] _structureList = 
    [
        new MainBasement_Room1            (12, 40, _bridgeList),
        new MainBasement_Room1_WithFloor  (14, 90, _bridgeList),
        new MainBasement_Room2            (13, 40, _bridgeList),
        new MainBasement_Room2_WithRoof   (15, 90, _bridgeList),
        new MainBasement_Room3            (8, 115, _bridgeList),
        new MainBasement_Room4            (11, 6, _bridgeList),
        new MainBasement_Room5            (13, 145, _bridgeList),
        new MainBasement_Room6            (14, 115, _bridgeList),
        new MainBasement_Room7            (14, 80, _bridgeList),
        new MainBasement_Hallway4         (7, 90, _bridgeList),
        new MainBasement_Hallway5         (9, 90, _bridgeList),
        new MainBasement_Hallway9         (7, 90, _bridgeList)
    ];
    
    
    public MainBasementChain(ushort x = 1000, ushort y = 1000, int seed = -1, byte status = StructureStatus.NotGenerated) :
        base((ushort)(80 * ModContent.GetInstance<SpawnHousesConfig>().SpawnPointBasementMultiplier), 
            (ushort)(48 * ModContent.GetInstance<SpawnHousesConfig>().SpawnPointBasementMultiplier),
            _structureList, x, y,
            (byte)Math.Round(1 * ModContent.GetInstance<SpawnHousesConfig>().SpawnPointBasementMultiplier), 
            (byte)Math.Round(3 * ModContent.GetInstance<SpawnHousesConfig>().SpawnPointBasementMultiplier),
            _rootStructures, 
            requiredStructureID: SpawnHousesModHelper.IsMSEnabled && ModContent.GetInstance<SpawnHousesConfig>().SpawnPointBasementMultiplier > 0.60 ?
                MainBasement_Room5._filePath_magicstorage : null,
            true, true, seed, status) {}

    public override void OnFound()
    {
        base.OnFound();
        
        Wiring.TripWire(EntryPosX + 2, EntryPosY + 11, 1, 1);
        Status = StructureStatus.GeneratedAndFound;
    }
    
    public override void Generate()
    {
        base.Generate();
        Status = StructureStatus.GeneratedButNotFound;
    }
}