using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.DataStructures;
using Terraria.ModLoader;

namespace SpawnHouses.Structures.StructureParts;

/// constructors are static methods
public class Shape
{
    private static readonly Color[] Colors =
    [
        Color.White,
        Color.Black,
        Color.Aquamarine,
        Color.Red,
        Color.Purple,
        Color.Orange,
        Color.Brown,
        Color.Blue,
        Color.Pink,
        Color.Green,
        Color.Yellow,
        Color.Gray,
        Color.SkyBlue,
        Color.SteelBlue,
        Color.Olive,
        Color.PeachPuff
    ];
    
    public static void CreateOutline(Shape[] shapes)
    {
        for (int i = 0; i < shapes.Length - 1; i++)
        {
            for (int j = 0; j < shapes[i].Points.Length - 1; j++)
                Dust.QuickDustLine(shapes[i].Points[j].ToVector2() * 16, shapes[i].Points[j + 1].ToVector2() * 16, 10f, Colors[i]);
            Dust.QuickDustLine(shapes[i].Points[0].ToVector2() * 16, shapes[i].Points[^1].ToVector2() * 16, 10f, Colors[i]);
        }
    }

    public (Point16 topLeft, Point16 bottomRight) BoundingBox;
    public Point16[] Points;
    private int _area = -1;
   
    /// <summary>
    /// 
    /// </summary>
    /// <param name="points">If only 2 points are passed, will assume a box</param>
    /// <returns></returns>
    public Shape(params Point16[] points)
    {
        Init(points);
    }
    
    /// <summary>
    /// 
    /// </summary>
    /// <param name="points">If only 2 points are passed, will assume a box</param>
    /// <returns></returns>
    public Shape(IEnumerable<Point16> points)
    {
        Point16[] pointsArray = points.ToArray();
        Init(pointsArray);
    }
    
    private void Init(Point16[] points)
    {
        switch (points.Length)
        {
            case < 2:
                throw new ArgumentException("Shape must have at least 2 points.");
            case 2:
                Points = 
                [
                    new Point16(points[0].X, points[0].Y),
                    new Point16(points[1].X, points[0].Y),
                    new Point16(points[1].X, points[1].Y),
                    new Point16(points[0].X, points[1].Y)
                ];
                break;
            default:
                Points = points;
                break;
        }
        int minX = Main.maxTilesX, maxX = 0, minY = Main.maxTilesY, maxY = 0;
        foreach (Point16 point in Points)
        {
            minX = Math.Min(point.X, minX);
            maxX = Math.Max(point.X, maxX);
            minY = Math.Min(point.Y, minY);
            maxY = Math.Max(point.Y, maxY);
        }
        BoundingBox = (new Point16(minX, minY), new Point16(maxX, maxY));
    }
    
    public int GetArea()
    {
        if (_area != -1)
            return _area;

        double area = 0;
        for (int i = 0; i < Points.Length; i++)
        {
            Point16 current = Points[i];
            Point16 next = Points[(i + 1) % Points.Length];

            area += current.X * next.Y - next.X * current.Y;
        }
        _area = (int)Math.Abs(area / 2);
        return _area;
    }
    
    public bool Contains(Point16 point)
    {
        int crossingCount = 0;
        for (int i = 0; i < Points.Length; i++)
        {
            Point16 point1 = Points[i];
            Point16 point2 = Points[(i + 1) % Points.Length];

            if (RayIntersectsSegment(point, point1, point2))
                crossingCount++;
        }

        // A point is inside the polygon if it crosses the edges an odd number of times when raycasting in a single direction
        return crossingCount % 2 == 1;
    }
    
    private bool RayIntersectsSegment(Point16 point, Point16 a, Point16 b)
    {
        if (a.Y > b.Y)
            (a, b) = (b, a);

        // Check if the point is outside the segment's Y range
        if (point.Y <= a.Y || point.Y > b.Y)
            return false;

        // Check if the point is to the right of the segment
        if (point.X >= Math.Max(a.X, b.X))
            return false;

        // Check for intersection
        double slope = (b.X - a.X) / (double)(b.Y - a.Y);
        double intersectX = a.X + (point.Y - a.Y) * slope;

        return point.X < intersectX;
    }
    
    public bool Intersects(Shape other)
    {
        List<Point16> axes = GetUniqueAxes(this).Concat(GetUniqueAxes(other)).ToList();

        foreach (var axis in axes)
        {
            Projection projection1 = ProjectOntoAxis(this, axis);
            Projection projection2 = ProjectOntoAxis(other, axis);

            if (!projection1.Overlaps(projection2))
                return false;
        }
        return true;
    }

    private List<Point16> GetUniqueAxes(Shape shape)
    {
        List<Point16> axes = new List<Point16>();

        for (int i = 0; i < shape.Points.Length; i++)
        {
            Point16 p1 = shape.Points[i];
            Point16 p2 = shape.Points[(i + 1) % shape.Points.Length]; // Next vertex (looping)

            Point16 edge = p2 - p1;
            Point16 normal = new Point16(-edge.Y, edge.X); // Perpendicular normal

            double length = Math.Sqrt(normal.X * normal.X + normal.Y * normal.Y);
            Point16 normalized = length == 0 ? new Point16(0, 0) : new Point16((int)(normal.X / length), (int)(normal.Y / length));

            axes.Add(normalized);
        }

        return axes;
    }

    private Projection ProjectOntoAxis(Shape shape, Point16 axis)
    {
        double min = double.MaxValue;
        double max = double.MinValue;

        foreach (var point in shape.Points)
        {
            double projection = point.X * axis.X + point.Y * axis.Y;
            min = Math.Min(min, projection);
            max = Math.Max(max, projection);
        }

        return new Projection(min, max);
    }
    
    private class Projection
    {
        private double Min { get; }
        private double Max { get; }

        public Projection(double min, double max)
        {
            Min = min;
            Max = max;
        }

        public bool Overlaps(Projection other)
        {
            return !(Max < other.Min || other.Max < Min);
        }
    }
    
}