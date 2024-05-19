using System;
using Terraria.ModLoader;
using Terraria.DataStructures;
using Microsoft.Xna.Framework;
using Terraria.WorldBuilding;
using SpawnHouses.Structures.StructureParts;

namespace SpawnHouses.Structures;

public class CustomChainStructure : CustomStructure
{
    public new ChainConnectPoint[] ConnectPoints { get; set; }
    public ChainConnectPoint[] TopConnectPoints { get; set; }
    public ChainConnectPoint[] BottomConnectPoints { get; set; }
    public ChainConnectPoint[] LeftConnectPoints { get; set; }
    public ChainConnectPoint[] RightConnectPoints { get; set; }
    
    public Bridge ChildBridgeType { get; set; }
    public sbyte Cost { get; set; }
    public BoundingBox StructureBoundingBox { get; set; }
    public ChainConnectPoint ParentChainConnectPoint { get; set; }
    private readonly byte _boundingBoxMargin;
    
    // you're not really intended to make a base customStructure, so this is private
    protected CustomChainStructure(String filePath, ushort structureXSize, ushort structureYSize, Floor[] floors,
        ChainConnectPoint[] connectPoints, ChainConnectPoint[] topConnectPoints, ChainConnectPoint[] bottomConnectPoints,
        ChainConnectPoint[] leftConnectPoints, ChainConnectPoint[] rightConnectPoints, Bridge childBridge,
        ushort x = 1, ushort y = 1, sbyte cost = -1, byte boundingBoxMargin = 0)
    {
        FilePath = filePath;
        StructureXSize = structureXSize;
        StructureYSize = structureYSize;
        X = x;
        Y = y;
        Floors = floors;
        ConnectPoints = connectPoints;
        TopConnectPoints = topConnectPoints;
        BottomConnectPoints = bottomConnectPoints;
        LeftConnectPoints = leftConnectPoints;
        RightConnectPoints = rightConnectPoints;
        ChildBridgeType = childBridge;
        Cost = cost;
        _boundingBoxMargin = boundingBoxMargin;
        SetSubstructurePositions();
    }
    
    protected override void SetSubstructurePositions()
    {
        foreach (Floor floor in Floors)
            floor.SetPosition(mainStructureX: X, mainStructureY: Y);
        foreach (var connectPoint in ConnectPoints)
            connectPoint.SetPosition(mainStructureX: X, mainStructureY: Y);
        StructureBoundingBox = new BoundingBox(X - _boundingBoxMargin, Y - _boundingBoxMargin,
            X + StructureXSize + _boundingBoxMargin, Y + StructureYSize + _boundingBoxMargin);
    }
    
    public override void SetPosition(ushort x, ushort y)
    {
        X = x;
        Y = y;
        SetSubstructurePositions();
    }
    
    protected static Floor[] CopyFloors(Floor[] floors)
    {
        Floor[] newFloors = (Floor[])floors.Clone();
        for (byte i = 0; i < newFloors.Length; i++)
            newFloors[i] = newFloors[i].Clone();
        return newFloors;
    }
    
    protected static ChainConnectPoint[] CopyConnectPoints(ChainConnectPoint[] connectPoints)
    {
        ChainConnectPoint[] newConnectPoints = (ChainConnectPoint[])connectPoints.Clone();
        for (byte i = 0; i < newConnectPoints.Length; i++)
            newConnectPoints[i] = newConnectPoints[i].Clone();
        return newConnectPoints;
    }
    
    public virtual CustomChainStructure Clone()
    {
        return new CustomChainStructure
        (
            FilePath = FilePath,
            StructureXSize = StructureXSize,
            StructureYSize = StructureYSize,
            Floors = CopyFloors(Floors),
            ConnectPoints = CopyConnectPoints(ConnectPoints),
            TopConnectPoints = CopyConnectPoints(TopConnectPoints),
            BottomConnectPoints = CopyConnectPoints(BottomConnectPoints),
            LeftConnectPoints = CopyConnectPoints(LeftConnectPoints),
            RightConnectPoints = CopyConnectPoints(RightConnectPoints),
            ChildBridgeType = ChildBridgeType,
            X = X,
            Y = Y,
            Cost = Cost
        );
    }
}
