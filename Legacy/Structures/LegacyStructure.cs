using System;
using Microsoft.Xna.Framework;
using SpawnHouses.Core;
using SpawnHouses.Core.DataStructures;
using SpawnHouses.Core.Tiles;
using SpawnHouses.Legacy.Helpers;
using SpawnHouses.Legacy.Structures.StructureParts;
using StructureHelper.API;
using Terraria;
using Terraria.DataStructures;
using Terraria.ModLoader;
using Terraria.WorldBuilding;

namespace SpawnHouses.Legacy.Structures;

public abstract class LegacyStructure { //: IBoundingBox, StructureRoot {
    public TileBox BoundingBox { get; protected set; }
    
    public ConnectPoint[][] ConnectPoints;
    public string FilePath;
    public StructureType Id2;
    public byte Status;

    protected LegacyStructure(string filePath, ushort structureXSize, ushort structureYSize,
        ConnectPoint[][] connectPoints, byte status, ushort x, ushort y, bool isChainStructure = false) {
        BoundingBox = new TileBox(x, y, structureXSize, structureYSize);
        
        FilePath = filePath;
        Status = status;

        if (Enum.TryParse(GetType().Name, out StructureType result))
            Id2 = result;
        else
            throw new Exception($"StructureId of {ToString()} not found");

        if (!isChainStructure) {
            ConnectPoints = connectPoints;
            SetSubstructurePositions();
        }
    }

    protected virtual void SetSubstructurePositions() {
        for (byte direction = 0; direction < 4; direction++)
            foreach (ConnectPoint connectPoint in ConnectPoints[direction])
                connectPoint.SetPosition(BoundingBox.Left, BoundingBox.Top);
    }

    public virtual void SetPosition(int x, int y) {
        BoundingBox = BoundingBox.SetPosition(x, y);
        SetSubstructurePositions();
    }

    public void FrameTiles() {
        WorldUtils.Gen(BoundingBox.CenterPoint, new Shapes.Circle(BoundingBox.Width + BoundingBox.Height), Actions.Chain(
            new Actions.SetFrames(),
            new Actions.Custom((i, j, _) => {
                Framing.WallFrame(i, j);
                return true;
            })
        ));
    }

    public void FrameTiles(int centerX, int centerY, int radius) {
        WorldUtils.Gen(new Point(centerX, centerY), new Shapes.Circle(radius), Actions.Chain(
            new Actions.SetFrames(),
            new Actions.Custom((i, j, _) => {
                Framing.WallFrame(i, j);
                return true;
            })
        ));
    }

    protected static ConnectPoint[][] CopyConnectPoints(ConnectPoint[][] connectPoints) {
        var newConnectPoints = (ConnectPoint[][])connectPoints.Clone();

        for (byte direction = 0; direction < 4; direction++) {
            newConnectPoints[direction] = (ConnectPoint[])connectPoints[direction].Clone();
            for (byte j = 0; j < newConnectPoints[direction].Length; j++)
                newConnectPoints[direction][j] = newConnectPoints[direction][j].Clone();
        }

        return newConnectPoints;
    }

    /// <summary>
    ///     Calls _GenerateStructure and changes structure status
    /// </summary>
    /// <param name="bare"></param>
    public virtual void Generate(bool bare = false) {
        _GenerateStructure();
        Status = StructureStatus.GeneratedButNotFound;
    }

    public string Name { get; }
    public ushort Id { get; }
    public StructureTilemap Tilemap { get; }
    public EntryPoint[] EntryPoints { get; }
    public bool HasBeenFound { get; }

    /// <summary>
    ///     Should return true when the player is close to the structure, whatever that means for each structure
    /// </summary>
    /// <param name="playerPos"></param>
    /// <returns></returns>
    public virtual bool IsFound(Point16 playerPos) => true;

    /// <summary>
    ///     Changes structure status
    /// </summary>
    public virtual void OnFound() {
        Status = StructureStatus.GeneratedAndFound;
    }

    public void LoadTilemap() {
        throw new NotImplementedException();
    }

    public void ApplyTilemap() {
        throw new NotImplementedException();
    }

    /// <summary>
    ///     Generates structure file, nothing else
    /// </summary>
    [NoJIT]
    public void _GenerateStructure() {
        Generator.GenerateStructure(FilePath, BoundingBox.TopLeftPoint16, SpawnHousesMod.Instance);
        FrameTiles();
    }

    public virtual LegacyStructure Clone() => (LegacyStructure)Activator.CreateInstance(GetType(), BoundingBox.TopLeftPoint16, Status)!;

    public void ActionOnEachConnectPoint(Action<ConnectPoint> function) {
        for (byte direction = 0; direction < 4; direction++)
            foreach (ConnectPoint connectPoint in ConnectPoints[direction])
                function(connectPoint);
    }
}