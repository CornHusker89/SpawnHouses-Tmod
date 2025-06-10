using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using SpawnHouses.Helpers;
using SpawnHouses.Types;
using StructureHelper.API;
using Terraria.DataStructures;
using Terraria.ModLoader;
using Terraria.WorldBuilding;
using BoundingBox = SpawnHouses.Types.BoundingBox;

namespace SpawnHouses.Structures.Bridges;

public class ParabolaBridge : Bridge {
    private readonly Mod _mod = ModContent.GetInstance<SpawnHousesMod>();
    private readonly double AttemptSlope;
    private readonly byte BoundingBoxYMargin;

    public readonly string StructureFilePath;
    public readonly ushort StructureHeight;
    public readonly ushort StructureLength;
    public readonly short StructureYOffset;
    private bool BackwardsGeneration;

    public ParabolaBridge(string structureFilePath, ushort structureLength, ushort structureHeight,
        short structureYOffset, double attemptSlope, byte boundingBoxYMargin,
        short minDeltaX, short maxDeltaX, short minDeltaY, short maxDeltaY, sbyte deltaXMultiple, sbyte deltaYMultiple,
        bool backwardsGeneration,
        ConnectPoint point1 = null, ConnectPoint point2 = null)
        :
        base(backwardsGeneration ? [Directions.Left, Directions.Right] : [Directions.Right, Directions.Left],
            minDeltaX, maxDeltaX, minDeltaY, maxDeltaY, deltaXMultiple, deltaYMultiple, point1, point2) {
        StructureFilePath = structureFilePath;
        StructureLength = structureLength;
        StructureHeight = structureHeight;
        StructureYOffset = structureYOffset;
        AttemptSlope = attemptSlope;
        BoundingBoxYMargin = boundingBoxYMargin;
        BackwardsGeneration = backwardsGeneration;

        if (point1 != null && point2 != null)
            SetPoints(point1, point2);
    }

    private Tuple<double, double, double, ushort, ushort> _CalculateParabolaBridge(double maxSlope) {
        ushort startX;
        ushort endX;
        ushort startY;
        ushort endY;
        if (Point1.X < Point2.X) {
            startX = Point1.X;
            endX = Point2.X;
            startY = Point1.Y;
            endY = Point2.Y;
        }
        else {
            startX = Point2.X;
            endX = Point1.X;
            startY = Point2.Y;
            endY = Point1.Y;
        }

        // straight up no clue how this works but make coefficients for ax^2 + bx + c parabola
        double a = -1 * Math.Abs((maxSlope - 2.0 * (endY - startY) / (endX - startX)) / (endX - startX));
        double b = (endY - startY - a * (endX * endX - startX * startX)) / (endX - startX);
        double c = startY - a * startX * startX - b * startX;
        return Tuple.Create(a, b, c, startX, endX);
    }

    public override void SetPoints(ConnectPoint point1, ConnectPoint point2) {
        Point1 = point1;
        Point2 = point2;

        var boundingBoxesList = new List<BoundingBox>();
        var parabola = _CalculateParabolaBridge(AttemptSlope);
        double a = parabola.Item1;
        double b = parabola.Item2;
        double c = parabola.Item3;
        ushort startX = parabola.Item4;
        ushort endX = parabola.Item5;

        uint cumulativeBridgeStructureY = 0;
        ushort tileCount = 0;

        for (ushort currentX = (ushort)(startX + 1); currentX < endX; currentX++) {
            cumulativeBridgeStructureY +=
                (uint)Math.Floor(a * currentX * currentX + b * currentX + c + StructureYOffset);
            if ((tileCount - 1) % StructureLength == 0) {
                ushort bridgeStructureX = (ushort)(currentX - StructureLength + 1);
                ushort bridgeStructureY = (ushort)(cumulativeBridgeStructureY / StructureLength);

                boundingBoxesList.Add(
                    new BoundingBox(
                        bridgeStructureX,
                        bridgeStructureY - BoundingBoxYMargin,
                        bridgeStructureX + StructureLength - 1,
                        bridgeStructureY + StructureHeight + BoundingBoxYMargin - 1)
                );

                cumulativeBridgeStructureY = 0;
            }

            tileCount++;
        }

        BoundingBoxes = boundingBoxesList.ToArray();
    }


    [NoJIT]
    public override void Generate() {
        if (Point1 == null || Point2 == null)
            throw new Exception("bridge point 1 or 2 is null");

        if ((Math.Abs(Point1.X - Point2.X) - 1) % StructureLength != 0)
            throw new Exception(
                $"Bridge length cannot be resolved with the given BridgeStructure's length, p1: {Point1.X}, p2 {Point2.X}, distance: {Math.Abs(Point1.X - Point2.X) - 1}, structureLength: {StructureLength}");

        var parabola = _CalculateParabolaBridge(AttemptSlope);
        double a = parabola.Item1;
        double b = parabola.Item2;
        double c = parabola.Item3;
        ushort startX = parabola.Item4;
        ushort endX = parabola.Item5;

        uint cumulativeBridgeStructureY = 0;
        ushort tileCount = 0;

        for (ushort currentX = (ushort)(startX + 1); currentX < endX; currentX++) {
            cumulativeBridgeStructureY +=
                (uint)Math.Floor(a * currentX * currentX + b * currentX + c + StructureYOffset);
            if ((tileCount - 1) % StructureLength == 0) {
                ushort bridgeStructureX = (ushort)(currentX - StructureLength + 1);
                ushort bridgeStructureY = (ushort)(cumulativeBridgeStructureY / StructureLength);
                Generator.GenerateStructure(StructureFilePath, new Point16(bridgeStructureX, bridgeStructureY), _mod);
                cumulativeBridgeStructureY = 0;
            }

            tileCount++;
        }

        ushort centerX = (ushort)((Point1.X + Point2.X) / 2);
        ushort centerY = (ushort)((Point1.Y + Point2.Y) / 2);
        int radius = Math.Abs(Point1.X - Point2.X) + 4;
        WorldUtils.Gen(new Point(centerX, centerY), new Shapes.Circle(radius), new Actions.SetFrames());
    }

    public override ParabolaBridge Clone() {
        Type type = GetType();
        ParabolaBridge bridge = (ParabolaBridge)Activator.CreateInstance(type)!;
        //if (Point1 is not null) bridge.Point1 = Point1;
        return bridge;
    }


    // --- bridge presets --
    public class TestBridgeLarge : ParabolaBridge {
        public TestBridgeLarge() : base("Assets/StructureFiles/woodBridge.shstruct",
            2, 3, -2, 0.4, 2, 40, 40, -27, 27, 2, 1, false) {
        }
    }

    public class TestBridgeLargeAltGen : ParabolaBridge {
        public TestBridgeLargeAltGen() : base("Assets/StructureFiles/woodBridge.shstruct",
            2, 3, -2, 0.4, 2, -42, -42, -27, 27, 2, 1, true) {
        }
    }


    public class TestBridgeSmall : ParabolaBridge {
        public TestBridgeSmall() : base("Assets/StructureFiles/woodBridge.shstruct",
            2, 3, -2, 0.4, 2, 35, 35, -27, 27, 2, 1, false) {
        }
    }

    public class TestBridgeSmallAltGen : ParabolaBridge {
        public TestBridgeSmallAltGen() : base("Assets/StructureFiles/woodBridge.shstruct",
            2, 3, -2, 0.4, 2, -37, -37, -27, 27, 2, 1, true) {
        }
    }
}