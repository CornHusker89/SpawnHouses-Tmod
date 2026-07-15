using JetBrains.Annotations;
using Microsoft.Xna.Framework;
using SpawnHouses.Legacy.Helpers;
using Terraria.ModLoader;
using Actions = Terraria.WorldBuilding.Actions;
using Generator = StructureHelper.API.Generator;
using Mod = Terraria.ModLoader.Mod;
using Point16 = Terraria.DataStructures.Point16;
using Shapes = Terraria.WorldBuilding.Shapes;
using WorldUtils = Terraria.WorldBuilding.WorldUtils;

namespace SpawnHouses.Legacy.Structures.StructureParts;

public class ChainConnectPoint : ConnectPoint {
    private readonly Mod _mod = SpawnHousesMod.Instance;
    public byte BranchLength;
    public Bridge ChildBridge;
    public byte GenerateChance;
    public LegacyChainStructure ParentStructure;

    [CanBeNull]
    public LegacyChainStructure ChildStructure;

    [CanBeNull]
    public ChainConnectPoint ChildConnectPoint;

    public bool RootPoint;
    public Seal SealObj;

    public ChainConnectPoint(short xOffset, short yOffset, byte direction, Seal sealObj = null, bool rootPoint = false,
        byte generateChance = GenerateChances.Neutral, Bridge childBridge = null, byte branchLength = 0,
        LegacyChainStructure childStructure = null, LegacyChainStructure parentStructure = null,
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
        byte generateChance, Bridge childBridge, byte branchLength, LegacyChainStructure childStructure,
        LegacyChainStructure parentStructure, ChainConnectPoint childConnectPoint) :
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

    public new ChainConnectPoint Clone() =>
        new(X, Y, XOffset, YOffset, Direction, SealObj, RootPoint, GenerateChance, ChildBridge,
            BranchLength, ChildStructure, ParentStructure, ChildConnectPoint);
}