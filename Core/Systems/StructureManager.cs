using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Microsoft.Xna.Framework;
using SpawnHouses.Common.Items.Debug;
using SpawnHouses.Core.AdvStructureCore;
using SpawnHouses.Core.Attributes;
using SpawnHouses.Core.DataStructures;
using SpawnHouses.Core.Debug;
using SpawnHouses.Core.Geometry;
using SpawnHouses.Core.RootStructureTypes;
using SpawnHouses.Core.Tagging;
using SpawnHouses.Helpers;
using Terraria;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.ModLoader.IO;

namespace SpawnHouses.Core.Systems;

#nullable enable

/// <summary>
///     provides, saves, and loads the global structure lists. calls debug drawing
/// </summary>
public class StructureManager : ModSystem {
    private static int _debugDrawFrameCount;

    private static Dictionary<DebugLabel, Point> _labels = [];

    private static List<FileStructure> _fileStructures = [];

    private static List<AdvStructure> _advStructures = [];

    /// <summary>
    ///     the version of the mod that generated this world. used for backwards compatibility
    /// </summary>
    public static Version WorldVersion = SpawnHousesMod.Instance.Version;

    /// <summary>
    ///     contains every possible variation as an individual object
    /// </summary>
    public static readonly List<FileStructure> AllFileStructureVariations = [];

    /// <summary>type corresponds to the final component's type</summary>
    public static readonly Dictionary<Type, List<IAdvGenerator>> AdvInstanceGenerators = new();
    
    /// <summary>
    ///     the number of generatable instances that have been made in this world. used to assign unique ids to components
    /// </summary>
    public static ushort GeneratableCount { get; private set; }

    public static readonly DebugInfoLevel DefaultDebugInfoLevel = new();

    /// <summary>
    ///     gets a shallow copy of the internal structure list. use <see cref="RegisterFileStructure" /> to add to the list
    /// </summary>
    /// <returns></returns>
    public static FileStructure[] GetFileStructureList() => _fileStructures.ToArray();

    /// <summary>
    ///     gets a shallow copy of the internal structure list. use <see cref="RegisterAdvStructure"/> to add to the list
    /// </summary>
    /// <returns></returns>
    public static AdvStructure[] GetAdvStructureList() => _advStructures.ToArray();

    /// <summary>
    ///     gets a shallow copy of all internal structure lists.
    /// </summary>
    public static StructureRoot[] GetAllStructuresList() => _advStructures.Concat(_fileStructures.Cast<StructureRoot>()).ToArray();

    /// <summary>
    ///     returns the next component id, and advances the counter. begins at id 1
    /// </summary>
    /// <returns></returns>
    public static ushort NextGeneratableId() {
        GeneratableCount++;
        return GeneratableCount;
    }

    /// <summary>
    ///     <see cref="FileStructure.LoadTilemap" /> must be called before this
    /// </summary>
    /// <param name="structure"></param>
    public static void RegisterFileStructure(FileStructure structure) {
        // if (!structure.Tilemap.IsAllTilesLoaded)
        //     throw new ArgumentException("structure must have an initialized tilemap");
        _fileStructures.Add(structure);
    }
    
    /// <summary>
    ///     <see cref="AdvStructure.ApplyLayoutMethod" /> and <see cref="AdvStructure.LoadTilemap" /> must be called before this
    /// </summary>
    /// <param name="structure"></param>
    public static void RegisterAdvStructure(AdvStructure structure) {
        if (structure.FailedLayoutGeneration)
            return;
        if (structure.StructureLayout == null) throw new ArgumentException("structure must have an initialized layout");
        if (!structure.Tilemap.IsAllTilesLoaded)
            throw new ArgumentException("structure must have an initialized tilemap");
        _advStructures.Add(structure);
    }

