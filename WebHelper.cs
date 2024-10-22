using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Security;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Text.Json;
using Terraria.ModLoader;

public class WebHelper
{
    private readonly HttpClient Client;
    
    public WebHelper()
    {
        Client = new HttpClient()
        {
            Timeout = TimeSpan.FromSeconds(3)
        };
        
        Client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", "AFadsfnjhAF7432Fjfh12272JHF82");
    }

    /// <summary>
    /// 
    /// </summary>
    /// <returns>keys are "beach_house", "main_basement", "main_house", "mineshaft"</returns>
    public Dictionary<string, int> GetSpawnCount()
    {
        try
        {
            ModContent.GetInstance<SpawnHouses.SpawnHouses>().Logger.Info("Getting spawn count info from Web API");
            HttpResponseMessage response = Client.GetAsync("https://spawnhousescounter.xyz/api/get").Result;
            response.EnsureSuccessStatusCode();
            string responseBody = response.Content.ReadAsStringAsync().Result;
            return JsonSerializer.Deserialize<Dictionary<string, int>>(responseBody);
        }
        catch
        {
            return null;
        }
    }
    
    public void AddSpawnCount(bool mainHouse = false, bool mainBasement = false, bool beachHouse = false, bool mineshaft = false)
    {
        try
        {
            ModContent.GetInstance<SpawnHouses.SpawnHouses>().Logger.Info("Sending spawn count info to Web API");
            Dictionary<string, int> dict = new Dictionary<string, int>
            {
                ["main_house"] = mainHouse? 1 : 0,
                ["main_basement"] = mainBasement? 1 : 0,
                ["beach_house"] = beachHouse? 1 : 0,
                ["mineshaft"] = mineshaft? 1 : 0
            };
            var content = new StringContent(JsonSerializer.Serialize(dict), Encoding.UTF8, "application/json");
            HttpResponseMessage response = Client.PostAsync("https://spawnhousescounter.xyz/api/add", content).Result;
            response.EnsureSuccessStatusCode();
        }
        catch
        {
            // ignored
        }
    }
}