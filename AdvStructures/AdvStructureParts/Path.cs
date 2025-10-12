using System;
using System.Collections.Generic;
using System.Linq;
using Terraria.DataStructures;

namespace SpawnHouses.AdvStructures.AdvStructureParts;

public class Path : PointGeometry {
    public bool CanStartBeExtended;
    public bool CanEndBeExtended;
    
    protected sealed override void Init(Point16[] points, bool optimize) {
        if (points.Length == 0) throw new Exception("path must have at least one point");

        Points = points;
        if (optimize) OptimizePoints(false);
    }

    public Path(bool canStartBeExtended, bool canEndBeExtended, params Point16[] points) {
        CanStartBeExtended = canStartBeExtended;
        CanEndBeExtended = canEndBeExtended;
        Init(points, true);
    }

    public Path(IEnumerable<Point16> points, bool canStartBeExtended = true, bool canEndBeExtended = true) {
        CanStartBeExtended = canStartBeExtended;
        CanEndBeExtended = canEndBeExtended;
        var pointsArray = points.ToArray();
        Init(pointsArray, true);
    }

    private Path(IEnumerable<Point16> points, bool optimize) {
        var pointsArray = points.ToArray();
        Init(pointsArray, optimize);
    }

    public Path Clone() {
        return new Path(Points, false);
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
}