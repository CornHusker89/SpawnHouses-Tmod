#nullable enable

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using SpawnHouses.Structures;
using SpawnHouses.Types;
using Terraria;
using Terraria.DataStructures;
using Terraria.ID;

namespace SpawnHouses.AdvStructures.AdvStructureParts;

#pragma warning disable CS0659 // Type overrides Object.Equals(object o) but does not override Object.GetHashCode()

/// <summary>
/// generic 2D shape. has support for triangles, rectangles, and n-gons.
/// </summary>
/// <remarks>it's assumed that the points are in clockwise order</remarks>
public class Shape {
    private static readonly Color[] Colors = [
        Color.White,
        Color.Black,
        Color.Aquamarine,
        Color.Red
    ];

    private int _area = -1;
    private int _expandedArea = -1;

    private Point16 _center = new(-1, -1);

    public (Point16 topLeft, Point16 bottomRight) BoundingBox;
    public Point16[] Points;
    public Point16 Size;


    public bool IsBox { get; private set; } // because many of the shapes will be boxes, introduce optimizations for boxes

    /// <summary>
    ///     the amount of tiles this shape encloses
    /// </summary>
    public int Area {
        get {
            if (_area == -1)
                _area = GetArea();
            return _area;
        }
    }

    /// <summary>
    ///     the number of tiles this shape encloses if it were expanded by 1 tile in every direction
    /// </summary>
    public int ExpandedArea {
        get {
            if (_expandedArea == -1)
                _expandedArea = GetExpandedShape(1).Area;
            return _expandedArea;
        }
    }

    public Point16 Center => BoundingBox.topLeft + Size / new Point16(2, 2);

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

