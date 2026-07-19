using System;
using System.Collections.Generic;
using SpawnHouses.Legacy.Helpers;
using SpawnHouses.Legacy.Structures.StructureParts;
using SpawnHouses.StructureCommon.Types.DataStructures;

namespace SpawnHouses.Legacy.Structures;

public class LegacyChainStructure : LegacyStructure {
    /// <summary>
    ///     structure will have it's big global bounding box, but for placement
    ///     with non-perfect-box structures, we need to know exactly what it looks like
    /// </summary>
    public TileBox[] DetailedBoundingBoxes;

    public List<byte> BridgeDirectionHistory = [];
    public new ChainConnectPoint[][] ConnectPoints;
    public sbyte Cost;
    public ChainConnectPoint ParentChainConnectPoint;

    public LegacyStructureChain ParentLegacyStructureChain;

    public ushort Weight;

    protected LegacyChainStructure(string filePath, ushort structureXSize, ushort structureYSize,
        ChainConnectPoint[][] connectPoints, ushort x = 1000, ushort y = 1000,
        byte status = StructureStatus.NotGenerated, sbyte cost = -1, ushort weight = 10) :
        base(filePath, structureXSize, structureYSize, null, status, x, y, true) {
        ConnectPoints =
            CopyChainConnectPoints(connectPoints); // need to overwrite LegacyStructure's connectPoints property
        Cost = cost;
        Weight = weight;

        for (byte direction = 0; direction < 4; direction++)
            foreach (ChainConnectPoint connectPoint in ConnectPoints[direction])
                connectPoint.ParentStructure = this;

        SetSubstructurePositions();
    }

    protected override void SetSubstructurePositions() {
        // can't inherit because the ConnectPoint type changes
        for (byte direction = 0; direction < 4; direction++)
            foreach (ChainConnectPoint connectPoint in ConnectPoints[direction])
                connectPoint.SetPosition(BoundingBox.Left, BoundingBox.Top);

        DetailedBoundingBoxes = [BoundingBox];
    }

    public override void SetPosition(int x, int y) {
        BoundingBox = BoundingBox.Move(x, y);
        SetSubstructurePositions();
    }

    public ChainConnectPoint GetRootConnectPoint() {
        for (byte direction = 0; direction < 4; direction++)
            foreach (ChainConnectPoint connectPoint in ConnectPoints[direction])
                if (connectPoint.RootPoint)
                    return connectPoint;
        return null;
    }

    protected static ChainConnectPoint[][] CopyChainConnectPoints(ChainConnectPoint[][] connectPoints) {
        var newConnectPoints = (ChainConnectPoint[][])connectPoints.Clone();

        for (byte direction = 0; direction < 4; direction++) {
            newConnectPoints[direction] = (ChainConnectPoint[])connectPoints[direction].Clone();
            for (byte j = 0; j < newConnectPoints[direction].Length; j++)
                newConnectPoints[direction][j] = newConnectPoints[direction][j].Clone();
        }

        return newConnectPoints;
    }

    public static List<byte> CloneBridgeDirectionHistory(LegacyChainStructure structure) {
        List<byte> newHistory = [];

        foreach (byte direction in structure.BridgeDirectionHistory)
            newHistory.Add(direction);

        return newHistory;
    }

    protected static Bridge[] CopyBridges(Bridge[] bridges) {
        var newBridges = (Bridge[])bridges.Clone();
        for (byte i = 0; i < newBridges.Length; i++)
            newBridges[i] = newBridges[i].Clone();
        return newBridges;
    }

    public override LegacyChainStructure Clone() {
        Type type = GetType();
        return (LegacyChainStructure)Activator.CreateInstance(type, BoundingBox.TopLeftPoint16, Status, Cost, Weight)!;
    }

    public void ActionOnEachChainConnectPoint(Action<ChainConnectPoint> function) {
        for (byte direction = 0; direction < 4; direction++)
            foreach (ChainConnectPoint connectPoint in ConnectPoints[direction])
                function(connectPoint);
    }
}