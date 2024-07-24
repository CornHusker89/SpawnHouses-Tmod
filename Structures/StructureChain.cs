using System;
using System.Collections.Generic;
using System.Linq;
using Terraria.DataStructures;
using Terraria.ModLoader;
using SpawnHouses.Structures.StructureParts;

using Terraria.Utilities;
using BoundingBox = SpawnHouses.Structures.StructureParts.BoundingBox;

namespace SpawnHouses.Structures;

public abstract class StructureChain
{
    public readonly ushort EntryPosX;
    public readonly ushort EntryPosY;
    public readonly int Seed;
    public byte Status = StructureStatus.NotGenerated;

    private readonly UnifiedRandom _randomNumberGen;
    private readonly CustomChainStructure _rootStructure;
    private readonly List<ChainConnectPoint> _failedConnectPointList = [];
    private readonly bool _successfulGeneration = false;
    

    public StructureChain(ushort maxCost, ushort minCost, CustomChainStructure[] structureList, ushort entryPosX, ushort entryPosY,
        byte minBranchLength, byte maxBranchLength, CustomChainStructure[] rootStructureList = null, string requiredStructureID = null, bool keepRootPointClear = true, bool ignoreInvalidDirections = false,
        int seed = -1, byte status = StructureStatus.NotGenerated)
    {
        Seed = seed == -1 ? Terraria.WorldGen.genRand.Next(0, int.MaxValue) : seed;
        EntryPosX = entryPosX;
        EntryPosY = entryPosY;
        Status = status;
        _randomNumberGen = new UnifiedRandom(Seed);
        
        // assume it's a blank object and just return
        if (entryPosX == 0 && entryPosY == 0)
            return;
        
        foreach (var structure in structureList)
            if (structure.Cost < 0)
                throw new Exception("invalid item cost in structureList");

        if (structureList.Length == 0)
            throw new Exception("structureList had no valid options");

        CustomChainStructure[] UseableStructureList = (CustomChainStructure[])structureList.Clone();
        for (byte i = 0; i < structureList.Length; i++)
            UseableStructureList[i] = structureList[i].Clone();
            
        int currentCost = 0;
        ushort structureListWeightSum;
        CalculateWeights();
        
        CustomChainStructure rootStructure = null;
        ChainConnectPoint rootConnectPoint = null;
        
        List<BoundingBox> boundingBoxes;
        List<ChainConnectPoint> queuedConnectPoints;
        List<ChainConnectPoint> failedConnectPointList = [];

        bool foundValidStructureChain = false;
        const ushort maxAttempts = 250;
        // try to find a configuration that satisfies the minCost
        for (byte attempts = 0; attempts < maxAttempts; attempts++)
        {
            if (rootStructureList == null)
                rootStructure = NewStructure(null, false, entryPosX, entryPosY);
            else
            {
                int index = _randomNumberGen.Next(0, rootStructureList.Length);
                rootStructure = rootStructureList[index].Clone();
                rootStructure.SetPosition(entryPosX, entryPosY);
            }
            rootConnectPoint = rootStructure.GetRootConnectPoint();
            if (rootConnectPoint != null)
                MoveConnectPointAndStructure(rootStructure, rootConnectPoint, entryPosX, entryPosY);
            
            currentCost = rootStructure.Cost;
            queuedConnectPoints = [];
            failedConnectPointList = [];
            boundingBoxes = [];
            boundingBoxes.AddRange(rootStructure.StructureBoundingBoxes);
            
            for (byte direction = 0; direction < 4; direction++)
            {
                for (byte index = 0; index < rootStructure.ConnectPoints[direction].Length; index++)
                {
                    if (keepRootPointClear && rootStructure.ConnectPoints[direction][index] == rootConnectPoint)
                        continue;

                    rootStructure.ConnectPoints[direction][index].BranchLength = 0;
                    queuedConnectPoints.Add(rootStructure.ConnectPoints[direction][index]);
                }
            }
            
            while (queuedConnectPoints.Count > 0)
            {
                CalculateChildrenStructures(queuedConnectPoints[0], rootStructure);
                queuedConnectPoints.RemoveAt(0);
            }

            if (currentCost >= minCost)
            {
                if (requiredStructureID is not null)
                {
                    bool found = false;
                    void SearchFilePathRecursive(CustomChainStructure structure)
                    {
                        if (found) return;
                        if (structure.FilePath == requiredStructureID)
                            found = true;
                        else
                            for (byte direction = 0; direction < 4; direction++)
                                foreach (var connectPoint in structure.ConnectPoints[direction])
                                {
                                    if (connectPoint.ChildStructure is null) continue;
                                    SearchFilePathRecursive(connectPoint.ChildStructure);
                                }
                    }
                    SearchFilePathRecursive(rootStructure);
                    if (!found && attempts < maxAttempts * 0.7)
                        continue;
                }
                
                foundValidStructureChain = true;
                break;
            }
        }

        // if we didn't find what we needed in 200 tries, abort
        if (!foundValidStructureChain)
        {
            ModContent.GetInstance<SpawnHouses>().Logger.Error($"Failed to generate StructureChain of type {this.ToString()} with seed {Seed}. Please report this error and it's information to the mod's author");
            return;
        }
        
        _rootStructure = rootStructure;
        _failedConnectPointList = failedConnectPointList;
        _successfulGeneration = true;
        
        
        
        
        // functions

        void CalculateWeights()
        {
            //make the weights in useableStructureList cumulative, and make the starting weight 0
            for (byte i = 1; i < UseableStructureList.Length; i++)
                UseableStructureList[i].Weight = (ushort)(UseableStructureList[i].Weight + UseableStructureList[i - 1].Weight);

            for (byte i = 0; i < UseableStructureList.Length; i++)
                UseableStructureList[i].Weight -= UseableStructureList[0].Weight;
            structureListWeightSum = UseableStructureList[^1].Weight;
        }
       
        CustomChainStructure NewStructure(ChainConnectPoint parentConnectPoint, bool closeToMaxBranchLength = false, int x = 500, int y = 500)
        {
            CustomChainStructure structure = null;
            for (int i = 0; i < 5000; i++)
            {
                double randomValue = _randomNumberGen.NextDouble() * structureListWeightSum;
                structure = UseableStructureList.Last(curStructure => curStructure.Weight <= randomValue).Clone();

                // don't generate a branching hallway right after another one :)
                if (parentConnectPoint.GenerateChance == GenerateChances.Guaranteed &&
                    CustomChainStructure.BranchingHallwayIDs.Contains(structure.FilePath))
                {
                    structure = null;
                    continue;
                }
                
                // don't generate a branching hallway if it means going over the max branch count
                if (closeToMaxBranchLength && CustomChainStructure.BranchingHallwayIDs.Contains(structure.FilePath))
                {
                    structure = null;
                    continue;
                }
                
                structure.ParentChainConnectPoint = parentConnectPoint;
                structure.SetPosition(x, y);
                break;
            }
            return structure;
        }

        void MoveConnectPointAndStructure(CustomChainStructure structure, ChainConnectPoint connectPoint, int x, int y)
        {
            int deltaX = connectPoint.X - x;
            int deltaY = connectPoint.Y - y;
            structure.SetPosition(structure.X - deltaX, structure.Y - deltaY);
        }

        Bridge GetBridgeOfDirection(Bridge[] bridges, byte direction)
        {
            for (ushort i = 0; i < 5000; i++)
            {
                int index = _randomNumberGen.Next(0, bridges.Length);
                if (bridges[index].InputDirections[0] == direction)
                    return bridges[index];
            }

            if (ignoreInvalidDirections)
                return null;
            else
                throw new Exception($"bridge of direction {direction} was not found in this structure's ChildBridgeTypes");
        }
        
        void CalculateChildrenStructures(ChainConnectPoint connectPoint, CustomChainStructure connectPointParentStructure)
        {
            byte currentBranchLength = connectPoint.BranchLength;
            byte targetDirection = connectPoint.Direction;
            
            if (connectPoint.GenerateChance != GenerateChances.Guaranteed)
                if ((_randomNumberGen.Next(0, maxBranchLength - currentBranchLength) == 0 || currentBranchLength >= maxBranchLength) && currentBranchLength >= minBranchLength)
                {
                    failedConnectPointList.Add(connectPoint);
                    return;
                }
            if (connectPoint.GenerateChance == GenerateChances.Rejected)
            {
                failedConnectPointList.Add(connectPoint);
                return;
            }
            
            // try to find a structure that won't result in a cost overrun
            bool foundStructure = false;
            CustomChainStructure newStructure = null;
            for (int attempts = 0; attempts < 20; attempts++)
            {
                bool closeToMaxBranchLen = connectPoint.BranchLength >= maxBranchLength - 1;
                newStructure = NewStructure(connectPoint, closeToMaxBranchLen);
                if (currentCost + newStructure.Cost <= maxCost)
                {
                    foundStructure = true;
                    break;
                }
            }
            
            if (!foundStructure)
            {
                failedConnectPointList.Add(connectPoint);
                return;
            }

            ChainConnectPoint targetConnectPoint = null;
            Bridge connectPointBridge = null;
            bool validLocation = false;
            
            for (int findLocationAttempts = 0; findLocationAttempts < 20; findLocationAttempts++)
            {
                // randomly pick either the top or bottom side
                int randomSide = 0; //Terraria.WorldGen.genRand.Next(0, 2);
                
                connectPointBridge = GetBridgeOfDirection(connectPointParentStructure.ChildBridgeTypes, targetDirection);
                if (connectPointBridge == null)
                {
                    failedConnectPointList.Add(connectPoint);
                    return;
                }
                
                int deltaX, deltaY;
                if (connectPointBridge.DeltaXMultiple != 0)
                {
                    int minRangeX = connectPointBridge.MinDeltaX / connectPointBridge.DeltaXMultiple;
                    int maxRangeX = connectPointBridge.MaxDeltaX / connectPointBridge.DeltaXMultiple;
                    deltaX = _randomNumberGen.Next(minRangeX, maxRangeX + 1) * connectPointBridge.DeltaXMultiple;
                }
                else
                    deltaX = 0;

                if (connectPointBridge.DeltaYMultiple != 0)
                {
                    int minRangeY = connectPointBridge.MinDeltaY / connectPointBridge.DeltaYMultiple;
                    int maxRangeY = connectPointBridge.MaxDeltaY / connectPointBridge.DeltaYMultiple;
                    deltaY = _randomNumberGen.Next(minRangeY, maxRangeY + 1) * connectPointBridge.DeltaYMultiple;
                }
                else
                    deltaY = 0;
                
                // note: the +/-1 is because connect points have no space between them even when the deltaX is 1
                
                int newStructureConnectPointX = connectPoint.X + deltaX + 1;
                int newStructureConnectPointY = connectPoint.Y + deltaY + 1;
                
                byte secondConnectPointDirection;
                if (connectPointBridge.InputDirections[0] == targetDirection)
                    secondConnectPointDirection = connectPointBridge.InputDirections[1];
                else
                    secondConnectPointDirection = connectPointBridge.InputDirections[0];
                    
                ChainConnectPoint[] targetConnectPointList = newStructure.ConnectPoints[secondConnectPointDirection];
                
                
                if (targetConnectPointList.Length != 0)
                    targetConnectPoint = targetConnectPointList[(targetConnectPointList.Length - 1) * randomSide];
                else
                    continue; // try another bridge
                
                MoveConnectPointAndStructure(newStructure, targetConnectPoint, newStructureConnectPointX, newStructureConnectPointY);
                connectPointBridge.SetPoints(connectPoint, targetConnectPoint);


                if (
                    !BoundingBox.IsAnyBoundingBoxesColliding(newStructure.StructureBoundingBoxes, boundingBoxes) &&
                    !BoundingBox.IsAnyBoundingBoxesColliding(connectPointBridge.BoundingBoxes, boundingBoxes)
                ) {
                    validLocation = true;
                    break;
                }
            }

            if (!validLocation)
            {
                failedConnectPointList.Add(connectPoint);
                return;
            }
            
            boundingBoxes.AddRange(newStructure.StructureBoundingBoxes);
            boundingBoxes.AddRange(connectPointBridge.BoundingBoxes);
            
            connectPoint.ChildStructure = newStructure;
            connectPoint.ChildConnectPoint = targetConnectPoint;
            connectPoint.ChildBridge = connectPointBridge;
            
            currentCost += newStructure.Cost;
            
            // reduce the weighting of the chosen structure so that we don't get 5 in a row 
            UseableStructureList.First(curStructure => curStructure.FilePath == newStructure.FilePath).Weight /= 2;
            CalculateWeights();
            
            for (byte direction = 0; direction < 4; direction++)
                foreach (ChainConnectPoint nextConnectPoint in newStructure.ConnectPoints[direction])
                {
                    // if the point already has the bridge on it
                    if (connectPoint.ChildConnectPoint == nextConnectPoint)
                        continue;
                    
                    // """recursive"""
                    nextConnectPoint.BranchLength = (byte)(currentBranchLength + 1);
                    queuedConnectPoints.Add(nextConnectPoint);
                }
        }
    }

