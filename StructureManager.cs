using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using SpawnHouses.Common;
using SpawnHouses.Common.Debug;
using SpawnHouses.Common.Tagging;
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
        DrawHelper.BeginWorldSpriteBatch();
        Dictionary<DebugLabel, Point> labels = [];
        List<Rectangle> structureRects = [];

        foreach (AdvStructure structure in StructureList)
        foreach (DebugLabel label in structure.DrawDebugInfo()) {
            labels.Add(label, label.Root.ToPoint() * new Point(16, 16));
            structureRects.Add(structure.StructureLayout.BoundingBox.ToRectangle());
        }

        // move each label away from each other and the structure
        for (int iter = 0; iter < 12; iter++) {
            foreach (var a in labels) {
                // label-label
                foreach (var b in labels) {
                    if (ReferenceEquals(a.Key, b.Key))
                        continue;

                    if (a.Key.GetRectangle(a.Value).Intersects(a.Key.GetRectangle(b.Value))) {
                        Vector2 delta = (a.Value - b.Value).ToVector2();

                        if (delta == Vector2.Zero)
                            delta = Vector2.UnitY;

                        labels[a.Key] += (Vector2.Normalize(delta) * 4f).ToPoint();
                    }
                }

                // label-structure
                foreach (Rectangle box in structureRects)
                    if (a.Key.GetRectangle(a.Value).Intersects(box)) {
                        Vector2 delta = (a.Value - box.Center).ToVector2();

                        if (delta == Vector2.Zero)
                            delta = Vector2.UnitY;

                        labels[a.Key] += (Vector2.Normalize(delta) * 4f).ToPoint();
                    }

                // gentle spring back to root
                Vector2 deltaToRoot = (a.Value - a.Key.Root.ToPoint()).ToVector2();
                labels[a.Key] += (deltaToRoot * 0.1f).ToPoint();
            }
        }

        Main.spriteBatch.End();
    }
}