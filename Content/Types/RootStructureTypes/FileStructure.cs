#nullable enable

using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using SpawnHouses.Content.Debug;
using SpawnHouses.Content.Tagging;
using SpawnHouses.Content.Tiles;
using SpawnHouses.Content.Types.DataStructures;
using SpawnHouses.Content.Types.Enums;
using SpawnHouses.Content.Types.Interfaces;
using SpawnHouses.Helpers;
using StructureHelper.API;
using Terraria;
using Terraria.DataStructures;
using WeightedStructure = (SpawnHouses.Content.Types.RootStructureTypes.FileStructure structure, int entryPoint1Index, int entryPoint2Index, double weight);

namespace SpawnHouses.Content.Types.RootStructureTypes;

/// <summary>
///     placeable instance of a structure. not to be created directly, structure manager handles the creation of these instances
/// </summary>
public sealed class FileStructure : IGeneratable, IStructureRoot, IStructureTags {
    // IGeneratable
    public ushort Id { get; private set; }

    // IStructureRoot
    public DebugInfoLevel DebugInfoVisibility { get; set; }

    /// <inheritdoc cref="IDebugDraw.Name" />
    /// . is formatted as follows for
    /// <see cref="FileStructure" />
    /// s:
    /// {template name}_{position id}={substructure name}
    public string Name { get; private set; }
    public StructureTilemap Tilemap { get; private set; }
    public EntryPoint[] EntryPoints { get; private set; }
    public bool HasBeenFound { get; set; }

    // IStructureTags
    public TagMap TagsCurrent { get; private set; }

    /// <inheritdoc cref="IStructureRoot.IsFound" />
    private Func<FileStructure, Point16, bool>? _isFound;

    /// <inheritdoc cref="IStructureRoot.OnFound" />
    private Action<FileStructure>? _onFound;

    /// <inheritdoc cref="OnTilemapLoaded" />
    private Action<FileStructure>? _onTilemapLoaded;

    public bool Standalone { get; internal set; }

    public bool Depreciated { get; internal set; }

    /// <summary>any special data this specific structure needs to store</summary>
    public Dictionary<string, object> Data;

    /// <summary>map of the substructures with each positionID being associated with a filename</summary>
    public Dictionary<string, string> PositionIdToFilename;

    /// <summary>map of the substructures with each positionID being associated with a position</summary>
    public Dictionary<string, Point16> PositionIdToPosition;

    public bool HasMultipleSubstructures => PositionIdToFilename.Keys.Count > 1;
    public Point16 Size => new(Tilemap.Width, Tilemap.Height);
    public Point16 Position => Tilemap.GlobalTileOffset;

    /// <summary>
    ///     gets a weighted list of all <see cref="FileStructure" />s that fulfill the parameters, weighted on how close the match is.
    /// </summary>
    /// <param name="groundEntryTarget1"></param>
    /// <param name="groundEntryTarget2"></param>
    /// <param name="entryPointDistDiff"></param>
    /// <param name="tagsRequired"></param>
    /// <param name="tagsNotAllowed"></param>
    /// <param name="weightingStrength">higher means it will be more likely to pick a structure that fits closer</param>
    /// <returns></returns>
    public static List<WeightedStructure> CreateCandidateList(Point16 groundEntryTarget1, Point16 groundEntryTarget2, NumRange entryPointDistDiff, TagMap tagsRequired, HashSet<Tag>? tagsNotAllowed = null, double weightingStrength = 1.0) {
        List<WeightedStructure> candidates = [];

        // calculate the distance between the two provided entry points
        double providedDistance = Vector2.Distance(groundEntryTarget1.ToVector2(), groundEntryTarget2.ToVector2());
        var allVariations = StructureManager.AllFileStructureVariations.Where(s => s is { Depreciated: false, Standalone: true });
        foreach (FileStructure variation in StructureManager.AllFileStructureVariations) {
            var groundEntryPoints = variation.EntryPoints
                .Where(ep => ep.Purpose is EntryPointPurpose.GroundLevel)
                .ToArray();
            if (groundEntryPoints.Length < 2)
                continue;
            if (tagsRequired.KeysSet.IsSubsetOf(variation.TagsCurrent.Keys))
                continue;
            if (tagsNotAllowed != null && variation.TagsCurrent.Keys.Any(tagsNotAllowed.Contains))
                continue;

            // find the best matching pair of entry points based on relative distance
            double bestDistanceDifference = double.MaxValue;
            int bestI = 0, bestJ = 1;
            for (int i = 0; i < groundEntryPoints.Length; i++)
            for (int j = i + 1; j < groundEntryPoints.Length; j++) {
                double variationDistance = Vector2.Distance(
                    groundEntryPoints[i].Center.ToVector2(),
                    groundEntryPoints[j].Center.ToVector2());

                double distanceDifference = Math.Abs(providedDistance - variationDistance);
                if (distanceDifference < bestDistanceDifference) {
                    bestDistanceDifference = distanceDifference;
                    bestI = i;
                    bestJ = j;
                }
            }

            if (bestDistanceDifference >= entryPointDistDiff.Max || bestDistanceDifference < entryPointDistDiff.Min)
                continue;
            double baseWeight = 1.0 / (bestDistanceDifference + 1.0);
            double weight = Math.Pow(baseWeight, weightingStrength);
            candidates.Add((variation, bestI, bestJ, weight));
        }

        return candidates;
    }