    /// <summary>
    ///     loads all types in the given assembly with <see cref="AdvGeneratorLoadable" /> and <see cref="StructureTemplateLoadable" /> attributes
    /// </summary>
    /// <param name="assembly"></param>
    public static void LoadStructureTypes(Assembly assembly) {
        var types = assembly.GetTypes();
        foreach (Type type in types) {
            // file structure loading 
            if (type.GetCustomAttribute<StructureTemplateLoadable>() is { } fileStructureInfo) {
                if (Activator.CreateInstance(type) is not FileStructureTemplate template) throw new Exception("the \"StructureTemplateLoadable\" attribute can only be applied to a class that inherits from \"FileStructureTemplate\"");

                string[] positionIds = template.PositionIds;
                var substructures = template.Substructures;

                // which substructures are legal at each position id (empty ValidPositionIds = wildcard)
                var optionsPerPosition = positionIds
                    .Select(id => substructures
                        .Where(s => s.ValidPositionIds.Length == 0 || s.ValidPositionIds.Contains(id))
                        .ToArray())
                    .ToArray();

                for (int i = 0; i < positionIds.Length; i++)
                    if (optionsPerPosition[i].Length == 0)
                        throw new InvalidOperationException($"{template.GetType().Name}: position id '{positionIds[i]}' is not assigned to any substructure");

                HashSet<string> seen = []; // guards against dupes when a slot gets skipped (-1,-1) regardless of which substructure was chosen for it

                foreach (var combo in CombinationHelper.CartesianProduct(optionsPerPosition)) {
                    var positionIdsToSubStructures = new Dictionary<string, FileSubstructureData>();
                    var namesToSubstructures = new Dictionary<string, FileSubstructureData>();
                    for (int i = 0; i < positionIds.Length; i++) {
                        positionIdsToSubStructures[positionIds[i]] = combo[i];
                        namesToSubstructures[combo[i].Name] = combo[i];
                    }

                    var structureInfo = template.GetStructureInfo(positionIdsToSubStructures);

                    // build a signature from what actually gets generated (post (-1,-1) filtering) to dedupe
                    string[] effectivePositionIds = positionIds
                        .Where(id => structureInfo.positionIdsToPositions.TryGetValue(id, out Point16 p) && p is not { X: -1, Y: -1 })
                        .ToArray();
                    string signature = string.Join(":", effectivePositionIds.Select(id => $"{id}={positionIdsToSubStructures[id].Name}"));

                    if (!seen.Add(signature)) continue;

                    // evaluate size
                    short maxRight = 0, maxBottom = 0;
                    foreach (string id in effectivePositionIds) {
                        Point16 pos = structureInfo.positionIdsToPositions[id];
                        if (pos.X < 0 || pos.Y < 0)
                            throw new Exception(
                                $"{template.GetType().Name}: position id '{id}' has a negative relative position {pos}. " +
                                "structure-relative positions must be normalized so the top-left of the structure is (0,0).");

                        FileSubstructureData sub = namesToSubstructures[positionIdsToSubStructures[id].Name];
                        short right = (short)(pos.X + sub.Size.X);
                        short bottom = (short)(pos.Y + sub.Size.Y);

                        if (right > maxRight) maxRight = right;
                        if (bottom > maxBottom) maxBottom = bottom;
                    }

                    Point16 size = new(maxRight, maxBottom);
                    var positionIdsToFilenames = new Dictionary<string, string>();
                    foreach (var kvp in positionIdsToSubStructures) positionIdsToFilenames[kvp.Key] = kvp.Value.FilePath;

                    FileStructure structure = new(
                        $"{template.GetType().Name}@{signature}",
                        size,
                        structureInfo.entryPoints,
                        structureInfo.tags,
                        template,
                        positionIdsToFilenames,
                        structureInfo.positionIdsToPositions);

                    foreach (string id in effectivePositionIds) {
                        structure.PositionIdToFilename[id] = positionIdsToSubStructures[id].FilePath;
                        structure.PositionIdToPosition[id] = structureInfo.positionIdsToPositions[id];
                    }

                    structure.Standalone = fileStructureInfo.Standalone;
                    structure.Depreciated = fileStructureInfo.Depreciated;
                    AllFileStructureVariations.Add(structure);
                    structure.LoadTilemap();
                }
            }
            
            // adv generator loading
            if (type.GetCustomAttribute<AdvGeneratorLoadable>() is { } advGeneratorInfo) {
                if (!AdvInstanceGenerators.TryGetValue(advGeneratorInfo.ModuleType, out var generatorList))
                    AdvInstanceGenerators[advGeneratorInfo.ModuleType] = generatorList = [];
                IAdvGenerator advGenerator = (IAdvGenerator)Activator.CreateInstance(type)!;
                advGenerator.Depreciated = advGeneratorInfo.Depreciated;
                generatorList.Add(advGenerator);
            }
        }

        // assign all file structures the upgradable tag if possible
        var groupedByTemplate = AllFileStructureVariations.GroupBy(s => s.TemplateName);
        foreach (var group in groupedByTemplate) {
            var variants = group.ToList();
            foreach (FileStructure s in variants) {
                var upgrades = s.Template.GetUpgrades(s);

                if (upgrades == null) {
                    var upgradeVariants = variants.Where(v => s.IsUpgrade(v)).Cast<StructureRoot>().ToArray();
                    if (upgradeVariants.Length != 0) s.TagsCurrent.Add(Tags.Structure_Upgradable, upgradeVariants);
                }
                else if (upgrades.Length == 0) {
                    continue;
                }
                else {
                    s.TagsCurrent.Add(Tags.Structure_Upgradable, upgrades);
                }
            }
        }
    }

