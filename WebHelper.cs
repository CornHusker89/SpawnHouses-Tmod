using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using Terraria.ModLoader;

namespace SpawnHouses;

public class WebHelper {
    private readonly HttpClient Client;

    public WebHelper() {
        Client = new HttpClient {
            Timeout = TimeSpan.FromSeconds(3)
        };

        Client.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", "AFadsfnjhAF7432Fjfh12272JHF82");
    }

    /// <summary>
    /// </summary>
    /// <returns>keys are "beach_houses", "main_basements", "main_houses", "mineshafts", "main_houses_extrapolated"</returns>
    public Dictionary<string, int> GetSpawnCount() {
        try {
            ModContent.GetInstance<SpawnHouses>().Logger.Info("Getting spawn count info from Web API");
            var response = Client.GetAsync("https://spawnhousescounter.xyz/api/get").Result;
            response.EnsureSuccessStatusCode();
            var responseBody = response.Content.ReadAsStringAsync().Result;
            return JsonSerializer.Deserialize<Dictionary<string, int>>(responseBody);
        }
        catch {
            return null;
        }
    }

    public void AddSpawnCount(bool mainHouse = false, bool mainBasement = false, bool beachHouse = false,
        bool mineshaft = false) {
        try {
            ModContent.GetInstance<SpawnHouses>().Logger.Info("Sending spawn count info to Web API");
            var dict = new Dictionary<string, int> {
                ["main_house"] = mainHouse ? 1 : 0,
                ["main_basement"] = mainBasement ? 1 : 0,
                ["beach_house"] = beachHouse ? 1 : 0,
                ["mineshaft"] = mineshaft ? 1 : 0
            };
            var content = new StringContent(JsonSerializer.Serialize(dict), Encoding.UTF8, "application/json");
            var response = Client.PostAsync("https://spawnhousescounter.xyz/api/add", content).Result;
            response.EnsureSuccessStatusCode();
        }
        catch {
            // ignored
        }
    }
}