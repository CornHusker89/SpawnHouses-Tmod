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
    public virtual ConnectPoint[] ConnectPoints { get; set; } = Array.Empty<ConnectPoint>();
    
    protected CustomStructure() {}

    protected virtual void SetSubstructurePositions()
    {
        foreach (Floor floor in Floors)
            floor.SetPosition(mainStructureX: X, mainStructureY: Y);
        foreach (ConnectPoint connectPoint in ConnectPoints)
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
        
        WorldUtils.Gen(new Point(centerX, centerY), new Shapes.Circle(Convert.ToInt32(StructureXSize + StructureYSize) ), new Actions.SetFrames());
    }
    
    protected void FrameTiles(int centerX, int centerY, int radius)    
    {
        WorldUtils.Gen(new Point(centerX, centerY), new Shapes.Circle(radius), new Actions.SetFrames());
    }

    [NoJIT]
    public void GenerateStructure()
    {
        StructureHelper.Generator.GenerateStructure(FilePath, new Point16(X:X, Y:Y), _mod);
        FrameTiles();
    }
}
