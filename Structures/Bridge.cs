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
using System.Reflection;

namespace SpawnHouses.Structures;

public class Bridge
{
    public readonly string StructureFilePath;
    public readonly ushort StructureLength;
    public readonly short StructureYOffset;
    public readonly ushort MaxDeltaY;
    public ConnectPoint Point1;
    public ConnectPoint Point2;
    
    public Bridge(string structureFilePath, ushort structureLength,
        short structureYOffset, ushort maxDeltaY, ConnectPoint point1 = null, ConnectPoint point2 = null)
    {
        StructureFilePath = structureFilePath;
        StructureLength = structureLength;
        StructureYOffset = structureYOffset;
        MaxDeltaY = maxDeltaY;
        Point1 = point1;
        Point2 = point2;
    }

    public virtual void Generate() {}
    
    public virtual Bridge Clone()
    {
        return new Bridge(StructureFilePath, StructureLength, StructureYOffset, MaxDeltaY, Point1, Point2);
    }
}