using System;
using System.Collections.Generic;
using System.Linq;
using Terraria.DataStructures;

namespace SpawnHouses.AdvStructures.AdvStructureParts;

public class Path : PointGeometry {
    public bool StartExtendable;
    public bool EndExtendable;
    
    protected sealed override void Init(Point16[] points, bool optimize) {
        if (points.Length == 0) throw new Exception("path must have at least one point");

        Points = points;
        if (optimize) OptimizePoints(false);

        SetBoundingBoxAndSize();
    }

    public Path(bool startExtendable, bool endExtendable, params Point16[] points) {
        StartExtendable = startExtendable;
        EndExtendable = endExtendable;
        Init(points, true);
    }

    public Path(IEnumerable<Point16> points, bool startExtendable = true, bool endExtendable = true) {
        StartExtendable = startExtendable;
        EndExtendable = endExtendable;
        var pointsArray = points.ToArray();
        Init(pointsArray, true);
    }

    private Path(IEnumerable<Point16> points, bool optimize, bool startExtendable = true, bool endExtendable = true) {
        StartExtendable = startExtendable;
        EndExtendable = endExtendable;
        var pointsArray = points.ToArray();
        Init(pointsArray, optimize);
    }

    public Path Clone() {
        return new Path(Points, false, StartExtendable, EndExtendable);
    }

    public (Point16 left, Point16 right) SortEndpoints() {
        return Points[0].X <= Points[^1].X ? (Points[0], Points[^1]) : (Points[^1], Points[0]);
    }

    /// <summary>
    ///     same as regular offset, but considers the slops of the line to create a consistent offset look
    /// </summary>
    /// <param name="offset"></param>
    public void OffsetEven(Point16 offset) {
        int[] offsets = new int[Points.Length];
        for (int i = 0; i < Points.Length - 1; i++) {
            Point16 thisPoint = Points[i];
            Point16 nextPoint = Points[i + 1];
            int offsetFromSlope = (int)Math.Floor(Math.Abs(GetSlope(thisPoint, nextPoint)));
            offsets[i] -= offsetFromSlope;
            offsets[i + 1] -= offsetFromSlope;
        }

        for (int i = 0; i < Points.Length; i++) Points[i] += new Point16(offset.X, offsets[i] + offset.Y);
        Init(Points, false);
    }

    /// <summary>
    ///     Reverses order of the path
    /// </summary>
    public void Reverse() {
        (StartExtendable, EndExtendable) = (EndExtendable, StartExtendable);
        Points = Points.Reverse().ToArray();
    }

    /// <summary>
    ///     creates loop and expands either up or down, forming a shape
    /// </summary>
    /// <param name="height"></param>
    /// <param name="offset"></param>
    /// <param name="startFromBottom"></param>
    /// <returns></returns>
    public Shape ToShape(int height, bool startFromBottom = true) {
        int heightChange = startFromBottom ? -height + 1 : height - 1;
        var points = Points.ToList();
        for (int i = points.Count - 1; i >= 0; i--) points.Add(new Point16(points[i].X, points[i].Y + heightChange));
        return new Shape(points);
    }

    /// <summary>
    ///     creates a shape by connecting any number of paths, end-to-end
    /// </summary>
    /// <param name="otherPaths"></param>
    /// <returns></returns>
    public Shape ToShape(params Path[] otherPaths) {
        List<Point16> newPoints = [];
        newPoints.AddRange(Points);
        foreach (Path path in otherPaths) newPoints.AddRange(path.Points);

        return new Shape(newPoints);
    }

    /// <summary>
    ///     returns new path from the start index (inclusive) to the end index (exclusive)
    /// </summary>
    /// <param name="start"></param>
    /// <param name="end"></param>
    /// <param name="startExtendable"></param>
    /// <param name="endExtendable"></param>
    /// <returns></returns>
    public Path Slice(int start, int end = -1, bool startExtendable = true, bool endExtendable = true) {
        if (end == -1) end = Points.Length;
        var newPoints = new Point16[end - start];
        if (end - start <= 0) throw new Exception("end must be greater than start");

        for (int i = start; i < end; i++) newPoints[i - start] = Points[i];

        return new Path(newPoints, false, startExtendable, endExtendable);
    }

    /// <summary>
    /// </summary>
    /// <param name="corner">determines where fill starts from, both axes should be either 0 or 1</param>
    /// <returns></returns>
    public Shape FillFromCorner(Point16 corner) {
        if (corner.X is not (0 or 1) || corner.Y is not (0 or 1)) throw new Exception("both axes of corner should be either 0 or 1");

        Point16 outsideCorner = BoundingBox.topLeft + (Size + Point16.NegativeOne) * corner;
        int xIndex = -1, xLength = int.MaxValue, yIndex = -1, yLength = int.MaxValue;
        for (int i = 0; i < Points.Length; i++) {
            Point16 point = Points[i];
            if (point.X == outsideCorner.X && Math.Abs(outsideCorner.Y - point.Y) < xLength) {
                xIndex = i;
                xLength = Math.Abs(outsideCorner.Y - point.Y);
            }

            if (point.Y == outsideCorner.Y && Math.Abs(outsideCorner.X - point.X) < yLength) {
                yIndex = i;
                yLength = Math.Abs(outsideCorner.X - point.X);
            }
        }

        int pathStartIndex = int.Min(xIndex, yIndex);
        int pathEndIndex = int.Max(xIndex, yIndex);
        var newPoints = new Point16[2 + (pathEndIndex - pathStartIndex)];
        Array.Copy(Points, pathStartIndex, newPoints, 0, 1 + (pathEndIndex - pathStartIndex));
        newPoints[^1] = outsideCorner;
        return new Shape(newPoints);
    }
}