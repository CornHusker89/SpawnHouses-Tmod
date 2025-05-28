using SpawnHouses.Enums;
using SpawnHouses.Helpers;
using SpawnHouses.Structures.Bridges;
using SpawnHouses.Structures.StructureParts;
using SpawnHouses.Structures.Structures.ChainStructures;

namespace SpawnHouses.Structures.Chains;

public class CaveTown1 : StructureChain {
    public static Bridge[] _bridgeListLarge = [
        new ParabolaBridge.TestBridgeLarge(),
        new ParabolaBridge.TestBridgeLargeAltGen()
    ];

    public static Bridge[] _bridgeListSmall = [
        new ParabolaBridge.TestBridgeSmall(),
        new ParabolaBridge.TestBridgeSmallAltGen()
    ];

    public static CustomChainStructure[] _structureList = [
        new CaveTown1_Test1(cost: 10, weight: 100),
        new CaveTown1_Test2(cost: 10, weight: 25)
    ];

    public CaveTown1(ushort x, ushort y) :
        base(40, 100, 2, 5, x, y, _structureList, null) {
    }

    // Only lets 1 structure to the left and right of the root structure
    protected override bool IsConnectPointValid(ChainConnectPoint connectPoint, ChainConnectPoint targetConnectPoint,
        CustomChainStructure targetStructure) {
        int netSideDistance = 0;
        foreach (byte direction in connectPoint.ParentStructure.BridgeDirectionHistory) {
            if (direction == Directions.Left) netSideDistance--;
            if (direction == Directions.Right) netSideDistance++;
        }

        if (connectPoint.Direction is Directions.Left)
            return netSideDistance >= 0;
        return netSideDistance <= 0;
    }

    protected override Bridge GetBridgeOfDirection(Bridge[] bridges, byte direction, CustomChainStructure structure) {
        Bridge[] newBridgeList;
        if (structure.ID == (ushort)StructureType.CaveTown1_Test1)
            newBridgeList = _bridgeListSmall;
        else
            newBridgeList = _bridgeListLarge;

        for (ushort i = 0; i < 5000; i++) {
            int index = Terraria.WorldGen.genRand.Next(0, newBridgeList.Length);
            if (newBridgeList[index].InputDirections[0] == direction)
                return newBridgeList[index];
        }

        return null;
    }
}