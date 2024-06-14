using System;
using Terraria;
using Terraria.ID;
using Terraria.WorldBuilding;
using Terraria.DataStructures;
using Microsoft.Xna.Framework;

namespace SpawnHouses.Structures.StructureParts;

public class ChainConnectPoint : ConnectPoint {
    public bool RootPoint { get; set; }
    public Bridge ChildBridge { get; set; }
    public CustomChainStructure ChildStructure { get; set; }
    public ChainConnectPoint ChildConnectPoint { get; set; }
    
    public ChainConnectPoint(short xOffset, short yOffset, bool facingLeft = true, bool rootPoint = false,
        Bridge childBridge = null, CustomChainStructure childStructure = null, ChainConnectPoint childConnectPoint = null) :
        base(xOffset, yOffset, facingLeft)
    {
        XOffset = xOffset;
        YOffset = yOffset;
        FacingLeft = facingLeft;
        ChildBridge = childBridge;
        ChildStructure = childStructure;
        ChildConnectPoint = childConnectPoint;
        RootPoint = rootPoint;
    }
    
    // for cloning
    private ChainConnectPoint(ushort x, ushort y, short xOffset, short yOffset, bool facingLeft, bool rootPoint,
        Bridge childBridge, CustomChainStructure childStructure, ChainConnectPoint childConnectPoint) :
        base(xOffset, yOffset, facingLeft)
    {
        FacingLeft = facingLeft;
        ChildBridge = childBridge;
        X = x;
        Y = y;
        XOffset = xOffset;
        YOffset = yOffset;
        ChildStructure = childStructure;
        ChildConnectPoint = childConnectPoint;
        RootPoint = rootPoint;
    }
    
    public new ChainConnectPoint Clone()
    {
        return new ChainConnectPoint(X, Y, XOffset, YOffset, FacingLeft, RootPoint, ChildBridge, ChildStructure, ChildConnectPoint);
    }
}