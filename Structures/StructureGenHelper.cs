using System;
using System.Linq;
using Microsoft.Xna.Framework;
using ReLogic.Utilities;
using Terraria;
using Terraria.ID;
using Terraria.WorldBuilding;

namespace SpawnHouses.Structures;

public static class StructureGenHelper
{
    private class ClearTileSafe : GenAction
    {
        private bool _frameNeighbors;

        public ClearTileSafe(bool frameNeighbors = false)
        {
            _frameNeighbors = frameNeighbors;
        }

        public override bool Apply(Point origin, int x, int y, params object[] args)
        {
            ClearChest(x, y);
            WorldUtils.ClearTile(x, y, _frameNeighbors);
            return UnitApply(origin, x, y, args);
        }
    }
    
    /// <summary>
    /// If any part of a chest is at (x, y), it will completely remove it
    /// </summary>
    /// <param name="x"></param>
    /// <param name="y"></param>
    public static void ClearChest(int x, int y)
    {
        void Clear(int chestX, int chestY)
        {
            int id = Chest.FindChest(chestX, chestY);
            Chest.DestroyChestDirect(chestX, chestY, id);
            WorldUtils.ClearTile(chestX, chestY, true);
            WorldUtils.ClearTile(chestX + 1, chestY, true);
            WorldUtils.ClearTile(chestX, chestY + 1, true);
            WorldUtils.ClearTile(chestX + 1, chestY + 1, true);
        }
        if (Chest.FindChest(x, y) != -1)
            Clear(x, y);
        if (Chest.FindChest(x - 1, y) != -1)
            Clear(x - 1, y);
        if (Chest.FindChest(x + 1, y) != -1)
            Clear(x + 1, y);
        if (Chest.FindChest(x, y - 1) != -1)
            Clear(x, y - 1);
        if (Chest.FindChest(x - 1, y - 1) != -1)
            Clear(x - 1, y - 1);
        if (Chest.FindChest(x + 1, y - 1) != -1)
            Clear(x + 1, y - 1);
        if (Chest.FindChest(x, y - 2) != -1)
            Clear(x, y - 2);
        if (Chest.FindChest(x - 1, y - 2) != -1)
            Clear(x - 1, y - 2);
        if (Chest.FindChest(x + 1, y - 2) != -1)
            Clear(x + 1, y - 2);
    }
    
