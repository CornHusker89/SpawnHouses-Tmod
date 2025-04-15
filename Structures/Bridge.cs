using System;
using SpawnHouses.Structures.StructureParts;
using Terraria;
using Terraria.ID;
using BoundingBox = SpawnHouses.Structures.StructureParts.BoundingBox;

namespace SpawnHouses.Structures;

public class Bridge {
    public readonly sbyte DeltaXMultiple;
    public readonly sbyte DeltaYMultiple;

    public readonly byte[] InputDirections;
    public readonly short MaxDeltaX;
    public readonly short MaxDeltaY;
    public readonly short MinDeltaX;
    public readonly short MinDeltaY;

    public BoundingBox[] BoundingBoxes;
    public ushort ID;
    public ConnectPoint Point1;
    public ConnectPoint Point2;


    public Bridge(byte[] inputDirections,
        short minDeltaX, short maxDeltaX, short minDeltaY, short maxDeltaY, sbyte deltaXMultiple = 1,
        sbyte deltaYMultiple = 1,
        ConnectPoint point1 = null, ConnectPoint point2 = null, BoundingBox[] boundingBoxes = null) {
        InputDirections = inputDirections;
        MinDeltaX = minDeltaX;
        MaxDeltaX = maxDeltaX;
        MinDeltaY = minDeltaY;
        MaxDeltaY = maxDeltaY;
        DeltaXMultiple = deltaXMultiple;
        DeltaYMultiple = deltaYMultiple;
        Point1 = point1;
        Point2 = point2;
        BoundingBoxes = boundingBoxes;

        if (Enum.TryParse(GetType().Name, out BridgeID result))
            ID = (ushort)result;
        else
            throw new Exception($"BridgeID of {ToString()} not found");
    }

    public virtual void Generate() {
        throw new Exception("Generate() was called on the Bridge class, this does nothing and should never happen");
    }

    public virtual void SetPoints(ConnectPoint point1, ConnectPoint point2) {
        throw new Exception(
            "SetPoints() was called on the Bridge class, this should never happen because it won't make bounding boxes");
    }

    public void ShowConnectPoints() {
        var tile = Main.tile[Point1.X, Point1.Y];
        tile.HasTile = true;
        tile.Slope = SlopeType.Solid;
        tile.IsHalfBlock = false;
        tile.TileType = TileID.Adamantite;

        tile = Main.tile[Point2.X, Point2.Y];
        tile.HasTile = true;
        tile.Slope = SlopeType.Solid;
        tile.IsHalfBlock = false;
        tile.TileType = TileID.Cobalt;
    }

    public virtual Bridge Clone() {
        var type = GetType();
        return (Bridge)Activator.CreateInstance(type)!;
    }
}