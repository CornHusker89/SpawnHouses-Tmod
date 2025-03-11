#nullable enable
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
    ];

    private static Color GetColor(int index)
    {
        return Colors[index % Colors.Length];
    }

    /// <param name="shapes">shapes to create outline with</param>
    /// <param name="duration">effect duration, in seconds</param>
    public static void CreateOutline(Shape[] shapes, int duration = 10)
    {
        Task.Run(() =>
        {
            Stopwatch stopwatch = Stopwatch.StartNew();
            while (stopwatch.ElapsedMilliseconds / 1000 < duration)
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
    public Point16 Size;
    public bool IsBox = false; // because many of the shapes will be boxes, introduce optimizations for boxes

#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.

    /// <param name="points">If only 2 points are passed, will assume a box</param>
    /// <returns></returns>
    public Shape(params Point16[] points)
    {
        Init(points);
    }

    /// <param name="points">If only 2 points are passed, will assume a box</param>
    /// <returns></returns>

    public Shape(IEnumerable<Point16> points)

    {
        Point16[] pointsArray = points.ToArray();
        Init(pointsArray);
    }

#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.

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
                IsBox = true;
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
        Size = new Point16(maxX - minX, maxY - minY);
    }

    public override string ToString()
    {
        string pointsStr = string.Empty;
        for (int i = 0; i < Points.Length; i++)
            pointsStr += $"point {i}: {Points[i]}\n";
        return pointsStr;
    }

    public int GetArea()
    {
        if (IsBox)
            return (BoundingBox.bottomRight.X - BoundingBox.topLeft.X) *
                   (BoundingBox.bottomRight.Y - BoundingBox.topLeft.Y);

        double area = 0;
        for (int i = 0; i < Points.Length; i++)
        {
            Point16 current = Points[i];
            Point16 next = Points[(i + 1) % Points.Length];

            area += current.X * next.Y - next.X * current.Y;
        }
        return (int)Math.Abs(area / 2);
    }



    #region Intersection Methods

    public bool Contains(Point16 point)
    {
        if (IsBox)
            return point.X >= BoundingBox.topLeft.X && point.X <= BoundingBox.bottomRight.X &&
                   point.Y >= BoundingBox.topLeft.Y && point.Y <= BoundingBox.bottomRight.Y;

        int crossingCount = 0;
        for (int i = 0; i < Points.Length; i++)
        {
            Point16 point1 = Points[i];
            Point16 point2 = Points[(i + 1) % Points.Length];

            if (RayIntersectsSegment(point, point1, point2))
                crossingCount++;
        }

        // a point is inside the polygon if it crosses the edges an odd number of times when raycasting in a single direction
        return crossingCount % 2 == 1;
    }

    public bool HasIntersection(Shape other)
    {
        if (IsBox && other.IsBox)
            return BoundingBox.topLeft.X <= other.BoundingBox.bottomRight.X && BoundingBox.bottomRight.X >= other.BoundingBox.topLeft.X &&
                   BoundingBox.topLeft.Y <= other.BoundingBox.bottomRight.Y && BoundingBox.bottomRight.Y >= other.BoundingBox.topLeft.Y;

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

        private bool RayIntersectsSegment(Point16 point, Point16 segmentStart, Point16 segmentEnd)
    {
        if (segmentStart.Y > segmentEnd.Y)
            (segmentStart, segmentEnd) = (segmentEnd, segmentStart);

        // check if the point is outside the segment's Y range
        if (point.Y <= segmentStart.Y || point.Y > segmentEnd.Y)
            return false;

        // check if the point is to the right of the segment
        if (point.X >= Math.Max(segmentStart.X, segmentEnd.X))
            return false;

        // check for intersection
        double slope = (segmentEnd.X - segmentStart.X) / (double)(segmentEnd.Y - segmentStart.Y);
        double intersectX = segmentStart.X + (point.Y - segmentStart.Y) * slope;

        return point.X < intersectX;
    }

    private List<Point16> GetUniqueAxes(Shape shape)
    {
        List<Point16> axes = [];

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

    private class Projection(double min, double max)
    {
        private double Min { get; } = min;
        private double Max { get; } = max;

        public bool Overlaps(Projection other)
        {
            return !(Max < other.Min || other.Max < Min);
        }
    }

    #endregion



    #region ExecuteIn Methods

    public void ExecuteOnPerimeter(Action<int, int> action, bool completeLoop = true)
    {
        if (IsBox)
        {
            for (int x = BoundingBox.topLeft.X; x <= BoundingBox.bottomRight.X; x++)
            {
                action(x, BoundingBox.topLeft.Y);
                if (completeLoop)
                    action(x, BoundingBox.bottomRight.Y);
            }
            for (int y = BoundingBox.topLeft.Y + 1; y < BoundingBox.bottomRight.Y; y++)
            {
                action(BoundingBox.topLeft.X, y);
                action(BoundingBox.bottomRight.X, y);
            }
        }
        else
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
                    double slope = (double)(Points[pointNum].Y - Points[pointNum + 1].Y) /
                                   (Points[pointNum].X - Points[pointNum + 1].X);

                    // determine whether to iterate along x/y-axis
                    if (Math.Abs(slope) > 1)
                    {
                        // by y
                        slope = 1 / slope;
                        int lowerY = Math.Min(Points[pointNum].Y, Points[pointNum + 1].Y);
                        int higherY = Math.Max(Points[pointNum].Y, Points[pointNum + 1].Y);
                        int startingX = Points[pointNum].Y < Points[pointNum + 1].Y
                            ? Points[pointNum + 1].X
                            : Points[pointNum].X;
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
                        int startingY = Points[pointNum].X < Points[pointNum + 1].X
                            ? Points[pointNum + 1].Y
                            : Points[pointNum].Y;
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
    }

    public void ExecuteInArea(Action<int, int> action)
    {
        if (IsBox)
        {
            for (int x = BoundingBox.topLeft.X; x <= BoundingBox.bottomRight.X; x++)
                for (int y = BoundingBox.topLeft.Y; y <= BoundingBox.bottomRight.Y; y++)
                    action(x, y);
        }
        else
        {
            for (int y = BoundingBox.topLeft.Y; y <= BoundingBox.bottomRight.Y; y++)
            {
                List<int> intersections = new List<int>();

                for (int i = 0; i < Points.Length; i++)
                {
                    Point16 p1 = Points[i];
                    Point16 p2 = Points[(i + 1) % Points.Length];

                    // **Handle horizontal edges properly**
                    if (p1.Y == p2.Y)
                    {
                        if (p1.Y == BoundingBox.bottomRight.Y) // If it's the bottom edge, count it
                        {
                            int startX = Math.Min(p1.X, p2.X);
                            int endX = Math.Max(p1.X, p2.X);
                            for (int x = startX; x <= endX; x++)
                                action(x, y);
                        }
                        continue;
                    }

                    // **Find intersection of edge with the current scanline**
                    if ((p1.Y <= y && p2.Y > y) || (p2.Y <= y && p1.Y > y))
                    {
                        int intersectX = (int)Math.Round(p1.X + (double)(y - p1.Y) * (p2.X - p1.X) / (p2.Y - p1.Y));
                        intersections.Add(intersectX);
                    }
                }

                intersections.Sort();

                for (int i = 0; i < intersections.Count; i += 2)
                {
                    if (i + 1 >= intersections.Count) break;

                    int startX = intersections[i];
                    int endX = intersections[i + 1];

                    for (int x = startX; x <= endX; x++)
                        action(x, y);
                }
            }
        }
    }

    #endregion



    #region Boolean methods

    public (Shape? lower, Shape? middle, Shape? higher) CutTwice(bool cutXAxis, int cutCoord1, int cutCoord2)
    {
        if (cutCoord2 < cutCoord1)
            (cutCoord1, cutCoord2) = (cutCoord2, cutCoord1);

        // upper left piece
        Shape? shapeA = ClipPolygon(cutXAxis, cutCoord1, true, false);

        // remainder after the first cut
        Shape? remainder = ClipPolygon(cutXAxis, cutCoord1, false, true);

        // middle piece (clip remainder again)
        Shape? shapeB = remainder?.ClipPolygon(cutXAxis, cutCoord2, true, true);

        // lower right piece
        Shape? shapeC = remainder?.ClipPolygon(cutXAxis, cutCoord2, false, false);

        return (shapeA, shapeB, shapeC);
    }

    private Shape? ClipPolygon(bool cutXAxis, int cutCoord, bool keepLower, bool includeCut)
    {
        var outputList = new List<Point16>();

        for (int i = 0; i < Points.Length; i++)
        {
            Point16 current = Points[i];
            Point16 next = Points[(i + 1) % Points.Length];

            bool currentInside = IsInside(current, cutXAxis, cutCoord, keepLower);
            bool nextInside = IsInside(next, cutXAxis, cutCoord, keepLower);

            if (currentInside)
                outputList.Add(current);  // always keep the current point if it's inside

            if (currentInside != nextInside) // edge crosses the clipping boundary
            {
                Point16? possibleIntersectPoint = Intersect(current, next, cutXAxis, cutCoord);
                if (possibleIntersectPoint.HasValue)
                {
                    Point16 intersectPoint = possibleIntersectPoint.Value;

                    // move the intersect point so that it's outside the cut instead of directly on it
                    if (!includeCut)
                        if (cutXAxis)
                            intersectPoint = keepLower ? new Point16(intersectPoint.X, intersectPoint.Y - 1) :
                                new Point16(intersectPoint.X, intersectPoint.Y + 1);
                        else
                            intersectPoint = keepLower ? new Point16(intersectPoint.X - 1, intersectPoint.Y) :
                                new Point16(intersectPoint.X + 1, intersectPoint.Y);
                    outputList.Add(intersectPoint);
                }
            }
        }
        return outputList.Count < 3 ? null : new Shape(outputList);
    }

    private bool IsInside(Point16 p, bool cutXAxis, int cutCoord, bool keepLower)
    {
        if (cutXAxis)
            return keepLower? p.Y <= cutCoord : p.Y >= cutCoord;
        return keepLower? p.X <= cutCoord : p.X >= cutCoord;
    }

    private Point16? Intersect(Point16 p1, Point16 p2, bool cutXAxis, int cutCoord)
    {
        int dx = p2.X - p1.X;
        int dy = p2.Y - p1.Y;

        if (cutXAxis)
        {
            if (dy == 0) return new Point16(p1.X, cutCoord);  // Horizontal line edge case
            double t = (cutCoord - p1.Y) / (double)dy;
            int newX = (int)Math.Round(p1.X + t * dx);
            return new Point16(newX, cutCoord);
        }
        else
        {
            if (dx == 0) return new Point16(cutCoord, p1.Y);  // Vertical line edge case
            double t = (cutCoord - p1.X) / (double)dx;
            int newY = (int)Math.Round(p1.Y + t * dy);
            return new Point16(cutCoord, newY);
        }
    }

    #endregion

}