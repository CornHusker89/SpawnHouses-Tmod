#nullable enable
#pragma warning disable CS0659 // Type overrides Object.Equals(object o) but does not override Object.GetHashCode()

using System;
using System.Collections.Generic;
using System.Linq;
using SpawnHouses.Common.DataStructures;
using SpawnHouses.Helpers;
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

    private (bool hasTile, Direction outwardNormal)[,]? _perimeterTilemap;
    private bool[,]? _tilemap;
    private Point16[]? _exteriorDrawPath;

    /// <summary>
    ///     if there is a tile at a specific coordinate for the interior of the shape, 0-indexed
    /// </summary>
    public (bool hasTile, Direction outwardNormal)[,] PerimeterTilemap {
        get {
            return _perimeterTilemap ??= GetPerimeterTilemap();
        }
    }

    /// <summary>
    ///     if there is a tile at a specific coordinate for the interior of the shape, 0-indexed
    /// </summary>
    public bool[,] Tilemap {
        get { return _tilemap ??= GetTilemap(); }
    }

    /// <summary>
    ///     path of points to draw the exterior of the shape, in local (relative to the shape's topleft bounding box corner),  world coordinates (not tile)
    /// </summary>
    /// <returns></returns>
    public Point16[] ExteriorDrawPath {
        get { return _exteriorDrawPath ??= GetExteriorDrawPath(); }
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


        _tilemap = null;
        _exteriorDrawPath = null;
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

    private Point16 ToLocal(Point16 point) => point - BoundingBox.topLeft;
    private (int x, int y) ToLocal(int x, int y) => (x - BoundingBox.topLeft.X, y - BoundingBox.topLeft.Y);
    private Point16 ToGlobal(Point16 point) => point + BoundingBox.topLeft;
    private (int x, int y) ToGlobal(int x, int y) => (x + BoundingBox.topLeft.X, y + BoundingBox.topLeft.Y);

    /// <summary>
    ///     expects shape's local coords
    /// </summary>
    public bool IsInsideBoundingBoxLocal(int x, int y) =>
        x >= 0
        && x < Size.X
        && y >= 0
        && y < Size.Y;

    /// <summary>
    ///     expects shape's local coords
    /// </summary>
    public bool IsInsideBoundingBoxLocal(Point16 pos) => IsInsideBoundingBoxLocal(pos.X, pos.Y);

    /// <summary>
    ///     expects shape's global coords
    /// </summary>
    public bool IsInsideBoundingBoxGlobal(int x, int y) =>
        x >= BoundingBox.topLeft.X
        && x <= BoundingBox.bottomRight.X
        && y >= BoundingBox.topLeft.Y
        && y <= BoundingBox.bottomRight.Y;

    /// <summary>
    ///     expects shape's global coords
    /// </summary>
    public bool IsInsideBoundingBoxGlobal(Point16 point) => IsInsideBoundingBoxLocal(ToLocal(point));
    
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
    ///     creates a new shape, moved by the offset. ex. if offset = (3, 0) will move shape 3 to the right in world coordinates
    /// </summary>
    /// <param name="offset"></param>
    public Shape GetMovedShape(Point16 offset) {
        var newPoints = new Point16[Points.Length];
        for (int i = 0; i < Points.Length; i++) newPoints[i] = Points[i] + offset;
        return new Shape(newPoints, optimize: false);
    }

    /// <summary>
    ///     returns this geometry's points with a uniform expansion
    /// </summary>
    /// <param name="distance"></param>
    /// <returns></returns>
    private Point16[] GetExpansionEven(int distance) {
        if (distance < 0 && (-distance > Size.X / 2 || -distance > Size.Y / 2))
            throw new Exception("negative shape expansion amount is larger than an axis size, at risk of turning shape inside-out");
        
        bool clockwise = IsClockwise();
        int count = Points.Length;
        var result = new Point16[count];

        for (int i = 0; i < count; i++) {
            Point16 prev = Points[(i - 1 + count) % count];
            Point16 curr = Points[i];
            Point16 next = Points[(i + 1) % count];

            OffsetEdgeEven(prev, curr, distance, clockwise, out Point16 e1A, out Point16 e1B);
            OffsetEdgeEven(curr, next, distance, clockwise, out Point16 e2A, out Point16 e2B);

            Point16 vertex = LineIntersection(e1A, e1B, e2A, e2B);

            if ((vertex - curr).ToVector2().Length() > distance * distance * 16)
                vertex = e1B; // bevel fallback

            result[i] = vertex;
        }
    
        return result;
    }
    
    /// <summary>
    ///     expands shape by <see cref="expansion" /> tiles
    /// </summary>
    /// <param name="expansion">the number of tiles to expand each point by (only whole numbers)</param>
    public void Expand(int expansion) {
        Init(GetExpansionEven(expansion).ToArray(),
            false);
    }

    /// <summary>
    ///     creates new shape, expanded by <paramref name="expansion" /> tiles
    /// </summary>
    /// <returns></returns>
    public Shape GetExpandedShape(int expansion) => new(GetExpansionEven(expansion).ToArray()
    );

    /// <summary>
    ///     find all corners of a shape (expanded out by 1 tile) based on their x and y positions, useful for ensuring beams and such make sense visually
    /// </summary>
    /// <param name="significantAngle">only vertices that create a deviation (in deg) larger than this will be considered</param>
    /// <returns></returns>
    public List<PartialPoint32> GetCorners(float significantAngle = 30f) {
        List<PartialPoint32> corners = [];
        foreach (Point16 point in GetExpandedShape(1).CollapseVertices(significantAngle)) {
            bool xCorner = point.X > BoundingBox.topLeft.X
                           && point.X < BoundingBox.bottomRight.X;
            bool yCorner = point.Y > BoundingBox.topLeft.Y
                           && point.Y < BoundingBox.bottomRight.Y;
            if (xCorner && yCorner)
                corners.Add(new PartialPoint32(point));
            else if (xCorner && !yCorner)
                corners.Add(new PartialPoint32(point.X, 0, hasY: false));
            else if (!xCorner && yCorner)
                corners.Add(new PartialPoint32(0, point.Y, false));
        }

        // sanitize list to remove repeat values
        for (int i = 0; i < corners.Count; i++) {
            PartialPoint32 target = corners[i];
            if (target.HasX == target.HasY) // only check cases where only 1 axis is valid
                continue;

            PartialPoint32 last = corners[i - 1 != -1 ? i - 1 : corners.Count - 1];
            PartialPoint32 next = corners[i + 1 != corners.Count ? i + 1 : 0];

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
    ///     makes a 2d bool array of this shape showing if there is a tile at a given local coordinate along the edge of the shape,
    ///     always 0-indexed, puts into <see cref="_tilemap"/>
    /// </summary>
    /// <returns></returns>
    private (bool hasTile, Direction outwardNormal)[,] GetPerimeterTilemap() {
        var map = new (bool hasTile, Direction outwardNormal)[Size.X, Size.Y];

        if (IsBox) {
            for (int x = 0; x < Size.X; x++)
                map[x, 0] = (true, Direction.Up);
            for (int x = 0; x < Size.X; x++)
                map[x, Size.Y - 1] = (true, Direction.Down);
            for (int y = 0; y < Size.Y; y++)
                map[0, y] = (true, Direction.Left);
            for (int y = 0; y < Size.Y; y++)
                map[Size.X - 1, y] = (true, Direction.Right);
        }
        else {
            for (int i = 0; i < Points.Length; i++) {
                Point16 p0 = ToLocal(Points[i]);
                Point16 p1 = ToLocal(Points[(i + 1) % Points.Length]);
                int x0 = p0.X;
                int y0 = p0.Y;
                int x1 = p1.X;
                int y1 = p1.Y;

                int ex = p1.X - p0.X;
                int ey = p1.Y - p0.Y;

                // calc normal for whole line
                int nx = ey;
                int ny = -ex;
                Direction normal;
                if (Math.Abs(nx) > Math.Abs(ny))
                    normal = nx > 0 ? Direction.Right : Direction.Left;
                else
                    normal = ny > 0 ? Direction.Down : Direction.Up;

                int dx = Math.Abs(x1 - x0);
                int sx = x0 < x1 ? 1 : -1;

                int dy = -Math.Abs(y1 - y0);
                int sy = y0 < y1 ? 1 : -1;

                int err = dx + dy;

                while (true) {
                    map[x0, y0] = (true, normal);

                    if (x0 == x1 && y0 == y1)
                        break;

                    int e2 = 2 * err;

                    if (e2 >= dy) {
                        err += dy;
                        x0 += sx;
                    }

                    if (e2 <= dx) {
                        err += dx;
                        y0 += sy;
                    }
                }
            }
        }

        return map;
    }

    /// <summary>
    ///     makes a 2d bool array of this shape showing if there is a tile at a given local coordinate within the shape,
    ///     always 0-indexed
    /// </summary>
    /// <returns></returns>
    private bool[,] GetTilemap() {
        bool[,] map = new bool[Size.X, Size.Y];

        if (IsBox) {
            for (int x = 0; x < Size.X; x++)
            for (int y = 0; y < Size.Y; y++)
                map[x, y] = true;

            return map;
        }

        var perimeterMap = PerimeterTilemap;
        for (int y = 0; y < Size.Y; y++) {
            // ensure that all edges are consistently added
            for (int x = 0; x < Size.X; x++)
                if (perimeterMap[x, y].hasTile)
                    map[x, y] = true;

            // do normal scanline
            double scanY = y + 0.5;
            List<double> intersections = [];

            for (int i = 0; i < Points.Length; i++) {
                Point16 p1 = ToLocal(Points[i]);
                Point16 p2 = ToLocal(Points[(i + 1) % Points.Length]);

                if ((p1.Y <= scanY && p2.Y > scanY) ||
                    (p2.Y <= scanY && p1.Y > scanY)) {
                    double intersectX =
                        p1.X + (scanY - p1.Y) *
                        (p2.X - p1.X) /
                        (p2.Y - p1.Y);

                    intersections.Add(intersectX);
                }
            }

            intersections.Sort();

            for (int i = 0; i + 1 < intersections.Count; i += 2) {
                int startX = (int)Math.Ceiling(intersections[i] + 0.5);
                int endX = (int)Math.Floor(intersections[i + 1] + 0.5);

                for (int x = startX; x <= endX; x++)
                    map[x, y] = true;
            }
        }

        return map;
    }

    /// <summary>
    ///     gets a path around the outside of the shape, in local world coordinates (not tile)
    /// </summary>
    /// <returns></returns>
    private Point16[] GetExteriorDrawPath() {
        List<Point16> path = [];

        // find start tile
        bool found = false;
        Point16 start = default;
        for (int x = 0; x < Size.X && !found; x++)
        for (int y = 0; y < Size.Y && !found; y++) {
            if (PerimeterTilemap[x, y].hasTile) {
                start = new Point16(x, y);
                found = true;
            }
        }

        if (!found)
            throw new Exception();

        // traverse perimeter
        bool loopComplete = false;
        int safety = Size.X * Size.Y * 2;
        int steps = 0;
        Point16 pos = start;
        Point16 prev = start + new Point16(0, 1); // because of the search pattern for the first tile, prev must be below in some way
        do {
            steps++;
            Direction direction = DirectionUtils.GetDirectionFromPoints(pos, prev);
            for (int i = 0; i < 8; i++) {
                direction = DirectionUtils.TurnRight(direction, true);

                // append to draw path on each corner traversal
                switch (direction) {
                    case Direction.UpRight:
                        path.Add((pos + BoundingBox.topLeft) * new Point16(16) + new Point16(16, 0));
                        break;
                    case Direction.DownRight:
                        path.Add((pos + BoundingBox.topLeft) * new Point16(16) + new Point16(16, 16));
                        break;
                    case Direction.DownLeft:
                        path.Add((pos + BoundingBox.topLeft) * new Point16(16) + new Point16(0, 16));
                        break;
                    case Direction.UpLeft:
                        path.Add((pos + BoundingBox.topLeft) * new Point16(16) + new Point16(0, 0));
                        break;
                }

                if (path.Count >= 3 && path[^1] == path[0]) {
                    path.RemoveAt(path.Count - 1);
                    loopComplete = true;
                    break;
                }

                // if possible, advance in the current direction
                Point16 testPos = DirectionUtils.Offset(pos, direction);
                if (IsInsideBoundingBoxLocal(testPos) && PerimeterTilemap[testPos.X, testPos.Y].hasTile) {
                    prev = pos;
                    pos = testPos;
                    break;
                }
            }
        } while (!loopComplete && steps < safety);

        if (steps == safety && Size != new Point16(1)) throw new Exception("perimeter traversal took longer than should be possible");

        for (int i = 0; i < path.Count - 1; i++) {
            if (path[i] == path[i + 1]) {
                path.RemoveAt(i);
                i--;
            }
        }
        
        return path.ToArray();
    }

    /// <summary>
    ///     returns true if the shape's points are in clockwise order, otherwise false
    /// </summary>
    /// <returns></returns>
    public bool IsClockwise() {
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
    public void ExecuteOnPerimeter(Action<int, int, Direction> action) {
        if (IsBox) {
            // go line-by-line
            for (int x = BoundingBox.topLeft.X; x <= BoundingBox.bottomRight.X; x++)
                action.Invoke(x, BoundingBox.topLeft.Y, Direction.Up);
            for (int x = BoundingBox.topLeft.X; x <= BoundingBox.bottomRight.X; x++)
                action.Invoke(x, BoundingBox.bottomRight.Y, Direction.Down);
            for (int y = BoundingBox.topLeft.Y; y <= BoundingBox.bottomRight.Y; y++)
                action.Invoke(BoundingBox.topLeft.X, y, Direction.Left);
            for (int y = BoundingBox.topLeft.Y; y <= BoundingBox.bottomRight.Y; y++)
                action.Invoke(BoundingBox.bottomRight.X, y, Direction.Right);
        }
        else {
            var map = PerimeterTilemap;
            for (int x = BoundingBox.topLeft.X; x <= BoundingBox.bottomRight.X; x++)
            for (int y = BoundingBox.topLeft.Y; y <= BoundingBox.bottomRight.Y; y++) {
                (int localX, int localY) = ToLocal(x, y);
                if (map[localX, localY].hasTile)
                    action.Invoke(x, y, map[localX, localY].outwardNormal);
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
                action.Invoke(x, y);
        }
        else {
            bool[,] map = Tilemap;
            for (int x = BoundingBox.topLeft.X; x <= BoundingBox.bottomRight.X; x++)
            for (int y = BoundingBox.topLeft.Y; y <= BoundingBox.bottomRight.Y; y++) {
                (int localX, int localY) = ToLocal(x, y);
                if (map[localX, localY])
                    action.Invoke(x, y);
            }

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

        bool[,] map = Tilemap;
        for (int x = BoundingBox.topLeft.X; x <= BoundingBox.bottomRight.X; x++) {
            for (int y = BoundingBox.topLeft.Y; y < BoundingBox.bottomRight.Y; y++) {
                (int localX, int localY) = ToLocal(x, y);
                if (map[localX, localY])
                    action(x, y, slopingAlgorithm(localX, localY, Tilemap));
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

    public (Shape? lower, Shape? middle, Shape? higher) SplitTwice(bool cutXAxis, int cutPos1, int cutPos2) {
        if (cutPos2 < cutPos1)
            (cutPos1, cutPos2) = (cutPos2, cutPos1);

        // upper left piece
        Shape? shapeA = SplitOnce(cutXAxis, cutPos1, true, false);

        // remainder after the first cut
        Shape? remainder = SplitOnce(cutXAxis, cutPos1, false, true);

        // middle piece (clip remainder again)
        Shape? shapeB = remainder?.SplitOnce(cutXAxis, cutPos2, true, true);

        // lower right piece
        Shape? shapeC = remainder?.SplitOnce(cutXAxis, cutPos2, false, false);

        return (shapeA, shapeB, shapeC);
    }


    /// <summary>
    ///     efficient way of getting the area of a shape after cutting along an axis. excludes area along cut
    /// </summary>
    /// <param name="splitAlongX"></param>
    /// <param name="cutPos"></param>
    /// <param name="keepLower">if the upper or lower half is used for the area calculation</param>
    /// <param name="includeCut">if the cut space itself is included in the cut</param>
    /// <returns></returns>
    public Shape? SplitOnce(bool splitAlongX, int cutPos, bool keepLower, bool includeCut) {
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
    ///     split the shape along an axis such that each section has the same width, and return the get split coordinates 
    /// </summary>
    /// <param name="alongXAxis"></param>
    /// <param name="targetSectionWidth"></param>
    /// <param name="splitWidth"></param>
    /// <param name="minSectionWidth"> defaults to 2/3 of the target section width, rounded</param>
    /// <returns>the lower coordinate of each split</returns>
    public HashSet<int> GetEvenSplits(bool alongXAxis, int targetSectionWidth, int splitWidth, int minSectionWidth = -1) {
        if (minSectionWidth == -1)
            minSectionWidth = (int)Math.Round(targetSectionWidth * 0.67);
        if (splitWidth <= 0)
            throw new ArgumentException("split width must be greater than zero");

        int bestSectionCount = 1;
        double bestError = double.MaxValue;

        int maxSections = ((alongXAxis ? Size.X : Size.Y) + splitWidth) / (minSectionWidth + splitWidth);

        for (int n = 1; n <= maxSections; n++) {
            int usable = (alongXAxis ? Size.X : Size.Y) - (n - 1) * splitWidth;

            if (usable < n * minSectionWidth)
                continue;

            double avg = (double)usable / n;
            double error = Math.Abs(avg - targetSectionWidth);

            if (error < bestError) {
                bestError = error;
                bestSectionCount = n;
            }
        }

        int sectionCount = bestSectionCount;
        int usableWidth = (alongXAxis ? Size.X : Size.Y) - (sectionCount - 1) * splitWidth;

        int baseWidth = usableWidth / sectionCount;
        int remainder = usableWidth % sectionCount;

        // initialize all sections
        int[] sections = new int[sectionCount];
        for (int i = 0; i < sectionCount; i++)
            sections[i] = baseWidth;

        // distribute remainder symmetrically from center outward
        int left = (sectionCount - 1) / 2;
        int right = sectionCount / 2;

        while (remainder > 0) {
            if (left == right) {
                sections[left]++;
                remainder--;
            }
            else {
                sections[left]++;
                remainder--;

                if (remainder > 0) {
                    sections[right]++;
                    remainder--;
                }
            }

            left--;
            right++;
        }

        // compute split positions
        var splits = new List<int>();
        int currentSplitCoordinate = 0;

        for (int i = 0; i < sectionCount - 1; i++) {
            currentSplitCoordinate += sections[i];
            splits.Add(currentSplitCoordinate);
            currentSplitCoordinate += splitWidth;
        }

        return splits.ToHashSet();
    }

    #endregion
}