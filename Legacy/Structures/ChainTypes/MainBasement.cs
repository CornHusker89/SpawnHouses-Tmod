using System;
using System.Linq;
using Microsoft.Xna.Framework;
using SpawnHouses.Common.DataStructures;
using SpawnHouses.Enums;
using SpawnHouses.Legacy.Helpers;
using SpawnHouses.Legacy.Structures.BridgeTypes;
using SpawnHouses.Legacy.Structures.StructureParts;
using SpawnHouses.Legacy.Structures.StructureTypes;
using SpawnHouses.Legacy.Structures.StructureTypes.ChainStructures;
using Terraria;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.WorldBuilding;

namespace SpawnHouses.Legacy.Structures.ChainTypes;

public class MainBasement : LegacyStructureChain {
    private readonly float _shape;

    public MainBasement(ushort x = 1000, ushort y = 1000, byte status = StructureStatus.NotGenerated, TileBox[] startingBoundingBoxes = null) :
        base((ushort)(58 * ModContent.GetInstance<SpawnHousesConfig>().SpawnPointBasementSizeMultiplier),
            (ushort)(80 * SpawnHousesMod.Config.SpawnPointBasementSizeMultiplier),
            (byte)Math.Round(SpawnHousesMod.Config.SpawnPointBasementSize >= 15 ? 2.5 : 1 * SpawnHousesMod.Config.SpawnPointBasementSizeMultiplier),
            (byte)Math.Round(4 * SpawnHousesMod.Config.SpawnPointBasementSizeMultiplier),
            x, y,
            [
                new MainBasementRoom1(cost: 12, weight: 40),
                new MainBasementRoom1WithFloor(cost: 14, weight: 130),
                new MainBasementRoom2(cost: 13, weight: 40),
                new MainBasementRoom2WithRoof(cost: 15, weight: 90),
                new MainBasementRoom3(cost: 8, weight: 115),
                new MainBasementRoom4(cost: 11, weight: 5),
                new MainBasementRoom5(cost: 13, weight: 145),
                new MainBasementRoom6(cost: 14, weight: 115),
                new MainBasementRoom7(cost: 14, weight: 80),
                new MainBasementRoom8(cost: 13, weight: 150),
                new MainBasementHallway4(cost: 5, weight: 110),
                new MainBasementHallway5(cost: 5, weight: 110),
                new MainBasementHallway9(cost: 4, weight: 110)
            ],
            [
                new SingleStructureBridge.MainBasementHallway1(),
                new SingleStructureBridge.MainBasementHallway1AltGen(),

                new SingleStructureBridge.MainBasementHallway2(),
                new SingleStructureBridge.MainBasementHallway2AltGen(),

                new SingleStructureBridge.MainBasementHallway2Reversed(),
                new SingleStructureBridge.MainBasementHallway2ReversedAltGen(),

                new SingleStructureBridge.MainBasementHallway3(),
                new SingleStructureBridge.MainBasementHallway3AltGen(),

                new SingleStructureBridge.MainBasementHallway3Reversed(),
                new SingleStructureBridge.MainBasementHallway3ReversedAltGen(),

                new SingleStructureBridge.MainBasementHallway6(),
                new SingleStructureBridge.MainBasementHallway6AltGen(),

                new SingleStructureBridge.MainBasementHallway7(),
                new SingleStructureBridge.MainBasementHallway7AltGen(),

                new SingleStructureBridge.MainBasementHallway8(),
                new SingleStructureBridge.MainBasementHallway8AltGen()
            ],
            [
                new MainBasementEntry2(cost: 10, weight: 100),
                new MainBasementEntry1(cost: 10, weight: 100)
            ],
            startingBoundingBoxes, status) {
        _shape = 0.5f; //SpawnHousesMod.Config.SpawnPointBasementShape;
    }


