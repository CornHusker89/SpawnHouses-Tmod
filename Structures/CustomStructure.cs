using System;
using System.Collections;
using System.Collections.Generic;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.DataStructures;
using Microsoft.Xna.Framework;
using Terraria.WorldBuilding;

using SpawnHouses.Structures.StructureParts;
using SpawnHouses.Structures;

namespace SpawnHouses.Structures;

public class CustomStructure {
    private readonly Mod _mod = ModContent.GetInstance<SpawnHouses>();
    
    public virtual string FilePath { get; set; } = "Structures/_";
    public virtual ushort StructureXSize { get; set; } = 1;
    public virtual ushort StructureYSize { get; set; } = 1;
    
    public ushort X { get; set; } = 10;
    public ushort Y { get; set; } = 10;

    public Floor[] Floors { get; set; } = Array.Empty<Floor>();

    public ConnectPoint[][] ConnectPoints { get; set; } = [ [], [], [], [] ];
    
    
    protected CustomStructure() {}
    
    protected virtual void SetSubstructurePositions()
    {
        foreach (var floor in Floors)
            floor.SetPosition(mainStructureX: X, mainStructureY: Y);
        for (byte direction = 0; direction < 4; direction++)
            foreach (var connectPoint in ConnectPoints[direction])
                connectPoint.SetPosition(mainStructureX: X, mainStructureY: Y);
    }

    public virtual void SetPosition(ushort x, ushort y)
    {
        X = x;
        Y = y;
        SetSubstructurePositions();
    }

    protected void FrameTiles()
    {
        int centerX = X + (StructureXSize / 2);
        int centerY = Y + (StructureXSize / 2);
        
        WorldUtils.Gen(new Point(centerX, centerY), new Shapes.Circle(StructureXSize + StructureYSize ), new Actions.SetFrames());
    }
    
    protected void FrameTiles(int centerX, int centerY, int radius)    
    {
        WorldUtils.Gen(new Point(centerX, centerY), new Shapes.Circle(radius), new Actions.SetFrames());
    }
    
    protected static Floor[] CopyFloors(Floor[] floors)
    {
        Floor[] newFloors = (Floor[])floors.Clone();
        for (byte i = 0; i < newFloors.Length; i++)
            newFloors[i] = newFloors[i].Clone();
        return newFloors;
    }
    
    protected static ConnectPoint[][] CopyConnectPoints(ConnectPoint[][] connectPoints)
    {
        ConnectPoint[][] newConnectPoints = (ConnectPoint[][])connectPoints.Clone();
        
        for (byte direction = 0; direction < 4; direction++)
        {
            newConnectPoints[direction] = (ConnectPoint[]) connectPoints[direction].Clone();
            for (byte j = 0; j < newConnectPoints[direction].Length; j++)
                newConnectPoints[direction][j] = newConnectPoints[direction][j].Clone();
        }
        return newConnectPoints;
    }

    public virtual void Generate()
    {
        throw new Exception("Generate() was called on a CustomStructure, this does not do anything and should never happen");
    }

    [NoJIT]
    // Generates structure file, nothing else
    public void _GenerateStructure(bool reverse = false)
    {
        String reverseString = "";
        if (reverse)
            reverseString = "_r";
        
        StructureHelper.Generator.GenerateStructure(FilePath + reverseString, new Point16(X:X, Y:Y), _mod);
        FrameTiles();
    }
}
