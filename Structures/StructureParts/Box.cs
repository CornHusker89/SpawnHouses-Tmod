using System.Collections;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.DataStructures;
using System.Collections.Generic;

namespace SpawnHouses.Structures.StructureParts;

public class Box
{
    // by convention, point1 is the top left, point2 is bottom right
    public Point16 Point1 { get; set; }
    public Point16 Point2 { get; set; }

    public Box(Point16 point1, Point16 point2)
    {
        Point1 = point1;
        Point2 = point2;
    }

    public Box(int x1, int y1, int x2, int y2)
    {
        Point1 = new Point16(x1, y1);
        Point2 = new Point16(x2, y2);
    }
    
    public static bool IsBoundingBoxColliding(Box structureBox, Box other)
    {
        // see if they aren't colliding
        if (structureBox.Point1.X > other.Point2.X || structureBox.Point2.X < other.Point1.X ||
            structureBox.Point1.Y > other.Point2.Y || structureBox.Point2.Y < other.Point1.Y)
        {
            return false;
        }

        return true;
    }

    public static bool IsAnyBoundingBoxesColliding(Box[] structureBoundingBoxes, Box[] otherBoundingBoxes)
    {
        foreach (Box structureBoundingBox in structureBoundingBoxes)
        {
            foreach (Box otherBoundingBox in otherBoundingBoxes)
            {
                if (IsBoundingBoxColliding(structureBoundingBox, otherBoundingBox))
                    return true;
            }
        }
        return false;
    }
    
    public static bool IsAnyBoundingBoxesColliding(Box[] structureBoundingBoxes, List<Box> otherBoundingBoxes)
    {
        return IsAnyBoundingBoxesColliding(structureBoundingBoxes, otherBoundingBoxes.ToArray());
    }
    
    public static void Visualize(Box[] boundingBoxes, ushort tileID = TileID.Adamantite)
    {
        foreach (var boundingBox in boundingBoxes)
        {
            for (int x = boundingBox.Point1.X; x <= boundingBox.Point2.X; x++)
            {
                for (int y = boundingBox.Point1.Y; y <= boundingBox.Point2.Y; y++)
                {
                    Tile tile = Main.tile[x, y];
                    tile.HasTile = true;
                    tile.Slope = SlopeType.Solid;
                    tile.IsHalfBlock = false;
                    tile.TileType = tileID;
                }
            }
        }
    }

    public static void VisualizeCollision(Box[] boundingBoxes1, Box[] boundingBoxes2,
        ushort tileID1 = TileID.Adamantite, ushort tileID2 = TileID.Cobalt, ushort collisionTileID = TileID.Dirt)
    {
        foreach (var boundingBox in boundingBoxes1)
        {
            for (int x = boundingBox.Point1.X; x <= boundingBox.Point2.X; x++)
            {
                for (int y = boundingBox.Point1.Y; y <= boundingBox.Point2.Y; y++)
                {
                    Tile tile = Main.tile[x, y];
                    tile.HasTile = true;
                    tile.Slope = SlopeType.Solid;
                    tile.IsHalfBlock = false;
                    tile.TileType = tileID1;
                }
            }
        }
        foreach (var boundingBox in boundingBoxes2)
        {
            for (int x = boundingBox.Point1.X; x <= boundingBox.Point2.X; x++)
            {
                for (int y = boundingBox.Point1.Y; y <= boundingBox.Point2.Y; y++)
                {
                    Tile tile = Main.tile[x, y];
                    tile.HasTile = true;
                    tile.Slope = SlopeType.Solid;
                    tile.IsHalfBlock = false;

                    if (tile.TileType == tileID1)
                        tile.TileType = collisionTileID;
                    else
                        tile.TileType = tileID2;
                }
            }
        }
    }
}