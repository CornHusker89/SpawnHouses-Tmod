using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using SpawnHouses.Structures.Chains;
using SpawnHouses.Structures.Structures;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.WorldBuilding;
using BoundingBox = SpawnHouses.Types.BoundingBox;

namespace SpawnHouses.Helpers;

public static class WorldGenHelper {
    private static byte _mainHouseOffsetDirection = Directions.None;

    public static void GenerateMainHouse() {
        bool spawnUnderworld =
            Main.ActiveWorldFileData.SeedText.ToLower().Replace(" ", "").Replace("'", "") == "dontdigup" ||
            Main.ActiveWorldFileData.SeedText.ToLower().Replace(" ", "") == "getfixedboi";

        int initialX = 1;
        int initialY = 1;

        bool foundValidSpot = false;

        for (ushort counts = 0; counts < 10; counts++) {
            initialX = Terraria.WorldGen.genRand.Next(Main.spawnTileX - 25, Main.spawnTileX + 25);
            if (!spawnUnderworld)
                initialY = Main.spawnTileY - 40;
            else
                initialY = Main.spawnTileY - 20;

            //make sure we're not under the surface
            if (!spawnUnderworld)
                while (!Is40AboveTilesClear(initialX, initialY))
                    initialY -= 30;

            bool Is40AboveTilesClear(int x, int y) {
                for (byte i = 1; i < 41; i++)
                    if (Terraria.WorldGen.SolidTile(x, y - i))
                        return false;

                return true;
            }

            try {
                if (CompatabilityHelper.IsRemnantsEnabled || ModContent.GetInstance<SpawnHousesConfig>().SpawnPointHouseOffset) {
                    List<((double average, double sd) raycast, int offset)> positions = [
                        ((0, 0), -200),
                        ((0, 0), -150),
                        ((0, 0), -100),
                        ((0, 0), 100),
                        ((0, 0), 150),
                        ((0, 0), 200)
                    ];

                    for (int i = 0; i < positions.Count; i++) {
                        int offset = positions[i].offset;
                        positions[i] = (StructureGenHelper.GetSurfaceLevel(initialX + offset - 42, initialX + offset + 42, initialY, maxCastDistance: 400), offset);
                    }
                    ((double average, double sd) raycast, int offset) selectedPosition = positions.MinBy(tuple => tuple.raycast.sd);

                    initialX += selectedPosition.offset;
                    initialY = (int)selectedPosition.raycast.average;
                    _mainHouseOffsetDirection = selectedPosition.offset > 0 ? Directions.Right : Directions.Left;
                }
                else {
                    (double average, double sd) surface = StructureGenHelper.GetSurfaceLevel(initialX - 42, initialX + 42, initialY,
                        maxCastDistance: 400);
                    initialY = (int)surface.average;
                }
            }
            catch (Exception e) {
                ModContent.GetInstance<SpawnHousesMod>().Logger.Error($"Main house failed to generate:\n{e}");
                return;
            }

            // if we found a good spot, break search loop
            if (spawnUnderworld) {
                if (initialY >= Main.spawnTileY - 25) {
                    foundValidSpot = true;
                    break;
                }
            }
            else {
                foundValidSpot = true;
                break;
            }
        }

        // just in case something above got fucked up
        if (!foundValidSpot) {
            ModContent.GetInstance<SpawnHousesMod>().Logger
                .Error(
                    "Failed to generate SpawnPointHouse. Please report this world seed and your client.log to the mod's author");
            return;
        }

        try {
            MainHouse house = new((ushort)(initialX - 31), (ushort)(initialY - 26), hasBasement: ModContent.GetInstance<SpawnHousesConfig>().EnableSpawnPointBasement, inUnderworld: spawnUnderworld);
            house.Generate();
            StructureManager.MainHouse = house;

            // move the spawn point to the upper floor of the house
            if (ModContent.GetInstance<SpawnHousesConfig>().SpawnPointHouseSetsSpawn) {
                Main.spawnTileX = initialX + house.LeftSize - 1 - 31;
                Main.spawnTileY = initialY + 5 - 15;
            }

            // replace all dirt with ash if we're in the underworld
            if (spawnUnderworld)
                WorldUtils.Gen(new Point(initialX, initialY), new Shapes.Circle(150, 100), Actions.Chain(
                    new Actions.Custom((i, j, args) => {
                        if (Main.tile[i, j].HasTile && Main.tile[i, j].TileType == TileID.Dirt) {
                            Tile tile = Main.tile[i, j];
                            tile.TileType = TileID.Ash;
                        }

                        if (Main.tile[i, j].HasTile && Main.tile[i, j].TileType == TileID.Grass) {
                            Tile tile = Main.tile[i, j];
                            tile.TileType = TileID.AshGrass;
                        }

                        return true;
                    })
                ));
        }
        catch (Exception e) {
            ModContent.GetInstance<SpawnHousesMod>().Logger.Error($"Main house failed to generate:\n{e}");
        }
    }

