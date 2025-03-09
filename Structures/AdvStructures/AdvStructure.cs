using System;
using System.Collections.Generic;
using System.Linq;
using SpawnHouses.Structures.StructureParts;
using Terraria;
using Terraria.DataStructures;

namespace SpawnHouses.Structures.AdvStructures;

public class AdvStructure
{
    public StructureParams Params;
    public readonly List<StructureTag> Tags = [];
    public readonly List<Shape> RoomVolumes = [];
    public readonly List<Shape> WallVolumes = [];
    public readonly List<Shape> FloorVolumes = [];
    public readonly List<Shape> ConnectingVolumes = [];
    public Shape BoundingShape;
    public Point16 Position;
    public int XSize, YSize, Volume, HousingCount;
    public double Scale;

    private Point16[] chairPositions;
    
    /// approximate height of the structure
    private int _approxHeight;
    /// bounding box which determines the boundary of rooms; rooms can stop before this, but cannot extend beyond
    public Shape OuterBoundingShape;
    
    public AdvStructure() {}
    
    
    /// <summary>
    /// 
    /// </summary>
    /// <param name="structureParams"></param>
    public void Calculate(StructureParams structureParams)
    {
        // calculate dimensions
        Params = structureParams;
            
        Position = new Point16(Params.Start.X, Params.Start.Y);
        XSize = Params.End.X - Params.Start.X;

        var method = AdvStructureLayouts.GetRandomMethod(structureParams);
    }
    
    
    public void RebuildDimensions()
    {
        
    }
    
    public void FinishHousing()
    {
        
    }
}