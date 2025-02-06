using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
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
    
    private static Color GetColor(int index)
    {
        return Colors[index % Colors.Length];
    }
    
    public static void CreateOutline(Shape[] shapes)
    {
        Task.Run(() =>
        {
            Stopwatch stopwatch = Stopwatch.StartNew();
            while (stopwatch.ElapsedMilliseconds / 1000 < 10)
            {
                for (int i = 0; i < shapes.Length; i++)
                {
                    for (int j = 0; j < shapes[i].Points.Length - 1; j++)
                        Dust.QuickDustLine(shapes[i].Points[j].ToVector2() * 16 - new Vector2(8, 8), 
                        shapes[i].Points[j + 1].ToVector2() * 16 - new Vector2(8, 8), 20f, GetColor(i));
                    
                    Dust.QuickDustLine(shapes[i].Points[0].ToVector2() * 16 - new Vector2(8, 8), 
                        shapes[i].Points[^1].ToVector2() * 16 - new Vector2(8, 8), 20f, GetColor(i));
                    
                }
                Thread.Sleep(200);
            }
        });

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
    
    public void ExecuteOnPerimeter(Action<int, int> action)
    {
        Point16 middle = new Point16(
            BoundingBox.topLeft.X + (BoundingBox.bottomRight.X - BoundingBox.topLeft.X) / 2,
            BoundingBox.topLeft.Y + (BoundingBox.bottomRight.Y - BoundingBox.topLeft.Y) / 2
        );
        
        for (int pointNum = 0; pointNum < Points.Length - 1; pointNum++)
        {
            // test for a vertical line
            if (Points[pointNum].X - Points[pointNum + 1].X == 0)
            {
                int lowerY = Math.Min(Points[pointNum].Y, Points[pointNum + 1].Y);
                int higherY = Math.Max(Points[pointNum].Y, Points[pointNum + 1].Y);
                for (int y = lowerY; y < higherY; y++)
                    action(Points[pointNum].X, y);
            }
            else
            {
                double slope = (double)(Points[pointNum].Y - Points[pointNum + 1].Y) / (Points[pointNum].X - Points[pointNum + 1].X);
                
                // determine whether to iterate along x/y-axis
                if (Math.Abs(slope) > 1)
                {
                    // by y
                    slope = 1 / slope;
                    int lowerY = Math.Min(Points[pointNum].Y, Points[pointNum + 1].Y);
                    int higherY = Math.Max(Points[pointNum].Y, Points[pointNum + 1].Y);
                    int startingX = Points[pointNum].Y < Points[pointNum + 1].Y? Points[pointNum + 1].X : Points[pointNum].X;
                    for (int y = lowerY; y < higherY; y++)
                    {
                        // round towards the middle
                        double x = startingX + slope * (y - lowerY);
                        if (x < middle.X)
                            action((int)Math.Floor(x), y);
                        else
                            action((int)Math.Ceiling(x), y);
                    }
                }
                else
                {
                    // by x
                    int lowerX = Math.Min(Points[pointNum].X, Points[pointNum + 1].X);
                    int higherX = Math.Max(Points[pointNum].X, Points[pointNum + 1].X);
                    int startingY = Points[pointNum].X < Points[pointNum + 1].X? Points[pointNum + 1].Y : Points[pointNum].Y;
                    for (int x = lowerX; x < higherX; x++)
                    {
                        double y = startingY + slope * (x - lowerX);
                        if (y < middle.Y)
                            action(x, (int)Math.Floor(y));
                        else
                            action(x, (int)Math.Ceiling(y));
                    }
                }
            }
        }
    }
    
    public void ExecuteInArea(Action<int, int> action)
    {
        // iterate through each scanline
        for (int y = BoundingBox.topLeft.Y; y <= BoundingBox.bottomRight.Y; y++)
        {
            List<int> intersections = new List<int>();

            // find intersection points with polygon edges
            for (int i = 0; i < Points.Length; i++)
            {
                Point16 p1 = Points[i];
                Point16 p2 = Points[(i + 1) % Points.Length]; // loop back to start

                // check if the scanline intersects the edge
                if ((p1.Y <= y && p2.Y > y) || (p2.Y <= y && p1.Y > y))
                {
                    // compute intersection X using linear interpolation
                    double intersectX = p1.X + (double)(y - p1.Y) * (p2.X - p1.X) / (p2.Y - p1.Y);
                    intersections.Add((int)Math.Floor(intersectX));
                }
            }

            // sort the intersection points
            intersections.Sort();

            // fill tiles between pairs of intersections
            for (int i = 0; i < intersections.Count; i += 2)
            {
                if (i + 1 >= intersections.Count) break; // ensure pairs exist

                int startX = intersections[i];
                int endX = intersections[i + 1];

                for (int x = startX; x <= endX; x++)
                {
                    action(x, y);
                }
            }
        }
    }
    
    private bool RayIntersectsSegment(Point16 point, Point16 segmentStart, Point16 segmentEnd)
    {
        if (segmentStart.Y > segmentEnd.Y)
            (segmentStart, segmentEnd) = (segmentEnd, segmentStart);

        // Check if the point is outside the segment's Y range
        if (point.Y <= segmentStart.Y || point.Y > segmentEnd.Y)
            return false;

        // Check if the point is to the right of the segment
        if (point.X >= Math.Max(segmentStart.X, segmentEnd.X))
            return false;

        // Check for intersection
        double slope = (segmentEnd.X - segmentStart.X) / (double)(segmentEnd.Y - segmentStart.Y);
        double intersectX = segmentStart.X + (point.Y - segmentStart.Y) * slope;

        return point.X < intersectX;
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