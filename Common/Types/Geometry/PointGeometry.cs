using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Terraria.DataStructures;

namespace SpawnHouses.Common.Types.Geometry;

public abstract class PointGeometry {
    /// <summary>
    ///     bounding corners. in global coordinates
    /// </summary>
    public (Point16 topLeft, Point16 bottomRight) BoundingBox;

    /// <summary>
    ///     geometry points. in global coordinates
    /// </summary>
    public Point16[] Points;
    public Point16 Size;

    /// <summary>
    ///     the geometric center of the bounding box
    /// </summary>
    public Point16 Center => BoundingBox.topLeft + Size / new Point16(2, 2);

    protected abstract void Init(Point16[] points, bool optimize);

    protected void SetBoundingBoxAndSize() {
        int minX = int.MaxValue, maxX = 0, minY = int.MaxValue, maxY = 0;
        foreach (Point16 point in Points) {
            minX = Math.Min(point.X, minX);
            maxX = Math.Max(point.X, maxX);
            minY = Math.Min(point.Y, minY);
            maxY = Math.Max(point.Y, maxY);
        }

        BoundingBox = (new Point16(minX, minY), new Point16(maxX, maxY));
        Size = new Point16(1 + maxX - minX, 1 + maxY - minY);
    }

    protected static int Cross(Point16 o, Point16 a, Point16 b) => (a.X - o.X) * (b.Y - o.Y) - (a.Y - o.Y) * (b.X - o.X);
    protected static float Cross(Vector2 a, Vector2 b) => a.X * b.Y - a.Y * b.X;
    protected static Vector2 Perp(Vector2 v) => new(v.Y, -v.X);
    
    /// <summary>
    /// </summary>
    /// <param name="edgeIndex">edge index to retrieve</param>
    /// <returns></returns>
    /// <remarks>edge count = point count - 1. will not wrap around, beware of out-of-bounds errors</remarks>
    public float GetSlope(int edgeIndex) => (float)(Points[edgeIndex].Y - Points[edgeIndex + 1].Y) / (Points[edgeIndex].X - Points[edgeIndex + 1].X);

    public static float GetSlope(Point16 point1, Point16 point2) {
        if (point1.X == point2.X) return float.MaxValue;
        return (float)(point2.Y - point1.Y) / (point2.X - point1.X);
    }

    /// <summary>
    ///     true if the presence of the middle point changes the line from first to last
    /// </summary>
    /// <param name="first"></param>
    /// <param name="middle"></param>
    /// <param name="last"></param>
    /// <returns></returns>
    protected bool MiddlePointAffectsSlope(Point16 first, Point16 middle, Point16 last) {
        float firstToLastSlope = GetSlope(first, last);
        float firstToMiddleSlope = GetSlope(first, middle);
        float middleToLastSlope = GetSlope(middle, last);
        bool slopeNegligible = Math.Abs(firstToLastSlope - firstToMiddleSlope) < 0.03 && Math.Abs(firstToLastSlope - middleToLastSlope) < 0.03;
        bool pointXsConsecutive = (first.X <= middle.X && middle.X <= last.X) || (first.X >= middle.X && middle.X >= last.X);
        bool pointYsConsecutive = (first.Y <= middle.Y && middle.Y <= last.Y) || (first.Y >= middle.Y && middle.Y >= last.Y);
        return !slopeNegligible || !pointXsConsecutive || !pointYsConsecutive;
    }

    /// <summary>
    ///     removes extra points in shape, lossless for the overall shape geometry
    /// </summary>
    /// <param name="wrapAround">if true, will assume that the first and last points are connected</param>
    /// <returns></returns>
    /// <remarks>destructive, modifies <see cref="Points" /></remarks>
    public void OptimizePoints(bool wrapAround) {
        var newPoints = Points.ToList();
        for (int i = wrapAround ? 0 : 1; i < (wrapAround ? newPoints.Count : newPoints.Count - 1); i++) {
            Point16 last = newPoints[i - 1 != -1 ? i - 1 : newPoints.Count - 1];
            Point16 target = newPoints[i];
            Point16 next = newPoints[(i + 1) % newPoints.Count];

            if (newPoints.Count == 1) break;

            if (target == next) {
                newPoints.RemoveAt(i);
                i--;
                continue; // so that we don't interfere with the next condition
            }

            if (!MiddlePointAffectsSlope(last, target, next)) {
                newPoints.RemoveAt(i);
                i--;
            }
        }

        if (newPoints[0] == newPoints[^1] && newPoints.Count > 1) newPoints.RemoveAt(newPoints.Count - 1);

        // ensure the shape isn't 1 wide/tall, which involves overlapping points by nature
        bool same = true;
        for (int i = 0; i < (wrapAround ? newPoints.Count : newPoints.Count - 1); i++) {
            Point16 target = newPoints[i];
            Point16 next = newPoints[(i + 1) % newPoints.Count];
            if (next != target) same = false;
        }

        Points = same ? [newPoints[0]] : newPoints.ToArray();
    }