    public override void PostSetupContent() {
        foreach (FileStructure structure in AllFileStructureVariations) structure.Tilemap.QueueRender();
    }

    public override void Load() {
        Tags.SetTagNameFields();
        LoadStructureTypes(Assembly.GetExecutingAssembly());
    }

    public override void SaveWorldData(TagCompound tag) {
        tag["WorldVersion"] = WorldVersion;
        tag["GeneratableCount"] = GeneratableCount;

        for (int i = 0; i < _fileStructures.Count; i++) tag["FileStructure" + i] = _fileStructures[i];

        for (int i = 0; i < _advStructures.Count; i++) tag["AdvStructure" + i] = _advStructures[i];
    }

    public override void LoadWorldData(TagCompound tag) {
        if (tag.ContainsKey("WorldVersion"))
            WorldVersion = new Version(tag.GetString("WorldVersion"));
        else
            WorldVersion = new Version("0.0.1");


        if (WorldVersion.Major == 0) {
            // whats a legacy support
        }
        else if (WorldVersion.Major == 1) {
            // never heard of it
        }
        else if (WorldVersion.Major == 2) {
            int i = 0;

            while (tag.ContainsKey("FileStructure" + i)) {
                RegisterFileStructure(tag.Get<FileStructure>("FileStructure" + i));
                i++;
            }
            
            i = 0;
            while (tag.ContainsKey("AdvStructure" + i)) {
                RegisterAdvStructure(tag.Get<AdvStructure>("AdvStructure" + i));
                i++;
            }
        }

        GeneratableCount = tag.ContainsKey("GeneratableCount") ? tag.Get<ushort>("GeneratableCount") : (ushort)0;
    }

    public override void ClearWorld() {
        WorldVersion = SpawnHousesMod.Instance.Version;
        
        GeneratableCount = 0;

        DebugWand.SelectedStructure = null;
    }

#if SPAWNHOUSES_DEBUG
    public override void PostDrawTiles() {
        DrawHelper.BeginWorldSpriteBatch();

        if (Main.netMode == NetmodeID.Server)
            return;

        _debugDrawFrameCount++;
        if (_debugDrawFrameCount < 20) {
            _debugDrawFrameCount = 0;
            UpdateLabelPositionsAndDraw();
        }
        else {
            foreach (StructureRoot structure in GetAllStructuresList())
                structure.DrawDebugGeometry();
        }

        foreach (DebugLabel label in _labels.Keys) {
            DrawHelper.DrawDebugLabel(label, _labels[label], DrawHelper.DebugDrawWidth, label.DrawColor);
        }

        Main.spriteBatch.End();
    }
#endif