    /// <summary>
    /// Places a bush (walls) with many variants, from 1x1 to 2x3 at the coordinates given
    /// </summary>
    /// <param name="start"></param>
    /// <param name="tileID"></param>
    /// <param name="wallBlacklistIDs"></param>
    public static void PlaceBush(Point start, ushort tileID = WallID.LivingLeaf, params ushort[] wallBlacklistIDs)
        {
        void PlaceWall(int i, int j)
        {
            Tile tile = Main.tile[i, j];
            if (!wallBlacklistIDs.Contains(tile.WallType))
                tile.WallType = tileID;
        }

        int x = start.X;
        int y = start.Y;
        
        switch (Terraria.WorldGen.genRand.Next(0, 6))
        {
            case 0:
                PlaceWall(x, y);
                PlaceWall(x, y + 1);
                PlaceWall(x - 1, y + 1);
                PlaceWall(x, y + 2);
                PlaceWall(x - 1, y + 2);
                break;
            
            case 1:
                PlaceWall(x, y);
                PlaceWall(x, y + 1);
                PlaceWall(x + 1, y);
                PlaceWall(x + 1, y + 1);
                PlaceWall(x, y + 2);
                PlaceWall(x + 1, y + 2);
                break;
            
            case 2:
                PlaceWall(x, y);
                PlaceWall(x, y + 1);
                PlaceWall(x + 1, y);
                PlaceWall(x + 1, y + 1);
                PlaceWall(x + 1, y - 1);
                PlaceWall(x + 1, y + 2);
                break;
            
            case 3:
                PlaceWall(x, y);
                PlaceWall(x, y - 1);
                PlaceWall(x, y + 1);
                PlaceWall(x, y + 2);
                break;
            
            case 4:
                PlaceWall(x, y);
                PlaceWall(x, y - 1);
                PlaceWall(x - 1, y - 1);
                PlaceWall(x - 2, y);
                PlaceWall(x, y + 1);
                PlaceWall(x - 2, y + 1);
                PlaceWall(x - 1, y + 2);
                break;
            
            case 5:
                PlaceWall(x - 1, y);
                PlaceWall(x + 1, y);
                PlaceWall(x, y - 1);
                PlaceWall(x + 1, y - 1);
                break;
        }
    }
    
    
    /// <summary>
    /// 65% chance of placing cobweb on every tile in designated area
    /// </summary>
    /// <param name="start"></param>
    /// <param name="length"></param>
    /// <param name="height"></param>
    /// <param name="wallWhitelistIDs"></param>
    public static void GenerateCobwebs(Point start, ushort length, ushort height, params ushort[] wallWhitelistIDs)
    {
        WorldUtils.Gen(start, new Shapes.Rectangle(length, height),
            Actions.Chain(
                new Modifiers.Dither(0.35),
                new Actions.Custom((i, j, args) =>
                {
                    if (!Main.tile[i, j].HasTile)
                    {
                        if (wallWhitelistIDs.Length > 0) // if whitelist mode is on
                        {
                            if (wallWhitelistIDs.Contains(Main.tile[i, j].WallType))
                            {
                                Tile tile = Main.tile[i, j];
                                tile.HasTile = true;
                                tile.TileType = TileID.Cobweb;
                                tile.Slope = SlopeType.Solid;
                                tile.IsHalfBlock = false;
                            }
                        }
                        else
                        {
                            Tile tile = Main.tile[i, j];
                            tile.HasTile = true;
                            tile.TileType = TileID.Cobweb;
                            tile.Slope = SlopeType.Solid;
                            tile.IsHalfBlock = false;
                        }
                    }
                    return true;
                }),
                new Actions.SetFrames()
            )
        );
    }
    
    
    /// <summary>
    /// Generates beams of a specific tile type
    /// </summary>
    /// <param name="start"></param>
    /// <param name="beamTile"></param>
    /// <param name="beamInterval"></param>
    /// <param name="beamsCount"></param>
    /// <param name="maxBeamSize"></param>
    /// <param name="stopOnNonSolidTile"></param>
    public static void GenerateBeams(Point start, Tile beamTile, ushort beamInterval, ushort beamsCount,
        ushort maxBeamSize = 50, bool stopOnNonSolidTile = false)
    {
        
        for (int i = 0; i < beamsCount; i++)
        {
            bool validBeamLocation = true;
            int y2 = 1; //put us 1 below the floor
            int x2 = start.X + (i * beamInterval);

            //if there's a tile there already, don't place a beam
            if (Terraria.WorldGen.SolidTile(x2, start.Y + y2)) continue; 
			
            while (!Terraria.WorldGen.SolidTile(x2, start.Y + y2) && !(stopOnNonSolidTile && Main.tile[x2, start.Y + y2].HasTile))
            {
                if (y2 >= maxBeamSize)
                {
                    validBeamLocation = false;
                    break;
                }
                y2++;
            }

            if (validBeamLocation)
            {
                for (int j = 0; j < y2; j++)
                {
                    Tile tile = Main.tile[x2, start.Y + j];
                    tile.HasTile = beamTile.HasTile;
                    tile.Slope = SlopeType.Solid;
                    tile.IsHalfBlock = false;
                    tile.TileType = beamTile.TileType;
                    tile.TileColor = beamTile.TileColor;
                }

                //make the tile beneath the beam (and the one just above) a full block
                Tile bottomTerrainTile = Main.tile[x2, start.Y + y2];
                bottomTerrainTile.Slope = SlopeType.Solid;
                bottomTerrainTile.IsHalfBlock = false;
            }
        }
        // set the tile frames
        WorldUtils.Gen(new Point(start.X + ( (beamInterval + 1) * beamsCount), start.Y + (maxBeamSize / 2)),
            new Shapes.Circle(radius: maxBeamSize), new Actions.SetFrames());
    }

    
    /// <summary>
    /// Generates a circle of a specific tile
    /// </summary>
    /// <param name="start"></param>
    /// <param name="tileID"></param>
    /// <param name="foundationRadius"></param>
    public static void GenerateFoundation(Point start, ushort tileID, int foundationRadius) 
    {
        WorldUtils.Gen(start, new Shapes.Circle(foundationRadius),
            new Actions.Custom((i, j, args) =>
            {
                {
                    Tile tile = Main.tile[i, j];
                    tile.HasTile = true;
                    tile.TileType = tileID;
                    tile.Slope = SlopeType.Solid;
                    tile.IsHalfBlock = false;
                }
                return true;
            })
        );
    }


    public static void DigVerticalTunnel(Point start, int randomStepOffset, int steps)
    {
        int initialYOffset = 0;
        for (int i = 0; i < steps; i++)
        {
            int width = Terraria.WorldGen.genRand.Next(7, 17);
            double toleranceFactor = width / 16.0;
            int baseXOffset = i == 0? 0 : (int)(Terraria.WorldGen.genRand.Next(-randomStepOffset, randomStepOffset + 1) * toleranceFactor);
            double vectorXOffset = i == 0? 0 : (int)(Terraria.WorldGen.genRand.Next(-randomStepOffset, randomStepOffset + 1) * toleranceFactor * 0.65);
            if (i == 0 || i == steps - 1)
            {
                initialYOffset = (int)(width * 1.7);
                baseXOffset = 0;
                vectorXOffset = 0;
            }

            WorldUtils.Gen(new Point(start.X + baseXOffset, start.Y + initialYOffset + i * 15), 
                new Shapes.Tail(width, new Vector2D(vectorXOffset, width * 1.7)),
                new ClearTileSafe(true)
            );
				
            WorldUtils.Gen(new Point(start.X + baseXOffset, start.Y + initialYOffset + 1 + i * 15), 
                new Shapes.Tail(width, new Vector2D(vectorXOffset, width * -1.7)),
                new ClearTileSafe(true)
            );
        }
    }
}