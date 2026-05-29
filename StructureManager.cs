using System;
using System.Collections.Generic;
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
    public static Version WorldVersion = new(ModInstance.Mod.Version.ToString());
    
    public static ushort GeneratableCount { get; private set; }

    public static List<AdvStructure> StructureList { get; private set; } = [];

    public static DebugInfoLevel DefaultDebugInfoLevel = new();

    /// <summary>
    ///     returns the next component id, and advances the counter. begins at id 1
    /// </summary>
    /// <returns></returns>
    public static ushort NextGeneratableId() {
        GeneratableCount++;
        return GeneratableCount;
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

        foreach (AdvStructure structure in StructureList)
            structure.DrawDebugInfo();

        if (SpawnTest2.Component != null)
            SpawnTest2.Component.DrawDebugInfo();

        Main.spriteBatch.End();
    }
}