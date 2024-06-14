using SpawnHouses.Structures.StructureParts;
using System;
using System.Collections;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.DataStructures;
using Terraria.WorldBuilding;
using BoundingBox = SpawnHouses.Structures.StructureParts.BoundingBox;

namespace SpawnHouses.Structures;

public class Bridge
{
    public readonly byte[] InputDirections;
    public readonly ushort MinDeltaX;
    public readonly ushort MaxDeltaX;
    public readonly ushort MinDeltaY;
    public readonly ushort MaxDeltaY;
    public readonly byte DeltaXMultiple;
    public readonly byte DeltaYMultiple;
    public ConnectPoint Point1;
    public ConnectPoint Point2;

    public BoundingBox[] BoundingBoxes;
    
    
    public Bridge(byte[] inputDirections,
        ushort minDeltaX, ushort maxDeltaX, ushort minDeltaY, ushort maxDeltaY, byte deltaXMultiple = 1, byte deltaYMultiple = 1,
        ConnectPoint point1 = null, ConnectPoint point2 = null, BoundingBox[] boundingBoxes = null)
    {
        InputDirections = inputDirections;
        MinDeltaX = minDeltaX;
        MaxDeltaX = maxDeltaX;
        MinDeltaY = minDeltaY;
        MaxDeltaY = maxDeltaY;
        DeltaXMultiple = deltaXMultiple;
        DeltaYMultiple = deltaYMultiple;
        Point1 = point1;
        Point2 = point2;
        BoundingBoxes = boundingBoxes;
    }

    public virtual void Generate()
    {
        throw new Exception("Generate() was called on the Bridge class, this does nothing and should never happen");
    }

    public virtual void SetPoints(ConnectPoint point1, ConnectPoint point2)
    {
        throw new Exception("SetPoints() was called on the Bridge class, this should never happen because it won't make bounding boxes");
    }
    
    public virtual Bridge Clone()
    {
        return new Bridge(InputDirections, MinDeltaX, MaxDeltaX, MinDeltaY, MaxDeltaY, 
            DeltaXMultiple, DeltaYMultiple, Point1, Point2, BoundingBoxes);
    }
}