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
    public virtual BoundingBox BoundingBox { get; set; }
    
    public sbyte Cost { get; set; } = -1;
    public ushort X { get; set; } = 10;
    public ushort Y { get; set; } = 10;

    public Floor[] Floors { get; set; } = Array.Empty<Floor>();
    public ConnectPoint[] ConnectPoints { get; set; } = Array.Empty<ConnectPoint>();

    // you're not really intended to make a base customStructure, so this is private
    private CustomStructure(String filePath, ushort structureXSize, ushort structureYSize, BoundingBox boundingBox,
        sbyte cost, ushort x, ushort y, Floor[] floors, ConnectPoint[] connectPoints)
    {
        FilePath = FilePath;
        StructureXSize = structureXSize;
        StructureYSize = structureYSize;
        BoundingBox = boundingBox;
        Cost = cost;
        X = x;
        Y = y;
        Floors = floors;
        ConnectPoints = connectPoints;
    }
    
    protected CustomStructure() {}

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
    
    public virtual CustomStructure Clone()
    {
        return new CustomStructure
        (
            FilePath = FilePath,
            StructureXSize = StructureXSize,
            StructureYSize = StructureYSize,
            BoundingBox = BoundingBox,
            Cost = Cost,
            X = X,
            Y = Y,
            Floors = (Floor[])Floors.Clone(),
            ConnectPoints = (ConnectPoint[])ConnectPoints.Clone()
        );
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
