using SpawnHouses.Structures.StructureParts;
using System;
using System.Collections;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.DataStructures;
using Terraria.WorldBuilding;
using BoundingBox = SpawnHouses.Structures.StructureParts.BoundingBox;

namespace SpawnHouses.Structures;

public class Bridge
{
    public readonly byte[] InputDirections;
    public readonly short MinDeltaX;
    public readonly short MaxDeltaX;
    public readonly short MinDeltaY;
    public readonly short MaxDeltaY;
    public readonly sbyte DeltaXMultiple;
    public readonly sbyte DeltaYMultiple;
    public ConnectPoint Point1;
    public ConnectPoint Point2;

    public BoundingBox[] BoundingBoxes;
    
    
    public Bridge(byte[] inputDirections,
        short minDeltaX, short maxDeltaX, short minDeltaY, short maxDeltaY, sbyte deltaXMultiple = 1, sbyte deltaYMultiple = 1,
        ConnectPoint point1 = null, ConnectPoint point2 = null, BoundingBox[] boundingBoxes = null)
    {
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
    }

    public virtual void Generate()
    {
        throw new Exception("Generate() was called on the Bridge class, this does nothing and should never happen");
    }

    public virtual void SetPoints(ConnectPoint point1, ConnectPoint point2)
    {
        throw new Exception("SetPoints() was called on the Bridge class, this should never happen because it won't make bounding boxes");
    }

    public void ShowConnectPoints()
    {
        Tile tile = Main.tile[Point1.X, Point1.Y];
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
    
    public virtual Bridge Clone()
    {
        return new Bridge(InputDirections, MinDeltaX, MaxDeltaX, MinDeltaY, MaxDeltaY, 
            DeltaXMultiple, DeltaYMultiple, Point1, Point2, BoundingBoxes);
    }
}