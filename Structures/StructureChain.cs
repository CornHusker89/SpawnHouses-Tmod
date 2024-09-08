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
    private int _structureListWeightSum;
    private CustomChainStructure[] _usableStructureList;

    public StructureChain(ushort maxCost, ushort minCost, CustomChainStructure[] structureList, ushort entryPosX, ushort entryPosY,
        byte minBranchLength, byte maxBranchLength, CustomChainStructure[] rootStructureList = null,
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
        
        CustomChainStructure[] CopiedStructureList = (CustomChainStructure[])structureList.Clone();
        for (byte i = 0; i < structureList.Length; i++)
            CopiedStructureList[i] = structureList[i].Clone();
            
        int currentCost = 0;
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
                foreach (ChainConnectPoint connectPoint in rootStructure.ConnectPoints[direction])
                {
                    if (!IsConnectPointValid(connectPoint)) continue;
                    
                    connectPoint.BranchLength = 0;
                    queuedConnectPoints.Add(connectPoint);
                }
            }
            
            while (queuedConnectPoints.Count > 0)
            {
                ChainConnectPoint connectPoint = queuedConnectPoints[0];
                
                if (IsConnectPointValid(connectPoint))
                    CalculateChildrenStructures(connectPoint, rootStructure);
                else
                    failedConnectPointList.Add(connectPoint);
                
                queuedConnectPoints.RemoveAt(0);
            }

            if (currentCost >= minCost)
            {
                if (IsChainComplete()) continue;
                
                foundValidStructureChain = true;
                break;
            }
        }

        // if we didn't find what we needed in 200 tries, abort
        if (!foundValidStructureChain)
        {
            ModContent.GetInstance<SpawnHouses>().Logger.Error($"Failed to generate StructureChain of type {this.ToString()} with seed {Seed}. Please report this error your client.log to the mod's author\n" + SpawnHousesSystem.WorldConfig);
            return;
        }
        
        _rootStructure = rootStructure;
        _failedConnectPointList = failedConnectPointList;
        _successfulGeneration = true;
        
        
        
        
        // functions
        void CalculateWeights()
        {
            _usableStructureList = (CustomChainStructure[])CopiedStructureList.Clone();
            for (byte i = 0; i < CopiedStructureList.Length; i++)
                _usableStructureList[i] = CopiedStructureList[i].Clone();
            
            _structureListWeightSum = CopiedStructureList.Sum(curStructure => curStructure.Weight);
            
            //make the weights in useableStructureList cumulative, and make the starting weight 0
            _usableStructureList[0].Weight = 0;
            
            for (int i = 1; i < CopiedStructureList.Length; i++)
                _usableStructureList[i].Weight = (ushort)(_usableStructureList[i - 1].Weight + CopiedStructureList[i - 1].Weight);
        }
       
        CustomChainStructure NewStructure(ChainConnectPoint parentConnectPoint, bool closeToMaxBranchLength = false, int x = 500, int y = 500)
        {
            CustomChainStructure structure = null;
            for (int i = 0; i < 100; i++)
            {
                structure = GetNewStructure(parentConnectPoint, closeToMaxBranchLength);
                
                if (structure is not null)
                {
                    structure.ParentChainConnectPoint = parentConnectPoint;
                    structure.SetPosition(x, y);
                    break;
                }
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
            return null;
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
                    //BoundingBox.VisualizeCollision();
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
            connectPoint.ChildStructure.BridgeDirectionHistory = CustomChainStructure.CloneBridgeDirectionHistory(connectPoint.ParentStructure);
            connectPoint.ChildStructure.BridgeDirectionHistory.Add(connectPoint.Direction);
            connectPoint.ChildConnectPoint = targetConnectPoint;
            connectPoint.ChildBridge = connectPointBridge;
            
            currentCost += newStructure.Cost;
            
            // reduce the weighting of the chosen structure so that we don't get 5 in a row 
            CopiedStructureList.First(curStructure => curStructure.ID == newStructure.ID).Weight /= 2;
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

    /// <summary>
    /// Recursively calls OnFound() on every structure in the chain
    /// </summary>
    public virtual void OnFound()
    {
        ActionOnEachStructure(function: (structure =>
        {
            structure.OnFound();
        }));
    }

    public virtual void Generate()
    {
        // if the initial setup didn't work, don't attempt to generate
        if (!_successfulGeneration)
            return;
        
        List<Bridge> bridgeList = [];
        
        ActionOnEachConnectPoint(function: (connectPoint =>
        {
            Bridge bridge = connectPoint.ChildBridge;
                    
            bridge.Point1 = connectPoint;
            bridge.Point2 = connectPoint.ChildConnectPoint;
            bridgeList.Add(bridge.Clone());
        }));
        
        foreach (var bridge in bridgeList)
            bridge.Generate();

        foreach (var connectPoint in _failedConnectPointList)
            connectPoint.GenerateSeal();
    }
    
    /// <summary>
    /// Calls the function with each structure in the chain.<para/>
    /// Sample Usage:<para/>
    /// ActionOnEachStructure(function: (structure =>
    /// {
    ///     structure.Generate();
    /// }));
    /// </summary>
    /// <param name="function"></param>
    public void ActionOnEachStructure(Action<CustomChainStructure> function)
    {
        void Recursive(CustomChainStructure structure)
        {
            function(structure);
            
            for (byte direction = 0; direction < 4; direction++)
                foreach (ChainConnectPoint connectPoint in structure.ConnectPoints[direction])
                {
                    if (connectPoint.ChildStructure is null) continue;
                    Recursive(connectPoint.ChildStructure);
                }
        }
        Recursive(_rootStructure);
    }
    /// <summary>
    /// Calls the function with each connectPoint in the chain.<para/>
    /// Sample Usage:<para/>
    /// ActionOnEachConnectPoint(function: (connectPoint =>
    /// {
    ///     connectPoint.GenerateSeal();
    /// }));
    /// </summary>
    /// <param name="function"></param>
    public void ActionOnEachConnectPoint(Action<ChainConnectPoint> function)
    {
        void Recursive(CustomChainStructure structure)
        {
            for (byte direction = 0; direction < 4; direction++)
                foreach (ChainConnectPoint connectPoint in structure.ConnectPoints[direction])
                {
                    function(connectPoint);
                    
                    if (connectPoint.ChildStructure is null) continue;
                    Recursive(connectPoint.ChildStructure);
                }
        }
        Recursive(_rootStructure);
    }
    
    
    
    /// <summary>
    /// Called whenever the chain needs to have another structure. Will call this with 100 attempts. Return null to retry
    /// </summary>
    /// <param name="parentConnectPoint"></param>
    /// <param name="closeToMaxBranchLength"></param>
    /// <returns>The new structure to be used for generation</returns>
    protected virtual CustomChainStructure GetNewStructure(ChainConnectPoint parentConnectPoint, bool closeToMaxBranchLength)
    {
        for (int i = 0; i < 50; i++)
        {
            double randomValue = _randomNumberGen.NextDouble() * _structureListWeightSum;
            CustomChainStructure structure = _usableStructureList.Last(curStructure => curStructure.Weight <= randomValue).Clone();
        
            // don't generate a branching hallway right after another one :)
            if (parentConnectPoint is not null && parentConnectPoint.GenerateChance == GenerateChances.Guaranteed && StructureID.IsBranchingHallway(structure))
                continue;
                
            // don't generate a branching hallway if it means going over the max branch count
            if (closeToMaxBranchLength && StructureID.IsBranchingHallway(structure)) continue;

            return structure;
        }

        return null;
    }
    
    /// <summary>
    /// Called at the very end of Chain generation when object is created
    /// </summary>
    /// <returns>Whether the chain is complete. If returns false, will retry generation</returns>
    protected virtual bool IsChainComplete()
    {
        return true;
    }

    /// <summary>
    /// Called before generating ConnectPoint's children. Defaults to just keeping root point clear
    /// </summary>
    /// <param name="connectPoint"></param>
    /// <returns>If true, children will generate</returns>
    protected virtual bool IsConnectPointValid(ChainConnectPoint connectPoint)
    {
        if (connectPoint.RootPoint && connectPoint.ParentStructure == _rootStructure)
            return false;
        
        return true;
    }
}
