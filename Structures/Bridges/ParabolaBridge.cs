using Microsoft.CodeAnalysis;
using System;
using Terraria.WorldBuilding;
using Terraria.DataStructures;
using SpawnHouses.Structures.StructureParts;
using Terraria.ModLoader;
using Microsoft.Xna.Framework;

namespace SpawnHouses.Structures.Bridges;

public class ParabolaBridge : Bridge
{
    private readonly Mod _mod = ModContent.GetInstance<SpawnHouses>();
    
    private double AttemptSlope;
    
    public ParabolaBridge(string structureFilePath, ushort structureLength,
        short structureYOffset, ushort maxDeltaY, double attemptSlope, ConnectPoint point1 = null, ConnectPoint point2 = null) : 
        base(structureFilePath, structureLength, structureYOffset, maxDeltaY, point1, point2)
    {
        AttemptSlope = attemptSlope;
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

    public override void Generate()
    {
        // probably do a check to see if the points aren't null
        
        if ((Math.Abs(Point1.X - Point2.X) - 1) % StructureLength != 0)
        {
            throw new Exception("Bridge length cannot be resolved with the given BridgeStructure's length");
        }

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
}