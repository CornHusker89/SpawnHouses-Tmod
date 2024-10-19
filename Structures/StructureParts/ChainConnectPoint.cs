using System;
using System.Collections.Generic;
using Terraria.WorldBuilding;
using Terraria.DataStructures;
using SpawnHouses.Structures.StructureParts;
using Terraria.ModLoader;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;

namespace SpawnHouses.Structures.StructureParts;


public class ChainConnectPoint : ConnectPoint 
{
    private readonly Mod _mod = ModContent.GetInstance<SpawnHouses>();

    public bool RootPoint;
    public byte GenerateChance;
    public Bridge ChildBridge;
    public byte BranchLength;
    public Seal SealObj;
    public CustomChainStructure ChildStructure;
    public CustomChainStructure ParentStructure;
    public ChainConnectPoint ChildConnectPoint;
    
    public ChainConnectPoint(short xOffset, short yOffset, byte direction, Seal sealObj = null, bool rootPoint = false,
        byte generateChance = GenerateChances.Neutral, Bridge childBridge = null, byte branchLength = 0,
        CustomChainStructure childStructure = null, CustomChainStructure parentStructure = null, ChainConnectPoint childConnectPoint = null) :
        base(xOffset, yOffset, direction)
    {
        SealObj = sealObj;
        ChildBridge = childBridge;
        BranchLength = branchLength;
        GenerateChance = generateChance;
        ChildStructure = childStructure;
        ParentStructure = parentStructure;
        ChildConnectPoint = childConnectPoint;
        RootPoint = rootPoint;
    }
    
    // for cloning
    private ChainConnectPoint(ushort x, ushort y, short xOffset, short yOffset, byte direction, Seal sealObj, bool rootPoint,
        byte generateChance, Bridge childBridge, byte branchLength, CustomChainStructure childStructure, CustomChainStructure parentStructure, ChainConnectPoint childConnectPoint) :
        base(xOffset, yOffset, direction)
    {
        X = x;
        Y = y;
        
        Direction = direction;
        SealObj = sealObj;
        ChildBridge = childBridge;
        BranchLength = branchLength;
        GenerateChance = generateChance;
        ChildStructure = childStructure;
        ParentStructure = parentStructure;
        ChildConnectPoint = childConnectPoint;
        RootPoint = rootPoint;
    }
    
    [NoJIT]
    public void GenerateSeal()
    {
        if (SealObj != null)
        {
            StructureHelper.Generator.GenerateStructure(SealObj.FilePath, new Point16(X + SealObj.XOffset, Y + SealObj.YOffset), _mod);
            WorldUtils.Gen(new Point(X, Y), new Shapes.Circle(10), new Actions.SetFrames());
        }
    }
    
    public new ChainConnectPoint Clone()
    {
        return new ChainConnectPoint(X, Y, XOffset, YOffset, Direction, SealObj, RootPoint, GenerateChance, ChildBridge, BranchLength, ChildStructure, ParentStructure, ChildConnectPoint);
    }
}