#nullable enable
using Microsoft.Xna.Framework;
using SpawnHouses.Legacy;
using SpawnHouses.Legacy.Structures;
using Terraria;
using Terraria.ModLoader;
using WorldGenTesting.Helpers;
using WorldGenTesting.Types;

namespace SpawnHouses.Testing;

[JITWhenModsEnabled("WorldGenTesting")]
public class SpawnHousesTesting {
    public static void Initialize() {
        WorldGenTesting.WorldGenTesting? testingMod = ModContent.GetInstance<WorldGenTesting.WorldGenTesting>();

        testingMod.AddTest(new Test(
            ModInstance.Mod, () => {
                TestingHelper.MakeWorld("SpawnHousesAutomatedTesting");
                return ScreenshotMainHouse();
            },
            "mainhouse"
        ));
        testingMod.AddTest(new Test(
            ModInstance.Mod, () => {
                TestingHelper.MakeWorld("SpawnHousesAutomatedTesting");
                return ScreenshotBeachHouse();
            },
            "beachhouse"
        ));
        testingMod.AddTest(new Test(
            ModInstance.Mod, () => {
                TestingHelper.MakeWorld("SpawnHousesAutomatedTesting");
                return ScreenshotMainBasement();
            },
            "mainbasement"
        ));
        testingMod.AddTest(new Test(
            ModInstance.Mod, () => {
                TestingHelper.MakeWorld("SpawnHousesAutomatedTesting");
                return ScreenshotMineshaft();
            },
            "mineshaft"
        ));
        testingMod.AddTest(new Test(
            ModInstance.Mod, () => {
                TestingHelper.MakeWorld("SpawnHousesAutomatedTesting");
                string output = string.Empty;
                string? result = ScreenshotMainHouse();
                output += result == null ? "" : result + "\n";
                result = ScreenshotBeachHouse();
                output += result == null ? "" : result + "\n";
                result = ScreenshotMainBasement();
                output += result == null ? "" : result + "\n";
                result = ScreenshotMineshaft();
                output += result == null ? "" : result + "\n";
                return output.Length > 0 ? output : null;
            },
            "all"
        ));
    }


    #region Test Helpers

    private static string? ScreenshotMainHouse() {
        if (LegacyStructureManager.MainHouse is null)
            return "No Main House";

        TestingHelper.TakeScreenshot(
            new Rectangle(
                LegacyStructureManager.MainHouse.X - 30,
                LegacyStructureManager.MainHouse.Y - 20,
                LegacyStructureManager.MainHouse.StructureXSize + 60,
                LegacyStructureManager.MainHouse.StructureYSize + 40
            ),
            Main.ActiveWorldFileData.Seed + "_MainHouse"
        );
        return null;
    }

    private static string? ScreenshotBeachHouse() {
        if (LegacyStructureManager.BeachHouse is null)
            return "No Beach House";

        TestingHelper.TakeScreenshot(
            new Rectangle(
                LegacyStructureManager.BeachHouse.X - 30,
                LegacyStructureManager.BeachHouse.Y - 30,
                LegacyStructureManager.BeachHouse.StructureXSize + 60,
                LegacyStructureManager.BeachHouse.StructureYSize + 60
            ),
            Main.ActiveWorldFileData.Seed + "_BeachHouse"
        );
        return null;
    }

    private static string? ScreenshotMainBasement() {
        if (LegacyStructureManager.MainBasement is null)
            return "No Main Basement";

        TestingHelper.TakeScreenshot(
            new Rectangle(
                LegacyStructureManager.MainBasement.EntryPosX - 60,
                LegacyStructureManager.MainBasement.EntryPosY - 20,
                120,
                200
            ),
            Main.ActiveWorldFileData.Seed + "_MainBasement"
        );
        return null;
    }

    private static string? ScreenshotMineshaft() {
        if (LegacyStructureManager.Mineshaft is null)
            return "No Mineshaft";

        TestingHelper.TakeScreenshot(
            new Rectangle(
                LegacyStructureManager.Mineshaft.X - 10,
                LegacyStructureManager.Mineshaft.Y - 6,
                LegacyStructureManager.Mineshaft.StructureXSize + 20,
                200
            ),
            Main.ActiveWorldFileData.Seed + "_Mineshaft"
        );
        return null;
    }

    #endregion
}