    /// <summary>
    ///     gets and removes a weighted random structure from the list
    /// </summary>
    /// <param name="candidates"></param>
    /// <returns></returns>
    public static WeightedStructure PopWeightedStructureList(List<WeightedStructure> candidates) {
        if (candidates.Count == 0) throw new ArgumentException("list is empty");

        double totalWeight = candidates.Sum(c => c.weight);
        double selection = WorldGen._genRand.NextDouble() * totalWeight;
        double accumulated = 0;
        double selectedWeight = 0;

        FileStructure selected = candidates[0].structure;
        int selectedI = candidates[0].entryPoint1Index;
        int selectedJ = candidates[0].entryPoint2Index;

        for (int i = 0; i < candidates.Count; i++) {
            (FileStructure structure, int entryI, int entryJ, double weight) = candidates[i];
            accumulated += weight;
            if (selection <= accumulated) {
                selected = structure;
                selectedI = entryI;
                selectedJ = entryJ;
                selectedWeight = weight;
                candidates.RemoveAt(i);
                break;
            }
        }

        return (selected, selectedI, selectedJ, selectedWeight);
    }

    /// <summary>
    ///     internal-only constructor for making a file structure entirely from raw parameters. used for loading structures and variants
    /// </summary>
    /// <param name="name"></param>
    /// <param name="size"></param>
    /// <param name="entryPoints"></param>
    /// <param name="tagMap"></param>
    /// <param name="positionIdToFilename"></param>
    /// <param name="positionIdToPosition"></param>
    /// <param name="isFound"></param>
    /// <param name="onFound"></param>
    /// <param name="onTilemapLoaded"></param>
    /// <exception cref="ArgumentException"></exception>
    internal FileStructure(string name, Point16 size, EntryPoint[] entryPoints, TagMap tagMap,
        Dictionary<string, string> positionIdToFilename, Dictionary<string, Point16> positionIdToPosition,
        Func<FileStructure, Point16, bool>? isFound = null, Action<FileStructure>? onFound = null, Action<FileStructure>? onTilemapLoaded = null) {
        if (entryPoints.Count(entryPoint => entryPoint.Purpose is EntryPointPurpose.GroundLevel) > 2)
            throw new ArgumentException("Cannot have more than 2 ground-level entry points");

        _isFound += isFound;
        _onFound += onFound;
        _onTilemapLoaded += onTilemapLoaded;
        DebugInfoVisibility = StructureManager.DefaultDebugInfoLevel.Clone();
        Id = StructureManager.NextGeneratableId();
        Tilemap = new StructureTilemap(this, (ushort)size.X, (ushort)size.Y);
        EntryPoints = entryPoints;
        TagsCurrent = tagMap;
        Name = name;

        Data = [];
        PositionIdToFilename = positionIdToFilename;
        PositionIdToPosition = positionIdToPosition;
    }

