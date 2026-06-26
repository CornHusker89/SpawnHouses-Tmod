using System;
using System.Collections.Generic;
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
using SpawnHouses.Legacy.Structures;
using Terraria;
using Terraria.ModLoader;
using Terraria.ModLoader.IO;

namespace SpawnHouses;

#nullable enable

public class StructureManager : ModSystem {
    private static int _debugDrawFrameCount;

    private static Dictionary<DebugLabel, Point> _labels = [];
    
    private static readonly List<AdvStructure> StructureList = [];
    
    public static Version WorldVersion = new(ModInstance.Mod.Version.ToString());
    
    public static ushort GeneratableCount { get; private set; }

    public static readonly DebugInfoLevel DefaultDebugInfoLevel = new();

    /// <summary>
    ///     shallow copies then exposes the internal structure list
    /// </summary>
    /// <returns></returns>
    public static AdvStructure[] GetStructureList() => StructureList.ToArray();

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
        StructureList.Add(structure);
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
        WorldVersion = tag.ContainsKey("WorldVersion")
            ? new Version(tag.GetString("WorldVersion"))
            : new Version("0.3.2");

        GeneratableCount = tag.ContainsKey("GeneratableCount") ? tag.Get<ushort>("GeneratableCount") : (ushort)0;
    }

    public override void ClearWorld() {
        WorldVersion = new Version(ModInstance.Mod.Version.ToString());
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
            foreach (AdvStructure structure in StructureList)
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

        foreach (AdvStructure structure in StructureList) {
            foreach (DebugLabel label in structure.DrawDebugGeometry()) {
                _labels.Add(label, label.Root.ToPoint() * new Point(16, 16));
                TileBox structureGlobalTileBoundingBox = structure.Tilemap.ConvertToGlobal(structure.StructureLayout.BoundingBox);
                structureRects.Add(structureGlobalTileBoundingBox.Scale(16));
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