    /// <summary>
    ///     moves geometry by the offset. ex. if offset = (3, 0) will move geometry 3 to the right in world coordinates
    /// </summary>
    /// <param name="offset"></param>
    public void Move(Point16 offset) {
        for (int i = 0; i < Points.Length; i++) Points[i] += offset;
        Init(Points, false);
    }

    /// <summary>
    ///     gets the angle between the 2 lines, or the angle formed by the middle point
    /// </summary>
    /// <param name="start"></param>
    /// <param name="middle"></param>
    /// <param name="end"></param>
    /// <returns></returns>
    private static float GetAngle(Point16 start, Point16 middle, Point16 end) {
        // Create vectors BA and BC
        float v1X = start.X - middle.X;
        float v1Y = start.Y - middle.Y;
        float v2X = end.X - middle.X;
        float v2Y = end.Y - middle.Y;

        double dot = v1X * v2X + v1Y * v2Y;
        double mag1 = Math.Sqrt(v1X * v1X + v1Y * v1Y);
        double mag2 = Math.Sqrt(v2X * v2X + v2Y * v2Y);

        if (mag1 == 0 || mag2 == 0) // avoid division by zero
            return 0f;

        double cosTheta = Math.Max(-1, Math.Min(1, dot / (mag1 * mag2)));
        return (float)(Math.Acos(cosTheta) * (180.0 / Math.PI));
    }

    /// <summary>
    ///     removes sets of vertices that affect the angle of their lines by less than <see cref="significantAngle" />
    /// </summary>
    /// <param name="significantAngle"></param>
    /// <param name="maxSetProportion">how physically large a set of points can be within a set. prevents combining (and removing) points into too large sets</param>
    /// <param name="maxSetSize">the largest number of points to consider in a single "set" to collapse. has serious effect on speed</param>
    /// <returns></returns>
    public List<Point16> CollapseVertices(float significantAngle = 20f, float maxSetProportion = 0.65f, int maxSetSize = 6) {
        if (Points.Length == 3)
            return Points.ToList();
        if (Points.Length <= maxSetSize + 2)
            maxSetSize = Points.Length - 2;

        HashSet<int> removedIndexes = [];
        float maxSetWidth = Size.X * maxSetProportion;
        float maxSetHeight = Size.X * maxSetProportion;

        bool removedItem = false;
        for (int setSize = maxSetSize; setSize >= 1; setSize--) {
            for (int i = 0; i < Points.Length; i++) {
                Point16 start = Points[(i - 1 + Points.Length) % Points.Length];
                Point16 end = Points[(i + setSize + 1) % Points.Length];
                int xSum = 0, ySum = 0, xMin = 0, xMax = 0, yMin = 0, yMax = 0;
                bool allRemoved = true;
                for (int newPointIndex = 0; newPointIndex < setSize; newPointIndex++) {
                    Point16 point = Points[(i + newPointIndex) % Points.Length];
                    if (!removedIndexes.Contains(i)) allRemoved = false;
                    xSum += point.X;
                    ySum += point.Y;
                    xMin = Math.Min(xMin, point.X);
                    xMax = Math.Max(xMax, point.X);
                    yMin = Math.Min(yMin, point.Y);
                    yMax = Math.Max(yMax, point.Y);
                }

                if (allRemoved) continue;
                if (xMax - xMin > maxSetWidth || yMax - yMin > maxSetHeight) continue;

                Point16 averagePoint = new(xSum / setSize, ySum / setSize);

                // ignore the set if the angle is significant
                //Console.WriteLine($"setsize: {setSize}, index: {i}, angle: {180 - GetAngle(start, averagePoint, end)}");

                if (180 - GetAngle(start, averagePoint, end) >= significantAngle) continue;

                for (int newPointIndex = 0; newPointIndex < setSize; newPointIndex++) {
                    removedIndexes.Add((i + newPointIndex) % Points.Length);
                    removedItem = true;
                }
            }

            if (removedItem) {
                removedItem = false;
                setSize++;
            }
        }

        var returnList = ((Point16[])Points.Clone()).ToList();
        for (int i = Points.Length - 1; i >= 0; i--)
            if (removedIndexes.Contains(i))
                returnList.RemoveAt(i);

        return returnList;
    }

