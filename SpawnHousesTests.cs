using Terraria;
using Terraria.ModLoader;

namespace SpawnHouses;

public static class SpawnHousesIngameTests
{
    public static void DoAllTests()
    {
        TestMainHouse();
        TestBeachHouse();
    }
    
    public static void TestMainHouse()
    {
        bool hasMainHouse = ModContent.GetInstance<SpawnHousesConfig>().EnableSpawnPointHouse;
        string mainHouseMessage = "Main house not enabled";
        if (hasMainHouse)
        {
            //check if it exists at the spawn point
            
            //check if there is a floor far below the spawn point
            
            //check if guide or automaton is in the floor
            
            //using the house's position, make sure size varibles and structure types align with actual dimensions
            
            //check if it blended properly
        }
        
        ModContent.GetInstance<SpawnHouses>().Logger.Info($"Main House: {mainHouseMessage}");
    }
    
    public static void TestBeachHouse()
    {
        
    }
}