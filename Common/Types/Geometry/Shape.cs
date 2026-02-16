#nullable enable
#pragma warning disable CS0659 // Type overrides Object.Equals(object o) but does not override Object.GetHashCode()

using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using SpawnHouses.Helpers;
using SpawnHouses.Structures;
using Terraria;
using Terraria.DataStructures;
using Terraria.ID;

namespace SpawnHouses.Common.Types.Geometry;

/// <summary>
///     generic 2D shape. has support for triangles, rectangles, and n-gons.
///     can also support shapes made up of 1 or 2 points
/// </summary>
/// <remarks>it's assumed that the points are in clockwise order</remarks>
public class Shape : PointGeometry {
    public bool IsBox { get; private set; } // because many shapes will be boxes, introduce optimizations for boxes

    private bool[,]? _booleanTilemap;

    /// <summary>
    ///     2d array of this shape showing if there is a tile, always 0-indexed
    /// </summary>
    public bool[,] BooleanTilemap {
        get { return _booleanTilemap ??= GetBooleanTilemap(); }
    }

    protected sealed override void Init(Point16[] points, bool optimize) {
        Points = points;

        if (optimize) OptimizePoints(true);

        switch (Points.Length) {
            case 1:
                IsBox = true;
                break;
            case 2:
                IsBox = Points[0].X == Points[1].X || Points[0].Y == Points[1].Y;
                break;
            case 3:
                break;
            default: {
                // test if the points form a box
                HashSet<int> x = [], y = [];
                foreach (Point16 point in Points) {
                    x.Add(point.X);
                    y.Add(point.Y);
                }

                IsBox = x.Count <= 2 && y.Count <= 2;
                break;
            }
        }

        SetBoundingBoxAndSize();
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

    /// <param name="points"></param>
    /// <returns></returns>
    /// <remarks>does not support box shorthand</remarks>
    public Shape(params Point16[] points) {
        Init(points, true);
    }

    /// <param name="boxShorthand">if true and only 2 points are passed, will assume a box with 4 intended points</param>
    /// <param name="points"></param>
    /// <returns></returns>
    public Shape(bool boxShorthand, params Point16[] points) {
        if (boxShorthand && points.Length == 2)
            points = [
                new Point16(points[0].X, points[0].Y),
                new Point16(points[1].X, points[0].Y),
                new Point16(points[1].X, points[1].Y),
                new Point16(points[0].X, points[1].Y)
            ];
        Init(points, true);
    }

    /// <param name="points"></param>
    /// <param name="boxShorthand">if true and only 2 points are passed, will assume a box with 4 intended points</param>
    /// <returns></returns>
    public Shape(IEnumerable<Point16> points, bool boxShorthand = false) {
        var pointsArray = points.ToArray();
        if (boxShorthand && pointsArray.Length == 2)
            pointsArray = [
                new Point16(pointsArray[0].X, pointsArray[0].Y),
                new Point16(pointsArray[1].X, pointsArray[0].Y),
                new Point16(pointsArray[1].X, pointsArray[1].Y),
                new Point16(pointsArray[0].X, pointsArray[1].Y)
            ];
        Init(pointsArray, true);
    }

    /// <param name="points"></param>
    /// <param name="optimize"></param>
    /// <param name="boxShorthand">if true and only 2 points are passed, will assume a box with 4 intended points</param>
    /// <returns></returns>
    private Shape(IEnumerable<Point16> points, bool optimize, bool boxShorthand = false) {
        var pointsArray = points.ToArray();
        Init(pointsArray, optimize);
    }

#pragma warning restore CS8618


    #region Shape Self-Geometry

    /// <summary>
    ///     the number of tiles this shape encloses
    /// </summary>
    /// <param name="approximate">if an approximation algorithm is used. otherwise, area is evaluated using <see cref="ExecuteInArea(System.Action{int,int})" /></param>
    public int GetArea(bool approximate = false) {
        if (IsBox)
            return (BoundingBox.bottomRight.X - BoundingBox.topLeft.X) *
                   (BoundingBox.bottomRight.Y - BoundingBox.topLeft.Y);

        if (approximate) {
            double area = 0;
            for (int i = 0; i < Points.Length; i++) {
                Point16 current = Points[i];
                Point16 next = Points[(i + 1) % Points.Length];

                area += current.X * next.Y - next.X * current.Y;
            }

            return (int)Math.Abs(area / 2);
        }

        int count = 0;
        ExecuteInArea((_, _) => count++);
        return count;
    }

    /// <summary>
    ///     gets the ratio of bounding box size to actual shape area. can indicate how box-like the shape is
    /// </summary>
    /// <returns></returns>
    public double GetBoundingBoxEfficiency() => (double)Size.X * Size.Y / GetArea();

    /// <summary>
    ///     gets number of tiles that are within the bounding box but not in the shape. can indicate how box-like the shape is
    /// </summary>
    /// <returns></returns>
    public int GetUnusedBoundingBoxArea() => Size.X * Size.Y - GetArea();
    
    /// <summary>
    ///     offsets a vertex on a CLOSED geometry so that the proportions of the geometry don't change
    /// </summary>
    /// <param name="prev"></param>
    /// <param name="current"></param>
    /// <param name="next"></param>
    /// <param name="distance"></param>
    /// <param name="clockwise"></param>
    /// <param name="result"></param>
    /// <returns></returns>
    private static bool OffsetVertexEven(Vector2 prev, Vector2 current, Vector2 next, float distance, bool clockwise, out Vector2 result) {
        Vector2 dir1 = Vector2.Normalize(current - prev);
        Vector2 dir2 = Vector2.Normalize(next - current);

        Vector2 n1 = Perp(dir1);
        Vector2 n2 = Perp(dir2);

        if (!clockwise) {
            n1 = -n1;
            n2 = -n2;
        }

        // convex or concave
        float cross = Cross(dir1, dir2);
        bool convex = clockwise ? cross < 0 : cross > 0;

        // concave corner, skip or bevel
        if (!convex) {
            // simple bevel fallback
            result = current + n1 * distance;
            return false; // indicates bevel used
        }

        // compute miter for sharp angles
        Vector2 miter = Vector2.Normalize(n1 + n2);

        float denom = Vector2.Dot(miter, n2);

        if (MathF.Abs(denom) < 0.001) {
            result = current + n1 * distance;
            return false;
        }

        float length = distance / denom;

        // clamp to avoid spikes
        float maxMiter = distance * 4f;
        length = MathF.Min(length, maxMiter);

        result = current + miter * length;
        return true;
    }

    /// <summary>
    ///     returns this geometry's points with a uniform expansion
    /// </summary>
    /// <param name="offset"></param>
    /// <returns></returns>
    protected Vector2[] GetOffsetEven(float offset) {
        var result = new Vector2[Points.Length];
        for (int i = 0; i < Points.Length; i++) {
            Vector2 prev = Points[(i - 1 + Points.Length) % Points.Length].ToVector2();
            Vector2 curr = Points[i].ToVector2();
            Vector2 next = Points[(i + 1) % Points.Length].ToVector2();

            OffsetVertexEven(prev, curr, next, offset, GetClockwise(), out Vector2 newPoint);
            result[i] = newPoint;
        }

        return result;
    }
    
    /// <summary>
    ///     expands shape by <see cref="expansion" /> tiles
    /// </summary>
    /// <param name="expansion">the number of tiles to expand each point by (only whole numbers)</param>
    public void Expand(float expansion) {
        Init(GetOffsetEven(expansion)
                .Select(vector => vector.ToPoint16())
                .ToArray(),
            false);
    }

    /// <summary>
    ///     creates new shape, expanded by <paramref name="expansion" /> tiles
    /// </summary>
    /// <returns></returns>
    public Shape GetExpandedShape(int expansion) => new(GetOffsetEven(expansion)
        .Select(vector2 => vector2.ToPoint16())
        .ToArray()
    );

    /// <summary>
    ///     find all corners of a shape (expanded out by 1 tile) based on their x and y positions, useful for ensuring beams and such make sense visually
    /// </summary>
    /// <param name="significantAngle">only vertices that create a deviation (in deg) larger than this will be considered</param>
    /// <returns></returns>
    public List<PartialPoint16> GetCorners(float significantAngle = 30f) {
        List<PartialPoint16> corners = [];
        foreach (Point16 point in GetExpandedShape(1).CollapseVertices(significantAngle)) {
            bool xCorner = point.X > BoundingBox.topLeft.X
                           && point.X < BoundingBox.bottomRight.X;
            bool yCorner = point.Y > BoundingBox.topLeft.Y
                           && point.Y < BoundingBox.bottomRight.Y;
            if (xCorner && yCorner)
                corners.Add(new PartialPoint16(point));
            else if (xCorner && !yCorner)
                corners.Add(new PartialPoint16(point.X, 0, hasY: false));
            else if (!xCorner && yCorner)
                corners.Add(new PartialPoint16(0, point.Y, false));
        }

        // sanitize list to remove repeat values
        for (int i = 0; i < corners.Count; i++) {
            PartialPoint16 target = corners[i];
            if (target.HasX == target.HasY) // only check cases where only 1 axis is valid
                continue;

            PartialPoint16 last = corners[i - 1 != -1 ? i - 1 : corners.Count - 1];
            PartialPoint16 next = corners[i + 1 != corners.Count ? i + 1 : 0];

            if ((last is { HasX: true, HasY: true } && (target.X == last.X || target.Y == last.Y))
                || (next is { HasX: true, HasY: true } && (target.X == next.X || target.Y == next.Y))) {
                corners.RemoveAt(i);
                i--;
            }
        }

        return corners;
    }

    /// <summary>
    ///     gets the largest, smallest, and average sizes along the entire shape, along the chosen axes
    /// </summary>
    /// <param name="acrossXAxis"></param>
    /// <returns></returns>
    /// <exception cref="Exception"></exception>
    public (int min, int max, double average) GetDetailedAxisSizes(bool acrossXAxis) {
        Dictionary<int, int> minValues = [], maxValues = [];
        ExecuteInArea((x, y) => {
            int axisVal = acrossXAxis ? x : y;
            int oppositeAxisVal = acrossXAxis ? y : x;
            if (!minValues.TryGetValue(oppositeAxisVal, out int oldMinValue)) {
                minValues[oppositeAxisVal] = axisVal;
            }
            else {
                if (axisVal < oldMinValue) minValues[oppositeAxisVal] = axisVal;
            }

            if (!maxValues.TryGetValue(oppositeAxisVal, out int oldMaxValue)) {
                maxValues[oppositeAxisVal] = axisVal;
            }
            else {
                if (axisVal > oldMaxValue) maxValues[oppositeAxisVal] = axisVal;
            }
        });

        // get the actual size for each slice using the min/max values for that slice
        List<int> sizes = [];
        foreach (int key in minValues.Keys) sizes.Add(maxValues[key] - minValues[key] + 1);

        if (sizes.Count == 0) throw new Exception("shape must have a minimum area of 1");

        int min = sizes[0], max = sizes[0], average = 0;
        foreach (int size in sizes) {
            if (size < min) min = size;

            if (size > max) max = size;
            average += size;
        }

        average /= sizes.Count;
        return (min, max, average);
    }

    /// <summary>
    ///     get a 2d array of this shape showing if there is a tile, always 0-indexed
    /// </summary>
    /// <returns></returns>
    private bool[,] GetBooleanTilemap() {
        bool[,] tilemap = new bool[Size.X, Size.Y];
        ExecuteInArea((x, y) => { tilemap[x - BoundingBox.topLeft.X, y - BoundingBox.topLeft.Y] = true; });
        return tilemap;
    }

    /// <summary>
    ///     returns true if the shape is clockwise, otherwise false
    /// </summary>
    /// <returns></returns>
    public bool GetClockwise() {
        float area = 0;
        for (int i = 0; i < Points.Length; i++) {
            Point16 a = Points[i];
            Point16 b = Points[(i + 1) % Points.Length];
            area += (b.X - a.X) * (b.Y + a.Y);
        }

        return area > 0;
    }

    #endregion


    #region Execute-In

    /// <param name="action">x, y, direction</param>
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

    /// <summary>
    ///     runs action on every tile contained in the shape
    /// </summary>
    /// <param name="action"></param>
    public void ExecuteInArea(Action<int, int> action) {
        if (IsBox) {
            for (int x = BoundingBox.topLeft.X; x <= BoundingBox.bottomRight.X; x++)
            for (int y = BoundingBox.topLeft.Y; y <= BoundingBox.bottomRight.Y; y++)
                action(x, y);
        }
        else {
            for (int y = BoundingBox.topLeft.Y; y <= BoundingBox.bottomRight.Y; y++) {
                var intersections = new List<double>();

                for (int i = 0; i < Points.Length; i++) {
                    Point16 p1 = Points[i];
                    Point16 p2 = Points[(i + 1) % Points.Length];

                    // Find intersection of edge with the current scanline
                    if ((p1.Y <= y && p2.Y > y) || (p2.Y <= y && p1.Y > y)) {
                        double intersectX = (int)Math.Round(p1.X + (double)(y - p1.Y) * (p2.X - p1.X) / (p2.Y - p1.Y));
                        intersections.Add(intersectX);
                    }
                }

                intersections.Sort();

                for (int i = 0; i < intersections.Count; i += 2) {
                    if (i + 1 >= intersections.Count) break;

                    int startX = (int)Math.Round(intersections[i]);
                    int endX = (int)Math.Round(intersections[i + 1] - 0.05); // very slightly bias inward

                    for (int x = startX; x <= endX; x++)
                        if (!Points.Contains(new Point16(x, y)))
                            action(x, y);
                }
            }

            // Handle bottom horizontal edge
            for (int i = 0; i < Points.Length; i++) {
                Point16 p1 = Points[i];
                Point16 p2 = Points[(i + 1) % Points.Length];
                if (p1.Y == BoundingBox.bottomRight.Y)
                    if (p1.Y == p2.Y) {
                        int startX = Math.Min(p1.X, p2.X);
                        int endX = Math.Max(p1.X, p2.X);
                        for (int x = startX + 1; x <= endX - 1; x++)
                            if (!Points.Contains(new Point16(x, p1.Y)))
                                action(x, p1.Y);
                    }
            }

            foreach (Point16 point in Points) action(point.X, point.Y);
        }
    }

    /// <summary>
    ///     runs action on every tile contained in the shape, passing the predicted tile slope
    /// </summary>
    /// <param name="action"></param>
    /// <param name="slopingAlgorithm">
    ///     function to determine the <see cref="BlockType" /> passed to the action.
    ///     if none is passed, this function will only pass BlockType <see cref="BlockType.Solid" />.
    ///     examples of these functions are found in <see cref="Helpers.SlopeHelper" />
    /// </param>
    public void ExecuteInArea(Action<int, int, BlockType> action, SlopingAlgorithm? slopingAlgorithm = null) {
        if (slopingAlgorithm is null) {
            ExecuteInArea((x, y) => { action(x, y, BlockType.Solid); });
            return;
        }

        for (int x = 0; x < Size.X; x++) {
            int xWorldCoord = x + BoundingBox.topLeft.X;
            for (int y = 0; y < Size.Y; y++) {
                int yWorldCoord = y + BoundingBox.topLeft.Y;
                if (!BooleanTilemap[x, y]) continue;
                action(xWorldCoord, yWorldCoord, slopingAlgorithm(x, y, BooleanTilemap));
            }
        }
    }

    #endregion


    #region General Shape Intersection

    public bool Contains(Point16 point) {
        if (IsBox)
            return point.X >= BoundingBox.topLeft.X && point.X <= BoundingBox.bottomRight.X &&
                   point.Y >= BoundingBox.topLeft.Y && point.Y <= BoundingBox.bottomRight.Y;

        int crossingCount = 0;
        for (int i = 0; i < Points.Length; i++) {
            Point16 point1 = Points[i];
            Point16 point2 = Points[(i + 1) % Points.Length];

            if (RightFacingRayIntersectsSegment(point, point1, point2))
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

    private List<Point16> GetUniqueAxes(Shape shape) {
        List<Point16> axes = [];

        for (int i = 0; i < shape.Points.Length; i++) {
            Point16 p1 = shape.Points[i];
            Point16 p2 = shape.Points[(i + 1) % shape.Points.Length]; // Next vertex (looping)

            Point16 edge = p2 - p1;
            Point16 normal = new(-edge.Y, edge.X); // Perpendicular normal

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

        public bool Overlaps(Projection other) => !(Max < other.Min || other.Max < Min);
    }

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
                   Cross(lower[^2], lower[^1], p) <= 0)
                lower.RemoveAt(lower.Count - 1);
            lower.Add(p);
        }

        var upper = new List<Point16>();
        for (int i = points.Count - 1; i >= 0; i--) {
            Point16 p = points[i];
            while (upper.Count >= 2 &&
                   Cross(upper[^2], upper[^1], p) <= 0)
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

    #endregion


    #region Slicing Shape

    public (Shape? lower, Shape? middle, Shape? higher) CutTwice(bool cutXAxis, int cutPos1, int cutPos2) {
        if (cutPos2 < cutPos1)
            (cutPos1, cutPos2) = (cutPos2, cutPos1);

        // upper left piece
        Shape? shapeA = ClipPolygon(cutXAxis, cutPos1, true, false);

        // remainder after the first cut
        Shape? remainder = ClipPolygon(cutXAxis, cutPos1, false, true);

        // middle piece (clip remainder again)
        Shape? shapeB = remainder?.ClipPolygon(cutXAxis, cutPos2, true, true);

        // lower right piece
        Shape? shapeC = remainder?.ClipPolygon(cutXAxis, cutPos2, false, false);

        return (shapeA, shapeB, shapeC);
    }

    private Shape? ClipPolygon(bool splitAlongX, int cutPos, bool keepLower, bool includeCut) {
        var outputList = new List<Point16>();

        for (int i = 0; i < Points.Length; i++) {
            Point16 current = Points[i];
            Point16 next = Points[(i + 1) % Points.Length];

            bool currentInside = IsInside(current, splitAlongX, cutPos, keepLower, includeCut);
            bool nextInside = IsInside(next, splitAlongX, cutPos, keepLower, includeCut);

            if (currentInside)
                outputList.Add(current); // always keep the current point if it's inside

            if (currentInside != nextInside) // edge crosses the clipping boundary
            {
                Point16 intersectPoint = GetIntersectionPoint(current, next, splitAlongX, cutPos);

                // move the intersect point so that it's outside the cut instead of directly on it
                if (!includeCut)
                    if (splitAlongX)
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

    /// <summary>
    ///     if a point is inside the cut polygon
    /// </summary>
    /// <param name="point"></param>
    /// <param name="cutXAxis"></param>
    /// <param name="cutCoord"></param>
    /// <param name="keepLower"></param>
    /// <param name="includeCut"></param>
    /// <returns></returns>
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

    /// <summary>
    ///     efficient way of getting the area of a shape after cutting along an axis. excludes area along cut
    /// </summary>
    /// <param name="splitAlongX"></param>
    /// <param name="cutPos"></param>
    /// <param name="keepLower">if the upper or lower half is used for the area calculation</param>
    /// <param name="preciseArea">if the area-getting algorithm uses a precise (though slower) version</param>
    /// <returns></returns>
    public Shape? CutOnce(bool splitAlongX, int cutPos, bool keepLower, bool preciseArea) => ClipPolygon(splitAlongX, cutPos, keepLower, false);

    #endregion
}