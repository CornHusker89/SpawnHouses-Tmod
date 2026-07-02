using System;
using System.Collections.Generic;
using System.Linq;

<<<<<<<<
Legacy / Structures / StructureChain.cs
using SpawnHouses.Legacy.Structures;
using SpawnHouses.Structures.StructureParts;
    ========
using SpawnHouses.Helpers;
>>>>>>>>
Types / StructureChain.cs
using Terraria;
using Terraria.DataStructures;
using Terraria.ModLoader;

namespace SpawnHouses.Types;

<<<<<<<<
Legacy / Structures / StructureChain.cs
public abstract class StructureChain : ISpawnable {
    ========

    public abstract class StructureChain {
        >>>>>>>>

    master:Types/StructureChain.cs
    private readonly Bridge[] _bridges;
    private readonly CustomChainStructure[] _originalStructureList;
    private readonly CustomChainStructure[] _rootStructureList;
    public readonly ushort EntryPosX;
    public readonly ushort EntryPosY;
    public readonly byte MaxBranchLength;
    public readonly ushort MaxCost;
    public readonly byte MinBranchLength;
    public readonly ushort MinCost;
        <<<<<<<< HEAD:Legacy/Structures/StructureChain.cs
    private readonly Box[] StartingBoundingBoxes;
    private List<Box> _boundingBoxes;
        ========

    private readonly BoundingBox[] StartingBoundingBoxes;

    private List<BoundingBox> _boundingBoxes;
        >>>>>>>> master:Types/StructureChain.cs
    private CustomChainStructure[] _copiedStructureList;
    private int _currentCost;
    private List<ChainConnectPoint> _failedConnectPointList = [];
    private List<ChainConnectPoint> _queuedConnectPoints;
    private int _structureListWeightSum;
    private CustomChainStructure[] _usableStructureList;

    public CustomChainStructure RootStructure;
    public byte Status;
    public bool SuccessfulGeneration = true;

    protected StructureChain(ushort minCost, ushort maxCost, byte minBranchLength, byte maxBranchLength,
        ushort entryPosX,
        ushort entryPosY, CustomChainStructure[] structureList, Bridge[] bridges,
        CustomChainStructure[] rootStructureList = null,
        Box[] startingBoundingBoxes = null,
        byte status = StructureStatus.NotGenerated) {
        MinCost = minCost;
        MaxCost = maxCost;
        MinBranchLength = minBranchLength;
        MaxBranchLength = maxBranchLength;
        EntryPosX = entryPosX;
        EntryPosY = entryPosY;
        _originalStructureList = structureList;
        _bridges = bridges;
        _rootStructureList = rootStructureList;
        StartingBoundingBoxes = startingBoundingBoxes;
        Status = status;
    }