    protected static bool RightFacingRayIntersectsSegment(Point16 rayStartPoint, Point16 segmentStart, Point16 segmentEnd) {
        if (segmentStart.Y > segmentEnd.Y)
            (segmentStart, segmentEnd) = (segmentEnd, segmentStart);

        // check if the point is outside the segment's Y range
        if (rayStartPoint.Y < segmentStart.Y || rayStartPoint.Y > segmentEnd.Y)
            return false;

        // check if the point is to the right of the segment
        if (rayStartPoint.X > Math.Max(segmentStart.X, segmentEnd.X))
            return false;

        // check for intersection
        int minX = Math.Min(segmentStart.X, segmentEnd.X);
        int maxX = Math.Max(segmentStart.X, segmentEnd.X);
        int minY = Math.Min(segmentStart.Y, segmentEnd.Y);
        int maxY = Math.Max(segmentStart.Y, segmentEnd.Y);
        double slope = (maxX - minX) / (double)(maxY - minY);
        double intersectX = minX + (maxY - minY) * slope;

        return rayStartPoint.X <= intersectX;
    }

    protected static Point16 GetIntersectionPoint(Point16 segmentStart, Point16 segmentEnd, bool cutXAxis, int cutCoord) {
        int dx = segmentEnd.X - segmentStart.X;
        int dy = segmentEnd.Y - segmentStart.Y;

        if (cutXAxis) {
            if (dy == 0) return new Point16(segmentStart.X, cutCoord); // horizontal line edge case
            double t = (cutCoord - segmentStart.Y) / (double)dy;
            int newX = (int)Math.Round(segmentStart.X + t * dx);
            return new Point16(newX, cutCoord);
        }
        else {
            if (dx == 0) return new Point16(cutCoord, segmentStart.Y); // vertical line edge case
            double t = (cutCoord - segmentStart.X) / (double)dx;
            int newY = (int)Math.Round(segmentStart.Y + t * dy);
            return new Point16(cutCoord, newY);
        }
    }
    
    /// <summary>
    ///     gets the outward facing normal of an edge made of 2 points
    /// </summary>
    /// <param name="p1"></param>
    /// <param name="p2"></param>
    /// <param name="isClockwise"></param>
    /// <returns></returns>
    protected Vector2 GetOutwardEdgeNormal(Point16 p1, Point16 p2, bool isClockwise) {
        Point16 edge = p2 - p1;

        // rotate 90° counterclockwise for outward normal
        Point16 normal = new(edge.Y * (isClockwise ? 1 : -1), -edge.X);
        return Vector2.Normalize(new Vector2(normal.X, normal.Y));
    }

    /// <summary>
    ///     gets outward facing normals for each edge
    /// </summary>
    /// <param name="wrapAround">if true, assumes there is an edge between the last and first points</param>
    /// <param name="isClockwise">if true, rotates normal to be facing outside a clockwise-shaped series of points. if false, will assume counterclockwise</param>
    /// <returns></returns>
    /// <remarks>edge index 0 is the edge between verts 0 and 1, with this pattern continuing and wrapping around</remarks>
    protected Vector2[] GetOutwardEdgeNormals(bool wrapAround, bool isClockwise) {
        int count = Points.Length;
        var normals = new Vector2[count];

        for (int i = 0; i < (wrapAround ? count : count - 1); i++) {
            Point16 p1 = Points[i];
            Point16 p2 = Points[(i + 1) % count];
            normals[i] = GetOutwardEdgeNormal(p1, p2, isClockwise);
        }

        if (!wrapAround) {
            Point16 p1 = Points[^1];
            Point16 p2 = Points[^2]; // reverse direction from earlier so we do the *-1 a few lines down
            normals[^1] = GetOutwardEdgeNormal(p1, p2, !isClockwise);
        }

        return normals;
    }

