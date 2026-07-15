using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using SpawnHouses.Common;
using SpawnHouses.Common.DataStructures;
using SpawnHouses.Common.Debug;
using SpawnHouses.Common.Modules;
using SpawnHouses.Common.Tagging;
using SpawnHouses.Common.Tiles;
using SpawnHouses.Common.Types;
using SpawnHouses.Helpers;
using SpawnHouses.Items.Debug;
using SpawnHouses.Legacy.Helpers;
using SpawnHouses.Legacy.Structures;
using SpawnHouses.Legacy.Structures.ChainTypes;
using SpawnHouses.Legacy.Structures.StructureTypes;
using Terraria;
using Terraria.DataStructures;
using Terraria.ModLoader;
using Terraria.ModLoader.IO;

namespace SpawnHouses;

#nullable enable

/// <summary>
///     provides, saves, and loads the global structure lists. calls debug drawing
/// </summary>
internal class StructureManager : ModSystem {
    private static int _debugDrawFrameCount;

    private static Dictionary<DebugLabel, Point> _labels = [];

    private static readonly List<AdvStructure> AdvStructures = [];

    /// <summary>
    ///     the version of the mod that generated this world. used for backwards compatibility
    /// </summary>
    public static Version WorldVersion = SpawnHousesMod.Instance.Version;

    public static readonly List<LegacyStructure> LegacyStructures = [];

    public static readonly List<LegacyStructureChain> LegacyStructureChains = [];

    /// <summary>
    ///     the number of generatable components (exclusive to AdvStructures) that have been generated in this world. used to assign unique ids to components
    /// </summary>
    public static ushort GeneratableCount { get; private set; }

    public static readonly DebugInfoLevel DefaultDebugInfoLevel = new();


    /// <summary>
    ///     shallow copies then exposes the internal structure list
    /// </summary>
    /// <returns></returns>
    public static AdvStructure[] GetStructureList() => AdvStructures.ToArray();

    /// <summary>
    ///     returns the next component id, and advances the counter. begins at id 1
    /// </summary>
    /// <returns></returns>
    public static ushort NextGeneratableId() {
        GeneratableCount++;
        return GeneratableCount;
    }

    /// <summary>
    ///     <see cref="AdvStructure.ApplyLayoutMethod" /> and <see cref="AdvStructure.FillComponents" /> must be called before this
    /// </summary>
    /// <param name="structure"></param>
    public static void RegisterStructure(AdvStructure structure) {
        if (structure.FailedLayoutGeneration)
            return;
        if (structure.StructureLayout == null) throw new ArgumentException("structure must have an initialized layout");
        AdvStructures.Add(structure);
    }

    public override void Load() {
        Tags.SetInternalTagNames();
        GlobalGeneratorUtils.LoadGenerators();
    }

    public override void SaveWorldData(TagCompound tag) {
        tag["WorldVersion"] = WorldVersion;
        tag["GeneratableCount"] = GeneratableCount;
    }

    public override void LoadWorldData(TagCompound tag) {
        LegacyStructures.Clear();
        LegacyStructureChains.Clear();

        if (tag.ContainsKey("WorldVersion"))
            WorldVersion = new Version(tag.GetString("WorldVersion"));
        else if (tag.ContainsKey("WorldModVersion")) // "WorldVersion" is the old name
            WorldVersion = new Version(tag.GetString("WorldModVersion"));
        else
            WorldVersion = new Version("0.3.2");


        if (WorldVersion.Major == 0) {
            // the rest are unrecoverable. mainhouse might use just 1 structure file, basement uses seeds, mineshaft doesn't exist
            LegacyStructures.Add(tag.Get<BeachHouse>("BeachHouse"));
        }
        else if (WorldVersion.Major == 1) {
            if (tag.ContainsKey("MainHouse")) LegacyStructures.Add(tag.Get<MainHouse>("MainHouse"));
            if (tag.ContainsKey("Mineshaft")) LegacyStructures.Add(tag.Get<Mineshaft>("Mineshaft"));
            if (tag.ContainsKey("Mineshaft")) LegacyStructures.Add(tag.Get<Mineshaft>("Mineshaft"));
            if (tag.ContainsKey("MainBasement")) {
                MainBasement basement = tag.Get<MainBasement>("MainBasement");
                LegacyStructureChains.Add(basement);
            }
        }
        else if (WorldVersion.Major == 2) {
            int i = 0;
            while (tag.ContainsKey("AdvStructure" + i))
                //AdvStructures.Add(tag.Get<AdvStructure>("AdvStructure" + i));
                i++;

            i = 0;
            while (tag.ContainsKey("LegacyStructure" + i)) {
                LegacyStructures.Add(tag.Get<LegacyStructure>("LegacyStructure" + i));
                i++;
            }

            i = 0;
            while (tag.ContainsKey("LegacyStructureChain" + i)) {
                LegacyStructureChains.Add(tag.Get<LegacyStructureChain>("LegacyStructureChain" + i));
                i++;
            }
        }

        GeneratableCount = tag.ContainsKey("GeneratableCount") ? tag.Get<ushort>("GeneratableCount") : (ushort)0;
    }

