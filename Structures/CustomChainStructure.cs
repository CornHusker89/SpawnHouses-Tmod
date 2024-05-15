using System;
using Terraria.ModLoader;
using Terraria.DataStructures;
using Microsoft.Xna.Framework;
using Terraria.WorldBuilding;
using SpawnHouses.Structures.StructureParts;

namespace SpawnHouses.Structures;

public class CustomChainStructure : CustomStructure
{
    public Bridge ChildBridge { get; set; }
    public sbyte Cost { get; set; }
    public BoundingBox StructureBoundingBox { get; set; }
    private readonly byte _boundingBoxMargin;
    
    // you're not really intended to make a base customStructure, so this is private
    protected CustomChainStructure(String filePath, ushort structureXSize, ushort structureYSize, Floor[] floors,
        ConnectPoint[] connectPoints, Bridge childBridge,  ushort x = 1, ushort y = 1, sbyte cost = -1, byte boundingBoxMargin = 0)
    {
        FilePath = filePath;
        StructureXSize = structureXSize;
        StructureYSize = structureYSize;
        X = x;
        Y = y;
        Floors = floors;
        ConnectPoints = connectPoints;
        ChildBridge = childBridge;
        Cost = cost;
        _boundingBoxMargin = boundingBoxMargin;
        SetSubstructurePositions();
    }
    
    protected override void SetSubstructurePositions()
    {
        foreach (Floor floor in Floors)
            floor.SetPosition(mainStructureX: X, mainStructureY: Y);
        foreach (ConnectPoint connectPoint in ConnectPoints)
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
    
    public virtual CustomChainStructure Clone()
    {
        return new CustomChainStructure
        (
            FilePath = FilePath,
            StructureXSize = StructureXSize,
            StructureYSize = StructureYSize,
            Floors = (Floor[])Floors.Clone(),
            ConnectPoints = (ConnectPoint[])ConnectPoints.Clone(),
            ChildBridge = ChildBridge,
            X = X,
            Y = Y,
            Cost = Cost
        );
    }
}
