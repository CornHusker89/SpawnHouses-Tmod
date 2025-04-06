using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using SpawnHouses.StructureHelper;
using SpawnHouses.Structures.StructureParts;
using Terraria.DataStructures;
using Terraria.ModLoader;
using Terraria.WorldBuilding;
using BoundingBox = SpawnHouses.Structures.StructureParts.BoundingBox;

namespace SpawnHouses.Structures.Bridges;

public class ParabolaBridge : Bridge {
    private readonly Mod _mod = ModContent.GetInstance<SpawnHouses>();

    public readonly string StructureFilePath;
    public readonly ushort StructureHeight;
    public readonly ushort StructureLength;
    public readonly short StructureYOffset;
    private readonly double AttemptSlope;
    private bool BackwardsGeneration;
    private readonly byte BoundingBoxYMargin;

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
        var a = -1 * Math.Abs((maxSlope - 2.0 * (endY - startY) / (endX - startX)) / (endX - startX));
        var b = (endY - startY - a * (endX * endX - startX * startX)) / (endX - startX);
        var c = startY - a * startX * startX - b * startX;
        return Tuple.Create(a, b, c, startX, endX);
    }

    public override void SetPoints(ConnectPoint point1, ConnectPoint point2) {
        Point1 = point1;
        Point2 = point2;

        var boundingBoxesList = new List<BoundingBox>();
        var parabola = _CalculateParabolaBridge(AttemptSlope);
        var a = parabola.Item1;
        var b = parabola.Item2;
        var c = parabola.Item3;
        var startX = parabola.Item4;
        var endX = parabola.Item5;

        uint cumulativeBridgeStructureY = 0;
        ushort tileCount = 0;

        for (var currentX = (ushort)(startX + 1); currentX < endX; currentX++) {
            cumulativeBridgeStructureY +=
                (uint)Math.Floor(a * currentX * currentX + b * currentX + c + StructureYOffset);
            if ((tileCount - 1) % StructureLength == 0) {
                var bridgeStructureX = (ushort)(currentX - StructureLength + 1);
                var bridgeStructureY = (ushort)(cumulativeBridgeStructureY / StructureLength);

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
        var a = parabola.Item1;
        var b = parabola.Item2;
        var c = parabola.Item3;
        var startX = parabola.Item4;
        var endX = parabola.Item5;

        uint cumulativeBridgeStructureY = 0;
        ushort tileCount = 0;

        for (var currentX = (ushort)(startX + 1); currentX < endX; currentX++) {
            cumulativeBridgeStructureY +=
                (uint)Math.Floor(a * currentX * currentX + b * currentX + c + StructureYOffset);
            if ((tileCount - 1) % StructureLength == 0) {
                var bridgeStructureX = (ushort)(currentX - StructureLength + 1);
                var bridgeStructureY = (ushort)(cumulativeBridgeStructureY / StructureLength);
                Generator.GenerateStructure(StructureFilePath, new Point16(bridgeStructureX, bridgeStructureY), _mod);
                cumulativeBridgeStructureY = 0;
            }

            tileCount++;
        }

        var centerX = (ushort)((Point1.X + Point2.X) / 2);
        var centerY = (ushort)((Point1.Y + Point2.Y) / 2);
        var radius = Math.Abs(Point1.X - Point2.X) + 4;
        WorldUtils.Gen(new Point(centerX, centerY), new Shapes.Circle(radius), new Actions.SetFrames());
    }

    public override ParabolaBridge Clone() {
        var type = GetType();
        var bridge = (ParabolaBridge)Activator.CreateInstance(type)!;
        //if (Point1 is not null) bridge.Point1 = Point1;
        return bridge;
    }


    // --- bridge presets --
    public class TestBridgeLarge : ParabolaBridge {
        public TestBridgeLarge() : base("Structures/StructureFiles/woodBridge",
            2, 3, -2, 0.4, 2, 40, 40, -27, 27, 2, 1, false) {
        }
    }

    public class TestBridgeLargeAltGen : ParabolaBridge {
        public TestBridgeLargeAltGen() : base("Structures/StructureFiles/woodBridge",
            2, 3, -2, 0.4, 2, -42, -42, -27, 27, 2, 1, true) {
        }
    }


    public class TestBridgeSmall : ParabolaBridge {
        public TestBridgeSmall() : base("Structures/StructureFiles/woodBridge",
            2, 3, -2, 0.4, 2, 35, 35, -27, 27, 2, 1, false) {
        }
    }

    public class TestBridgeSmallAltGen : ParabolaBridge {
        public TestBridgeSmallAltGen() : base("Structures/StructureFiles/woodBridge",
            2, 3, -2, 0.4, 2, -37, -37, -27, 27, 2, 1, true) {
        }
    }
}