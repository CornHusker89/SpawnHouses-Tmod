#nullable enable

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using SpawnHouses.Structures;
using Terraria;
using Terraria.DataStructures;

namespace SpawnHouses.AdvStructures.AdvStructureParts;

#pragma warning disable CS0659 // Type overrides Object.Equals(object o) but does not override Object.GetHashCode()

/// constructors are static methods
public class Shape {
    private static readonly Color[] Colors = [
        Color.White,
        Color.Black,
        Color.Aquamarine,
        Color.Red
    ];

    private int _area = -1;

    private Point16 _center = new(-1, -1);

    public (Point16 topLeft, Point16 bottomRight) BoundingBox;


    public bool IsBox; // because many of the shapes will be boxes, introduce optimizations for boxes
    public Point16[] Points;
    public Point16 Size;

    public int Area {
        get {
            if (_area == -1)
                _area = GetArea();
            return _area;
        }
    }

    public Point16 Center {
        get {
            if (_center == new Point16(-1, -1))
                _center = GetCenter();
            return _center;
        }
    }

    private static Color GetColor(int index) {
        return Colors[index % Colors.Length];
    }

    /// <param name="shapes">shapes to create outline with</param>
    /// <param name="duration">effect duration, in seconds</param>
    public static void CreateOutline(Shape[] shapes, int duration = 10) {
        Task.Run(() => {
            Stopwatch stopwatch = Stopwatch.StartNew();
            while (stopwatch.ElapsedMilliseconds / 1000 < duration) {
                for (int i = 0; i < shapes.Length; i++) {
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

    private void Init(Point16[] points) {
        switch (points.Length) {
            case < 2:
                throw new ArgumentException("Shape must have at least 2 points.");
            case 2:
                Points = [
                    new Point16(points[0].X, points[0].Y),
                    new Point16(points[1].X, points[0].Y),
                    new Point16(points[1].X, points[1].Y),
                    new Point16(points[0].X, points[1].Y)
                ];
                IsBox = true;
                break;
            default:
                Points = points;

                // test if there is only 2 unique x and y's, indicating if its a box
                IsBox = true;
                int x1 = Points[0].X, x2 = -1, y1 = Points[0].Y, y2 = -1;
                foreach (Point16 point in Points) {
                    if (point.X != x1)
                        if (x2 == -1) {
                            x2 = point.X;
                        }
                        else if (point.X != x2) {
                            IsBox = false;
                            break;
                        }

                    if (point.Y != y1)
                        if (y2 == -1) {
                            y2 = point.Y;
                        }
                        else if (point.Y != y2) {
                            IsBox = false;
                            break;
                        }
                }

                // optimize points if box
                if (IsBox) {
                    if (x2 == -1)
                        x2 = x1;
                    if (y2 == -1)
                        y2 = y1;
                    Points = [
                        new Point16(x1, y1),
                        new Point16(x2, y1),
                        new Point16(x2, y2),
                        new Point16(x1, y2)
                    ];
                }

                break;
        }

        int minX = Main.maxTilesX, maxX = 0, minY = Main.maxTilesY, maxY = 0;
        foreach (Point16 point in Points) {
            minX = Math.Min(point.X, minX);
            maxX = Math.Max(point.X, maxX);
            minY = Math.Min(point.Y, minY);
            maxY = Math.Max(point.Y, maxY);
        }

        BoundingBox = (new Point16(minX, minY), new Point16(maxX, maxY));
        Size = new Point16(1 + maxX - minX, 1 + maxY - minY);
    }

    public override string ToString() {
        string pointsStr = string.Empty;
        for (int i = 0; i < Points.Length; i++)
            pointsStr += $"point {i}: {Points[i]}\n";
        return pointsStr;
    }

    public override bool Equals(object? obj) {
        if (obj is not Shape otherShape || otherShape.Points.Length != Points.Length) return false;

        for (int i = 0; i < otherShape.Points.Length; i++)
            if (Points[i] != otherShape.Points[i])
                return false;

        return true;
    }

    private int GetArea() {
        if (IsBox)
            return (BoundingBox.bottomRight.X - BoundingBox.topLeft.X) *
                   (BoundingBox.bottomRight.Y - BoundingBox.topLeft.Y);

        double area = 0;
        for (int i = 0; i < Points.Length; i++) {
            Point16 current = Points[i];
            Point16 next = Points[(i + 1) % Points.Length];

            area += current.X * next.Y - next.X * current.Y;
        }

        return (int)Math.Abs(area / 2);
    }

    private Point16 GetCenter() {
        return BoundingBox.topLeft + Size / new Point16(2, 2);
    }

#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.

    /// <param name="points">If only 2 points are passed, will assume a box</param>
    /// <returns></returns>
    public Shape(params Point16[] points) {
        Init(points);
    }

    /// <param name="points">If only 2 points are passed, will assume a box</param>
    /// <returns></returns>
    public Shape(IEnumerable<Point16> points) {
        var pointsArray = points.ToArray();
        Init(pointsArray);
    }

#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.


    #region Intersection Methods

    public bool Contains(Point16 point) {
        if (IsBox)
            return point.X >= BoundingBox.topLeft.X && point.X <= BoundingBox.bottomRight.X &&
                   point.Y >= BoundingBox.topLeft.Y && point.Y <= BoundingBox.bottomRight.Y;

        int crossingCount = 0;
        for (int i = 0; i < Points.Length; i++) {
            Point16 point1 = Points[i];
            Point16 point2 = Points[(i + 1) % Points.Length];

            if (RayIntersectsSegment(point, point1, point2))
                crossingCount++;
        }

        // a point is inside the polygon if it crosses the edges an odd number of times when raycasting in a single direction
        return crossingCount % 2 == 1;
    }

    public bool HasIntersection(Shape other) {
        if (IsBox && other.IsBox)
            return BoundingBox.topLeft.X <= other.BoundingBox.bottomRight.X &&
                   BoundingBox.bottomRight.X >= other.BoundingBox.topLeft.X &&
                   BoundingBox.topLeft.Y <= other.BoundingBox.bottomRight.Y &&
                   BoundingBox.bottomRight.Y >= other.BoundingBox.topLeft.Y;

        var axes = GetUniqueAxes(this).Concat(GetUniqueAxes(other)).ToList();

        foreach (Point16 axis in axes) {
            Projection projection1 = ProjectOntoAxis(this, axis);
            Projection projection2 = ProjectOntoAxis(other, axis);

            if (!projection1.Overlaps(projection2))
                return false;
        }

        return true;
    }

    private bool RayIntersectsSegment(Point16 point, Point16 segmentStart, Point16 segmentEnd) {
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

    private List<Point16> GetUniqueAxes(Shape shape) {
        List<Point16> axes = [];

        for (int i = 0; i < shape.Points.Length; i++) {
            Point16 p1 = shape.Points[i];
            Point16 p2 = shape.Points[(i + 1) % shape.Points.Length]; // Next vertex (looping)

            Point16 edge = p2 - p1;
            Point16 normal = new Point16(-edge.Y, edge.X); // Perpendicular normal

            double length = Math.Sqrt(normal.X * normal.X + normal.Y * normal.Y);
            Point16 normalized = length == 0
                ? new Point16(0, 0)
                : new Point16((int)(normal.X / length), (int)(normal.Y / length));

            axes.Add(normalized);
        }

        return axes;
    }

    private Projection ProjectOntoAxis(Shape shape, Point16 axis) {
        double min = double.MaxValue;
        double max = double.MinValue;

        foreach (Point16 point in shape.Points) {
            double projection = point.X * axis.X + point.Y * axis.Y;
            min = Math.Min(min, projection);
            max = Math.Max(max, projection);
        }

        return new Projection(min, max);
    }

    private class Projection(double min, double max) {
        private double Min { get; } = min;
        private double Max { get; } = max;

        public bool Overlaps(Projection other) {
            return !(Max < other.Min || other.Max < Min);
        }
    }

    #endregion


    #region ExecuteIn Methods

    /// <param name="action">(int x, int y, byte direction</param>
    /// <param name="completeLoop"></param>
    public void ExecuteOnPerimeter(Action<int, int, byte> action, bool completeLoop = true) {
        if (IsBox) {
            // go line-by-line
            for (int x = BoundingBox.topLeft.X; x <= BoundingBox.bottomRight.X; x++)
                action(x, BoundingBox.topLeft.Y, Directions.Up);
            if (completeLoop)
                for (int x = BoundingBox.topLeft.X; x <= BoundingBox.bottomRight.X; x++)
                    action(x, BoundingBox.bottomRight.Y, Directions.Down);
            for (int y = BoundingBox.topLeft.Y; y <= BoundingBox.bottomRight.Y; y++)
                action(BoundingBox.topLeft.X, y, Directions.Left);
            for (int y = BoundingBox.topLeft.Y; y <= BoundingBox.bottomRight.Y; y++)
                action(BoundingBox.bottomRight.X, y, Directions.Right);
        }
        else {
            for (int pointNum = 0; pointNum < Points.Length - 1; pointNum++)
                // test for a vertical line
                if (Points[pointNum].X == Points[pointNum + 1].X) {
                    int lowerY = Math.Min(Points[pointNum].Y, Points[pointNum + 1].Y);
                    int higherY = Math.Max(Points[pointNum].Y, Points[pointNum + 1].Y);
                    for (int y = lowerY; y < higherY; y++)
                        action(Points[pointNum].X, y,
                            Points[pointNum].X > Center.X ? Directions.Right : Directions.Left);
                }
                else {
                    double slope = (double)(Points[pointNum].Y - Points[pointNum + 1].Y) /
                                   (Points[pointNum].X - Points[pointNum + 1].X);

                    // determine whether to iterate along x/y-axis
                    if (Math.Abs(slope) > 1) {
                        // by y
                        slope = 1 / slope;
                        int lowerY = Math.Min(Points[pointNum].Y, Points[pointNum + 1].Y);
                        int higherY = Math.Max(Points[pointNum].Y, Points[pointNum + 1].Y);
                        int startingX = Points[pointNum].Y < Points[pointNum + 1].Y
                            ? Points[pointNum + 1].X
                            : Points[pointNum].X;
                        for (int y = lowerY; y < higherY; y++) {
                            // round towards the middle
                            double x = startingX + slope * (y - lowerY);
                            if (x < Center.X)
                                action((int)Math.Floor(x), y, Directions.Left);
                            else
                                action((int)Math.Ceiling(x), y, Directions.Right);
                        }
                    }
                    else {
                        // by x
                        int lowerX = Math.Min(Points[pointNum].X, Points[pointNum + 1].X);
                        int higherX = Math.Max(Points[pointNum].X, Points[pointNum + 1].X);
                        int startingY = Points[pointNum].X < Points[pointNum + 1].X
                            ? Points[pointNum + 1].Y
                            : Points[pointNum].Y;
                        for (int x = lowerX; x < higherX; x++) {
                            double y = startingY + slope * (x - lowerX);
                            if (y < Center.Y)
                                action(x, (int)Math.Floor(y), Directions.Up);
                            else
                                action(x, (int)Math.Ceiling(y), Directions.Down);
                        }
                    }
                }
        }
    }

    public void ExecuteInArea(Action<int, int> action) {
        if (IsBox)
            for (int x = BoundingBox.topLeft.X; x <= BoundingBox.bottomRight.X; x++)
            for (int y = BoundingBox.topLeft.Y; y <= BoundingBox.bottomRight.Y; y++)
                action(x, y);
        else
            for (int y = BoundingBox.topLeft.Y; y <= BoundingBox.bottomRight.Y; y++) {
                var intersections = new List<int>();

                for (int i = 0; i < Points.Length; i++) {
                    Point16 p1 = Points[i];
                    Point16 p2 = Points[(i + 1) % Points.Length];

                    // **Handle horizontal edges properly**
                    if (p1.Y == p2.Y) {
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
                    if ((p1.Y <= y && p2.Y > y) || (p2.Y <= y && p1.Y > y)) {
                        int intersectX = (int)Math.Round(p1.X + (double)(y - p1.Y) * (p2.X - p1.X) / (p2.Y - p1.Y));
                        intersections.Add(intersectX);
                    }
                }

                intersections.Sort();

                for (int i = 0; i < intersections.Count; i += 2) {
                    if (i + 1 >= intersections.Count) break;

                    int startX = intersections[i];
                    int endX = intersections[i + 1];

                    for (int x = startX; x <= endX; x++)
                        action(x, y);
                }
            }
    }

    #endregion


    #region Boolean methods

    /// <summary>
    ///     Shapes must not overlap
    /// </summary>
    public static Shape Union(List<Shape> shapes) {
        // Step 1: Collect all points
        var allPoints = new List<Point16>();
        foreach (Shape shape in shapes)
            allPoints.AddRange(shape.Points);

        // Step 2: Convex hull (Graham's scan) to order the points into a single polygon
        var result = ConvexHull(allPoints);

        return new Shape(result);
    }

    private static List<Point16> ConvexHull(List<Point16> points) {
        if (points.Count <= 1) return points;

        // Sort points by x (then y) to get consistent ordering
        points = points.OrderBy(p => p.X).ThenBy(p => p.Y).ToList();

        var lower = new List<Point16>();
        foreach (Point16 p in points) {
            while (lower.Count >= 2 &&
                   Cross(lower[lower.Count - 2], lower[lower.Count - 1], p) <= 0)
                lower.RemoveAt(lower.Count - 1);
            lower.Add(p);
        }

        var upper = new List<Point16>();
        for (int i = points.Count - 1; i >= 0; i--) {
            Point16 p = points[i];
            while (upper.Count >= 2 &&
                   Cross(upper[upper.Count - 2], upper[upper.Count - 1], p) <= 0)
                upper.RemoveAt(upper.Count - 1);
            upper.Add(p);
        }

        // Remove the last point of each half because it's repeated at the beginning of the other half
        lower.RemoveAt(lower.Count - 1);
        upper.RemoveAt(upper.Count - 1);

        // Combine lower and upper parts into a single looped shape
        lower.AddRange(upper);
        return lower;
    }

    private static int Cross(Point16 o, Point16 a, Point16 b) {
        return (a.X - o.X) * (b.Y - o.Y) - (a.Y - o.Y) * (b.X - o.X);
    }

    public (Shape? lower, Shape? middle, Shape? higher) CutTwice(bool cutXAxis, int cutCoord1, int cutCoord2) {
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

    private Shape? ClipPolygon(bool cutXAxis, int cutCoord, bool keepLower, bool includeCut) {
        var outputList = new List<Point16>();

        for (int i = 0; i < Points.Length; i++) {
            Point16 current = Points[i];
            Point16 next = Points[(i + 1) % Points.Length];

            bool currentInside = IsInside(current, cutXAxis, cutCoord, keepLower, includeCut);
            bool nextInside = IsInside(next, cutXAxis, cutCoord, keepLower, includeCut);

            if (currentInside)
                outputList.Add(current); // always keep the current point if it's inside

            if (currentInside != nextInside) // edge crosses the clipping boundary
            {
                Point16 intersectPoint = Intersect(current, next, cutXAxis, cutCoord);

                // move the intersect point so that it's outside the cut instead of directly on it
                if (!includeCut)
                    if (cutXAxis)
                        intersectPoint = keepLower
                            ? new Point16(intersectPoint.X, intersectPoint.Y - 1)
                            : new Point16(intersectPoint.X, intersectPoint.Y + 1);
                    else
                        intersectPoint = keepLower
                            ? new Point16(intersectPoint.X - 1, intersectPoint.Y)
                            : new Point16(intersectPoint.X + 1, intersectPoint.Y);

                outputList.Add(intersectPoint);
            }
        }

        return outputList.Count < 3 ? null : new Shape(outputList);
    }

    private bool IsInside(Point16 point, bool cutXAxis, int cutCoord, bool keepLower, bool includeCut) {
        if (includeCut) {
            if (cutXAxis)
                return keepLower ? point.Y <= cutCoord : point.Y >= cutCoord;
            return keepLower ? point.X <= cutCoord : point.X >= cutCoord;
        }

        if (cutXAxis)
            return keepLower ? point.Y < cutCoord : point.Y > cutCoord;
        return keepLower ? point.X < cutCoord : point.X > cutCoord;
    }

    private Point16 Intersect(Point16 p1, Point16 p2, bool cutXAxis, int cutCoord) {
        int dx = p2.X - p1.X;
        int dy = p2.Y - p1.Y;

        if (cutXAxis) {
            if (dy == 0) return new Point16(p1.X, cutCoord); // Horizontal line edge case
            double t = (cutCoord - p1.Y) / (double)dy;
            int newX = (int)Math.Round(p1.X + t * dx);
            return new Point16(newX, cutCoord);
        }
        else {
            if (dx == 0) return new Point16(cutCoord, p1.Y); // Vertical line edge case
            double t = (cutCoord - p1.X) / (double)dx;
            int newY = (int)Math.Round(p1.Y + t * dy);
            return new Point16(cutCoord, newY);
        }
    }

    #endregion
}