    protected override bool IsChainComplete() {
        if (CompatabilityHelper.IsMSEnabled && SpawnHousesMod.Config.SpawnPointBasementSizeMultiplier > 0.60) {
            bool found = false;
            ActionOnEachStructure(structure => {
                if (structure.Id is StructureType.MainBasementRoom5) found = true;
            });
            return found;
        }

        return true;
    }

    protected override bool IsConnectPointValid(ChainConnectPoint connectPoint, ChainConnectPoint targetConnectPoint,
        LegacyChainStructure targetStructure) {
        // clear root point
        if (connectPoint.ParentStructure.Id is StructureType.MainBasementEntry1 or StructureType.MainBasementEntry2 && connectPoint.RootPoint) return false;

        // ensure it's at/under the rootstructure
        bool valid = true;
        targetStructure.ActionOnEachConnectPoint(point => {
            if (point.Y < RootStructure.BoundingBox.Top + 10) valid = false;
        });
        if (!valid) return false;

        // change base direction chances based on desired shape
        if (connectPoint.ParentStructure != RootStructure)
            if (_shape <= 0.21f) {
                int rootY = RootStructure.ConnectPoints[LegacyDirections.Left][0].Y;
                if (connectPoint.Y == rootY || targetConnectPoint.Y == rootY)
                    return false;
            }
        
        int maxDistance = 999;
        if (_shape <= 0.41f) {
            maxDistance = 120;
            if (_shape <= 0.31f) {
                maxDistance = 80;
                if (_shape <= 0.21f) {
                    maxDistance = 50;
                    if (_shape <= 0.11f && SpawnHousesMod.Config.SpawnPointBasementSize <= 14) {
                        maxDistance = 35;
                        if (_shape <= 0.01f) maxDistance = 29;
                    }
                }
            }
        }
        
        byte direction = connectPoint.Direction;
        if (direction == LegacyDirections.Down) direction = LegacyDirections.Left;
        
        int startX = RootStructure.ConnectPoints[direction][0].X;
        if (Math.Abs(targetConnectPoint.X - startX) > maxDistance) return false;

        return valid;
    }

    protected override LegacyChainStructure GetNewStructure(ChainConnectPoint parentConnectPoint,
        bool closeToMaxBranchLength, int structureWeightSum, LegacyChainStructure[] usableStructureList) {
        LegacyChainStructure chosenStructure = null;
        for (int i = 0; i < 50; i++) {
            double randomValue = Terraria.WorldGen.genRand.NextDouble() * structureWeightSum;
            LegacyChainStructure structure = usableStructureList.Last(curStructure => curStructure.Weight <= randomValue).Clone();

            if (structure is null)
                continue;

            // don't generate a branching hallway right after another one :) but only if the shape isn't too vertical
            if (_shape > 0.31 && parentConnectPoint is not null &&
                parentConnectPoint.GenerateChance == GenerateChances.Guaranteed &&
                StructureIdHelper.IsBranchingHallway(structure))
                continue;

            // don't generate a branching hallway if it means going over the max branch count
            if (closeToMaxBranchLength && StructureIdHelper.IsBranchingHallway(structure)) continue;

            if (_shape >= 0.89f && StructureIdHelper.IsBranchingHallway(structure)) continue;

            // if vertical shape, make the first left and right a branching hallway
            if (_shape <= 0.11 && parentConnectPoint is not null)
                if (parentConnectPoint.ParentStructure == RootStructure &&
                    !StructureIdHelper.IsBranchingHallway(structure))
                    continue;

            // get some elevation changes
            if (_shape <= 0.41 && parentConnectPoint is not null) {
                if (_shape <= 0.21) {
                    if (!(StructureIdHelper.IsBranchingHallway(structure) ||
                          StructureIdHelper.IsBranchingHallway(parentConnectPoint.ParentStructure)))
                        continue;
                }
                else {
                    if (parentConnectPoint.ParentStructure is not null) {
                        if (parentConnectPoint.ParentStructure.ParentChainConnectPoint is not null &&
                            !(StructureIdHelper.IsBranchingHallway(structure) ||
                              StructureIdHelper.IsBranchingHallway(parentConnectPoint.ParentStructure)))
                            continue;
                        if (parentConnectPoint.ParentStructure.ParentChainConnectPoint is not null
                            && parentConnectPoint.ParentStructure.ParentChainConnectPoint.ParentStructure is not null
                            && StructureIdHelper.IsBranchingHallway(
                                parentConnectPoint.ParentStructure.ParentChainConnectPoint!.ParentStructure))
                            continue;
                    }
                }
            }

            chosenStructure = structure;
        }

        return chosenStructure;
    }

