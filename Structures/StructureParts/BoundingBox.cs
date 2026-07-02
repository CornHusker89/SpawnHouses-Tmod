using System.Collections.Generic;
using Terraria;
using Terraria.DataStructures;
using Terraria.ID;

namespace SpawnHouses.Types;

public class Box {
    public Box(Point16 point1, Point16 point2) {
        Point1 = point1;
        Point2 = point2;
    }

    public Box(int x1, int y1, int x2, int y2) {
        Point1 = new Point16(x1, y1);
        Point2 = new Point16(x2, y2);
    }

    // by convention, point1 is the top left, point2 is bottom right
    public Point16 Point1 { get; set; }
    public Point16 Point2 { get; set; }

    <<<<<<<< HEAD:Legacy/Structures/StructureParts/Box.cs
    public static bool IsBoundingBoxColliding(Box structureBox, Box other) {
        ==
        ==
        ==
        ==

    public override string ToString() => $"point1: {Point1}, point2: {Point2}";

    public static bool IsPointInside(BoundingBox boundingBox, Point16 point) =>
        !(boundingBox.Point1.X > point.X ||
          boundingBox.Point2.X < point.X ||
          boundingBox.Point1.Y > point.Y ||
          boundingBox.Point2.Y < point.Y);

    public static bool IsBoundingBoxColliding(BoundingBox structureBoundingBox, BoundingBox other) {
        >>>>>>>>
        Types / BoundingBox.cs
        // see if they aren't colliding
        if (structureBox.Point1.X > other.Point2.X || structureBox.Point2.X < other.Point1.X ||
            structureBox.Point1.Y > other.Point2.Y || structureBox.Point2.Y < other.Point1.Y)
            return false;

        return true;
    }
    <<<<<<<< HEAD:Legacy/Structures/StructureParts/Box.cs
    public static bool IsAnyBoundingBoxesColliding(Box[] structureBoundingBoxes, Box[] otherBoundingBoxes) {
        foreach (Box structureBoundingBox in structureBoundingBoxes)
        foreach (Box otherBoundingBox in otherBoundingBoxes)
            ========

        public static bool IsAnyBoundingBoxesColliding(BoundingBox[] structureBoundingBoxes,
            BoundingBox[] otherBoundingBoxes) {
            foreach (BoundingBox structureBoundingBox in structureBoundingBoxes)
            foreach (BoundingBox otherBoundingBox in otherBoundingBoxes)
                >>>>>>>>
            Types / BoundingBox.cs
            if (IsBoundingBoxColliding(structureBoundingBox, otherBoundingBox))
                return true;
        return false;
    }

    public static bool IsAnyBoundingBoxesColliding(Box[] structureBoundingBoxes, List<Box> otherBoundingBoxes) => IsAnyBoundingBoxesColliding(structureBoundingBoxes, otherBoundingBoxes.ToArray());
        <<<<<<<<

    Legacy / Structures / StructureParts / Box.cs
    public static void Visualize(Box[] boundingBoxes, ushort tileID = TileID.Adamantite) {
        foreach (Box boundingBox in boundingBoxes)
            ========

        public static void Visualize(BoundingBox[] boundingBoxes, ushort tileID = TileID.Adamantite) {
            foreach (BoundingBox boundingBox in boundingBoxes)
                >>>>>>>>
            Types / BoundingBox.cs
            for (int x = boundingBox.Point1.X; x <= boundingBox.Point2.X; x++)
            for (int y = boundingBox.Point1.Y; y <= boundingBox.Point2.Y; y++) {
                Tile tile = Main.tile[x, y];
                tile.HasTile = true;
                tile.Slope = SlopeType.Solid;
                tile.IsHalfBlock = false;
                tile.TileType = tileID;
            }
    }

    public static void VisualizeCollision(Box[] boundingBoxes1, Box[] boundingBoxes2,
        ushort tileID1 = TileID.Adamantite, ushort tileID2 = TileID.Cobalt, ushort collisionTileID = TileID.Dirt) {
        <<<<<<<<
        Legacy / Structures / StructureParts / Box.cs
        foreach (Box boundingBox in boundingBoxes1)
            ========
        foreach (BoundingBox boundingBox in boundingBoxes1)
            >>>>>>>>
        Types / BoundingBox.cs
            for (int x = boundingBox.Point1.X; x <= boundingBox.Point2.X; x++)
            for (int y = boundingBox.Point1.Y; y <= boundingBox.Point2.Y; y++) {
                Tile tile = Main.tile[x, y];
                tile.HasTile = true;
                tile.Slope = SlopeType.Solid;
                tile.IsHalfBlock = false;
                tile.TileType = tileID1;
            }
            <<<<<<<<

            Legacy / Structures / StructureParts / Box.cs
        foreach (Box boundingBox in boundingBoxes2)
            ========
        foreach (BoundingBox boundingBox in boundingBoxes2)
            >>>>>>>>
        Types / BoundingBox.cs
            for (int x = boundingBox.Point1.X; x <= boundingBox.Point2.X; x++)
            for (int y = boundingBox.Point1.Y; y <= boundingBox.Point2.Y; y++) {
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