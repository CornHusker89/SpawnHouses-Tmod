#nullable enable

using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using SpawnHouses.Core.DataStructures;
using SpawnHouses.Core.Debug;
using SpawnHouses.Core.Enums;
using SpawnHouses.Core.Systems;
using SpawnHouses.Core.Tagging;
using SpawnHouses.Core.Tiles;
using SpawnHouses.Helpers;
using StructureHelper.API;
using StructureHelper.Models;
using Terraria;
using Terraria.DataStructures;
using WeightedStructure = (SpawnHouses.Core.RootStructureTypes.FileStructure structure, int entryPoint1Index, int entryPoint2Index, double weight);

namespace SpawnHouses.Core.RootStructureTypes;

/// <summary>
///     placeable instance of a structure. not to be created directly, structure manager handles the creation of these instances
/// </summary>
public sealed class FileStructure : StructureRoot {
    
    /// <summary>
    ///     <inheritdoc cref="IDebugDraw.InternalName" />. is formatted as follows for <see cref="FileStructure" />s:
    ///     {template name}@{position id}={substructure name}:{position id}={substructure name}...
    /// </summary>
    public override string InternalName { get; protected set; }

    public override TagMap TagsCurrent { get; protected set; }

    public FileStructureTemplate Template { get; private set; }

    /// <inheritdoc cref="StructureRoot.IsFound" />
    private Func<FileStructure, Point16, bool>? _isFound;

    /// <inheritdoc cref="StructureRoot.OnFound" />
    private Action<FileStructure>? _onFound;

    /// <summary>
    ///     called just after all files are loaded into the tilemap
    /// </summary>
    private Action<FileStructure>? _onTilemapLoaded;

    public bool Standalone { get; internal set; }

    public bool Depreciated { get; internal set; }

    /// <summary>any special data this specific structure needs to store</summary>
    public Dictionary<string, object> Data;

    /// <summary>map of the substructures with each positionID being associated with a filename</summary>
    public Dictionary<string, string> PositionIdToFilename;

    /// <summary>map of the substructures with each positionID being associated with a position</summary>
    public Dictionary<string, Point16> PositionIdToPosition;

    /// <summary>
    ///     same as <see cref="InternalName" /> but only contains the template name (everything before the @)
    /// </summary>
    public string TemplateName => InternalName[..InternalName.IndexOf('@')];
    public bool HasMultipleSubstructures => PositionIdToFilename.Keys.Count > 1;
    public Point16 Size => new(Tilemap.Width, Tilemap.Height);
    public Point16 Position => Tilemap.GlobalTileOffset;

