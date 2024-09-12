using System;
using System.Collections.Generic;
using Terraria.ModLoader;
using Terraria.DataStructures;
using Terraria.WorldBuilding;
using SpawnHouses.Structures.StructureParts;

namespace SpawnHouses.Structures;

public class CustomChainStructure : CustomStructure
{    
    public new ChainConnectPoint[][] ConnectPoints { get; set; }
    public Bridge[] ChildBridgeTypes { get; set; }
    public sbyte Cost { get; set; }
    public ushort Weight { get; set; }
    public BoundingBox[] StructureBoundingBoxes { get; set; }
    public ChainConnectPoint ParentChainConnectPoint { get; set; }
    public byte BoundingBoxMargin;
    public List<byte> BridgeDirectionHistory { get; set; } = [];
    
    protected CustomChainStructure(String filePath, ushort structureXSize, ushort structureYSize, Floor[] floors,
        ChainConnectPoint[][] connectPoints, Bridge[] childBridges, byte status = StructureStatus.NotGenerated,
        ushort x = 1000, ushort y = 1000, sbyte cost = -1, ushort weight = 10) :
            base(filePath, structureXSize, structureYSize, floors, null, status, x, y)
    {
        ConnectPoints = connectPoints; // need to overwrite CustomStructure's connectPoints property
        ChildBridgeTypes = childBridges;
        Cost = cost;
        Weight = weight;
        
        for (byte direction = 0; direction < 4; direction++)
            foreach (ChainConnectPoint connectPoint in ConnectPoints[direction])
                connectPoint.ParentStructure = this;
    }
    
    protected override void SetSubstructurePositions()
    {
        foreach (var floor in Floors)
            floor.SetPosition(mainStructureX: X, mainStructureY: Y);
        for (byte direction = 0; direction < 4; direction++)
            foreach (var connectPoint in ConnectPoints[direction])
                connectPoint.SetPosition(mainStructureX: X, mainStructureY: Y);
        
        StructureBoundingBoxes =
        [
            new BoundingBox(X, Y, X + StructureXSize - 1, Y + StructureYSize - 1)
        ];
    }
    
    public override void SetPosition(int x, int y)
    {
        X = (ushort)x;
        Y = (ushort)y;
        SetSubstructurePositions();
    }

    public ChainConnectPoint GetRootConnectPoint()
    {
        for (byte direction = 0; direction < 4; direction++)
            foreach (var connectPoint in ConnectPoints[direction])
                if (connectPoint.RootPoint)
                    return connectPoint;
        return null;
    }
    
    protected static ChainConnectPoint[][] CopyChainConnectPoints(ChainConnectPoint[][] connectPoints)
    {
        ChainConnectPoint[][] newConnectPoints = (ChainConnectPoint[][])connectPoints.Clone();
        
        for (byte direction = 0; direction < 4; direction++)
        {
            newConnectPoints[direction] = (ChainConnectPoint[]) connectPoints[direction].Clone();
            for (byte j = 0; j < newConnectPoints[direction].Length; j++)
                newConnectPoints[direction][j] = newConnectPoints[direction][j].Clone();
        }
        return newConnectPoints;
    }

    public static List<byte> CloneBridgeDirectionHistory(CustomChainStructure structure)
    {
        List<byte> newHistory = [];
        
        foreach (byte direction in structure.BridgeDirectionHistory)
            newHistory.Add(direction);

        return newHistory;
    }

    protected static Bridge[] CopyBridges(Bridge[] bridges)
    {
        Bridge[] newBridges = (Bridge[]) bridges.Clone();
        for (byte i = 0; i < newBridges.Length; i++)
            newBridges[i] = newBridges[i].Clone();
        return newBridges;
    }
    
    public virtual CustomChainStructure Clone()
    {
        return null;
    }
    
    public void ActionOnEachConnectPoint(Action<ChainConnectPoint> function)
    {
        for (byte direction = 0; direction < 4; direction++)
            foreach (ChainConnectPoint connectPoint in this.ConnectPoints[direction])
                function(connectPoint);
    }
    
}