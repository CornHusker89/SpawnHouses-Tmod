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
using Terraria.GameContent.Drawing;

namespace SpawnHouses.Structures;

public abstract class CustomStructure
{
    public short ID { get; init; }= 0;
    public string FilePath { get; set; } = "Structures/_";
    public ushort StructureXSize { get; set; } = 1;
    public ushort StructureYSize { get; set; } = 1;
    public ushort X { get; set; } = 10;
    public ushort Y { get; set; } = 10;
    public byte Status { get; set; } = global::SpawnHouses.Structures.StructureStatus.NotGenerated;

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

    public virtual void SetPosition(int x, int y)
    {
        X = (ushort)x;
        Y = (ushort)y;
        SetSubstructurePositions();
    }

    protected void FrameTiles()
    {
        int centerX = X + (StructureXSize / 2);
        int centerY = Y + (StructureXSize / 2);
        
        WorldUtils.Gen(new Point(centerX, centerY), new Shapes.Circle(StructureXSize + StructureYSize), Actions.Chain(
            new Actions.SetFrames(),
            new Actions.Custom((i, j, args) =>
            {
                Framing.WallFrame(i, j);
                return true;
            })
        ));
    }
    
    protected void FrameTiles(int centerX, int centerY, int radius)
    {
        WorldUtils.Gen(new Point(centerX, centerY), new Shapes.Circle(radius), Actions.Chain(
            new Actions.SetFrames(),
            new Actions.Custom((i, j, args) =>
            {
                Framing.WallFrame(i, j);
                return true;
            })
        ));
    }
    
    protected static Floor[] CopyFloors(Floor[] floors)
    {
        Floor[] newFloors = (Floor[]) floors.Clone();
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
        _GenerateStructure();
    }

    public virtual void OnFound() {}

    [NoJIT]
    // Generates structure file, nothing else
    public void _GenerateStructure()
    {
        StructureHelper.Generator.GenerateStructure(FilePath, new Point16(X:X, Y:Y), ModInstance.Mod);
        FrameTiles();
    }

    public virtual void ActionOnEachConnectPoint(Action<ConnectPoint> function)
    {
        for (byte direction = 0; direction < 4; direction++)
            foreach (ConnectPoint connectPoint in this.ConnectPoints[direction])
                function(connectPoint);
    }
}
