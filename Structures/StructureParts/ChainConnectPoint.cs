using System;
using Terraria;
using Terraria.ID;
using Terraria.WorldBuilding;
using Terraria.DataStructures;
using Microsoft.Xna.Framework;

namespace SpawnHouses.Structures.StructureParts;

public class ChainConnectPoint : ConnectPoint {
    public Bridge ChildBridge { get; set; }
    public CustomChainStructure ChildStructure { get; set; }
    
    public ChainConnectPoint(short xOffset, short yOffset, bool facingLeft = true,
        Bridge childBridge = null, CustomChainStructure childStructure = null) : base(xOffset, yOffset, facingLeft)
    {
        _XOffset = xOffset;
        _YOffset = yOffset;
        FacingLeft = facingLeft;
        ChildBridge = childBridge;
        ChildStructure = childStructure;
    }
    
    // for cloning
    private ChainConnectPoint(bool facingLeft, ushort x, ushort y, short xOffset, short yOffset,
    Bridge childBridge, CustomChainStructure childStructure) : base(xOffset, yOffset, facingLeft)
    {
        FacingLeft = facingLeft;
        ChildBridge = childBridge;
        X = x;
        Y = y;
        _XOffset = xOffset;
        _YOffset = yOffset;
        ChildStructure = childStructure;
    }
    
    public new ChainConnectPoint Clone()
    {
        return new ChainConnectPoint(FacingLeft, X, Y, _XOffset, _YOffset, ChildBridge, ChildStructure);
    }
}