    public virtual void OnFound()
    {
        //recusively call onfound() on every structure in the chain
        void OnFoundRecursive(CustomChainStructure structure)
        {
            structure.OnFound();

            for (byte direction = 0; direction < 4; direction++)
            {
                foreach (ChainConnectPoint connectPoint in structure.ConnectPoints[direction])
                {
                    if (connectPoint.ChildStructure == null) continue;
                    OnFoundRecursive(connectPoint.ChildStructure);
                }
            }
        }
        
        OnFoundRecursive(_rootStructure);
    }

    public virtual void Generate()
    {
        // if the initial setup didn't work, don't attempt to generate
        if (!_successfulGeneration)
            return;
        
        List<Bridge> bridgeList = [];
        GenerateChildren(_rootStructure);
        
        foreach (var bridge in bridgeList)
            bridge.Generate();

        foreach (var connectPoint in _failedConnectPointList)
            connectPoint.GenerateSeal();
        
        
        // functions
        void GenerateChildren(CustomChainStructure structure)
        {
            structure.Generate();
            for (byte direction = 0; direction < 4; direction++)
            {
                foreach (ChainConnectPoint connectPoint in structure.ConnectPoints[direction])
                {
                    //if (connectPoint.GenerateChance != GenerateChances.Guaranteed && connectPoint.BranchLength > maxBranchLength) continue;
                    if (connectPoint.ChildStructure is null) continue;

                    Bridge bridge = connectPoint.ChildBridge;
                    
                    bridge.Point1 = connectPoint;
                    bridge.Point2 = connectPoint.ChildConnectPoint;
                    bridgeList.Add(bridge.Clone());
                    
                    //recursive call
                    GenerateChildren(connectPoint.ChildStructure);
                }
            }
        }
    }
}
