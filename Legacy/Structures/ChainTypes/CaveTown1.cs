using SpawnHouses.Legacy.Helpers;
using SpawnHouses.Legacy.Structures.BridgeTypes;
using SpawnHouses.Legacy.Structures.StructureParts;
using SpawnHouses.Legacy.Structures.StructureTypes.ChainStructures;

namespace SpawnHouses.Legacy.Structures.ChainTypes;

public class CaveTown1 : LegacyStructureChain {
    public static Bridge[] _bridgeListLarge = [
        new ParabolaBridge.TestBridgeLarge(),
        new ParabolaBridge.TestBridgeLargeAltGen()
    ];

    public static Bridge[] _bridgeListSmall = [
        new ParabolaBridge.TestBridgeSmall(),
        new ParabolaBridge.TestBridgeSmallAltGen()
    ];

    public static LegacyChainStructure[] _structureList = [
        new CaveTown1_Test1(cost: 10, weight: 100),
        new CaveTown1_Test2(cost: 10, weight: 25)
    ];

    public CaveTown1(ushort x, ushort y) :
        base(40, 100, 2, 5, x, y, _structureList, null) {
    }

    // Only lets 1 structure to the left and right of the root structure
    protected override bool IsConnectPointValid(ChainConnectPoint connectPoint, ChainConnectPoint targetConnectPoint,
        LegacyChainStructure targetStructure) {
        int netSideDistance = 0;
        foreach (byte direction in connectPoint.ParentStructure.BridgeDirectionHistory) {
            if (direction == LegacyDirections.Left) netSideDistance--;
            if (direction == LegacyDirections.Right) netSideDistance++;
        }

        if (connectPoint.Direction is LegacyDirections.Left)
            return netSideDistance >= 0;
        return netSideDistance <= 0;
    }

    protected override Bridge GetBridgeOfDirection(Bridge[] bridges, byte direction, LegacyChainStructure structure) {
        Bridge[] newBridgeList;
        newBridgeList = structure.Id2 is StructureType.CaveTown1Test1 ? _bridgeListSmall : _bridgeListLarge;

        for (ushort i = 0; i < 5000; i++) {
            int index = Terraria.WorldGen.genRand.Next(0, newBridgeList.Length);
            if (newBridgeList[index].InputDirections[0] == direction)
                return newBridgeList[index];
        }

        return null;
    }
}