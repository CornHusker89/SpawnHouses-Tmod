using System.Collections;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.DataStructures;
using System.Collections.Generic;

namespace SpawnHouses.Structures.StructureParts;

public class BoundingBox
{
    // by convention, point1 is the top left, point2 is bottom right
    public Point16 Point1 { get; set; }
    public Point16 Point2 { get; set; }

    public BoundingBox(Point16 point1, Point16 point2)
    {
        Point1 = point1;
        Point2 = point2;
    }

    public BoundingBox(int x1, int y1, int x2, int y2)
    {
        Point1 = new Point16(x1, y1);
        Point2 = new Point16(x2, y2);
    }
    
    public static bool IsBoundingBoxColliding(BoundingBox structureBoundingBox, BoundingBox other)
    {
        // see if they aren't colliding
        if (structureBoundingBox.Point1.X > other.Point2.X || structureBoundingBox.Point2.X < other.Point1.X ||
            structureBoundingBox.Point1.Y > other.Point2.Y || structureBoundingBox.Point2.Y < other.Point1.Y)
        {
            return false;
        }

        return true;
    }

    public static bool IsAnyBoundingBoxesColliding(BoundingBox[] structureBoundingBoxes, BoundingBox[] otherBoundingBoxes)
    {
        foreach (BoundingBox structureBoundingBox in structureBoundingBoxes)
        {
            foreach (BoundingBox otherBoundingBox in otherBoundingBoxes)
            {
                if (IsBoundingBoxColliding(structureBoundingBox, otherBoundingBox))
                    return true;
            }
        }
        return false;
    }
    
    public static bool IsAnyBoundingBoxesColliding(BoundingBox[] structureBoundingBoxes, List<BoundingBox> otherBoundingBoxes)
    {
        return IsAnyBoundingBoxesColliding(structureBoundingBoxes, otherBoundingBoxes.ToArray());
    }
}