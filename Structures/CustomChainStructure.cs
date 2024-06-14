using System;
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
    public BoundingBox[] StructureBoundingBoxes { get; set; }
    public ChainConnectPoint ParentChainConnectPoint { get; set; }
    public byte BoundingBoxMargin;
    
    // you're not really intended to make a base customChainStructure, so this is private. It's used for cloning
    protected CustomChainStructure(String filePath, ushort structureXSize, ushort structureYSize, Floor[] floors,
        ChainConnectPoint[][] connectPoints, Bridge[] childBridges,
        ushort x = 1, ushort y = 1, sbyte cost = -1, byte boundingBoxMargin = 0)
    {
        FilePath = filePath;
        StructureXSize = structureXSize;
        StructureYSize = structureYSize;
        X = x;
        Y = y;
        Floors = floors;
        ConnectPoints = connectPoints;
        ChildBridgeTypes = childBridges;
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
                X + StructureXSize + BoundingBoxMargin - 1, Y + StructureYSize + BoundingBoxMargin - 1)
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

    protected static Bridge[] CopyBridges(Bridge[] bridges)
    {
        Bridge[] newBridges = (Bridge[]) bridges.Clone();
        for (byte i = 0; i < newBridges.Length; i++)
            newBridges[i] = newBridges[i].Clone();
        return newBridges;
    }
    
    public virtual CustomChainStructure Clone()
    {
        return new CustomChainStructure
        (
            FilePath = FilePath,
            StructureXSize = StructureXSize,
            StructureYSize = StructureYSize,
            Floors = CopyFloors(Floors),
            ConnectPoints = CopyChainConnectPoints(ConnectPoints),
            ChildBridgeTypes = CopyBridges(ChildBridgeTypes),
            X = X,
            Y = Y,
            Cost = Cost
        );
    }
}