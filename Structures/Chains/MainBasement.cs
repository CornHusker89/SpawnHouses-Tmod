using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using SpawnHouses.Structures.Bridges;
using SpawnHouses.Structures.StructureParts;
using SpawnHouses.Structures.Structures.ChainStructures;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.WorldBuilding;
using BoundingBox = SpawnHouses.Structures.StructureParts.BoundingBox;

namespace SpawnHouses.Structures.Chains;

public class MainBasement : StructureChain
{
    private readonly float _shape;
    
    public MainBasement(ushort x = 1000, ushort y = 1000, byte status = StructureStatus.NotGenerated,
        BoundingBox[] startingBoundingBoxes = null) :
        base((ushort)(58 * ModContent.GetInstance<SpawnHousesConfig>().SpawnPointBasementMultiplier),
            (ushort)(80 * ModContent.GetInstance<SpawnHousesConfig>().SpawnPointBasementMultiplier),
            (byte)Math.Round(1 * ModContent.GetInstance<SpawnHousesConfig>().SpawnPointBasementMultiplier),
            (byte)Math.Round(4 * ModContent.GetInstance<SpawnHousesConfig>().SpawnPointBasementMultiplier),
            x, y,
            [
                new MainBasement_Room1(cost: 12, weight: 40),
                new MainBasement_Room1_WithFloor(cost: 14, weight: 130),
                new MainBasement_Room2(cost: 13, weight: 40),
                new MainBasement_Room2_WithRoof(cost: 15, weight: 90),
                new MainBasement_Room3(cost: 8, weight: 115),
                new MainBasement_Room4(cost: 11, weight: 5),
                new MainBasement_Room5(cost: 13, weight: 145),
                new MainBasement_Room6(cost: 14, weight: 115),
                new MainBasement_Room7(cost: 14, weight: 80),
                new MainBasement_Hallway4(cost: 5, weight: 100),
                new MainBasement_Hallway5(cost: 5, weight: 100),
                new MainBasement_Hallway9(cost: 4, weight: 100)
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
                new MainBasement_Entry2(cost: 10, weight: 100),
                new MainBasement_Entry1(cost: 10, weight: 100)
            ],
            startingBoundingBoxes, status)
    {
        _shape = ModContent.GetInstance<SpawnHousesConfig>().MainBasementShape;
    }


    protected override bool IsChainComplete()
    {
        if (ModHelper.IsMSEnabled && ModContent.GetInstance<SpawnHousesConfig>().SpawnPointBasementMultiplier > 0.60)
        {
            bool found = false;
            ActionOnEachStructure(structure =>
            {
                if (structure.ID == (ushort)StructureID.MainBasement_Room5) found = true;
            });
            if (found)
                return true;
        }
        return true;
    }

    protected override bool IsConnectPointValid(ChainConnectPoint connectPoint, ChainConnectPoint targetConnectPoint, 
        CustomChainStructure targetStructure)
    {
        // clear root point
        if (connectPoint.ParentStructure.ID is (ushort)StructureID.MainBasement_Entry1 or (ushort)StructureID.MainBasement_Entry2 &&
            connectPoint.RootPoint)
            return false;

        // ensure it's at/under the rootstructure
        bool valid = true;
        targetStructure.ActionOnEachConnectPoint((ChainConnectPoint point) =>
        {
            if (point.Y < RootStructure.Y + 10)
                valid = false;
        });
        if (!valid) return false;
        
        // change base direction chances based on desired shape
        if (connectPoint.ParentStructure != RootStructure)
        {
            if (_shape <= 0.21f)
            {
                int rootY = RootStructure.ConnectPoints[Directions.Left][0].Y;
                if (connectPoint.Y == rootY || targetConnectPoint.Y == rootY)
                    return false;
            }
        }

        int maxDistance = 999;
        if (_shape <= 0.41f)
        {
            maxDistance = 120;
            if (_shape <= 0.31f)
            {
                maxDistance = 80;
                if (_shape <= 0.21f)
                {
                    maxDistance = 50;
                    if (_shape <= 0.11f)
                    
                        maxDistance = 35;
                    if (_shape <= 0.01f)
                        maxDistance = 25;
                }
            }
        }
        byte direction = connectPoint.Direction;
        if (direction == Directions.Down) direction = Directions.Left;
        
        int startX = RootStructure.ConnectPoints[direction][0].X;
        if (Math.Abs(targetConnectPoint.X - startX) > maxDistance)
            return false;
        targetStructure.ActionOnEachConnectPoint((ChainConnectPoint point) =>
        {
            if (Math.Abs(point.X - startX) > maxDistance * 1.18)
                valid = false;
        });
        if (!valid) return false;
        
        return true;
    }