    /// <summary>
    ///     creates a deepcopy from a reference <see cref="FileStructure" />
    /// </summary>
    /// <param name="referenceStructure"></param>
#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider adding the 'required' modifier or declaring as nullable.
    internal FileStructure(FileStructure referenceStructure) {
        InitFromFileStructure(referenceStructure);
    }
#pragma warning restore CS8618

    /// <summary>
    ///     creates a new structure based with the name and specific substructures at each position IDs
    /// </summary>
    /// <param name="pos"></param>
    /// <param name="name">template's class name or full variation name, depending on <paramref name="fullname"/></param>
    /// <param name="positionIdToName">if <paramref name="fullname"/> is true, leave null</param>
    /// <param name="fullname">
    ///     if true, expects <paramref name="name"/> to be full structure variation name.
    ///     otherwise <paramref name="name"/> should just be the template's class name
    /// </param>
    /// <param name="generate"></param>
#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider adding the 'required' modifier or declaring as nullable.
    public FileStructure(Point16 pos, string name, Dictionary<string, string> positionIdToName, bool fullname = false, bool generate = false) {
        FileStructure? referenceStructure;
        if (fullname) {
            referenceStructure = StructureManager.AllFileStructureVariations.FirstOrDefault(s => s.Name == name);
        }
        else {
            string searchString = name + "_";
            foreach (var kvp in positionIdToName) searchString += kvp.Key + "=" + kvp.Value;
            referenceStructure = StructureManager.AllFileStructureVariations.FirstOrDefault(s => s.Name == searchString);
        }
        
        if (referenceStructure == null)
            throw new ArgumentException($"No matching structure found for name '{name}' with the specified substructures");

        InitFromFileStructure(referenceStructure);
        SetPosition(pos);

        if (generate) {
            LoadTilemap();
            StructureManager.RegisterFileStructure(this);
            ApplyTilemap();
        }
    }
#pragma warning restore CS8618

    /// <summary>
    ///     creates a new structure based on the given parameters and entry locations
    /// </summary>
    /// <param name="groundEntryTarget1"></param>
    /// <param name="groundEntryTarget2">must be ground-level</param>
    /// <param name="tagsRequired"></param>
    /// <param name="tagsNotAllowed"></param>
    /// <param name="maxEntryPointDistDiff"></param>
    /// <param name="entryPointAnchor"></param>
    /// <param name="weightingStrength"></param>
    /// <param name="generate"></param>
#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider adding the 'required' modifier or declaring as nullable.
    public FileStructure(Point16 groundEntryTarget1, Point16 groundEntryTarget2, TagMap tagsRequired, HashSet<Tag>? tagsNotAllowed = null, int maxEntryPointDistDiff = 5,
        double weightingStrength = 1.0, EntryPointPositionAnchor entryPointAnchor = EntryPointPositionAnchor.Neutral, bool generate = false) {
        var candidates = CreateCandidateList(groundEntryTarget1, groundEntryTarget2, new NumRange(0, maxEntryPointDistDiff),
            tagsRequired, tagsNotAllowed, weightingStrength);

        if (candidates.Count == 0)
            throw new ValidStructureNotFoundException($"no variations found with required tags and matching entry point spacing within {maxEntryPointDistDiff} tiles");

        (FileStructure selected, int selectedI, int selectedJ, double _) = PopWeightedStructureList(candidates);
        InitFromFileStructure(selected);

        var selectedGroundEntryPoints = selected.EntryPoints
            .Where(ep => ep.Purpose is EntryPointPurpose.GroundLevel)
            .ToArray();
        ((IStructureRoot)this).AlignEntryPoints(selectedGroundEntryPoints[selectedI], groundEntryTarget1, selectedGroundEntryPoints[selectedJ], groundEntryTarget2, entryPointAnchor);

        if (generate) {
            LoadTilemap();
            StructureManager.RegisterFileStructure(this);
            ApplyTilemap();
        }
    }
#pragma warning restore CS8618

