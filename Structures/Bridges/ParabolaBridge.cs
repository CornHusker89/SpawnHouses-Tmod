using System;
using System.Collections.Generic;
using Terraria.WorldBuilding;
using Terraria.DataStructures;
using SpawnHouses.Structures.StructureParts;
using Terraria.ModLoader;
using Microsoft.Xna.Framework;
using Terraria;
using BoundingBox = SpawnHouses.Structures.StructureParts.BoundingBox;

namespace SpawnHouses.Structures.Bridges;

public class ParabolaBridge : Bridge
{
    private readonly Mod _mod = ModContent.GetInstance<SpawnHouses>();
    
    public readonly string StructureFilePath;
    public readonly ushort StructureLength;
    public readonly ushort StructureHeight;
    public readonly short StructureYOffset;
    private double AttemptSlope;
    private byte BoundingBoxYMargin;
    
    public ParabolaBridge(string structureFilePath, ushort structureLength, ushort structureHeight, short structureYOffset, double attemptSlope, byte boundingBoxYMargin,
        short minDeltaX, short maxDeltaX, short minDeltaY, short maxDeltaY, sbyte deltaXMultiple, sbyte deltaYMultiple, 
        ConnectPoint point1 = null, ConnectPoint point2 = null) 
        : 
        base([Directions.Left, Directions.Right], minDeltaX, maxDeltaX, minDeltaY, maxDeltaY, 
            deltaXMultiple, deltaYMultiple, point1, point2)
    {
        StructureFilePath = structureFilePath;
        StructureLength = structureLength;
        StructureHeight = structureHeight;
        StructureYOffset = structureYOffset;
        AttemptSlope = attemptSlope;
        BoundingBoxYMargin = boundingBoxYMargin;
        
        if (point1 != null && point2 != null)
            SetPoints(point1, point2);
    }
    
    private Tuple<double, double, double, ushort, ushort> _CalculateParabolaBridge(double maxSlope)
    {
        ushort startX;
        ushort endX;
        ushort startY;
        ushort endY;
        if (Point1.X < Point2.X)
        {
            startX = Point1.X;
            endX = Point2.X;
            startY = Point1.Y;
            endY = Point2.Y;
        }
        else
        {
            startX = Point2.X;
            endX = Point1.X;
            startY = Point2.Y;
            endY = Point1.Y;
        }

        // straight up no clue how this works but make coefficients for ax^2 + bx + c parabola
        double a = -1 * Math.Abs((maxSlope - (2.0 * (endY - startY) / (endX - startX))) / (endX - startX));
        double b = (endY - startY - a * (endX * endX - startX * startX)) / (endX - startX);
        double c = startY - a * startX * startX - b * startX;
        return Tuple.Create(a, b, c, startX, endX);
    }
    
    public override void SetPoints(ConnectPoint point1, ConnectPoint point2)
    {
        Point1 = point1;
        Point2 = point2;

        List<BoundingBox> boundingBoxesList = new List<BoundingBox>(); 
        var parabola = _CalculateParabolaBridge(AttemptSlope);
        double a = parabola.Item1;
        double b = parabola.Item2;
        double c = parabola.Item3;
        ushort startX = parabola.Item4;
        ushort endX = parabola.Item5;
        
        uint cumulativeBridgeStructureY = 0;
        ushort tileCount = 0;

        for (ushort currentX = (ushort)(startX + 1); currentX < endX; currentX++)
        {
            cumulativeBridgeStructureY += (uint)Math.Floor(a * currentX * currentX + b * currentX + c + StructureYOffset);
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
    public override void Generate()
    {
        if (Point1 == null || Point2 == null)
            throw new Exception("bridge point 1 or 2 is null");
        
        if ((Math.Abs(Point1.X - Point2.X) - 1) % StructureLength != 0)
            throw new Exception($"Bridge length cannot be resolved with the given BridgeStructure's length, p1: {Point1.X}, p2 {Point2.X}, distance: {Math.Abs(Point1.X - Point2.X) - 1}, structureLength: {StructureLength}");

        var parabola = _CalculateParabolaBridge(AttemptSlope);
        double a = parabola.Item1;
        double b = parabola.Item2;
        double c = parabola.Item3;
        ushort startX = parabola.Item4;
        ushort endX = parabola.Item5;
        
        uint cumulativeBridgeStructureY = 0;
        ushort tileCount = 0;

        for (ushort currentX = (ushort)(startX + 1); currentX < endX; currentX++)
        {
            cumulativeBridgeStructureY += (uint)Math.Floor(a * currentX * currentX + b * currentX + c + StructureYOffset);
            if ((tileCount - 1) % StructureLength == 0) {
                ushort bridgeStructureX = (ushort)(currentX - StructureLength + 1);
                ushort bridgeStructureY = (ushort)(cumulativeBridgeStructureY / StructureLength);
                StructureHelper.Generator.GenerateStructure(StructureFilePath, new Point16(X:bridgeStructureX, Y:bridgeStructureY), _mod);
                cumulativeBridgeStructureY = 0;
            }
            tileCount++;
        }

        ushort centerX = (ushort)((Point1.X + Point2.X) / 2);
        ushort centerY = (ushort)((Point1.Y + Point2.Y) / 2);
        int radius = Math.Abs(Point1.X - Point2.X) + 4;
        WorldUtils.Gen(new Point(centerX, centerY), new Shapes.Circle(radius), new Actions.SetFrames());
    }

    public override ParabolaBridge Clone()
    {
        return new ParabolaBridge(StructureFilePath, StructureLength, StructureHeight, StructureYOffset, AttemptSlope, BoundingBoxYMargin,
            MinDeltaX, MaxDeltaX, MinDeltaY, MaxDeltaY, DeltaXMultiple, DeltaYMultiple, Point1, Point2);
    }
    
    
    
    
    // --- bridge presets --
    public class TestBridge : ParabolaBridge
    {
        public TestBridge() : base("Structures/StructureFiles/woodBridge", 
            2, 3, -2, 0.4, 2, 10, 12, 0, 12, 2, 1) {}
    } 
}