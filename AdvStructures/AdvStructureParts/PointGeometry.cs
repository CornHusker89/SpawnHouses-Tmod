using System;
using System.Collections.Generic;
using System.Linq;
using Terraria.DataStructures;

namespace SpawnHouses.AdvStructures.AdvStructureParts;

public abstract class PointGeometry {
    public Point16[] Points;
    public (Point16 topLeft, Point16 bottomRight) BoundingBox;
    public Point16 Size;

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

    /// <summary>
    /// </summary>
    /// <param name="edgeIndex">edge index to retrieve</param>
    /// <returns></returns>
    /// <remarks>edge count = point count - 1. will not wrap around, beware of out-of-bounds errors</remarks>
    public float GetSlope(int edgeIndex) {
        return (float)(Points[edgeIndex].Y - Points[edgeIndex + 1].Y) / (Points[edgeIndex].X - Points[edgeIndex + 1].X);
    }

    public static float GetSlope(Point16 point1, Point16 point2) {
        if (point1.X == point2.X) return float.MaxValue;
        return (float)(point2.Y - point1.Y) / (point2.X - point1.X);
    }

    /// <summary>
    /// </summary>
    /// <param name="first"></param>
    /// <param name="middle"></param>
    /// <param name="last"></param>
    /// <returns></returns>
    protected bool IsMiddlePointNeeded(Point16 first, Point16 middle, Point16 last) {
        float firstToLastSlope = GetSlope(first, last);
        float firstToMiddleSlope = GetSlope(first, middle);
        float middleToLastSlope = GetSlope(middle, last);
        bool slopeNegligible = Math.Abs(firstToLastSlope - firstToMiddleSlope) < 0.03 && Math.Abs(firstToLastSlope - middleToLastSlope) < 0.03;
        bool pointXsConsecutive = (first.X <= middle.X && middle.X <= last.X) || (first.X >= middle.X && middle.X >= last.X);
        bool pointYsConsecutive = (first.Y <= middle.Y && middle.Y <= last.Y) || (first.Y >= middle.Y && middle.Y >= last.Y);
        return !slopeNegligible || !pointXsConsecutive || !pointYsConsecutive;
    }

    /// <summary>
    ///     removes extra points in shape.
    /// </summary>
    /// <param name="wrapAround">if true, will assume that the first and last points are connected</param>
    /// <returns></returns>
    /// <remarks>destructive, modifies <see cref="Points"/></remarks>
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

            if (!IsMiddlePointNeeded(last, target, next)) {
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

    public void Offset(Point16 offset) {
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
        float v1Y = start.Y - middle.X;
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
    /// <param name="maxSetSize">the largest number of points to consider in a single "set" to collapse. has serious effect on speed</param>
    /// <returns></returns>
    public List<Point16> CollapseVertices(float significantAngle = 10f, int maxSetSize = 6) {
        if (Points.Length <= maxSetSize + 2) return Points.ToList();

        HashSet<int> removedIndexes = [];

        bool removedItem = false;
        for (int setSize = maxSetSize; setSize >= 1; setSize--) {
            for (int i = 0; i < Points.Length; i++) {
                Point16 start = Points[i];
                Point16 end = Points[(i + setSize + 1) % Points.Length];
                int xSum = 0, ySum = 0;
                bool allRemoved = true;
                for (int newPointIndex = 0; newPointIndex < setSize; newPointIndex++) {
                    Point16 point = Points[(i + newPointIndex) % Points.Length];
                    if (!removedIndexes.Contains(i)) allRemoved = false;
                    xSum += point.X;
                    ySum += point.Y;
                }

                if (allRemoved) continue;

                Point16 averagePoint = new(xSum / setSize, ySum / setSize);

                // ignore the set if the angle is significant
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

    protected static bool RayIntersectsSegment(Point16 point, Point16 segmentStart, Point16 segmentEnd) {
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

    protected static int Cross(Point16 o, Point16 a, Point16 b) {
        return (a.X - o.X) * (b.Y - o.Y) - (a.Y - o.Y) * (b.X - o.X);
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
}