    private static void UpdateLabelPositionsAndDraw() {
        List<Rectangle> structureRects = [];
        _labels.Clear();

        foreach (StructureRoot structure in GetAllStructuresList()) {
            TileBox structureGlobalTileBoundingBox;
            if (structure is FileStructure fileStructure)
                structureGlobalTileBoundingBox = structure.Tilemap.ConvertToGlobal(fileStructure.Tilemap.BoundingBox);
            else if (structure is AdvStructure advStructure)
                structureGlobalTileBoundingBox = structure.Tilemap.ConvertToGlobal(advStructure.StructureLayout.BoundingBox);
            else
                throw new Exception("unknown structure type");

            structureRects.Add(structureGlobalTileBoundingBox.Scale(16));
            
            foreach (DebugLabel label in structure.DrawDebugGeometry()) {
                _labels.Add(label, label.Root.ToPoint() * new Point(16, 16));
            }
        }

        // move each label away from each other and the structure
        for (int i = 0; i < 7; i++) {
            foreach (var a in _labels) {
                // gentle spring back to root
                Vector2 deltaToRoot = (a.Value - a.Key.Root.ToPoint() * new Point(16, 16)).ToVector2();
                _labels[a.Key] -= (deltaToRoot * 0.03f).ToPoint();

                // label-structure
                foreach (Rectangle box in structureRects) {
                    Rectangle aRect = a.Key.GetTextBoundingBox(a.Value);
                    if (aRect.Intersects(box)) {
                        Vector2 delta = (aRect.Center - box.Center).ToVector2();

                        if (delta == Vector2.Zero)
                            delta = Vector2.UnitY;

                        _labels[a.Key] += (Vector2.Normalize(delta) * 7).ToPoint();
                    }
                }

                // label-label
                foreach (var b in _labels) {
                    Rectangle aRect = a.Key.GetTextBoundingBox(a.Value);
                    if (ReferenceEquals(a.Key, b.Key))
                        continue;
                    if (aRect.Intersects(a.Key.GetTextBoundingBox(b.Value))) {
                        Vector2 delta = (a.Value - b.Value).ToVector2() * 0.65f;

                        if (delta == Vector2.Zero)
                            delta = Vector2.UnitY;

                        _labels[a.Key] += delta.ToPoint();
                        _labels[b.Key] -= delta.ToPoint();
                    }
                }
            }
        }
    }
}

internal class DictionarySerializer : TagSerializer<Dictionary<string, object>, TagCompound> {
    public override TagCompound Serialize(Dictionary<string, object> data) {
        TagCompound tag = [];
        foreach (var kvp in data)
            tag[kvp.Key] = kvp.Value;
        return tag;
    }

    public override Dictionary<string, object> Deserialize(TagCompound tag) => tag.ToDictionary();
}

internal class TagMapSerializer : TagSerializer<TagMap, TagCompound> {
    public override TagCompound Serialize(TagMap tagMap) {
        TagCompound tag = [];
        foreach (var kvp in tagMap.GetEntries()) {
            if (kvp.Key.Name == null) continue;
            tag[kvp.Key.Name] = kvp.Value;
        }

        return tag;
    }

    public override TagMap Deserialize(TagCompound tagCompound) {
        TagMap map = new();
        foreach (var kvp in tagCompound) {
            object? value = tagCompound.Get<object>(kvp.Key);
            try {
                var tag = (Tag<object>)typeof(Tags).GetField(kvp.Key)!.GetValue(null)!;
                map.Add(tag, value);
            }
            catch {
                SpawnHousesMod.Instance.Logger.Error($"Tag \"{kvp.Key}\" in saved structure data, but not found in tag list");
            }
        }

        return map;
    }
}

internal class FileStructureSerializer : TagSerializer<FileStructure, TagCompound> {
    public override TagCompound Serialize(FileStructure structure) {
        TagCompound tag = [];
        tag["InternalName"] = structure.InternalName;
        tag["Id"] = structure.Id;
        tag["UserName"] = structure.UserName;
        tag["Position"] = structure.Position;
        tag["Found"] = structure.HasBeenFound;
        tag["Data"] = structure.Data;
        return tag;
    }

    public override FileStructure Deserialize(TagCompound tag) {
        FileStructure structure = new(
            tag.Get<Point16>("Position"),
            tag.Get<string>("InternalName"),
            null,
            tag.Get<string>("UserName"),
            tag.Get<ushort>("Id"));
        structure.HasBeenFound = tag.GetBool("Found");
        structure.Data = tag.Get<Dictionary<string, object>>("Data");
        return structure;
    }
}