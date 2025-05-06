using System;
using Microsoft.Xna.Framework;
using SpawnHouses.StructureHelper;
using SpawnHouses.Structures.StructureParts;
using Terraria;
using Terraria.DataStructures;
using Terraria.ModLoader;
using Terraria.WorldBuilding;

namespace SpawnHouses.Structures;

public abstract class CustomStructure {
    public ConnectPoint[][] ConnectPoints;
    public string FilePath;
    public ushort ID;
    public byte Status;
    public ushort StructureXSize;
    public ushort StructureYSize;
    public ushort X;
    public ushort Y;


    protected CustomStructure(string filePath, ushort structureXSize, ushort structureYSize,
        ConnectPoint[][] connectPoints, byte status, ushort x, ushort y, bool isChainStructure = false) {
        FilePath = filePath;
        StructureXSize = structureXSize;
        StructureYSize = structureYSize;
        Status = status;
        X = x;
        Y = y;

        if (Enum.TryParse(GetType().Name, out StructureID result))
            ID = (ushort)result;
        else
            throw new Exception($"StructureID of {ToString()} not found");

        if (!isChainStructure) {
            ConnectPoints = connectPoints;
            SetSubstructurePositions();
        }
    }

    protected virtual void SetSubstructurePositions() {
        for (byte direction = 0; direction < 4; direction++)
            foreach (ConnectPoint connectPoint in ConnectPoints[direction])
                connectPoint.SetPosition(X, Y);
    }

    public virtual void SetPosition(int x, int y) {
        X = (ushort)x;
        Y = (ushort)y;
        SetSubstructurePositions();
    }

    public void FrameTiles() {
        int centerX = X + StructureXSize / 2;
        int centerY = Y + StructureXSize / 2;

        WorldUtils.Gen(new Point(centerX, centerY), new Shapes.Circle(StructureXSize + StructureYSize), Actions.Chain(
            new Actions.SetFrames(),
            new Actions.Custom((i, j, args) => {
                Framing.WallFrame(i, j);
                return true;
            })
        ));
    }

    public void FrameTiles(int centerX, int centerY, int radius) {
        WorldUtils.Gen(new Point(centerX, centerY), new Shapes.Circle(radius), Actions.Chain(
            new Actions.SetFrames(),
            new Actions.Custom((i, j, args) => {
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

    /// <summary>
    ///     Changes structure status
    /// </summary>
    public virtual void OnFound() {
        Status = StructureStatus.GeneratedAndFound;
    }

    /// <summary>
    ///     Generates structure file, nothing else
    /// </summary>
    [NoJIT]
    public void _GenerateStructure() {
        Generator.GenerateStructure(FilePath, new Point16(X, Y), ModInstance.Mod);
        FrameTiles();
    }

    public virtual CustomStructure Clone() {
        Type type = GetType();
        return (CustomStructure)Activator.CreateInstance(type, X, Y, Status)!;
    }

    public void ActionOnEachConnectPoint(Action<ConnectPoint> function) {
        for (byte direction = 0; direction < 4; direction++)
            foreach (ConnectPoint connectPoint in ConnectPoints[direction])
                function(connectPoint);
    }
}