using Microsoft.Xna.Framework;
using SpawnHouses.StructureHelper;
using Terraria.DataStructures;
using Terraria.ModLoader;
using Terraria.WorldBuilding;

namespace SpawnHouses.Structures.StructureParts;

public class ChainConnectPoint : ConnectPoint {
    private readonly Mod _mod = ModContent.GetInstance<SpawnHouses>();
    public byte BranchLength;
    public Bridge ChildBridge;
    public byte GenerateChance;
    public CustomChainStructure ParentStructure;

    public bool RootPoint;
    public Seal SealObj;

    public ChainConnectPoint(short xOffset, short yOffset, byte direction, Seal sealObj = null, bool rootPoint = false,
        byte generateChance = GenerateChances.Neutral, Bridge childBridge = null, byte branchLength = 0,
        CustomChainStructure childStructure = null, CustomChainStructure parentStructure = null,
        ChainConnectPoint childConnectPoint = null) :
        base(xOffset, yOffset, direction) {
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
    private ChainConnectPoint(ushort x, ushort y, short xOffset, short yOffset, byte direction, Seal sealObj,
        bool rootPoint,
        byte generateChance, Bridge childBridge, byte branchLength, CustomChainStructure childStructure,
        CustomChainStructure parentStructure, ChainConnectPoint childConnectPoint) :
        base(xOffset, yOffset, direction) {
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
    public void GenerateSeal() {
        if (SealObj != null) {
            Generator.GenerateStructure(SealObj.FilePath, new Point16(X + SealObj.XOffset, Y + SealObj.YOffset), _mod);
            WorldUtils.Gen(new Point(X, Y), new Shapes.Circle(10), new Actions.SetFrames());
        }
    }

    public new ChainConnectPoint Clone() {
        return new ChainConnectPoint(X, Y, XOffset, YOffset, Direction, SealObj, RootPoint, GenerateChance, ChildBridge,
            BranchLength, ChildStructure, ParentStructure, ChildConnectPoint);
    }

#nullable enable

    public CustomChainStructure? ChildStructure;
    public ChainConnectPoint? ChildConnectPoint;
}