    protected override void OnStructureGenerate(LegacyChainStructure structure) {
        if ((structure.Id is not StructureType.MainBasementRoom5 || !CompatabilityHelper.IsMSEnabled) && structure.Id is not StructureType.MainBasementRoom8)
            foreach (TileBox boundingBox in structure.DetailedBoundingBoxes)
                StructureGenHelper.GenerateCobwebs(
                    boundingBox.TopLeftPoint16,
                    (ushort)boundingBox.Width,
                    (ushort)boundingBox.Height
                );

        int centerX = structure.BoundingBox.Left + structure.BoundingBox.Width / 2;
        int centerY = structure.BoundingBox.Top + structure.BoundingBox.Height / 2;

        WorldUtils.Gen(new Point(centerX, centerY),
            new Shapes.Circle((structure.BoundingBox.Width + structure.BoundingBox.Height + 2) / 2), new Actions.Custom((i, j, args) => {
                Tile tile = Main.tile[i, j];
                if (tile.WallType is WallID.DirtUnsafe or WallID.DirtUnsafe1 or WallID.DirtUnsafe2
                    or WallID.DirtUnsafe3 or
                    WallID.DirtUnsafe4 or WallID.FlowerUnsafe or WallID.RocksUnsafe1 or WallID.RocksUnsafe2 or
                    WallID.RocksUnsafe3 or WallID.RocksUnsafe4 or WallID.GrassUnsafe or WallID.Cave2Unsafe
                    or WallID.Cave3Unsafe
                    or WallID.Cave4Unsafe or WallID.Cave5Unsafe or WallID.Cave6Unsafe or WallID.Cave7Unsafe) {
                    tile.HasTile = true;
                    tile.Slope = SlopeType.Solid;
                    tile.IsHalfBlock = false;

                    if (!tile.HasTile || tile.TileType is TileID.SmallPiles or TileID.Vines or TileID.Grass) tile.TileType = TileID.Dirt;
                }

                return true;
            }));

        WorldUtils.Gen(new Point(centerX, centerY),
            new Shapes.Circle((structure.BoundingBox.Width + structure.BoundingBox.Height + 2) / 2), Actions.Chain(
                new Modifiers.OnlyWalls(WallID.DirtUnsafe, WallID.GrassUnsafe, WallID.Cave2Unsafe, WallID.Cave3Unsafe,
                    WallID.Cave4Unsafe, WallID.Cave5Unsafe, WallID.Cave6Unsafe, WallID.Cave7Unsafe),
                new Actions.PlaceTile(TileID.Dirt)
            ));

        structure.FrameTiles();
    }

    public override bool Generate() {
        if (!base.Generate()) return false;

        // clear the extra walls on top, if the basement generates directly on the surface
        if (StructureManager.LegacyStructures.Find(structure => structure is MainHouse) is not null)
            for (int i = -6; i <= 6; i++)
                WorldUtils.ClearWall(EntryPosX + i, EntryPosY);

        return true;
    }

    public override bool IsFound(Point16 playerPos) {
        bool found = false;
        ActionOnEachStructure(structure => {
            if (!found && structure.BoundingBox.Contains(playerPos))
                found = true;
        });
        return found;
    }

    public override void OnFound() {
        base.OnFound();

        Wiring.TripWire(EntryPosX + 2, EntryPosY + 11, 1, 1);
        //Status = StructureStatus.GeneratedAndFound;
    }
}