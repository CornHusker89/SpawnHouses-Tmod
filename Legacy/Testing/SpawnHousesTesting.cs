#nullable enable
using Microsoft.Xna.Framework;
using SpawnHouses.Legacy.Structures.ChainTypes;
using SpawnHouses.Legacy.Structures.StructureTypes;
using Terraria;
using Terraria.ModLoader;
using WorldGenTesting.Helpers;
using WorldGenTesting.Types;

namespace SpawnHouses.Legacy.Testing;

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
        if (StructureManager.LegacyStructures.Find(s => s is MainHouse) is not MainHouse mainHouse)
            return "No Main House";

        TestingHelper.TakeScreenshot(
            new Rectangle(
                mainHouse.BoundingBox.Left - 30,
                mainHouse.BoundingBox.Top - 20,
                mainHouse.BoundingBox.Width + 60,
                mainHouse.BoundingBox.Height + 40
            ),
            Main.ActiveWorldFileData.Seed + "_MainHouse"
        );
        return null;
    }

    private static string? ScreenshotBeachHouse() {
        if (StructureManager.LegacyStructures.Find(s => s is BeachHouse) is not BeachHouse beachHouse)
            return "No Beach House";

        TestingHelper.TakeScreenshot(
            new Rectangle(
                beachHouse.BoundingBox.Left - 30,
                beachHouse.BoundingBox.Top - 30,
                beachHouse.BoundingBox.Width + 60,
                beachHouse.BoundingBox.Height + 60
            ),
            Main.ActiveWorldFileData.Seed + "_BeachHouse"
        );
        return null;
    }

    private static string? ScreenshotMainBasement() {
        if (StructureManager.LegacyStructureChains.Find(s => s is MainBasement) is not MainBasement mainBasement)
            return "No Main Basement";

        TestingHelper.TakeScreenshot(
            new Rectangle(
                mainBasement.EntryPosX - 60,
                mainBasement.EntryPosY - 20,
                120,
                200
            ),
            Main.ActiveWorldFileData.Seed + "_MainBasement"
        );
        return null;
    }

    private static string? ScreenshotMineshaft() {
        if (StructureManager.LegacyStructures.Find(s => s is Mineshaft) is not Mineshaft mineshaft)
            return "No Mineshaft";

        TestingHelper.TakeScreenshot(
            new Rectangle(
                mineshaft.BoundingBox.Left - 10,
                mineshaft.BoundingBox.Top - 6,
                mineshaft.BoundingBox.Width + 20,
                200
            ),
            Main.ActiveWorldFileData.Seed + "_Mineshaft"
        );
        return null;
    }

    #endregion
}