    protected override CustomChainStructure GetNewStructure(ChainConnectPoint parentConnectPoint, 
        bool closeToMaxBranchLength, int structureWeightSum, CustomChainStructure[] usableStructureList)
    {
        CustomChainStructure chosenStructure = null;
        for (int i = 0; i < 50; i++)
        {
            double randomValue = Terraria.WorldGen.genRand.NextDouble() * structureWeightSum;
            CustomChainStructure structure = usableStructureList.Last(curStructure => curStructure.Weight <= randomValue).Clone();
        
            // don't generate a branching hallway right after another one :) but only if the shape isn't too vertical
            if (_shape > 0.31 && (parentConnectPoint is not null && parentConnectPoint.GenerateChance == GenerateChances.Guaranteed && 
                StructureIDUtils.IsBranchingHallway(structure)))
                continue;
                
            // don't generate a branching hallway if it means going over the max branch count
            if (closeToMaxBranchLength && StructureIDUtils.IsBranchingHallway(structure)) continue;

            if (_shape >= 0.89f && StructureIDUtils.IsBranchingHallway(structure)) continue;
        
            // if vertical shape, make the first left and right a branching hallway
            if (_shape <= 0.11 && parentConnectPoint is not null)
            {
                if (parentConnectPoint.ParentStructure == RootStructure &&
                    !StructureIDUtils.IsBranchingHallway(structure))
                {
                    continue;
                }
            }
        
            // get some elevation changes
            if (_shape <= 0.41 && parentConnectPoint is not null)
            {
                if (_shape <= 0.21)
                {
                    if (!(StructureIDUtils.IsBranchingHallway(structure) || StructureIDUtils.IsBranchingHallway(parentConnectPoint.ParentStructure)) )
                        continue;
                }
                else
                {
                    if (parentConnectPoint.ParentStructure!.ParentChainConnectPoint is not null &&
                        !(StructureIDUtils.IsBranchingHallway(structure) || StructureIDUtils.IsBranchingHallway(parentConnectPoint.ParentStructure))
                        || StructureIDUtils.IsBranchingHallway(parentConnectPoint.ParentStructure.ParentChainConnectPoint!.ParentStructure))
                        continue;
                }
                    
            }
            chosenStructure = structure;
        }
        return chosenStructure;

    }

    protected override void OnStructureGenerate(CustomChainStructure structure)
    {
        if (structure.ID != (ushort)StructureID.MainBasement_Room5 || !ModHelper.IsMSEnabled)
            foreach (var boundingBox in structure.StructureBoundingBoxes)
                StructureGenHelper.GenerateCobwebs(new Point(boundingBox.Point1.X, boundingBox.Point1.Y),
                    (ushort)(boundingBox.Point2.X - boundingBox.Point1.X + 1),
                    (ushort)(boundingBox.Point2.Y - boundingBox.Point1.Y + 1));
        
        int centerX = structure.X + (structure.StructureXSize / 2);
        int centerY = structure.Y + (structure.StructureYSize / 2);
        
        WorldUtils.Gen(new Point(centerX, centerY), new Shapes.Circle((structure.StructureXSize + structure.StructureYSize + 2) / 2), new Actions.Custom((i, j, args) =>
        {
            Tile tile = Main.tile[i, j];
            if (tile.WallType is WallID.DirtUnsafe or WallID.DirtUnsafe1 or WallID.DirtUnsafe2 or WallID.DirtUnsafe3 or 
                WallID.DirtUnsafe4 or WallID.FlowerUnsafe or WallID.RocksUnsafe1 or WallID.RocksUnsafe2 or 
                WallID.RocksUnsafe3 or WallID.RocksUnsafe4 or WallID.GrassUnsafe or WallID.Cave2Unsafe or WallID.Cave3Unsafe 
                or WallID.Cave4Unsafe or WallID.Cave5Unsafe or WallID.Cave6Unsafe or WallID.Cave7Unsafe)
            {
                tile.HasTile = true;
                tile.Slope = SlopeType.Solid;
                tile.IsHalfBlock = false;
                
                if (!tile.HasTile || tile.TileType is TileID.SmallPiles or TileID.Vines or TileID.Grass)
                    tile.TileType = TileID.Dirt;
            }
            return true;
        }));
        
        WorldUtils.Gen(new Point(centerX, centerY), new Shapes.Circle((structure.StructureXSize + structure.StructureYSize + 2) / 2), Actions.Chain(
            new Modifiers.OnlyWalls(WallID.DirtUnsafe, WallID.GrassUnsafe, WallID.Cave2Unsafe, WallID.Cave3Unsafe, WallID.Cave4Unsafe, WallID.Cave5Unsafe, WallID.Cave6Unsafe, WallID.Cave7Unsafe),
            new Actions.PlaceTile(TileID.Dirt)
        ));
        
        structure.FrameTiles();
    }

    public override bool Generate()
    {
        if (!base.Generate()) return false;
        
        // clear the extra walls on top, if the basement generates directly on the surface
        if (SpawnHousesSystem.MainHouse is null)
            for (int i = -6; i <= 6; i++)
                WorldUtils.ClearWall(EntryPosX + i, EntryPosY);

        return true;
    }

    public override void OnFound()
    {
        base.OnFound();
        
        Wiring.TripWire(EntryPosX + 2, EntryPosY + 11, 1, 1);
        Status = StructureStatus.GeneratedAndFound;
    }
}