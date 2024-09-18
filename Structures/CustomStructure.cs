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
    public string FilePath { get; set; }
    public ushort StructureXSize { get; set; }
    public ushort StructureYSize { get; set; }
    public ushort X { get; set; }
    public ushort Y { get; set; }
    public byte Status { get; set; } = global::SpawnHouses.Structures.StructureStatus.NotGenerated;

    public ConnectPoint[][] ConnectPoints { get; set; }
    
    
    protected CustomStructure(String filePath, ushort structureXSize, ushort structureYSize,
        ConnectPoint[][] connectPoints, byte status, ushort x = 1000, ushort y = 1000) 
    {
        FilePath = filePath;
        StructureXSize = structureXSize;
        StructureYSize = structureYSize;
        ConnectPoints = connectPoints;
        Status = status;
        X = x;
        Y = y;
    }
    
    protected virtual void SetSubstructurePositions()
    {
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

    /// <summary>
    /// Calls _GenerateStructure and changes structure status
    /// </summary>
    public virtual void Generate()
    {
        _GenerateStructure();
        Status = StructureStatus.GeneratedButNotFound;
    }

    public virtual void OnFound()
    {
        Status = StructureStatus.GeneratedAndFound;
    }

    /// <summary>
    /// Generates structure file, nothing else
    /// </summary>
    [NoJIT]
    public void _GenerateStructure()
    {
        StructureHelper.Generator.GenerateStructure(FilePath, new Point16(X:X, Y:Y), ModInstance.Mod);
        FrameTiles();
    }

    public void ActionOnEachConnectPoint(Action<ConnectPoint> function)
    {
        for (byte direction = 0; direction < 4; direction++)
            foreach (ConnectPoint connectPoint in this.ConnectPoints[direction])
                function(connectPoint);
    }
}
