using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.DataStructures;

namespace SpawnHouses.Structures;

public class BoundingBox
{
    // by convention, point1 is the top left, point2 is bottom right
    public Point16 Point1 { get; set; }
    public Point16 Point2 { get; set; }

    BoundingBox(Point16 point1, Point16 point2)
    {
        Point1 = point1;
        Point2 = point2;
    }

    BoundingBox(int x1, int y1, int x2, int y2)
    {
        Point1 = new Point16(x1, y1);
        Point2 = new Point16(x2, y2);
    }

    public bool IsBoundingBoxColliding(BoundingBox other)
    {
        // see if they aren't colliding
        if (Point1.X > other.Point2.X || Point2.X < other.Point1.X ||
            Point1.Y > other.Point2.Y || Point2.Y < other.Point1.Y)
        {
            return false;
        }

        return true;
    }
}