    private void Init(Point16[] points, bool optimize) {
        _center = new Point16(-1, -1);
        _area = -1;
        _expandedArea = -1;
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
                if (points.Length <= 3) {
                    Points = points;
                    break;
                }

                Points = optimize ? OptimizePoints(points) : points;

                switch (Points.Length) {
                    // this will only happen if the points are the same (shape area of 1)
                    case 1:
                        IsBox = true;
                        break;
                    // test if the points form a box
                    case 4: {
                        HashSet<int> x = [], y = [];
                        foreach (Point16 point in Points) {
                            x.Add(point.X);
                            y.Add(point.Y);
                        }
                        IsBox = x.Count <= 2 && y.Count <= 2;
                        break;
                    }
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

    /// <summary>
    /// removes extra points in shape.
    /// </summary>
    /// <param name="points"></param>
    /// <returns></returns>
    /// <remarks>destructive, returns the input list</remarks>
    public static Point16[] OptimizePoints(List<Point16> points) {
        for (int i = 0; i < points.Count; i++) {
            Point16 last = points[i - 1 != -1 ? i - 1 : points.Count - 1];
            Point16 target = points[i];
            Point16 next = points[i + 1 != points.Count ? i + 1 : 0];

            if (points.Count == 1) {
                break;
            }

            if (target == next) {
                points.RemoveAt(i);
                i--;
                continue; // so that we don't interfere with the next condition
            }

            if ((target.X == last.X && target.X == next.X && ((last.Y < target.Y && target.Y < next.Y) || (last.Y > target.Y && target.Y > next.Y)))
                || (target.Y == last.Y && target.Y == next.Y && ((last.X < target.X && target.X < next.X) || (last.X > target.X && target.X > next.X)))) {
                points.RemoveAt(i);
                i--;
            }
        }

        if (points[0] == points[^1] && points.Count > 1) {
            points.RemoveAt(points.Count - 1);
        }
        
        // ensure the shape isn't 1 wide/tall, which involves overlapping points by nature
        bool same = true;
        for (int i = 0; i < points.Count; i++) {
            Point16 target = points[i];
            Point16 next = points[i + 1 != points.Count ? i + 1 : 0];
            if (next != target) {
                same = false;
            }
        }
        if (same) {
            return [points[0]];
        }

        return points.ToArray();
    }

    public static Point16[] OptimizePoints(Point16[] points) => OptimizePoints(points.ToList());


#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.

    /// <param name="points">If only 2 points are passed, will assume a box</param>
    /// <returns></returns>
    public Shape(params Point16[] points) {
        Init(points, true);
    }

    /// <param name="points">If only 2 points are passed, will assume a box</param>
    /// <returns></returns>
    public Shape(IEnumerable<Point16> points) {
        var pointsArray = points.ToArray();
        Init(pointsArray, true);
    }

    private Shape(IEnumerable<Point16> points, bool optimize) {
        var pointsArray = points.ToArray();
        Init(pointsArray, optimize);
    }

#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.


    #region Shape Self-Geometry
    
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

    /// <summary>
    /// normalize vector, intended to be used when getting edge/vertex normals
    /// </summary>
    /// <param name="normal"></param>
    /// <param name="round"></param>
    /// <returns></returns>
    private static (double x, double y) Normalize((double x, double y) normal, bool round) {
        double largestMagnitude = Math.Max(double.Abs(normal.x), double.Abs(normal.y));
        if (largestMagnitude < 0.00001f) {
            return (0, 0);
        }

        double normalX = normal.x / largestMagnitude;
        double normalY = normal.y / largestMagnitude;
        return (round ? Math.Round(normalX) : normalX, round ? Math.Round(normalY) : normalY);
    }

    /// <summary>
    /// gets outward facing normals for each edge
    /// </summary>
    /// <param name="round">if each normal is rounded to 0 or 1</param>
    /// <returns></returns>
    /// <remarks>edge index 0 is the edge between verts 0 and 1, with this pattern continuing and wrapping around</remarks>
    public (double x, double y)[] GetEdgeNormals(bool round) {
        int count = Points.Length;
        (double x, double y)[] normals = new (double, double)[count];

        for (int i = 0; i < count; i++) {
            Point16 p1 = Points[i];
            Point16 p2 = Points[(i + 1) % count];
            Point16 edge = p2 - p1;

            // rotate 90° counterclockwise for outward normal
            Point16 normal = new(edge.Y, -edge.X);
            normals[i] = Normalize((normal.X, normal.Y), round);
        }

        return normals;
    }

    /// <summary>
    /// gets outward facing normals for each vertex
    /// </summary>
    /// <returns></returns>
    /// <remarks>rounds each vector component to 0 or 1</remarks>
    public Point16[] GetVertexNormals() {
        int count = Points.Length;
        var normals = new Point16[count];
        var edgeNormals = GetEdgeNormals(false);

        for (int i = 0; i < count; i++) {
            // average the normals of the two adjacent edges
            (double x, double y) n1 = edgeNormals[(i - 1 + count) % count];
            (double x, double y) n2 = edgeNormals[i];
            (double x, double y) normal = Normalize(((n1.x + n2.x) / 2, (n1.y + n2.y) / 2), true);
            normals[i] = new Point16((int)normal.x, (int)normal.y);
        }

        return normals;
    }

    /// <summary>
    ///     gets ratio of bounding box size to actual shape area. can indicate how box-like the shape is
    /// </summary>
    /// <returns></returns>
    public double GetBoundingBoxEfficiency() => (double)Size.X * Size.Y / Area;

    /// <summary>
    ///     gets number of tiles that are within the bounding box but not in the shape. can indicate how box-like the shape is
    /// </summary>
    /// <returns></returns>
    public int GetUnusedBoundingBoxArea() => Size.X * Size.Y - Area;

    /// <summary>
    ///     returns list of points, expanded by their outward facing normals
    /// </summary>
    /// <param name="expansion">the number of tiles to expand each point by (only whole numbers)</param>
    private Point16[] ExpandPoints(int expansion) {
        var expandedPoints = new Point16[Points.Length];
        var normals = GetVertexNormals();
        for (int i = 0; i < Points.Length; i++) {
            expandedPoints[i] = Points[i] + normals[i];
        }
        return expandedPoints;
    }

    /// <summary>
    ///     expands shape by <see cref="expansion"/> tiles
    /// </summary>
    /// <param name="expansion">the number of tiles to expand each point by (only whole numbers)</param>
    public void Expand(int expansion) {
        Points = ExpandPoints(expansion);
    }

    /// <summary>
    ///     creates new shape, expanded by <see cref="expansion"/> tiles
    /// </summary>
    /// <returns></returns>
    public Shape GetExpandedShape(int expansion) {
        return new Shape(ExpandPoints(expansion), false);
    }
    
    /// <summary>
    ///     find all corners of a shape based on their x and y positions, useful for ensuring beams and such make sense visually
    /// </summary>
    /// <returns></returns>
    public List<Point16> GetCorners() {
        List<Point16> corners = [];
        Shape expandedShape = GetExpandedShape(1);

        foreach (Point16 point in expandedShape.Points) {
            bool xCorner = point.X != expandedShape.BoundingBox.topLeft.X
                && point.X != expandedShape.BoundingBox.bottomRight.X;
            bool yCorner = point.Y != expandedShape.BoundingBox.topLeft.Y 
                && point.Y != expandedShape.BoundingBox.bottomRight.Y;
            if (xCorner && yCorner) {
                corners.Add(new Point16(point.X, point.Y));
            }
            else if (xCorner && !yCorner) {
                corners.Add(new Point16(point.X, (short)-1));
            }
            else if (!xCorner && yCorner) {
                corners.Add(new Point16((short)-1, point.Y));
            }
        }
        
        // sanitize list to remove repeat values
        for (int i = 0; i < corners.Count; i++) {
            Point16 target = corners[i];
            if (target.X == -1 == (target.Y == -1)) { // only check cases where only 1 axis is valid
                continue;
            }
            
            Point16 last = corners[i - 1 != -1 ? i - 1 : corners.Count - 1];
            Point16 next = corners[i + 1 != corners.Count ? i + 1 : 0];

            if ((last.X != -1 && last.Y != -1 && (target.X == last.X || target.Y == last.Y))
                || (next.X != -1 && next.Y != -1 && (target.X == next.X || target.Y == next.Y))) {
                corners.RemoveAt(i);
                i--;
            }
        }

        return corners;
    }

    /// <summary>
    ///     offsets entire shape by given point
    /// </summary>
    /// <param name="offset"></param>
    public void Offset(Point16 offset) {
        for (int i = 0; i < Points.Length; i++) {
            Points[i] += offset;
        }
        Init(Points, false);
    }


    public (int min, int max, double average) GetTrueSize(bool xAxis) {
        Dictionary<int, int> minValues = [], maxValues = [];
        ExecuteInArea((x, y) => {
            if (!minValues.TryGetValue(xAxis ? y : x, out int oldMinValue)) {
                minValues[xAxis ? y : x] = x;
            }
            else {
                if (x < oldMinValue) {
                    minValues[xAxis? y : x] = y;
                }
            }
            if (!maxValues.TryGetValue(xAxis ? y : x, out int oldMaxValue)) {
                maxValues[xAxis ? y : x] = x;
            }
            else {
                if (x < oldMaxValue) {
                    maxValues[xAxis? y : x] = y;
                }
            }
        });
        
        // get the actual size for each slice using the min/max values for that slice
        List<int> sizes = [];
        foreach (int key in minValues.Keys) {
            sizes.Add(maxValues[key] - minValues[key]);
        }

        if (sizes.Count == 0) {
            throw new Exception("shape must have a minimum area of 1");
        }

        int min = sizes[0], max = sizes[0], average = 0;
        foreach (int size in sizes) {
            if (size < min) {
                min = size;
            }

            if (size > max) {
                max = size;
            }
            average += size;
        }
        average /= sizes.Count;
        return (min, max, average);
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
        if (IsBox)
            for (int x = BoundingBox.topLeft.X; x <= BoundingBox.bottomRight.X; x++)
            for (int y = BoundingBox.topLeft.Y; y <= BoundingBox.bottomRight.Y; y++)
                action(x, y);
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
                if (p1.Y == BoundingBox.bottomRight.Y) {
                    if (p1.Y == p2.Y) {
                        int startX = Math.Min(p1.X, p2.X);
                        int endX = Math.Max(p1.X, p2.X);
                        for (int x = startX + 1; x <= endX - 1; x++)
                            if (!Points.Contains(new Point16(x, p1.Y)))
                                action(x, p1.Y);
                    }
                }
            }

            foreach (Point16 point in Points) {
                action(point.X, point.Y);
            }
        }
    }

