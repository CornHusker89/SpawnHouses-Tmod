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

using SpawnHouses.Structures.StructureParts;
using SpawnHouses.Structures;

namespace SpawnHouses.Structures;

public class CustomStructure {
    private readonly Mod _mod = ModContent.GetInstance<SpawnHouses>();
    
    public virtual string FilePath { get; set; } = "Structures/_";
    public virtual ushort StructureXSize { get; set; } = 1;
    public virtual ushort StructureYSize { get; set; } = 1;
    public virtual BoundingBox boundingBox { get; set; }
    
    public sbyte Cost { get; set; } = -1;
    public ushort X { get; set; } = 10;
    public ushort Y { get; set; } = 10;

    public Floor[] Floors { get; set; } = Array.Empty<Floor>();
    public ConnectPoint[] ConnectPoints { get; set; } = Array.Empty<ConnectPoint>();

    protected void SetSubstructurePositions()
    {
        foreach (Floor floor in Floors)
            floor.SetPosition(mainStructureX: X, mainStructureY: Y);

        foreach (ConnectPoint connectPoint in ConnectPoints)
            connectPoint.SetPosition(mainStructureX: X, mainStructureY: Y);
    }

    public void SetPosition(ushort x, ushort y)
    {
        X = x;
        Y = y;
        SetSubstructurePositions();
    }

    protected void FrameTiles()    
    {
        int centerX = X + (StructureXSize / 2);
        int centerY = Y + (StructureXSize / 2);
        
        WorldUtils.Gen(new Point(centerX, centerY), new Shapes.Circle(Convert.ToInt32(StructureXSize + StructureYSize) ), new Actions.SetFrames());
    }
    
    protected void FrameTiles(int centerX, int centerY, int radius)    
    {
        WorldUtils.Gen(new Point(centerX, centerY), new Shapes.Circle(radius), new Actions.SetFrames());
    }

    public void GenerateStructure() 
    {
        StructureHelper.Generator.GenerateStructure(FilePath, new Point16(X:X, Y:Y), _mod);
        FrameTiles();
    }
    
    public CustomStructure Clone()
    {
        Type type = GetType();
        CustomStructure copy = (CustomStructure)Activator.CreateInstance(type);

        // Get all fields of the class
        FieldInfo[] fields = type.GetFields(BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public);

        foreach (FieldInfo field in fields)
        {
            // Check if the field is a reference type and not null
            if (!field.FieldType.IsValueType && field.GetValue(this) != null)
            {
                // If it's a reference type, create a deep copy
                object fieldValue = field.GetValue(this);
                MethodInfo method = typeof(CustomStructure).GetMethod("Clone").MakeGenericMethod(field.FieldType);
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

public partial class CustomStructureID
{
    public const byte TestHouse = 1;
    public const byte MainHouse = 2;
    public const byte BeachHouse = 3;
    public const byte BridgeTest = 4;

    public static List<short> MakeCostList(short testHouse = -1, short mainHouse = -1, short beachHouse = -1,
        short bridgeTest = -1)
    {
        return [testHouse, mainHouse, beachHouse, bridgeTest];
    }
}
