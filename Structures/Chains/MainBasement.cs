using System;
using System.Collections.Generic;
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
    public MainBasement(ushort x = 1000, ushort y = 1000, byte status = StructureStatus.NotGenerated, 
        BoundingBox[] startingBoundingBoxes = null, bool generateSubstructures = true) :
        base((ushort)(80 * ModContent.GetInstance<SpawnHousesConfig>().SpawnPointBasementMultiplier), 
            (ushort)(58 * ModContent.GetInstance<SpawnHousesConfig>().SpawnPointBasementMultiplier),
            [
                new MainBasement_Room1            (cost: 12, weight: 40),
                new MainBasement_Room1_WithFloor  (cost: 14, weight: 130),
                new MainBasement_Room2            (cost: 13, weight: 40),
                new MainBasement_Room2_WithRoof   (cost: 15, weight: 90),
                new MainBasement_Room3            (cost: 8, weight: 115),
                new MainBasement_Room4            (cost: 11, weight: 5),
                new MainBasement_Room5            (cost: 13, weight: 145),
                new MainBasement_Room6            (cost: 14, weight: 115),
                new MainBasement_Room7            (cost: 14, weight: 80),
                new MainBasement_Hallway4         (cost: 5, weight: 100),
                new MainBasement_Hallway5         (cost: 5, weight: 100),
                new MainBasement_Hallway9         (cost: 4, weight: 100)
            ],
            x, y,
            (byte)Math.Round(1 * ModContent.GetInstance<SpawnHousesConfig>().SpawnPointBasementMultiplier), 
            (byte)Math.Round(3 * ModContent.GetInstance<SpawnHousesConfig>().SpawnPointBasementMultiplier),
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
            startingBoundingBoxes, status, generateSubstructures) {}


    protected override bool IsChainComplete()
    {
        if (ModHelper.IsMSEnabled && ModContent.GetInstance<SpawnHousesConfig>().SpawnPointBasementMultiplier > 0.60)
        {
            bool found = false;
            ActionOnEachStructure(structure =>
            {
                if (structure.ID == (ushort)StructureID.MainBasement_Room5) found = true;
            });
            return found;
        }
        else return true;
    }

    protected override bool IsConnectPointValid(ChainConnectPoint connectPoint, ChainConnectPoint targetConnectPoint, CustomChainStructure rootStructure)
    {
        if ((connectPoint.ParentStructure.ID == (ushort)StructureID.MainBasement_Entry1 || connectPoint.ParentStructure.ID == (ushort)StructureID.MainBasement_Entry2) &&
            connectPoint.RootPoint)
            return false;

        if (targetConnectPoint.Y < rootStructure.Y + 10)
            return false;
        
        return true;
    }

    // protected override bool ConnectPointAttrition(ChainConnectPoint connectPoint, byte currentBranchLength, byte minBranchLength,
    //     byte maxBranchLength)
    // {
    //     float shape = ModContent.GetInstance<SpawnHousesConfig>().MainBasementShape;
    //     if (connectPoint.Direction is Directions.Left or Directions.Right)
    //     {
    //         if (Terraria.WorldGen.genRand.NextDouble() >= shape)
    //             return false;
    //     }
    //     else
    //         if (Terraria.WorldGen.genRand.NextDouble() <= shape)
    //             return false;
    //     
    //     if (connectPoint.GenerateChance != GenerateChances.Guaranteed)
    //         if ((Terraria.WorldGen.genRand.Next(0, maxBranchLength - currentBranchLength) == 0 ||
    //              currentBranchLength >= maxBranchLength) && currentBranchLength >= minBranchLength)
    //             return false;
    //         
    //     if (connectPoint.GenerateChance == GenerateChances.Rejected)
    //         return false;
    //     return true;
    // }

    protected override void OnStructureGenerate(CustomChainStructure structure)
    {
        if (structure.ID != (ushort)StructureID.MainBasement_Room5 || !ModHelper.IsMSEnabled)
            foreach (var boundingBox in structure.StructureBoundingBoxes)
                GenHelper.GenerateCobwebs(new Point(boundingBox.Point1.X, boundingBox.Point1.Y),
                    (ushort)(boundingBox.Point2.X - boundingBox.Point1.X + 1),
                    (ushort)(boundingBox.Point2.Y - boundingBox.Point1.Y + 1));
        
        int centerX = structure.X + (structure.StructureXSize / 2);
        int centerY = structure.Y + (structure.StructureXSize / 2);
        WorldUtils.Gen(new Point(centerX, centerY), new Shapes.Circle((structure.StructureXSize + structure.StructureYSize + 2) / 2), Actions.Chain(
            new Modifiers.OnlyWalls(WallID.DirtUnsafe, WallID.GrassUnsafe, WallID.Cave2Unsafe, WallID.Cave3Unsafe, WallID.Cave4Unsafe, WallID.Cave5Unsafe, WallID.Cave6Unsafe, WallID.Cave7Unsafe),
            new Actions.PlaceTile(TileID.Dirt)
        ));
        
        structure.FrameTiles();
    }

    public override void OnFound()
    {
        base.OnFound();
        
        Wiring.TripWire(EntryPosX + 2, EntryPosY + 11, 1, 1);
        Status = StructureStatus.GeneratedAndFound;
    }
}