using System;
using System.Collections.Generic;
using SpawnHouses.Structures.StructureParts;

namespace SpawnHouses.Structures;

public class CustomChainStructure : CustomStructure {
    public List<byte> BridgeDirectionHistory = [];
    public new ChainConnectPoint[][] ConnectPoints;
    public sbyte Cost;
    public ChainConnectPoint ParentChainConnectPoint;
    public Box[] StructureBoundingBoxes;
    public ushort Weight;

    protected CustomChainStructure(string filePath, ushort structureXSize, ushort structureYSize,
        ChainConnectPoint[][] connectPoints, ushort x = 1000, ushort y = 1000,
        byte status = StructureStatus.NotGenerated, sbyte cost = -1, ushort weight = 10) :
        base(filePath, structureXSize, structureYSize, null, status, x, y, true) {
        ConnectPoints =
            CopyChainConnectPoints(connectPoints); // need to overwrite CustomStructure's connectPoints property
        Cost = cost;
        Weight = weight;

        for (byte direction = 0; direction < 4; direction++)
            foreach (var connectPoint in ConnectPoints[direction])
                connectPoint.ParentStructure = this;

        SetSubstructurePositions();
    }

    protected override void SetSubstructurePositions() {
        // can't inherit because the ConnectPoint type changes
        for (byte direction = 0; direction < 4; direction++)
            foreach (var connectPoint in ConnectPoints[direction])
                connectPoint.SetPosition(X, Y);

        StructureBoundingBoxes = [
            new Box(X, Y, X + StructureXSize - 1, Y + StructureYSize - 1)
        ];
    }

    public override void SetPosition(int x, int y) {
        X = (ushort)x;
        Y = (ushort)y;
        SetSubstructurePositions();
    }

    public ChainConnectPoint GetRootConnectPoint() {
        for (byte direction = 0; direction < 4; direction++)
            foreach (var connectPoint in ConnectPoints[direction])
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

    public static List<byte> CloneBridgeDirectionHistory(CustomChainStructure structure) {
        List<byte> newHistory = [];

        foreach (var direction in structure.BridgeDirectionHistory)
            newHistory.Add(direction);

        return newHistory;
    }

    protected static Bridge[] CopyBridges(Bridge[] bridges) {
        var newBridges = (Bridge[])bridges.Clone();
        for (byte i = 0; i < newBridges.Length; i++)
            newBridges[i] = newBridges[i].Clone();
        return newBridges;
    }

    public override CustomChainStructure Clone() {
        var type = GetType();
        return (CustomChainStructure)Activator.CreateInstance(type, X, Y, Status, Cost, Weight)!;
    }

    public void ActionOnEachConnectPoint(Action<ChainConnectPoint> function) {
        for (byte direction = 0; direction < 4; direction++)
            foreach (var connectPoint in ConnectPoints[direction])
                function(connectPoint);
    }
}