    /// <summary>
    ///     gets outward facing normals for each vertex
    /// </summary>
    /// <param name="wrapAround">if true, assumes there is an edge between the last and first points</param>
    /// ///
    /// <param name="isClockwise">if true, rotates normal to be facing outside a clockwise-shaped series of points. if false, will assume counterclockwise</param>
    /// <returns></returns>
    /// <remarks>rounds each vector component to 0 or 1</remarks>
    public Vector2[] GetVertexNormals(bool wrapAround, bool isClockwise) {
        int count = Points.Length;
        var normals = new Vector2[count];
        var edgeNormals = GetOutwardEdgeNormals(wrapAround, isClockwise);

        for (int i = wrapAround ? 0 : 1; i < count; i++) {
            // average the normals of the two adjacent edges
            Vector2 n1 = edgeNormals[(i - 1 + count) % count];
            Vector2 n2 = edgeNormals[i];
            Vector2 normal = Vector2.Normalize(new Vector2((n1.X + n2.X) / 2, (n1.Y + n2.Y) / 2));
            normals[i] = new Vector2(normal.X, normal.Y);
        }

        if (!wrapAround) {
            normals[0] = new Vector2(edgeNormals[0].X, edgeNormals[0].Y);
            normals[^1] = new Vector2(edgeNormals[0].X, edgeNormals[0].Y);
        }

        return normals;
    }

    /// <summary>
    ///     get the greatest common denominator between 2 numbers
    /// </summary>
    /// <param name="a"></param>
    /// <param name="b"></param>
    /// <returns></returns>
    private static int Gcd(int a, int b) {
        while (b != 0) {
            int t = b;
            b = a % b;
            a = t;
        }

        return Math.Abs(a);
    }

    /// <summary>
    ///     get the simplest possible integer direction vector of a delta
    /// </summary>
    /// d
    /// <param name="delta"></param>
    /// <returns></returns>
    protected static Point16 GetIntegerDirection(Point16 delta) {
        int dx = delta.X;
        int dy = delta.Y;

        int g = Gcd(Math.Abs(dx), Math.Abs(dy));
        if (g == 0) return new Point16(0, 0);

        return new Point16(dx / g, dy / g);
    }

    /// <summary>
    ///     get full (not normalized) normal of a delta
    /// </summary>
    /// <param name="delta"></param>
    /// <param name="reverse"></param>
    /// <returns></returns>
    protected static Point16 GetNormal(Point16 delta, bool reverse) => reverse ? new Point16(-delta.Y, delta.X) : new Point16(delta.Y, -delta.X);

    /// <summary>
    ///     scales a normal to a distance
    /// </summary>
    /// <param name="normal"></param>
    /// <param name="distance"></param>
    /// <returns></returns>
    protected static Point16 ScaleNormal(Point16 normal, float distance) {
        float len = MathF.Sqrt(normal.X * normal.X + normal.Y * normal.Y);
        if (len == 0) return new Point16(0, 0);

        float fx = normal.X * distance / len;
        float fy = normal.Y * distance / len;

        int ix = (int)MathF.Round(fx);
        int iy = (int)MathF.Round(fy);

        if (ix * ix + iy * iy > distance * distance) {
            // Pull back along the larger component
            if (Math.Abs(ix) > Math.Abs(iy))
                ix -= Math.Sign(ix);
            else
                iy -= Math.Sign(iy);
        }

        return new Point16(ix, iy);
    }

    protected static void OffsetEdgeEven(Point16 a, Point16 b, float distance, bool reverseNormal, out Point16 oa, out Point16 ob) {
        Point16 delta = b - a;
        Point16 dir = GetIntegerDirection(delta);
        Point16 normal = GetNormal(dir, reverseNormal);
        normal = ScaleNormal(normal, distance);
        Point16 offset = new(
            (int)MathF.Round(normal.X),
            (int)MathF.Round(normal.Y)
        );

        oa = a + offset;
        ob = b + offset;
    }

    protected static Point16 LineIntersection(Point16 a1, Point16 a2, Point16 b1, Point16 b2) {
        float x1 = a1.X, y1 = a1.Y;
        float x2 = a2.X, y2 = a2.Y;
        float x3 = b1.X, y3 = b1.Y;
        float x4 = b2.X, y4 = b2.Y;

        float denom = (x1 - x2) * (y3 - y4) - (y1 - y2) * (x3 - x4);

        if (denom == 0) return a2; // parallel fallback

        float numX = (x1 * y2 - y1 * x2) * (x3 - x4) - (x1 - x2) * (x3 * y4 - y3 * x4);
        float numY = (x1 * y2 - y1 * x2) * (y3 - y4) - (y1 - y2) * (x3 * y4 - y3 * x4);

        // round once
        return new Point16((int)Math.Round(numX / denom), (int)Math.Round(numY / denom));
    }
}