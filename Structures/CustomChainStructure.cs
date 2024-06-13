using System;
using Terraria.ModLoader;
using Terraria.DataStructures;
using Terraria.WorldBuilding;
using SpawnHouses.Structures.StructureParts;

namespace SpawnHouses.Structures;

public class CustomChainStructure : CustomStructure
{
    public new ChainConnectPoint[][] ConnectPoints { get; set; }
    public Bridge ChildBridgeType { get; set; }
    public sbyte Cost { get; set; }
    public BoundingBox[] StructureBoundingBoxes { get; set; }
    public ChainConnectPoint ParentChainConnectPoint { get; set; }
    public byte BoundingBoxMargin;
    
    // you're not really intended to make a base customChainStructure, so this is private. It's used for cloning
    protected CustomChainStructure(String filePath, ushort structureXSize, ushort structureYSize, Floor[] floors,
        ChainConnectPoint[][] connectPoints, Bridge childBridge,
        ushort x = 1, ushort y = 1, sbyte cost = -1, byte boundingBoxMargin = 0)
    {
        FilePath = filePath;
        StructureXSize = structureXSize;
        StructureYSize = structureYSize;
        X = x;
        Y = y;
        Floors = floors;
        ConnectPoints = connectPoints;
        ChildBridgeType = childBridge;
        Cost = cost;
        BoundingBoxMargin = boundingBoxMargin;
        SetSubstructurePositions();
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
            new BoundingBox(X - BoundingBoxMargin, Y - BoundingBoxMargin,
                X + StructureXSize + BoundingBoxMargin, Y + StructureYSize + BoundingBoxMargin)
        ];
    }
    
    public override void SetPosition(ushort x, ushort y)
    {
        X = x;
        Y = y;
        SetSubstructurePositions();
    }
    
    protected static ChainConnectPoint[][] CopyConnectPoints(ChainConnectPoint[][] connectPoints)
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
    
    public virtual CustomChainStructure Clone()
    {
        return new CustomChainStructure
        (
            FilePath = FilePath,
            StructureXSize = StructureXSize,
            StructureYSize = StructureYSize,
            Floors = CopyFloors(Floors),
            ConnectPoints = CopyConnectPoints(ConnectPoints),
            ChildBridgeType = ChildBridgeType,
            X = X,
            Y = Y,
            Cost = Cost
        );
    }
}