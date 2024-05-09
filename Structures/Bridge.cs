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
    public string StructureFilePath;
    public ushort StructureLength;
    public short StructureYOffset;
    public ushort MaxDeltaY;
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
    
    public Bridge Clone()
    {
        Type type = GetType();
        Bridge copy = (Bridge)Activator.CreateInstance(type);

        // Get all fields of the class
        FieldInfo[] fields = type.GetFields(BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public);

        foreach (FieldInfo field in fields)
        {
            // Check if the field is a reference type and not null
            if (!field.FieldType.IsValueType && field.GetValue(this) != null)
            {
                // If it's a reference type, create a deep copy
                object fieldValue = field.GetValue(this);
                MethodInfo method = typeof(Bridge).GetMethod("Clone").MakeGenericMethod(field.FieldType);
                object copiedFieldValue = method.Invoke(fieldValue, null);
                field.SetValue(copy, copiedFieldValue);
            }
            else
            {
                // If it's a value type or null, just copy the value
                field.SetValue(copy, field.GetValue(this));
            }
        }

        return copy;
    }
}