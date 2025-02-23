using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ModLoader;

namespace SpawnHouses;

public class SpawnHousesTesting
{
    [JITWhenModsEnabled("WorldGenTesting")]
    public static string TestMainHouse()
    {
        if (SpawnHousesSystem.MainHouse is null)
            return "No Main House";
        
        WorldGenTesting.Helpers.TestingHelper.TakeScreenshot(
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
    
    [JITWhenModsEnabled("WorldGenTesting")]
    public static string TestBeachHouse()
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
    
    [JITWhenModsEnabled("WorldGenTesting")]
    public static string TestMainBasement()
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
    
    [JITWhenModsEnabled("WorldGenTesting")]
    public static string TestMineshaft()
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
}