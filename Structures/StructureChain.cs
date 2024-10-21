using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
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
    public byte Status;
    public CustomChainStructure RootStructure;
    public readonly bool SuccessfulGeneration = true;
    
    private readonly List<ChainConnectPoint> _failedConnectPointList = [];
    private readonly BoundingBox[] StartingBoundingBoxes;
    private int _structureListWeightSum;
    private CustomChainStructure[] _usableStructureList;
    private Bridge[] Bridges;
    private List<ChainConnectPoint> failedConnectPointList = [];

    public StructureChain(ushort maxCost, ushort minCost, CustomChainStructure[] structureList, ushort entryPosX, ushort entryPosY,
        byte minBranchLength, byte maxBranchLength, Bridge[] bridges, CustomChainStructure[] rootStructureList = null, BoundingBox[] startingBoundingBoxes = null,
        byte status = StructureStatus.NotGenerated, bool generateSubstructures = true)
    {
        EntryPosX = entryPosX;
        EntryPosY = entryPosY;
        Status = status;
        StartingBoundingBoxes = startingBoundingBoxes;
        Bridges = bridges;
        
        // assume it's a blank object and just return
        if (!generateSubstructures)
            return;
        
        SuccessfulGeneration = true;
        
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

        bool foundValidStructureChain = false;
        const ushort maxAttempts = 250;
        // try to find a configuration that satisfies the minCost
        for (byte attempts = 0; attempts < maxAttempts; attempts++)
        {
            CopiedStructureList = (CustomChainStructure[])structureList.Clone();
            for (byte i = 0; i < structureList.Length; i++)
                CopiedStructureList[i] = structureList[i].Clone();
            
            if (rootStructureList == null)
                rootStructure = NewStructure(null, false, entryPosX, entryPosY);
            else
            {
                int index = Terraria.WorldGen.genRand.Next(0, rootStructureList.Length);
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
            if (StartingBoundingBoxes is not null && StartingBoundingBoxes.Length != 0)
                boundingBoxes.AddRange(StartingBoundingBoxes);
            boundingBoxes.AddRange(rootStructure.StructureBoundingBoxes);

            var points = queuedConnectPoints; // because scope
            rootStructure.ActionOnEachConnectPoint(connectPoint =>
            {
                connectPoint.BranchLength = 0;
                points.Add(connectPoint);
            });
            
            while (queuedConnectPoints.Count > 0)
            {
                ChainConnectPoint connectPoint = queuedConnectPoints[0];
                CalculateChildrenStructures(connectPoint);
                queuedConnectPoints.RemoveAt(0);
            }
            
            if (currentCost >= minCost)
            {
                RootStructure = rootStructure;
                if (!IsChainComplete()) continue;
                
                foundValidStructureChain = true;
                break;
            }
        }

        // if we didn't find what we needed in 250 tries, abort
        if (!foundValidStructureChain)
        {
            ModContent.GetInstance<SpawnHouses>().Logger.Error($"Failed to generate StructureChain of type {this.ToString()}. Please report this error, this seed, and your client.log to the mod's author");
            return;
        }
        
        RootStructure = rootStructure;
        _failedConnectPointList = failedConnectPointList;
        SuccessfulGeneration = true;
        
        
        
        
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
        
        void CalculateChildrenStructures(ChainConnectPoint connectPoint)
        {
            byte currentBranchLength = connectPoint.BranchLength;
            byte targetDirection = connectPoint.Direction;

            if (currentBranchLength > maxBranchLength) // extra failsafe
            {
                failedConnectPointList.Add(connectPoint);
                return;
            }

            if (!ConnectPointAttrition(connectPoint, currentBranchLength, minBranchLength, maxBranchLength))
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
                if (newStructure is null)
                {
                    failedConnectPointList.Add(connectPoint);
                    return;
                }
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
            
            for (int findLocationAttempts = 0; findLocationAttempts < 25; findLocationAttempts++)
            {
                // randomly pick either the top or bottom side
                int randomSide = 0; //Terraria.WorldGen.genRand.Next(0, 2);
                
                connectPointBridge = GetBridgeOfDirection(Bridges, targetDirection, newStructure);
                if (connectPointBridge is null)
                {
                    failedConnectPointList.Add(connectPoint);
                    return;
                }

                var position = SetProspectiveStructurePosition(newStructure, connectPoint, connectPointBridge);
                int newStructureConnectPointX = position.newStructureConnectPointX;
                int newStructureConnectPointY = position.newStructureConnectPointY;
                
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

                if (!IsConnectPointValid(connectPoint, targetConnectPoint, rootStructure)) continue;
                
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
            connectPoint.ChildBridge.Point1 = connectPoint;
            connectPoint.ChildBridge.Point2 = targetConnectPoint;
            
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
    /// Called to physcially generate the structures of the chain. Changes status
    /// </summary>
    /// <returns>true if success, otherwise false</returns>
    public virtual bool Generate()
    {
        // if the initial setup didn't work, don't attempt to generate
        if (!SuccessfulGeneration)
            return false;
        
        List<Bridge> bridgeList = [];
        
        ActionOnEachStructure(structure =>
        {
            structure.Generate();
            OnStructureGenerate(structure);
            structure.ActionOnEachConnectPoint(connectPoint =>
            {
                if (connectPoint.ChildBridge is not null)
                {
                    Bridge bridge = connectPoint.ChildBridge;
                    bridgeList.Add(bridge); 
                }
            });
        });
        RootStructure.Generate();
        OnStructureGenerate(RootStructure);
        
        foreach (var bridge in bridgeList)
            bridge.Generate();
        
        foreach (var connectPoint in _failedConnectPointList)
            connectPoint.GenerateSeal();
        
        Status = StructureStatus.GeneratedButNotFound;
        
        return true;
    }
    
    /// <summary>
    /// Recursively calls OnFound() on every structure in the chain
    /// </summary>
    public virtual void OnFound()
    {
        ActionOnEachStructure(structure =>
        {
            structure.OnFound();
        });
    }
    
    /// <summary>
    /// Calls the function with each structure in the chain.<para/>
    /// Sample Usage:<para/>
    /// ActionOnEachStructure(structure =>
    /// {
    ///     structure.Generate();
    /// });
    /// </summary>
    /// <param name="function"></param>
    public void ActionOnEachStructure(Action<CustomChainStructure> function)
    {
        void Recursive(CustomChainStructure structure)
        {
            function(structure);
            structure.ActionOnEachConnectPoint(connectPoint =>
            {
                if (connectPoint.ChildStructure is not null)
                    Recursive(connectPoint.ChildStructure);
            });
        }
        Recursive(RootStructure);
    }
    /// <summary>
    /// Calls the function with each connectPoint in the chain.<para/>
    /// Sample Usage:<para/>
    /// ActionOnEachConnectPoint(connectPoint =>
    /// {
    ///     connectPoint.GenerateSeal();
    /// });
    /// </summary>
    /// <param name="function"></param>
    public void ActionOnEachConnectPoint(Action<ChainConnectPoint> function)
    {
        void Recursive(CustomChainStructure structure)
        {
            structure.ActionOnEachConnectPoint(connectPoint =>
            {
                function(connectPoint);
                
                if (connectPoint.ChildStructure is not null)
                    Recursive(connectPoint.ChildStructure);
            });
        }
        Recursive(RootStructure);
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
            double randomValue = Terraria.WorldGen.genRand.NextDouble() * _structureListWeightSum;
            CustomChainStructure structure = _usableStructureList.Last(curStructure => curStructure.Weight <= randomValue).Clone();
        
            // don't generate a branching hallway right after another one :)
            if (parentConnectPoint is not null && parentConnectPoint.GenerateChance == GenerateChances.Guaranteed && StructureIDUtils.IsBranchingHallway(structure))
                continue;
                
            // don't generate a branching hallway if it means going over the max branch count
            if (closeToMaxBranchLength && StructureIDUtils.IsBranchingHallway(structure)) continue;

            return structure;
        }

        return null;
    }
    
    /// <summary>
    /// Called when we need a new bridge. will call with 1 attempt
    /// </summary>
    /// <param name="bridges"></param>
    /// <param name="direction"></param>
    /// <param name="structure"></param>
    /// <returns>The bridge object to be used for the generation</returns>
    protected virtual Bridge GetBridgeOfDirection(Bridge[] bridges, byte direction, CustomChainStructure structure)
    {
        for (ushort i = 0; i < 5000; i++)
        {
            int index = Terraria.WorldGen.genRand.Next(0, bridges.Length);
            if (bridges[index].InputDirections[0] == direction)
                return bridges[index].Clone();
        }
        return null;
    }

    /// <summary>
    /// Called after choosing bridge and structure. This method decides where the next structure goes (based on its cp)
    /// </summary>
    /// <param name="structure"></param>
    /// <param name="connectPoint"></param>
    /// <param name="bridge"></param>
    /// <returns></returns>
    protected virtual (int newStructureConnectPointX, int newStructureConnectPointY) SetProspectiveStructurePosition(
        CustomChainStructure structure, ChainConnectPoint connectPoint, Bridge bridge)
    {
        int deltaX, deltaY;
        if (bridge.DeltaXMultiple != 0)
        {
            int minRangeX = bridge.MinDeltaX / bridge.DeltaXMultiple;
            int maxRangeX = bridge.MaxDeltaX / bridge.DeltaXMultiple;
            deltaX = Terraria.WorldGen.genRand.Next(minRangeX, maxRangeX + 1) * bridge.DeltaXMultiple;
        }
        else
            deltaX = 0;

        if (bridge.DeltaYMultiple != 0)
        {
            int minRangeY = bridge.MinDeltaY / bridge.DeltaYMultiple;
            int maxRangeY = bridge.MaxDeltaY / bridge.DeltaYMultiple;
            deltaY = Terraria.WorldGen.genRand.Next(minRangeY, maxRangeY + 1) * bridge.DeltaYMultiple;
        }
        else
            deltaY = 0;
                
        // note: the +/-1 is because connect points have no space between them even when the deltaX is 1
        return (connectPoint.X + deltaX + 1, connectPoint.Y + deltaY + 1);
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
    /// Called before attempting to generate a connect point's children
    /// </summary>
    /// <param name="connectPoint"></param>
    /// <param name="currentBranchLength"></param>
    /// <param name="minBranchLength"></param>
    /// <param name="maxBranchLength"></param>
    /// <returns>True if connect point is valid</returns>
    protected virtual bool ConnectPointAttrition(ChainConnectPoint connectPoint, byte currentBranchLength,
        byte minBranchLength, byte maxBranchLength)
    {
        if (connectPoint.GenerateChance != GenerateChances.Guaranteed)
            if ((Terraria.WorldGen.genRand.Next(0, maxBranchLength - currentBranchLength) == 0 ||
                 currentBranchLength >= maxBranchLength) && currentBranchLength >= minBranchLength)
                return false;
        if (connectPoint.GenerateChance == GenerateChances.Rejected)
            return false;
        return true;
    }

    /// <summary>
    /// Called before generating ConnectPoint's children.
    /// </summary>
    /// <param name="connectPoint"></param>
    /// <param name="targetConnectPoint"></param>
    /// <param name="rootStructure"></param>
    /// <returns>If true, children will generate</returns>
    protected virtual bool IsConnectPointValid(ChainConnectPoint connectPoint, ChainConnectPoint targetConnectPoint, CustomChainStructure rootStructure)
    {
        return true;
    }

    /// <summary>
    /// Called for each CustomChainStructure right after it gets generated.
    /// </summary>
    protected virtual void OnStructureGenerate(CustomChainStructure structure)
    {
        return;
    }
}
