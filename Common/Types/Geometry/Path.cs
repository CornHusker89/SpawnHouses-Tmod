using System;
using System.Collections.Generic;
using System.Linq;
using SpawnHouses.Common.Types.Geometry;
using SpawnHouses.Types;
using Terraria.DataStructures;

namespace SpawnHouses.AdvStructures.AdvStructureParts;

public class Path : PointGeometry {
    public bool EndExtendable;
    public bool StartExtendable;

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

    public bool LowerXExtendable {
        get => Points[0].X <= Points[^1].X ? StartExtendable : EndExtendable;
        set {
            if (Points[0].X <= Points[^1].X)
                StartExtendable = value;
            else
                EndExtendable = value;
        }
    }

    public bool HigherXExtendable {
        get => Points[0].X <= Points[^1].X ? EndExtendable : StartExtendable;
        set {
            if (Points[0].X <= Points[^1].X)
                EndExtendable = value;
            else
                StartExtendable = value;
        }
    }

    /// <summary>
    ///     vector that represents the net change over the whole path
    /// </summary>
    public Point16 PathVector => new(Points[^1].X - Points[0].X, Points[^1].Y - Points[0].Y);

    protected sealed override void Init(Point16[] points, bool optimize) {
        if (points.Length == 0) throw new Exception("path must have at least one point");

        Points = points;
        if (optimize) OptimizePoints(false);

        SetBoundingBoxAndSize();
    }

    public Path Clone() => new(Points, false, StartExtendable, EndExtendable);

    public (Point16 left, Point16 right) SortEndpoints() => Points[0].X <= Points[^1].X ? (Points[0], Points[^1]) : (Points[^1], Points[0]);

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
    ///     returns an array of all slopes that occur along the path.
    /// </summary>
    /// <returns></returns>
    /// <remarks>signs are kept so depending on geometry, some slopes may be negative</remarks>
    public float[] GetSlopes() {
        float[] returnValue = new float[Points.Length - 1];
        for (int i = 0; i < Points.Length - 1; i++) returnValue[i] = GetSlope(i);
        return returnValue;
    }

    /// <summary>
    ///     creates loop and expands either up or down, forming a shape
    /// </summary>
    /// <param name="height"></param>
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
    ///     gets the points in the form of the difference from the previous point
    /// </summary>
    /// <param name="startIndex">inclusive</param>
    /// <param name="endIndex">exclusive</param>
    /// <returns></returns>
    /// <remarks>returned list will be 1 shorter than the length of the input points</remarks>
    public Point16[] GetPointVectors(int startIndex = 0, int endIndex = -1) {
        if (endIndex == -1)
            endIndex = Points.Length;
        var returnValue = new Point16[endIndex - startIndex - 1];
        for (int i = startIndex; i < endIndex; i++)
            returnValue[i] = Points[i + 1] - Points[i];
        return returnValue;
    }

    /// <summary>
    ///     gets the sub path that can be "seen" from the given corner. used in <see cref="FillFromCorner" />
    /// </summary>
    /// <param name="corner">determines which of the 4 bounding box corners to use, both axes should be either 0 or 1</param>
    /// <returns>start index (inclusive) and end index (inclusive) of the subpath</returns>
    public (int startIndex, int endIndex) GetVisibleSubpathFromCorner(Point16 corner) {
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

        return (int.Min(xIndex, yIndex), int.Max(xIndex, yIndex));
    }

    /// <summary>
    ///     creates new shape by filling from one of the 4 bounding box corners. to fill from the bounding box edge, use <see cref="FillFromBoundingBox" />
    /// </summary>
    /// <param name="corner">determines where fill starts from, both axes should be either 0 or 1</param>
    /// <param name="offset">offest of the corner, applied after the visible subpath is determined</param>
    /// <returns></returns>
    public Shape FillFromCorner(Point16 corner, Point16 offset = default) {
        if (corner.X is not (0 or 1) || corner.Y is not (0 or 1)) throw new Exception("both axes of corner should be either 0 or 1 to represent the lower or higher sides respectively");
        Point16 outsideCorner = BoundingBox.topLeft + (Size + Point16.NegativeOne) * corner + offset;

        // make sure that it only captures points that create the bounding box on the applicable corner
        (int pathStartIndex, int pathEndIndex) = GetVisibleSubpathFromCorner(corner);

        var shapePointsArray = new Point16[pathEndIndex - pathStartIndex + 2];
        Array.Copy(Points, pathStartIndex, shapePointsArray, 0, 1 + (pathEndIndex - pathStartIndex));
        shapePointsArray[^1] = outsideCorner;
        return new Shape(shapePointsArray);
    }