    /// <summary>
    ///     gets a mapping of position ids to names with a given fullname
    /// </summary>
    /// <param name="name"></param>
    /// <returns></returns>
    public static Dictionary<string, string> GetPositionIdsToNames(string name) {
        var positionIdsToNames = new Dictionary<string, string>();
        string[] kvps = name[(name.IndexOf('@') + 1)..].Split(':');
        foreach (string kvp in kvps) {
            string[] keyAndValue = kvp.Split('=');
            positionIdsToNames.Add(keyAndValue[0], keyAndValue[1]);
        }

        return positionIdsToNames;
    }
    
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
        foreach (FileStructure variation in allVariations) {
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
    /// <param name="internalName"></param>
    /// <param name="size"></param>
    /// <param name="entryPoints"></param>
    /// <param name="tagMap"></param>
    /// <param name="template"></param>
    /// <param name="positionIdToFilename"></param>
    /// <param name="positionIdToPosition"></param>
    /// <exception cref="ArgumentException"></exception>
    internal FileStructure(string internalName, Point16 size, EntryPoint[] entryPoints, TagMap tagMap, FileStructureTemplate template,
        Dictionary<string, string> positionIdToFilename, Dictionary<string, Point16> positionIdToPosition) {
        if (entryPoints.Count(entryPoint => entryPoint.Purpose is EntryPointPurpose.GroundLevel) > 2)
            throw new ArgumentException("Cannot have more than 2 ground-level entry points");

        _isFound += template.IsFound;
        _onFound += template.OnFound;
        _onTilemapLoaded += template.OnTilemapLoaded;
        Id = 0;
        UserName = "_";
        Tilemap = new StructureTilemap(this, (ushort)size.X, (ushort)size.Y);
        EntryPoints = entryPoints;
        TagsCurrent = tagMap;
        InternalName = internalName;
        Template = template;

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
    ///     creates a new structure based with the internalName and specific substructures at each position IDs
    /// </summary>
    /// <param name="pos"></param>
    /// <param name="internalName">
    ///     template's class internalName or full variation internalName, depending on if <paramref internalName="targetPositionIdToName"/>
    ///     is passed. full variation names are created in this format:
    ///     {template internalName}@{position id}={substructure internalName}:{position id}={substructure internalName}...
    /// </param>
    /// <param name="targetPositionIdToName">if <paramref internalName="internalName"/> is the full variation internalName, leave null</param>
    /// <param name="userName">the user facing name for the structure</param>
    /// <param name="id">if -1, creates a new random seed from the normal terraria random generator</param>
    /// <param name="generate"></param>
#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider adding the 'required' modifier or declaring as nullable.
    public FileStructure(Point16 pos, string internalName, Dictionary<string, string>? targetPositionIdToName, string? userName = null, int id = -1, bool generate = false) {
        string templateName = internalName[..internalName.IndexOf('@')];
        targetPositionIdToName ??= GetPositionIdsToNames(internalName);
        FileStructure? referenceStructure = StructureManager.AllFileStructureVariations.FirstOrDefault(s => {
            if (templateName != s.TemplateName)
                return false;

            var positionIdToName = GetPositionIdsToNames(s.InternalName);

            if (targetPositionIdToName.Count != positionIdToName.Count)
                return false;

            foreach (var kvp in targetPositionIdToName)
                if (!positionIdToName.TryGetValue(kvp.Key, out string? value) || value != kvp.Value)
                    return false;

            return true;
        });
        
        
        if (referenceStructure == null)
            throw new ArgumentException($"No matching structure found for internalName '{internalName}' with the specified substructures");

        InitFromFileStructure(referenceStructure, id, userName);
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
    /// <param name="userName">the user facing name for the structure</param>
    /// <param name="tagsNotAllowed"></param>
    /// <param name="maxEntryPointDistDiff"></param>
    /// <param name="entryPointAnchor"></param>
    /// <param name="weightingStrength"></param>
    /// <param name="generate"></param>
#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider adding the 'required' modifier or declaring as nullable.
    public FileStructure(Point16 groundEntryTarget1, Point16 groundEntryTarget2, TagMap tagsRequired, string? userName = null, HashSet<Tag>? tagsNotAllowed = null, int maxEntryPointDistDiff = 5,
        double weightingStrength = 1.0, EntryPointPositionAnchor entryPointAnchor = EntryPointPositionAnchor.Neutral, bool generate = false) {
        var candidates = CreateCandidateList(groundEntryTarget1, groundEntryTarget2, new NumRange(0, maxEntryPointDistDiff),
            tagsRequired, tagsNotAllowed, weightingStrength);

        if (candidates.Count == 0)
            throw new ValidStructureNotFoundException($"no variations found with required tags and matching entry point spacing within {maxEntryPointDistDiff} tiles");

        (FileStructure selected, int selectedI, int selectedJ, double _) = PopWeightedStructureList(candidates);
        InitFromFileStructure(selected, -1, userName);

        var selectedGroundEntryPoints = selected.EntryPoints
            .Where(ep => ep.Purpose is EntryPointPurpose.GroundLevel)
            .ToArray();
        AlignEntryPoints(selectedGroundEntryPoints[selectedI], groundEntryTarget1, selectedGroundEntryPoints[selectedJ], groundEntryTarget2, entryPointAnchor);

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
    /// <param name="id">if -1, creates a new random seed from the normal terraria random generator</param>
    /// <param name="userName">the user facing name for the structure. leave null for random</param>
    private void InitFromFileStructure(FileStructure referenceFileStructure, int id = -1, string? userName = null) {
        _isFound += referenceFileStructure._isFound;
        _onFound += referenceFileStructure._onFound;
        _onTilemapLoaded += referenceFileStructure._onTilemapLoaded;
        Id = id == -1 ? StructureManager.NextGeneratableId() : (ushort)id;
        UserName = userName ?? GetStructureRandomName();
        Tilemap = new StructureTilemap(this, (ushort)referenceFileStructure.Tilemap.Width, (ushort)referenceFileStructure.Tilemap.Height);

        List<EntryPoint> entryPoints = [];
        foreach (EntryPoint entryPoint in referenceFileStructure.EntryPoints) entryPoints.Add(new EntryPoint(entryPoint.Start, entryPoint.Size, entryPoint.EntryDirection, entryPoint.Purpose));
        EntryPoints = entryPoints.ToArray();

        TagsCurrent = new TagMap();
        TagsCurrent.AddRange(referenceFileStructure.TagsCurrent);

        InternalName = referenceFileStructure.InternalName;

        Template = referenceFileStructure.Template;

        Data = [];
        PositionIdToFilename = [];
        foreach (var kvp in referenceFileStructure.PositionIdToFilename)
            PositionIdToFilename[kvp.Key] = kvp.Value;
        PositionIdToPosition = [];
        foreach (var kvp in referenceFileStructure.PositionIdToPosition)
            PositionIdToPosition[kvp.Key] = kvp.Value;
    }

    public override List<DebugLabel> DrawDebugGeometry() {
        // draw geometry
        if (DebugInfoVisibility.DisplayBounds) {
            DrawHelper.DrawWorldBasedRectangularPath(Tilemap.BoundingBox.GetDrawPath(), GetDrawColor(), DrawHelper.DebugDrawWidth);

            if (HasMultipleSubstructures) {
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
        }

        if (DebugInfoVisibility.DisplayPoints) {
            for (int i = 0; i < Tilemap.NbtData.Count; i++) {
                (StructureNBTEntry _, Point16 localPos) = Tilemap.NbtData[i];
                DrawHelper.DrawWorldBasedPoint(
                    Tilemap.ConvertToGlobal(localPos).ToPoint() * new Point(16, 16) + new Point(8, 8),
                    DrawHelper.GetColor((ushort)(Id + 1 + i)),
                    DrawHelper.DebugDrawWidth * 3);
            }
        }

        // create labels
        DebugLabel mainLabel = new(Tilemap.GlobalTileOffset, this);
        if (mainLabel.IsVisible(Tilemap.BoundingBox)) {
            List<DebugLabel> labels = [mainLabel];
            if (HasMultipleSubstructures) {
                foreach (string positionId in PositionIdToFilename.Keys) {
                    labels.Add(new DebugLabel(
                        PositionIdToPosition[positionId],
                        DebugInfoVisibility,
                        DrawHelper.GetColor((ushort)PositionIdToFilename[positionId].GetHashCode()),
                        positionId,
                        PositionIdToFilename[positionId].Replace("Assets/StructureFiles/", "")
                    ));
                }
            }

            for (int i = 0; i < Tilemap.NbtData.Count; i++) {
                (StructureNBTEntry nbt, Point16 localPos) = Tilemap.NbtData[i];
                labels.Add(new DebugLabel(
                    Tilemap.ConvertToGlobal(localPos),
                    DebugInfoVisibility,
                    DrawHelper.GetColor((ushort)(Id + 1 + i)),
                    TemplateName + $"_NBT_{i}",
                    nbt.GetType().Name
                ));
            }
            

            return labels;
        }

        return [];
    }

    public override bool IsFound(Point16 playerPos) => _isFound?.Invoke(this, playerPos) ?? true;
    public override void OnFound() => _onFound?.Invoke(this);

    public override void LoadTilemap() {
        if (Tilemap.IsAllTilesLoaded)
            return;
        
        foreach (var substructure in PositionIdToFilename) Tilemap.PlaceFile(PositionIdToPosition[substructure.Key], substructure.Value);

        _onTilemapLoaded?.Invoke(this);
        Tilemap.IsAllTilesLoaded = true;
    }

    public override void ApplyTilemap() => Tilemap.ApplyTilemap();

    /// <summary>
    ///     set the position of the structure in the world. this is the inclusive top-left corner of the structure
    /// </summary>
    /// <param name="position"></param>
    public override void SetPosition(Point16 position) {
        Tilemap.SetPosition(position);
        foreach (EntryPoint entryPoint in EntryPoints) entryPoint.SetOffset(position);
    }
}