    /// <summary>
    ///     Calculates full chain. Call before Generate()
    /// </summary>
    /// <exception cref="Exception"></exception>
    public void CalculateChain() {
        foreach (CustomChainStructure structure in _originalStructureList)
            if (structure.Cost < 0)
                throw new Exception("invalid item cost in structureList");

        if (_originalStructureList.Length == 0)
            throw new Exception("structureList had no valid options");

        _copiedStructureList = (CustomChainStructure[])_originalStructureList.Clone();
        for (byte i = 0; i < _originalStructureList.Length; i++)
            _copiedStructureList[i] = _originalStructureList[i].Clone();

        CalculateWeights();
            <<<<<<<<
        Legacy / Structures / StructureChain.cs
        ChainConnectPoint rootConnectPoint = null;
        bool foundValidStructureChain = false;
        // try to find a configuration that satisfies the minCost
        for (int attempts = 0; attempts < 15000; attempts++) {
        ========

        bool foundValidStructureChain = false;
        // try to find a configuration that satisfies the minCost
        for (int attempts = 0; attempts < 2500; attempts++) {
            >>>>>>>>
            Types / StructureChain.cs
            _copiedStructureList = (CustomChainStructure[])_originalStructureList.Clone();
            for (byte i = 0; i < _originalStructureList.Length; i++)
                _copiedStructureList[i] = _originalStructureList[i].Clone();

            if (_rootStructureList == null) {
                RootStructure = PlaceNewStructure(null, false, EntryPosX, EntryPosY);
            }
            else {
                <<<<<<<<
                Legacy / Structures / StructureChain.cs
                int index = WorldGen.genRand.Next(0, _rootStructureList.Length);
                    ========
                int index = WorldGen.genRand.Next(0, _rootStructureList.Length);
                    >>>>>>>>
                Types / StructureChain.cs
                RootStructure = _rootStructureList[index].Clone();
                RootStructure.SetPosition(EntryPosX, EntryPosY);
            }

            ChainConnectPoint rootConnectPoint = RootStructure.GetRootConnectPoint();
            if (rootConnectPoint != null)
                MoveConnectPointAndStructure(RootStructure, rootConnectPoint, EntryPosX, EntryPosY);

            _currentCost = RootStructure.Cost;
            _queuedConnectPoints = [];
            _failedConnectPointList = [];
            _boundingBoxes = [];
            if (StartingBoundingBoxes is not null && StartingBoundingBoxes.Length != 0)
                _boundingBoxes.AddRange(StartingBoundingBoxes);
            _boundingBoxes.AddRange(RootStructure.StructureBoundingBoxes);

            var points = _queuedConnectPoints; // because scope
            RootStructure.ActionOnEachConnectPoint(connectPoint => {
                connectPoint.BranchLength = 0;
                points.Add(connectPoint);
            });

            while (_queuedConnectPoints.Count > 0) {
                ChainConnectPoint connectPoint = _queuedConnectPoints[0];
                CalculateChildrenStructures(connectPoint);
                _queuedConnectPoints.RemoveAt(0);
            }

            if (_currentCost >= MinCost) {
                if (!IsChainComplete()) continue;

                foundValidStructureChain = true;
                break;
            }
        }

        // if we didn't find what we needed in however many tries, abort
        if (!foundValidStructureChain) {
            ModContent.GetInstance<SpawnHousesMod>().Logger.Error(
                $"Failed to generate StructureChain of type {ToString()}. Please report this error, this seed ({Main.ActiveWorldFileData.Seed}), and your client.log to the mod's author");
            SuccessfulGeneration = false;
            return;
        }

        // prune any dangling branching hallways
        ActionOnEachStructure(structure => {
            <<<<<<<<
            Legacy / Structures / StructureChain.cs
            if (StructureIDUtils.IsBranchingHallway(structure)) {
                ========
                if (StructureIdHelper.IsBranchingHallway(structure)) {
                    >>>>>>>>
                    Types / StructureChain.cs
                int count = 0;
                structure.ActionOnEachConnectPoint(point => {
                    if (point.ChildStructure is not null)
                        count++;
                });

                if (count == 0 &&
                    WorldGen.genRand
                        .NextBool()) // 0 because the bridge going into this is a parent, not child. also random bool cuz otherwise it feels too empty
                {
                    structure.ActionOnEachConnectPoint(point => { _failedConnectPointList.Remove(point); });
                    _failedConnectPointList.Add(structure.ParentChainConnectPoint);
                    structure.ParentChainConnectPoint.ChildBridge = null;
                    structure.ParentChainConnectPoint.ChildConnectPoint = null;
                    structure.ParentChainConnectPoint.ChildStructure = null;
                }
            }
        });
    }

    private void CalculateWeights() {
        _usableStructureList = (CustomChainStructure[])_copiedStructureList.Clone();
        for (byte i = 0; i < _copiedStructureList.Length; i++)
            _usableStructureList[i] = _copiedStructureList[i].Clone();

        _structureListWeightSum = _copiedStructureList.Sum(curStructure => curStructure.Weight);

        //make the weights in useableStructureList cumulative, and make the starting weight 0
        _usableStructureList[0].Weight = 0;

        for (int i = 1; i < _copiedStructureList.Length; i++)
            _usableStructureList[i].Weight =
                (ushort)(_usableStructureList[i - 1].Weight + _copiedStructureList[i - 1].Weight);
    }

    private CustomChainStructure PlaceNewStructure(ChainConnectPoint parentConnectPoint, bool closeToMaxBranchLength = false, int x = 500, int y = 500) {
        CustomChainStructure structure = null;
        for (int i = 0; i < 100; i++) {
            <<<<<<<<
            Legacy / Structures / StructureChain.cs
            structure = GetNewStructure(parentConnectPoint, closeToMaxBranchLength, _structureListWeightSum,
                _usableStructureList);
                ========
            structure = GetNewStructure(parentConnectPoint, closeToMaxBranchLength, _structureListWeightSum, _usableStructureList);
                >>>>>>>>
            Types / StructureChain.cs

            if (structure is not null) {
                structure.ParentChainConnectPoint = parentConnectPoint;
                structure.SetPosition(x, y);
                structure.ParentStructureChain = this;
                break;
            }
        }

        return structure;
    }
    <<<<<<<<

    Legacy / Structures / StructureChain.cs
    private void MoveConnectPointAndStructure(CustomChainStructure structure, ChainConnectPoint connectPoint, int x,
        int y) {
    ========

    private void MoveConnectPointAndStructure(CustomChainStructure structure, ChainConnectPoint connectPoint, int x, int y) {
        >>>>>>>>
        Types / StructureChain.cs
        int deltaX = connectPoint.X - x;
        int deltaY = connectPoint.Y - y;
        structure.SetPosition(structure.X - deltaX, structure.Y - deltaY);
    }

    private void CalculateChildrenStructures(ChainConnectPoint connectPoint) {
        byte currentBranchLength = connectPoint.BranchLength;
        byte targetDirection = connectPoint.Direction;

        if (currentBranchLength > MaxBranchLength) {
            // extra failsafe
            _failedConnectPointList.Add(connectPoint);
            return;
        }

        if (!ConnectPointAttrition(connectPoint, currentBranchLength, MinBranchLength, MaxBranchLength)) {
            _failedConnectPointList.Add(connectPoint);
            return;
        }

        // try to find a structure that won't result in a cost overrun
        bool foundStructure = false;
        CustomChainStructure newStructure = null;
        for (int attempts = 0; attempts < 20; attempts++) {
            bool closeToMaxBranchLen = connectPoint.BranchLength >= MaxBranchLength - 1;
            newStructure = PlaceNewStructure(connectPoint, closeToMaxBranchLen);
            if (newStructure is null) {
                _failedConnectPointList.Add(connectPoint);
                return;
            }

            if (_currentCost + newStructure.Cost <= MaxCost) {
                foundStructure = true;
                break;
            }
        }

        if (!foundStructure) {
            _failedConnectPointList.Add(connectPoint);
            return;
        }

        ChainConnectPoint targetConnectPoint = null;
        Bridge connectPointBridge = null;
        bool validLocation = false;

        for (int findLocationAttempts = 0; findLocationAttempts < 15; findLocationAttempts++) {
            // randomly pick either the top or bottom side
            int randomSide = 0; //Terraria.WorldGen.genRand.Next(0, 2);

            connectPointBridge = GetBridgeOfDirection(_bridges, targetDirection, newStructure);
            if (connectPointBridge is null) {
                _failedConnectPointList.Add(connectPoint);
                return;
            }
            <<<<<<<<

            Legacy / Structures / StructureChain.cs
            (int newStructureConnectPointX, int newStructureConnectPointY) position =
                SetProspectiveStructurePosition(newStructure, connectPoint, connectPointBridge);
                ========
            (int newStructureConnectPointX, int newStructureConnectPointY) position = SetProspectiveStructurePosition(newStructure, connectPoint, connectPointBridge);
                >>>>>>>>
            Types / StructureChain.cs
            int newStructureConnectPointX = position.newStructureConnectPointX;
            int newStructureConnectPointY = position.newStructureConnectPointY;

            byte secondConnectPointDirection;
            if (connectPointBridge.InputDirections[0] == targetDirection)
                secondConnectPointDirection = connectPointBridge.InputDirections[1];
            else
                secondConnectPointDirection = connectPointBridge.InputDirections[0];

            var targetConnectPointList = newStructure.ConnectPoints[secondConnectPointDirection];


            if (targetConnectPointList.Length != 0)
                targetConnectPoint = targetConnectPointList[(targetConnectPointList.Length - 1) * randomSide];
            else
                continue; // try another bridge

            MoveConnectPointAndStructure(newStructure, targetConnectPoint, newStructureConnectPointX,
                newStructureConnectPointY);
            connectPointBridge.SetPoints(connectPoint, targetConnectPoint);

            if (!IsConnectPointValid(connectPoint, targetConnectPoint, newStructure)) continue;

            if (!Box.IsAnyBoundingBoxesColliding(newStructure.StructureBoundingBoxes, _boundingBoxes) &&
                !Box.IsAnyBoundingBoxesColliding(connectPointBridge.BoundingBoxes, _boundingBoxes)) {
                //BoundingBox.VisualizeCollision();
                validLocation = true;
                break;
            }
        }

        if (!validLocation) {
            _failedConnectPointList.Add(connectPoint);
            return;
        }

        _boundingBoxes.AddRange(newStructure.StructureBoundingBoxes);
        _boundingBoxes.AddRange(connectPointBridge.BoundingBoxes);

        connectPoint.ChildStructure = newStructure;
        connectPoint.ChildStructure.BridgeDirectionHistory =
            CustomChainStructure.CloneBridgeDirectionHistory(connectPoint.ParentStructure);
        connectPoint.ChildStructure.BridgeDirectionHistory.Add(connectPoint.Direction);
        connectPoint.ChildConnectPoint = targetConnectPoint;
        connectPoint.ChildBridge = connectPointBridge;
        connectPoint.ChildBridge.Point1 = connectPoint;
        connectPoint.ChildBridge.Point2 = targetConnectPoint;

        _currentCost += newStructure.Cost;

        // reduce the weighting of the chosen structure so that we don't get 5 in a row
        _copiedStructureList.First(curStructure => curStructure.Id == newStructure.Id).Weight /= 2;
        CalculateWeights();

        for (byte direction = 0; direction < 4; direction++)
            foreach (ChainConnectPoint nextConnectPoint in newStructure.ConnectPoints[direction]) {
                // if the point already has the bridge on it
                if (connectPoint.ChildConnectPoint == nextConnectPoint)
                    continue;

                // """recursive"""
                nextConnectPoint.BranchLength = (byte)(currentBranchLength + 1);
                _queuedConnectPoints.Add(nextConnectPoint);
            }
    }

    /// <summary>
    ///     Called to physcially generate the structures of the chain. Changes status
    /// </summary>
    /// <returns>true if success, otherwise false</returns>
    public virtual bool Generate() {
        // if the initial setup didn't work, don't attempt to generate
        if (!SuccessfulGeneration)
            return false;

        List<Bridge> bridgeList = [];

        ActionOnEachStructure(structure => {
            structure.Generate();
            OnStructureGenerate(structure);
            structure.ActionOnEachConnectPoint(connectPoint => {
                if (connectPoint.ChildBridge is not null) {
                    Bridge bridge = connectPoint.ChildBridge;
                    bridgeList.Add(bridge);
                }
            });
        });
        RootStructure.Generate();
        OnStructureGenerate(RootStructure);

        foreach (Bridge bridge in bridgeList)
            bridge.Generate();

        foreach (ChainConnectPoint connectPoint in _failedConnectPointList)
            connectPoint.GenerateSeal();

        Status = StructureStatus.GeneratedButNotFound;

        return true;
    }

    /// <summary>
    ///     Should return true when the player is close to the structure, whatever that means for each structure
    /// </summary>
    /// <param name="playerPos"></param>
    /// <returns></returns>
    public virtual bool IsFound(Point16 playerPos) => true;

    /// <summary>
    ///     Recursively calls OnFound() on every structure in the chain
    /// </summary>
    public virtual void OnFound() {
        ActionOnEachStructure(structure => { structure.OnFound(); });
    }

    /// <summary>
    ///     Calls the function with each structure in the chain.
    ///     <para />
    ///     Sample Usage:
    ///     <para />
    ///     ActionOnEachStructure(structure =>
    ///     {
    ///     structure.Generate();
    ///     });
    /// </summary>
    /// <param name="function"></param>
    public void ActionOnEachStructure(Action<CustomChainStructure> function) {
        void Recursive(CustomChainStructure structure) {
            function(structure);
            structure.ActionOnEachConnectPoint(connectPoint => {
                if (connectPoint.ChildStructure is not null)
                    Recursive(connectPoint.ChildStructure);
            });
        }

        if (SuccessfulGeneration)
            Recursive(RootStructure);
    }

    /// <summary>
    ///     Calls the function with each connectPoint in the chain.
    ///     <para />
    ///     Sample Usage:
    ///     <para />
    ///     ActionOnEachConnectPoint(connectPoint =>
    ///     {
    ///     connectPoint.GenerateSeal();
    ///     });
    /// </summary>
    /// <param name="function"></param>
    public void ActionOnEachConnectPoint(Action<ChainConnectPoint> function) {
        void Recursive(CustomChainStructure structure) {
            structure.ActionOnEachConnectPoint(connectPoint => {
                function(connectPoint);

                if (connectPoint.ChildStructure is not null)
                    Recursive(connectPoint.ChildStructure);
            });
        }

        if (SuccessfulGeneration)
            Recursive(RootStructure);
    }

    /// <summary>
    ///     Called whenever the chain needs to have another structure. Will call this with 100 attempts. Return null to retry
    /// </summary>
    /// <param name="parentConnectPoint"></param>
    /// <param name="closeToMaxBranchLength"></param>
    /// <param name="structureWeightSum"></param>
    /// <param name="usableStructureList"></param>
    /// <returns>The new structure to be used for generation</returns>
    protected virtual CustomChainStructure GetNewStructure(ChainConnectPoint parentConnectPoint,
        bool closeToMaxBranchLength, int structureWeightSum, CustomChainStructure[] usableStructureList) {
        for (int i = 0; i < 50; i++) {
            <<<<<<<<
            Legacy / Structures / StructureChain.cs
            double randomValue = WorldGen.genRand.NextDouble() * structureWeightSum;
            CustomChainStructure structure =
                usableStructureList.Last(curStructure => curStructure.Weight <= randomValue).Clone();
                ========
            double randomValue = WorldGen.genRand.NextDouble() * structureWeightSum;
            CustomChainStructure structure = usableStructureList.Last(curStructure => curStructure.Weight <= randomValue).Clone();
                >>>>>>>>
            Types / StructureChain.cs

            // don't generate a branching hallway right after another one :)
            if (parentConnectPoint is not null && parentConnectPoint.GenerateChance == GenerateChances.Guaranteed &&
                StructureIdHelper.IsBranchingHallway(structure))
                continue;

            // don't generate a branching hallway if it means going over the max branch count
            if (closeToMaxBranchLength && StructureIdHelper.IsBranchingHallway(structure)) continue;

            return structure;
        }

        return null;
    }

    /// <summary>
    ///     Called when we need a new bridge. will call with 1 attempt
    /// </summary>
    /// <param name="bridges"></param>
    /// <param name="direction"></param>
    /// <param name="structure"></param>
    /// <returns>The bridge object to be used for the generation</returns>
    protected virtual Bridge GetBridgeOfDirection(Bridge[] bridges, byte direction, CustomChainStructure structure) {
        for (ushort i = 0; i < 5000; i++) {
            <<<<<<<<
            Legacy / Structures / StructureChain.cs
            int index = WorldGen.genRand.Next(0, bridges.Length);
                ========
            int index = WorldGen.genRand.Next(0, bridges.Length);
                >>>>>>>>
            Types / StructureChain.cs
            if (bridges[index].InputDirections[0] == direction)
                return bridges[index].Clone();
        }

        return null;
    }

    /// <summary>
    ///     Called after choosing bridge and structure. This method decides where the next structure goes (based on its cp)
    /// </summary>
    /// <param name="structure"></param>
    /// <param name="connectPoint"></param>
    /// <param name="bridge"></param>
    /// <returns></returns>
    protected virtual (int newStructureConnectPointX, int newStructureConnectPointY) SetProspectiveStructurePosition(
        CustomChainStructure structure, ChainConnectPoint connectPoint, Bridge bridge) {
        int deltaX, deltaY;
        if (bridge.DeltaXMultiple != 0) {
            int minRangeX = bridge.MinDeltaX / bridge.DeltaXMultiple;
            int maxRangeX = bridge.MaxDeltaX / bridge.DeltaXMultiple;
                <<<<<<<<
            Legacy / Structures / StructureChain.cs
            deltaX = WorldGen.genRand.Next(minRangeX, maxRangeX + 1) * bridge.DeltaXMultiple;
                ========
            deltaX = WorldGen.genRand.Next(minRangeX, maxRangeX + 1) * bridge.DeltaXMultiple;
                >>>>>>>>
            Types / StructureChain.cs
        }
        else {
            deltaX = 0;
        }

        if (bridge.DeltaYMultiple != 0) {
            int minRangeY = bridge.MinDeltaY / bridge.DeltaYMultiple;
            int maxRangeY = bridge.MaxDeltaY / bridge.DeltaYMultiple;
                <<<<<<<<
            Legacy / Structures / StructureChain.cs
            deltaY = WorldGen.genRand.Next(minRangeY, maxRangeY + 1) * bridge.DeltaYMultiple;
                ========
            deltaY = WorldGen.genRand.Next(minRangeY, maxRangeY + 1) * bridge.DeltaYMultiple;
                >>>>>>>>
            Types / StructureChain.cs
        }
        else {
            deltaY = 0;
        }

        // note: the +/-1 is because connect points have no space between them even when the deltaX is 1
        return (connectPoint.X + deltaX + 1, connectPoint.Y + deltaY + 1);
    }

    /// <summary>
    ///     Called at the very end of Chain generation when object is created
    /// </summary>
    /// <returns>Whether the chain is complete. If returns false, will retry generation</returns>
    protected virtual bool IsChainComplete() => true;

    /// <summary>
    ///     Called before attempting to generate a connect point's children
    /// </summary>
    /// <param name="connectPoint"></param>
    /// <param name="currentBranchLength"></param>
    /// <param name="minBranchLength"></param>
    /// <param name="maxBranchLength"></param>
    /// <returns>True if connect point is valid</returns>
    protected virtual bool ConnectPointAttrition(ChainConnectPoint connectPoint, byte currentBranchLength,
        byte minBranchLength, byte maxBranchLength) {
        if (connectPoint.GenerateChance != GenerateChances.Guaranteed)
            if ((WorldGen.genRand.Next(0, maxBranchLength - currentBranchLength) == 0 ||
                 currentBranchLength >= maxBranchLength) && currentBranchLength >= minBranchLength)
                return false;
        if (connectPoint.GenerateChance == GenerateChances.Rejected)
            return false;
        return true;
    }

    /// <summary>
    ///     Called after generatring a prospective childbridge and childstructure, but before generating ConnectPoint's
    ///     children
    /// </summary>
    /// <param name="connectPoint"></param>
    /// <param name="targetConnectPoint"></param>
    /// <param name="targetStructure"></param>
    /// <param name="rootStructure"></param>
    /// <returns>If true, children will generate</returns>
    protected virtual bool IsConnectPointValid(ChainConnectPoint connectPoint, ChainConnectPoint targetConnectPoint,
        CustomChainStructure targetStructure) =>
        true;

    /// <summary>
    ///     Called for each CustomChainStructure right after it gets generated.
    /// </summary>
    protected virtual void OnStructureGenerate(CustomChainStructure structure) {
    }
}