    /// <summary>
    ///     fills a path until a bounding box edge(s), edge(s) can be offset. similar to <see cref="FillFromCorner" />
    /// </summary>
    /// <param name="axes">leave axis empty to ignore axis, otherwise either 0 or 1 to fill from lower or higher edge respectively</param>
    /// <param name="offset">the offset of the selected axeso</param>
    /// <returns></returns>
    public Shape FillFromBoundingBox(PartialPoint16 axes, Point16 offset) {
        if ((axes.X is not (0 or 1) && axes.HasX) || (axes.Y is not (0 or 1) && axes.HasY))
            throw new Exception("both axes of corner should be either 0 or 1 (if they exist) to represent the lower or higher bounding box edges respectively");
        if (axes is { HasX: false, HasY: false })
            throw new Exception("both axes don't exist and are ignored, call is redundant");

        // determine which indexes to include in the shape
        int xStartIndex = int.MaxValue, xEndIndex = -1, yStartIndex = int.MaxValue, yEndIndex = -1;
        if (axes.HasX) {
            (int startIndex, int endIndex) lowerIndexes = GetVisibleSubpathFromCorner(new Point16(axes.X, 0));
            (int startIndex, int endIndex) higherIndexes = GetVisibleSubpathFromCorner(new Point16(axes.X, 1));
            xStartIndex = int.Min(lowerIndexes.startIndex, higherIndexes.startIndex);
            xEndIndex = int.Max(lowerIndexes.endIndex, higherIndexes.endIndex);
        }

        if (axes.HasY) {
            (int startIndex, int endIndex) lowerIndexes = GetVisibleSubpathFromCorner(new Point16(0, axes.Y));
            (int startIndex, int endIndex) higherIndexes = GetVisibleSubpathFromCorner(new Point16(1, axes.Y));
            yStartIndex = int.Min(lowerIndexes.startIndex, higherIndexes.startIndex);
            yEndIndex = int.Max(lowerIndexes.endIndex, higherIndexes.endIndex);
        }

        // create new shape's point array, and copy path's points over
        int numAddedPoints = axes is { HasX: true, HasY: true } ? 3 : 2;
        int pathStartIndex = int.Min(xStartIndex, yStartIndex);
        int pathEndIndex = int.Max(xEndIndex, yEndIndex);

        var shapePointsArray = new Point16[pathEndIndex - pathStartIndex + 1 + numAddedPoints];
        Array.Copy(Points, pathStartIndex, shapePointsArray, 0, 1 + (pathEndIndex - pathStartIndex));

        // create added points in a lower --> higher order, then reverse if needed
        List<Point16> newPointsFromBoundingBox = [];
        if (axes is { HasX: true, X: 0 } or { HasY: true, Y: 0 }) // top left
            newPointsFromBoundingBox.Add(BoundingBox.topLeft +
                                         offset * new Point16(axes is { HasX: true, X: 0 } ? 1 : 0, axes is { HasY: true, Y: 0 } ? 1 : 0));
        if (axes is { HasX: true, X: 1 } or { HasY: true, Y: 0 }) // top right
            newPointsFromBoundingBox.Add(new Point16(BoundingBox.bottomRight.X, BoundingBox.topLeft.Y) +
                                         offset * new Point16(axes is { HasX: true, X: 1 } ? 1 : 0, axes is { HasY: true, Y: 0 } ? 1 : 0));
        if (axes is { HasX: true, X: 0 } or { HasY: true, Y: 1 }) // bottom left
            newPointsFromBoundingBox.Add(new Point16(BoundingBox.topLeft.X, BoundingBox.bottomRight.Y) +
                                         offset * new Point16(axes is { HasX: true, X: 0 } ? 1 : 0, axes is { HasY: true, Y: 1 } ? 1 : 0));
        if (axes is { HasX: true, X: 1 } or { HasY: true, Y: 1 }) // bottom right
            newPointsFromBoundingBox.Add(BoundingBox.bottomRight +
                                         offset * new Point16(axes is { HasX: true, X: 1 } ? 1 : 0, axes is { HasY: true, Y: 1 } ? 1 : 0));

        bool pathStartsHigher;
        if (axes is { HasX: true, HasY: false })
            pathStartsHigher = Points[^1].Y <= Points[0].Y;
        else if (axes is { HasX: false, HasY: true })
            pathStartsHigher = Points[^1].X <= Points[0].X;
        else
            pathStartsHigher = Points[^1].X + Points[^1].Y <= Points[0].X + Points[0].Y;

        if (!pathStartsHigher) newPointsFromBoundingBox.Reverse();

        newPointsFromBoundingBox.CopyTo(shapePointsArray, shapePointsArray.Length - numAddedPoints);
        return new Shape(shapePointsArray);
    }
}