    /// <summary>
    ///     fills fields with deepcopies from the reference <see cref="FileStructure" />
    /// </summary>
    /// <param name="referenceFileStructure"></param>
    private void InitFromFileStructure(FileStructure referenceFileStructure) {
        _isFound += referenceFileStructure._isFound;
        _onFound += referenceFileStructure._onFound;
        _onTilemapLoaded += referenceFileStructure._onTilemapLoaded;
        DebugInfoVisibility = StructureManager.DefaultDebugInfoLevel.Clone();
        Id = StructureManager.NextGeneratableId();
        Tilemap = new StructureTilemap(this, (ushort)referenceFileStructure.Tilemap.Width, (ushort)referenceFileStructure.Tilemap.Height);

        List<EntryPoint> entryPoints = [];
        foreach (EntryPoint entryPoint in referenceFileStructure.EntryPoints) entryPoints.Add(new EntryPoint(entryPoint.Start, entryPoint.Size, entryPoint.EntryDirection, entryPoint.Purpose));
        EntryPoints = entryPoints.ToArray();

        TagsCurrent = new TagMap();
        TagsCurrent.AddRange(referenceFileStructure.TagsCurrent);

        Name = referenceFileStructure.Name;

        Data = [];
        PositionIdToFilename = [];
        foreach (var kvp in referenceFileStructure.PositionIdToFilename)
            PositionIdToFilename[kvp.Key] = kvp.Value;
        PositionIdToPosition = [];
        foreach (var kvp in referenceFileStructure.PositionIdToPosition)
            PositionIdToPosition[kvp.Key] = kvp.Value;
    }

    public Color GetDrawColor() => DrawHelper.GetColor(Id);

    public List<DebugLabel> DrawDebugGeometry() {
        // draw geometry
        if (DebugInfoVisibility.DisplayBounds) {
            DrawHelper.DrawWorldBasedRectangularPath(Tilemap.BoundingBox.GetDrawPath(), GetDrawColor(), DrawHelper.DebugDrawWidth);

            if (HasMultipleSubstructures)
                foreach (string positionId in PositionIdToFilename.Keys) {
                    Point worldPos = PositionIdToPosition[positionId].ToPoint() * new Point(16, 16);
                    Point worldSize = Generator.GetStructureDimensions(PositionIdToFilename[positionId], SpawnHousesMod.Instance).ToPoint();
                    Point[] path = [
                        worldPos,
                        worldPos + new Point(worldSize.X, 0),
                        worldPos + new Point(worldSize.X, worldSize.Y),
                        worldPos + new Point(0, worldSize.Y)
                    ];
                    DrawHelper.DrawWorldBasedRectangularPath(path, DrawHelper.GetColor((ushort)PositionIdToFilename[positionId].GetHashCode()), DrawHelper.DebugDrawWidth);
                }
        }

        // create labels
        DebugLabel mainLabel = new(Tilemap.GlobalTileOffset, this);
        if (mainLabel.IsVisible(Tilemap.BoundingBox)) {
            List<DebugLabel> labels = [mainLabel];
            if (HasMultipleSubstructures)
                foreach (string positionId in PositionIdToFilename.Keys)
                    labels.Add(new DebugLabel(
                        PositionIdToPosition[positionId],
                        DebugInfoVisibility,
                        DrawHelper.GetColor((ushort)PositionIdToFilename[positionId].GetHashCode()),
                        positionId,
                        PositionIdToFilename[positionId].Replace("Content/Assets/StructureFiles/", "")
                    ));

            return labels;
        }

        return [];
    }

    public bool IsFound(Point16 playerPos) => _isFound?.Invoke(this, playerPos) ?? true;
    public void OnFound() => _onFound?.Invoke(this);

    public void LoadTilemap() {
        foreach (var substructure in PositionIdToFilename) Tilemap.PlaceFile(PositionIdToPosition[substructure.Key], substructure.Value);

        _onTilemapLoaded?.Invoke(this);
        Tilemap.IsAllTilesLoaded = true;
    }

    public void ApplyTilemap() {
        Tilemap.ApplyTilemap();
    }

    /// <summary>
    ///     set the position of the structure in the world. this is the inclusive top-left corner of the structure
    /// </summary>
    /// <param name="position"></param>
    public void SetPosition(Point16 position) {
        Tilemap.SetPosition(position);
        foreach (EntryPoint entryPoint in EntryPoints) entryPoint.SetOffset(position);
    }

    /// <summary>called just after the structure files are loaded into the tilemap</summary>
    public void OnTilemapLoaded() => _onTilemapLoaded?.Invoke(this);
}