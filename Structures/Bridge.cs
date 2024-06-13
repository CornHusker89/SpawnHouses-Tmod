using SpawnHouses.Structures.StructureParts;
using System;
using System.Collections;
using System.Collections.Generic;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.DataStructures;
using Microsoft.Xna.Framework;
using Terraria.WorldBuilding;

namespace SpawnHouses.Structures;

public class Bridge
{
    public readonly string StructureFilePath;
    public readonly ushort StructureLength;
    public readonly short StructureYOffset;
    public readonly ushort MinDeltaX;
    public readonly ushort MaxDeltaX;
    public readonly ushort MinDeltaY;
    public readonly ushort MaxDeltaY;
    public readonly byte DeltaXMultiple;
    public readonly byte DeltaYMultiple;
    public ConnectPoint Point1;
    public ConnectPoint Point2;
    
    public Bridge(string structureFilePath, ushort structureLength, short structureYOffset, 
        ushort minDeltaX, ushort maxDeltaX, ushort minDeltaY, ushort maxDeltaY, byte deltaXMultiple = 1, byte deltaYMultiple = 1,
        ConnectPoint point1 = null, ConnectPoint point2 = null)
    {
        StructureFilePath = structureFilePath;
        StructureLength = structureLength;
        StructureYOffset = structureYOffset;
        MinDeltaX = minDeltaX;
        MaxDeltaX = maxDeltaX;
        MinDeltaY = minDeltaY;
        MaxDeltaY = maxDeltaY;
        DeltaXMultiple = deltaXMultiple;
        DeltaYMultiple = deltaYMultiple;
        Point1 = point1;
        Point2 = point2;
    }

    public virtual void Generate()
    {
        throw new Exception("Generate() was called on the Bridge class, this does nothing and should never happen");
    }
    
    public virtual Bridge Clone()
    {
        return new Bridge(StructureFilePath, StructureLength, StructureYOffset, 
            MinDeltaX, MaxDeltaX, MinDeltaY, MaxDeltaY, DeltaXMultiple, DeltaYMultiple, Point1, Point2);
    }
}