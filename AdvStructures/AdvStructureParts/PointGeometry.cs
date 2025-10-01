using System.Linq;
using Terraria.DataStructures;

namespace SpawnHouses.AdvStructures.AdvStructureParts;

public abstract class PointGeometry {
    public Point16[] Points;

    protected abstract void Init(Point16[] points, bool optimize);

    /// <summary>
    ///     removes extra points in shape.
    /// </summary>
    /// <param name="wrapAround">if true, loop will assume that the first and last points are connected</param>
    /// <returns></returns>
    /// <remarks>destructive, returns the input list</remarks>
    public void OptimizePoints(bool wrapAround) {
        var newPoints = Points.ToList();
        for (int i = wrapAround ? 0 : 1; i < (wrapAround ? newPoints.Count : newPoints.Count - 1); i++) {
            Point16 last = newPoints[i - 1 != -1 ? i - 1 : newPoints.Count - 1];
            Point16 target = newPoints[i];
            Point16 next = newPoints[i + 1 != newPoints.Count ? i + 1 : 0];

            if (newPoints.Count == 1) break;

            if (target == next) {
                newPoints.RemoveAt(i);
                i--;
                continue; // so that we don't interfere with the next condition
            }

            if ((target.X == last.X && target.X == next.X && ((last.Y < target.Y && target.Y < next.Y) || (last.Y > target.Y && target.Y > next.Y)))
                || (target.Y == last.Y && target.Y == next.Y && ((last.X < target.X && target.X < next.X) || (last.X > target.X && target.X > next.X)))) {
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
}