    public override void ClearWorld() {
        WorldVersion = SpawnHousesMod.Instance.Version;

        LegacyStructures.Clear();
        LegacyStructureChains.Clear();

        GeneratableCount = 0;

        DebugWand.SelectedStructure = null;
    }

    public override void PostDrawTiles() {
        _debugDrawFrameCount++;

        DrawHelper.BeginWorldSpriteBatch();

        if (_debugDrawFrameCount < 20) {
            _debugDrawFrameCount = 0;
            UpdateLabelPositionsAndDraw();
        }
        else {
            foreach (AdvStructure structure in AdvStructures)
                structure.DrawDebugGeometry();
        }

        foreach (DebugLabel label in _labels.Keys) {
            Color color;
            if (label.ParentObj is IComponent component)
                color = DrawHelper.GetColor(component);
            else if (label.ParentObj is IGeneratable generatable)
                color = DrawHelper.GetColor(generatable.Id);
            else if (label.ParentObj is StructureTilemap tilemap)
                color = DrawHelper.GetColor(tilemap.Structure.StructureLayout.Id);
            else
                color = DrawHelper.GetColor((ushort)label.ParentObj.GetHashCode());
            DrawHelper.DrawDebugLabel(label, _labels[label], DrawHelper.DebugDrawWidth, color);
        }

        Main.spriteBatch.End();
    }