    /// <summary>
    ///     runs action on every tile contained in the shape, passing the predicted tile slope
    /// </summary>
    /// <param name="action"></param>
    /// <param name="slopingAlgorithm">
    ///     function to determine the <see cref="BlockType"/> passed to the action.
    ///     if none is passed, this function will only pass <see cref="BlockType.Solid"/>.
    ///     examples of these functions are found in <see cref="Helpers.SlopeHelper"/>
    /// </param>
    public void ExecuteInArea(Action<int, int, BlockType> action, Func<int, int, bool[,], BlockType>? slopingAlgorithm = null) {
        if (slopingAlgorithm is null) {
            ExecuteInArea((x, y) => {
                action(x, y, BlockType.Solid);
            });
            return;
        }
        
        bool[,] tilemap = new bool[Size.X, Size.Y];
        ExecuteInArea((x, y) => tilemap[x - BoundingBox.topLeft.X, y - BoundingBox.topLeft.Y] = true);
        for (int x = 0; x < Size.X; x++) {
            int xWorldCoord = x + BoundingBox.topLeft.X;
            for (int y = 0; y < Size.Y; y++) {
                int yWorldCoord = y + BoundingBox.topLeft.Y;
                if (!tilemap[x, y]) {
                    continue;
                }
                action(xWorldCoord, yWorldCoord, slopingAlgorithm(x, y, tilemap));
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

        public bool Overlaps(Projection other) {
            return !(Max < other.Min || other.Max < Min);
        }
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

    #endregion


    #region Slicing Shape

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
            if (dy == 0) return new Point16(p1.X, cutCoord); // horizontal line edge case
            double t = (cutCoord - p1.Y) / (double)dy;
            int newX = (int)Math.Round(p1.X + t * dx);
            return new Point16(newX, cutCoord);
        }
        else {
            if (dx == 0) return new Point16(cutCoord, p1.Y); // vertical line edge case
            double t = (cutCoord - p1.X) / (double)dx;
            int newY = (int)Math.Round(p1.Y + t * dy);
            return new Point16(cutCoord, newY);
        }
    }

    #endregion


    #region Shapes From Tilemap

    /// <summary>
    /// gets a shape that represents the interior of the structure, and excludes any exterior components
    /// </summary>
    /// <param name="tilemap"></param>
    /// <returns></returns>
    /// <exception cref="Exception"></exception>
    /// <remarks>assumes only one interior in the tilemap</remarks>
    public static Shape GetStructureInterior(StructureTilemap tilemap) {
        List<Point16> outline = [];

        Point16? start = null;
        for (int y = 0; y < tilemap.Height - 1 && start == null; y++)
        for (int x = 0; x < tilemap.Width - 1; x++)
            if (GetMarchingSquareIndex(tilemap, x, y) != 0) {
                start = new Point16(x, y);
                break;
            }

        if (start == null)
            throw new Exception("valid shape not found from tilemap");

        Point16 pos = start.Value;
        Point16 dir = new (0, 1);
        var visited = new HashSet<Point16>();
        int steps = 0;
        int maxSteps = tilemap.Width * tilemap.Height * 4;

        do {
            // add previous iteration's position
            visited.Add(pos);

            int value = GetMarchingSquareIndex(tilemap, pos.X, pos.Y);

            // assumes clockwise direction
            Point16 nextDir = value switch {
                1 => new Point16(0, 1), // BL only: down
                2 => new Point16(1, 0), // BR only: right
                3 => new Point16(1, 0), // BL + BR: right
                4 => new Point16(0, -1), // TR only: up
                5 => dir.X == -1 ? new Point16(0, -1) : new Point16(0, 1), // BL + TR: up if we were going left, otherwise down
                6 => new Point16(0, -1), // BR + TR: up
                7 => new Point16(0, -1), // BL + BR + TR: up
                8 => new Point16(-1, 0), // TL only: left
                9 => new Point16(0, 1), // BL + TL: down
                10 => dir.Y == -1 ? new Point16(0, 1) : new Point16(0, -1), // BR + TL: down if we were going left, otherwise up
                11 => new Point16(1, 0), // BL + BR + TL: right
                12 => new Point16(-1, 0), // TR + TL: left
                13 => new Point16(0, 1), // BL + TR + TL: down
                14 => new Point16(-1, 0), // BR + TR + TL: left
                _ => new Point16(0, 0), // 0 or 15
            };

            Point16 outlineOffset = value switch {
                1 => new Point16(0, 0), // BL only: BL
                2 => new Point16(1, 0), // BR only: BR
                3 => new Point16(0, 0), // BL + BR: BL
                4 => new Point16(1, -1), // TR only: TR
                5 => dir.X == -1 ? new Point16(1, -1) : new Point16(0, 0), // BL + TR: TR if we were going left, otherwise BL
                6 => new Point16(1, 0), // BR + TR: BR
                7 => new Point16(1, 0), // BL + BR + TR: BR
                8 => new Point16(0, -1), // TL only: TL
                9 => new Point16(0, -1), // BL + TL: TL
                10 => dir.Y == -1 ? new Point16(1, 0) : new Point16(0, -1), // BR + TL: BR if we were going left, otherwise TL
                11 => new Point16(0, 0), // BL + BR + TL: BL
                12 => new Point16(1, -1), // TR + TL: TR
                13 => new Point16(0, -1), // BL + TR + TL: TL
                14 => new Point16(1, -1), // BR + TR + TL: TR
                _ => new Point16(0, 0), // 0 or 15
            };

            if (nextDir != dir) {
                outline.Add(pos + outlineOffset);
            }

            pos += nextDir;
            dir = nextDir;
            steps++;
        } while (pos != start.Value && !visited.Contains(pos) && steps < maxSteps);

        return new Shape(outline.ToArray());
    }

    /// <summary>
    /// converts 2x2 grid cell into a marching square index
    /// </summary>
    /// <param name="tilemap"></param>
    /// <param name="x"></param>
    /// <param name="y"></param>
    /// <returns></returns>
    private static int GetMarchingSquareIndex(StructureTilemap tilemap, int x, int y) {
        int value = 0;
        // bottom-left
        if (tilemap.InInterior(x, y))
            value |= 1;

        // bottom-right
        if (tilemap.InInterior(x + 1, y))
            value |= 2;

        // top-right
        if (tilemap.InInterior(x + 1, y - 1))
            value |= 4;

        // top-left
        if (tilemap.InInterior(x, y - 1))
            value |= 8;

        return value;
    }

    #endregion
}