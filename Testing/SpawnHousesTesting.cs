#nullable enable
using System;
using Microsoft.Xna.Framework;
using SpawnHouses.Structures;
using Terraria;
using Terraria.ModLoader;
using WorldGenTesting.Helpers;
using WorldGenTesting.Types;

namespace SpawnHouses.Testing;

[JITWhenModsEnabled("WorldGenTesting")]
public class SpawnHousesTesting
{
    public static void Initialize(){
        var testingMod = ModContent.GetInstance<WorldGenTesting.WorldGenTesting>();

        testingMod.AddTest(new Test(
            ModInstance.Mod, TestMainHouse, "mainhouse"
        ));
        testingMod.AddTest(new Test(
            ModInstance.Mod, TestBeachHouse, "beachhouse"
        ));
        testingMod.AddTest(new Test(
            ModInstance.Mod, TestMainBasement, "mainbasement"
        ));
        testingMod.AddTest(new Test(
            ModInstance.Mod, TestMineshaft, "mineshaft"
        ));
        testingMod.AddTest(new Test(
            ModInstance.Mod, TestAll, "all"
        ));
    }


    #region Test Helpers

    private static string? ScreenshotMainHouse()
    {
        if (SpawnHousesSystem.MainHouse is null)
            return "No Main House";

        TestingHelper.TakeScreenshot(
            new Rectangle(
                SpawnHousesSystem.MainHouse.X - 30,
                SpawnHousesSystem.MainHouse.Y - 20,
                SpawnHousesSystem.MainHouse.StructureXSize + 60,
                SpawnHousesSystem.MainHouse.StructureYSize + 40
            ),
            Main.ActiveWorldFileData.Seed + "_MainHouse"
        );
        return null;
    }

    private static string? ScreenshotBeachHouse()
    {
        if (SpawnHousesSystem.BeachHouse is null)
            return "No Beach House";

        WorldGenTesting.Helpers.TestingHelper.TakeScreenshot(
            new Rectangle(
                SpawnHousesSystem.BeachHouse.X - 30,
                SpawnHousesSystem.BeachHouse.Y - 30,
                SpawnHousesSystem.BeachHouse.StructureXSize + 60,
                SpawnHousesSystem.BeachHouse.StructureYSize + 60
            ),
            Main.ActiveWorldFileData.Seed + "_BeachHouse"
        );
        return null;
    }

    private static string? ScreenshotMainBasement()
    {
        if (SpawnHousesSystem.MainBasement is null)
            return "No Main Basement";

        WorldGenTesting.Helpers.TestingHelper.TakeScreenshot(
            new Rectangle(
                SpawnHousesSystem.MainBasement.EntryPosX - 60,
                SpawnHousesSystem.MainBasement.EntryPosY - 20,
                120,
                200
            ),
            Main.ActiveWorldFileData.Seed + "_MainBasement"
        );
        return null;
    }

    private static string? ScreenshotMineshaft()
    {
        if (SpawnHousesSystem.Mineshaft is null)
            return "No Mineshaft";

        WorldGenTesting.Helpers.TestingHelper.TakeScreenshot(
            new Rectangle(
                SpawnHousesSystem.Mineshaft.X - 10,
                SpawnHousesSystem.Mineshaft.Y - 6,
                SpawnHousesSystem.Mineshaft.StructureXSize + 20,
                200
            ),
            Main.ActiveWorldFileData.Seed + "_Mineshaft"
        );
        return null;
    }

    #endregion


    #region Tests

    public static string? TestMainHouse() {
        TestingHelper.MakeWorld("SpawnHousesAutomatedTesting", WorldSize.Medium);
        return ScreenshotMainHouse();
    }

    public static string? TestBeachHouse() {
        TestingHelper.MakeWorld("SpawnHousesAutomatedTesting", WorldSize.Medium);
        return ScreenshotBeachHouse();
    }

    public static string? TestMainBasement() {
        TestingHelper.MakeWorld("SpawnHousesAutomatedTesting", WorldSize.Medium);
        return ScreenshotMainBasement();
    }

    public static string? TestMineshaft() {
        TestingHelper.MakeWorld("SpawnHousesAutomatedTesting", WorldSize.Medium);
        return ScreenshotMineshaft();
    }

    public static string? TestAll() {
        TestingHelper.MakeWorld("SpawnHousesAutomatedTesting", WorldSize.Medium);
        string output = String.Empty;
        output += "\n" + ScreenshotMainHouse();
        output += "\n" + ScreenshotBeachHouse();
        output += "\n" + ScreenshotMainBasement();
        output += "\n" + ScreenshotMineshaft();
        return output;
    }

    #endregion
}