    private static void UpdateLabelPositionsAndDraw() {
        List<Rectangle> structureRects = [];
        _labels.Clear();

        foreach (AdvStructure structure in AdvStructures)
        foreach (DebugLabel label in structure.DrawDebugGeometry()) {
            _labels.Add(label, label.Root.ToPoint() * new Point(16, 16));
            TileBox structureGlobalTileBoundingBox = structure.Tilemap.ConvertToGlobal(structure.StructureLayout.BoundingBox);
            structureRects.Add(structureGlobalTileBoundingBox.Scale(16));
        }

        // move each label away from each other and the structure
        for (int i = 0; i < 7; i++)
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

internal class Point16Serializer : TagSerializer<Point16, TagCompound> {
    public override TagCompound Serialize(Point16 data) {
        TagCompound tag = [];
        tag["X"] = data.X;
        tag["Y"] = data.Y;
        return tag;
    }

    public override Point16 Deserialize(TagCompound tag) => new(tag.Get<short>("X"), tag.Get<short>("Y"));
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

internal static class ChainProcessor {
    internal static Dictionary<string, object> SerializeChain(LegacyChainStructure processingStructure) {
        var dict = new Dictionary<string, object> {
            ["ID"] = (ushort)processingStructure.Id,
            ["Pos"] = processingStructure.BoundingBox.TopLeftPoint16,
            ["Status"] = processingStructure.Status
        };

        int i = 0;
        processingStructure.ActionOnEachChainConnectPoint(connectPoint => {
            if (connectPoint.ChildStructure is not null) {
                dict[$"Substructure{i}"] = SerializeChain(connectPoint.ChildStructure);
                dict[$"Substructure{i}Bridge"] = new Dictionary<string, object> {
                    ["ID"] = (ushort)connectPoint.ChildBridge.Id,
                    ["X1"] = connectPoint.ChildBridge.Point1.X,
                    ["Y1"] = connectPoint.ChildBridge.Point1.Y,
                    ["X2"] = connectPoint.ChildBridge.Point2.X,
                    ["Y2"] = connectPoint.ChildBridge.Point2.Y
                };
            }

            i++;
        });

        return dict;
    }

    internal static LegacyChainStructure DeserializeChain(LegacyStructureChain legacyStructureChain, TagCompound structureDict) {
        LegacyChainStructure? structure = (LegacyChainStructure)StructureIdHelper.CreateStructure(
            (ushort)(short)structureDict["ID"],
            (ushort)(short)structureDict["X"],
            (ushort)(short)structureDict["Y"],
            (byte)structureDict["Status"]
        );

        int i = 0;
        structure.ParentLegacyStructureChain = legacyStructureChain;
        structure.ActionOnEachChainConnectPoint(point => {
            if (structureDict.ContainsKey($"Substructure{i}")) {
                point.ChildStructure = DeserializeChain(legacyStructureChain, (TagCompound)structureDict[$"Substructure{i}"]);
                if (structureDict.ContainsKey($"Substructure{i}Bridge")) {
                    TagCompound? bridgeDict = (TagCompound)structureDict[$"Substructure{i}Bridge"];
                    Bridge? bridge = BridgeIdHelper.CreateBridge((ushort)(short)bridgeDict["ID"]);

                    // get the child connect point
                    ushort goalX = (ushort)(short)bridgeDict["X2"];
                    ushort goalY = (ushort)(short)bridgeDict["Y2"];
                    bool found = false;
                    point.ChildStructure.ActionOnEachChainConnectPoint(nextPoint => {
                        if (nextPoint.X == goalX && nextPoint.Y == goalY) {
                            found = true;
                            point.ChildConnectPoint = nextPoint;
                            bridge.SetPoints(point, nextPoint);
                        }
                    });
                    if (!found) throw new Exception("Bridge loading failed");

                    point.ChildBridge = bridge;
                }
            }

            i++;
        });

        return structure;
    }
}

internal class MainHouseSerializer : TagSerializer<MainHouse, TagCompound> {
    public override TagCompound Serialize(MainHouse structure) =>
        new() {
            ["Pos"] = structure.BoundingBox.TopLeftPoint16,
            ["Status"] = structure.Status,
            ["HasBasement"] = structure.HasBasement,
            ["InUnderworld"] = structure.InUnderworld,
            ["LeftType"] = structure.LeftType,
            ["RightType"] = structure.RightType
        };

    public override MainHouse Deserialize(TagCompound tag) =>
        new(
            tag.Get<ushort>("X"),
            tag.Get<ushort>("Y"),
            tag.GetByte("Status"),
            tag.GetBool("HasBasement"),
            tag.GetBool("InUnderworld"),
            tag.GetByte("LeftType") != 0 ? tag.GetByte("LeftType") : (byte)1, // if its 0 (which only happens if it's a <= v0.2.7 world) set to default (large)
            tag.GetByte("RightType") != 0 ? tag.GetByte("RightType") : (byte)1
        );
}

internal class MainBasementSerializer : TagSerializer<MainBasement, TagCompound> {
    public override TagCompound Serialize(MainBasement chain) =>
        new() {
            ["X"] = chain.EntryPosX,
            ["Y"] = chain.EntryPosY,
            ["Status"] = chain.Status,
            ["RootStructure"] = ChainProcessor.SerializeChain(chain.RootStructure)
        };

    public override MainBasement Deserialize(TagCompound tag) {
        MainBasement basement = new(
            (ushort)tag.Get<short>("X"),
            (ushort)tag.Get<short>("Y"),
            tag.GetByte("Status")
        );
        basement.RootStructure = ChainProcessor.DeserializeChain(basement, (TagCompound)tag["RootStructure"]);
        return basement;
    }
}

internal class MineshaftSerializer : TagSerializer<Mineshaft, TagCompound> {
    public override TagCompound Serialize(Mineshaft structure) =>
        new() {
            ["Pos"] = structure.BoundingBox.TopLeftPoint16,
            ["Status"] = structure.Status
        };

    public override Mineshaft Deserialize(TagCompound tag) =>
        new(
            tag.Get<ushort>("X"),
            tag.Get<ushort>("Y"),
            tag.GetByte("Status")
        );
}

internal class BeachHouseSerializer : TagSerializer<BeachHouse, TagCompound> {
    public override TagCompound Serialize(BeachHouse structure) =>
        new() {
            ["Pos"] = structure.BoundingBox.TopLeftPoint16,
            ["Status"] = structure.Status,
            ["Reverse"] = structure.Reverse,
            ["HasDeck"] = structure.HasDeck
        };

    public override BeachHouse Deserialize(TagCompound tag) =>
        new(
            tag.Get<ushort>("X"),
            tag.Get<ushort>("Y"),
            tag.GetByte("Status"),
            tag.GetBool("Reverse"),
            tag.ContainsKey("HasDeck") && tag.GetBool("HasDeck")
        );
}