    public static void GenerateMineshaft() {
        int x, y;
        try {
            bool FindValidLocation(bool left = true) {
                if (StructureManager.MainHouse != null) {
                    int centerHouse = StructureManager.MainHouse.X + StructureManager.MainHouse.StructureXSize / 2;
                    if (left)
                        x = centerHouse - Terraria.WorldGen.genRand.Next(18, 38) - 35;
                    else
                        x = centerHouse + Terraria.WorldGen.genRand.Next(18, 38) + 35;
                }
                else {
                    if (left)
                        x = Main.spawnTileX - Terraria.WorldGen.genRand.Next(18, 34) - 35;
                    else
                        x = Main.spawnTileX + Terraria.WorldGen.genRand.Next(18, 34) + 35;
                }

                (double average, double sd) surfaceLevel = StructureGenHelper.GetSurfaceLevel(x - 10, x + 11, Main.spawnTileY - 24);
                y = (int)surfaceLevel.average;

                return surfaceLevel.sd <= 2.8;
            }

            bool startLeftSide;
            if (_mainHouseOffsetDirection == Directions.None)
                startLeftSide = Terraria.WorldGen.genRand.NextBool();
            else
                startLeftSide = _mainHouseOffsetDirection == Directions.Left;

            if (FindValidLocation(startLeftSide) || FindValidLocation(!startLeftSide)) {
                Mineshaft mineshaft = new((ushort)(x - 13), (ushort)(y - 13));
                mineshaft.Generate();
                StructureManager.Mineshaft = mineshaft;
            }
        }
        catch (Exception e) {
            ModContent.GetInstance<SpawnHousesMod>().Logger.Error($"Mineshaft failed to generate:\n{e}");
        }
    }


    public static void GenerateMainBasement() {
        BoundingBox[] mineshaftBoundingBox = [];
        if (StructureManager.Mineshaft is not null) {
            Mineshaft structure = StructureManager.Mineshaft;
            BoundingBox structureBox = new(structure.X - 8, structure.Y,
                structure.X + structure.StructureXSize + 8, structure.Y + 200);
            mineshaftBoundingBox = [structureBox];
        }

        MainBasement chain;
        if (StructureManager.MainHouse is not null)
            chain = new MainBasement((ushort)StructureManager.MainHouse.BasementEntryPos.X,
                (ushort)StructureManager.MainHouse.BasementEntryPos.Y,
                startingBoundingBoxes: mineshaftBoundingBox);
        else
            chain = new MainBasement((ushort)Main.spawnTileX, (ushort)Main.spawnTileY,
                startingBoundingBoxes: mineshaftBoundingBox);

        if (chain.SuccessfulGeneration)
            try {
                chain.CalculateChain();
                chain.Generate();
                StructureManager.MainBasement = chain;
            }
            catch (Exception e) {
                ModContent.GetInstance<SpawnHousesMod>().Logger.Error($"Main basement failed to generate:\n{e}");
            }
    }


    public static void GenerateBeachHouse() {
        ushort tileX = 0, tileY = 0;
        short yModifier = 0;
        ushort fillTileType = TileID.Sand;

        void FindShoreline(bool rightSide = false) {
            ushort x, y;
            if (!rightSide)
                x = 70;
            else
                x = (ushort)(Main.maxTilesX - 70);

            while (true) {
                if (!rightSide)
                    x++;
                else
                    x--;

                y = (ushort)(Main.worldSurface * 2 / 5 - yModifier);
                if (Terraria.WorldGen.SolidTile(x, y))
                    yModifier -= 70;

                while (true) {
                    // sample vertical segment
                    y++;

                    if (Terraria.WorldGen.SolidTile(x, y)) {
                        if (!Terraria.WorldGen.SolidTile(x, y + 20) ||
                            !Terraria.WorldGen.SolidTile(x, y + 28)) // if we're on an "island" keep going
                            break;
                        
                        tileX = x;
                        tileY = y;

                        //sample a deeper tile
                        fillTileType = Main.tile[x, y + 10].TileType;
                        if (fillTileType == TileID.ShellPile)
                            fillTileType = TileID.Sand;

                        return;
                    }

                    if (Main.tile[x, y].LiquidAmount != 0)
                        break;
                }
            }
        }

        bool dungeonIsLeftSide = Main.dungeonX < Main.maxTilesX / 2;
        bool rightSide = dungeonIsLeftSide;
        
        FindShoreline(rightSide);

        if (tileX == 0 || tileY == 0) return;
        
        try {
            BeachHouse beachHouse = !rightSide
                ? new BeachHouse((ushort)(tileX - 9), (ushort)(tileY - 32))
                : new BeachHouse((ushort)(tileX - 23), (ushort)(tileY - 32), reverse: true);
            beachHouse.Generate();
            StructureManager.BeachHouse = beachHouse;

            // firepit generation
            if (Terraria.WorldGen.genRand.Next(0, 2) == 0) // 1/2 chance
            {
                bool foundLocation = false;
                ushort x;

                if (rightSide)
                    x = (ushort)(tileX - 9 + 35 + Terraria.WorldGen.genRand.Next(8, 12));
                else
                    x = (ushort)(tileX - 23 - Terraria.WorldGen.genRand.Next(8, 12));

                ushort y = 10;
                while (!foundLocation) {
                    y = tileY;
                    while (y < Main.worldSurface) {
                        if (Terraria.WorldGen.SolidTile(x, y)) break;
                        y++;
                    }

                    foundLocation = true;
                }

                y = (ushort)(y - 2);
                x = (ushort)(x - 3);

                Firepit structure = new(x, y);
                structure.Generate();
            }

            // replace all sand with filltype sand (for when the beaches are corrupt blocks)
            if (fillTileType is not TileID.Sand)
                WorldUtils.Gen(new Point(tileX, tileY), new Shapes.Circle(60, 50), Actions.Chain(
                    new Actions.Custom((i, j, args) => {
                        if (Terraria.WorldGen.InWorld(i, j) && Main.tile[i, j].HasTile &&
                            Main.tile[i, j].TileType == TileID.Sand) {
                            Tile tile = Main.tile[i, j];
                            tile.TileType = fillTileType;
                        }

                        return true;
                    })
                ));
        }
        catch (Exception e) {
            ModContent.GetInstance<SpawnHousesMod>().Logger.Error($"Beach house failed to generate:\n{e}");
        }
    }
}