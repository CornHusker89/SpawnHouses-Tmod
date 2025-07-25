using System.Collections.Generic;
using Microsoft.Xna.Framework;
using SpawnHouses.Helpers;
using Terraria;
using Terraria.ID;
using Terraria.IO;
using Terraria.Localization;
using Terraria.ModLoader;
using Terraria.WorldBuilding;
using static SpawnHouses.Helpers.WorldGenHelper;


namespace SpawnHouses.WorldGen;

public class WorldGenPasses : ModSystem {
    // 3. These lines setup the localization for the message shown during world generation. Update your localization files after building and reloading the mod to provide values for this.
    public static LocalizedText WorldGenCustomHousesPassMessage { get; private set; }

    public override void SetStaticDefaults() {
        WorldGenCustomHousesPassMessage =
            Language.GetOrRegister(Mod.GetLocalizationKey($"WorldGen.{nameof(WorldGenCustomHousesPassMessage)}"));
    }

    // 4. We use the ModifyWorldGenTasks method to tell the game the order that our world generation code should run
    public override void ModifyWorldGenTasks(List<GenPass> tasks, ref double totalWeight) {
        // 5. We use FindIndex to locate the index of the vanilla world generation task called "Sunflowers". This ensures our code runs at the correct step.
        int sunflowersIndex = tasks.FindIndex(genpass => genpass.Name.Equals("Sunflowers"));
        if (sunflowersIndex != -1)
            // 6. We register our world generation pass by passing in an instance of our custom GenPass class below. The GenPass class will execute our world generation code.
            tasks.Insert(sunflowersIndex + 1, new MainHousePass("Main House Pass", 100f));
        else
            tasks.Insert(tasks.Count - 8, new MainHousePass("Main House Pass", 100f));

        int iceIndex = tasks.FindIndex(genpass => genpass.Name.Equals("Ice"));
        if (iceIndex != -1)
            // 6. We register our world generation pass by passing in an instance of our custom GenPass class below. The GenPass class will execute our world generation code.
            tasks.Insert(iceIndex + 1, new ClearSpawnPointPass("Spawn Point Basement Prep Pass", 100f));
        else
            tasks.Insert(tasks.Count - 40, new ClearSpawnPointPass("Spawn Point Basement Prep Pass", 100f));


        tasks.Insert(tasks.Count - 2, new BeachHousePass("Beach House Pass", 100f));
    }

    public override void PreWorldGen() {
        StructureManager.WorldModVersion = SpawnHousesMod.Instance.Version;
    }

    public override void PostWorldGen() {
        // move guide into the main house (if it's there)
        if (StructureManager.MainHouse is not null)
            foreach (NPC npc in Main.npc)
                if (npc.type is NPCID.Guide or NPCID.TaxCollector || (CompatabilityHelper.IsMSEnabled && npc.type == CompatabilityHelper.AutomatonNpcID)) {
                    npc.position.X = (StructureManager.MainHouse.X + StructureManager.MainHouse.LeftSize - 1 + Terraria.WorldGen.genRand.Next(-4, 5)) * 16; // tiles to pixels
                    npc.position.Y = (StructureManager.MainHouse.Y + 13) * 16;
                }

        //so that it won't go out of bounds
        if (Main.ActiveWorldFileData.SeedText.ToLower().Replace(" ", "").Replace("'", "") == "dontdigup" ||
            Main.ActiveWorldFileData.SeedText.ToLower().Replace(" ", "") == "getfixedboi" ||
            CompatabilityHelper.IsRemnantsEnabled) {
            ModContent.GetInstance<SpawnHousesConfig>().SpawnPointBasementShape = float.Max(ModContent.GetInstance<SpawnHousesConfig>().SpawnPointBasementShape, 0.4f);
        }

        if (ModContent.GetInstance<SpawnHousesConfig>().EnableSpawnPointBasement)
            GenerateMainBasement();

        SpawnHousesMod.WebClient.AddSpawnCount(
            StructureManager.MainHouse is not null,
            StructureManager.MainBasement is not null,
            StructureManager.BeachHouse is not null,
            StructureManager.Mineshaft is not null
        );
    }
}

public class ClearSpawnPointPass : GenPass {
    public ClearSpawnPointPass(string name, float loadWeight) : base(name, loadWeight) {
    }

    // 8. The ApplyPass method is where the actual world generation code is placed.
    protected override void ApplyPass(GenerationProgress progress, GameConfiguration configuration) {
        // 9. Finally, we do the actual world generation code.

        if (ModContent.GetInstance<SpawnHousesConfig>().EnableSpawnPointBasement) {
            int x = Main.maxTilesX / 2;
            int y = (int)(Main.worldSurface / 2);

            //make sure we're not under the surface
            while (!Is40AboveTilesClear(x, y))
                y -= 30;

            bool Is40AboveTilesClear(int startX, int startY) {
                for (byte i = 1; i < 41; i++)
                    if (Terraria.WorldGen.SolidTile(startX, startY - i))
                        return false;

                return true;
            }

            // move down to the surface
            while (y < Main.worldSurface + 50) {
                if (Terraria.WorldGen.SolidTile(x, y))
                    break;

                y++;
            }

            GenVars.structures.AddProtectedStructure(new Rectangle(x - 75, y - 75, 150, 150));
        }
    }
}

// 7. Make sure to inherit from the GenPass class.
public class MainHousePass : GenPass {
    public MainHousePass(string name, float loadWeight) : base(name, loadWeight) {
    }

    // 8. The ApplyPass method is where the actual world generation code is placed.
    protected override void ApplyPass(GenerationProgress progress, GameConfiguration configuration) {
        // 9. Finally, we do the actual world generation code.

        bool spawnUnderworld =
            Main.ActiveWorldFileData.SeedText.ToLower().Replace(" ", "").Replace("'", "") == "dontdigup" ||
            Main.ActiveWorldFileData.SeedText.ToLower().Replace(" ", "") == "getfixedboi";

        if (ModContent.GetInstance<SpawnHousesConfig>().EnableSpawnPointHouse)
            GenerateMainHouse();

        if (!spawnUnderworld && ModContent.GetInstance<SpawnHousesConfig>().EnableMineshaft)
            GenerateMineshaft();
    }
}

public class BeachHousePass : GenPass {
    public BeachHousePass(string name, float loadWeight) : base(name, loadWeight) {
    }

    // 8. The ApplyPass method is where the actual world generation code is placed.
    protected override void ApplyPass(GenerationProgress progress, GameConfiguration configuration) {
        // 9. Finally, we do the actual world generation code.

        if (ModContent.GetInstance<SpawnHousesConfig>().EnableBeachHouse)
            GenerateBeachHouse();
    }
}