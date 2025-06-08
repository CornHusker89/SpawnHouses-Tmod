#nullable enable
using Microsoft.Xna.Framework;
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
            SpawnHousesMod.Instance, () => {
                TestingHelper.MakeWorld("SpawnHousesAutomatedTesting");
                return ScreenshotMainHouse();
            },
            "mainhouse"
        ));
        testingMod.AddTest(new Test(
            SpawnHousesMod.Instance, () => {
                TestingHelper.MakeWorld("SpawnHousesAutomatedTesting");
                return ScreenshotBeachHouse();
            },
            "beachhouse"
        ));
        testingMod.AddTest(new Test(
            SpawnHousesMod.Instance, () => {
                TestingHelper.MakeWorld("SpawnHousesAutomatedTesting");
                return ScreenshotMainBasement();
            },
            "mainbasement"
        ));
        testingMod.AddTest(new Test(
            SpawnHousesMod.Instance, () => {
                TestingHelper.MakeWorld("SpawnHousesAutomatedTesting");
                return ScreenshotMineshaft();
            },
            "mineshaft"
        ));
        testingMod.AddTest(new Test(
            SpawnHousesMod.Instance, () => {
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
        if (StructureManager.MainHouse is null)
            return "No Main House";

        TestingHelper.TakeScreenshot(
            new Rectangle(
                StructureManager.MainHouse.X - 30,
                StructureManager.MainHouse.Y - 20,
                StructureManager.MainHouse.StructureXSize + 60,
                StructureManager.MainHouse.StructureYSize + 40
            ),
            Main.ActiveWorldFileData.Seed + "_MainHouse"
        );
        return null;
    }

    private static string? ScreenshotBeachHouse() {
        if (StructureManager.BeachHouse is null)
            return "No Beach House";

        TestingHelper.TakeScreenshot(
            new Rectangle(
                StructureManager.BeachHouse.X - 30,
                StructureManager.BeachHouse.Y - 30,
                StructureManager.BeachHouse.StructureXSize + 60,
                StructureManager.BeachHouse.StructureYSize + 60
            ),
            Main.ActiveWorldFileData.Seed + "_BeachHouse"
        );
        return null;
    }

    private static string? ScreenshotMainBasement() {
        if (StructureManager.MainBasement is null)
            return "No Main Basement";

        TestingHelper.TakeScreenshot(
            new Rectangle(
                StructureManager.MainBasement.EntryPosX - 60,
                StructureManager.MainBasement.EntryPosY - 20,
                120,
                200
            ),
            Main.ActiveWorldFileData.Seed + "_MainBasement"
        );
        return null;
    }

    private static string? ScreenshotMineshaft() {
        if (StructureManager.Mineshaft is null)
            return "No Mineshaft";

        TestingHelper.TakeScreenshot(
            new Rectangle(
                StructureManager.Mineshaft.X - 10,
                StructureManager.Mineshaft.Y - 6,
                StructureManager.Mineshaft.StructureXSize + 20,
                200
            ),
            Main.ActiveWorldFileData.Seed + "_Mineshaft"
        );
        return null;
    }

    #endregion
}