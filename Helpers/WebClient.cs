using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using Terraria.ModLoader;

namespace SpawnHouses.Helpers;

public class WebClient {
    private readonly HttpClient _client;

    public WebClient() {
        _client = new HttpClient {
            Timeout = TimeSpan.FromSeconds(3)
        };

        _client.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", "AFadsfnjhAF7432Fjfh12272JHF82");
    }

    /// <summary>
    /// </summary>
    /// <returns>keys are "beach_houses", "main_basements", "main_houses", "mineshafts", "main_houses_extrapolated"</returns>
    public Dictionary<string, int> GetSpawnCount() {
        try {
            ModContent.GetInstance<SpawnHousesMod>().Logger.Info("Getting spawn count info from Web API");
            HttpResponseMessage response = _client.GetAsync("https://spawnhousescounter.xyz/api/get").Result;
            response.EnsureSuccessStatusCode();
            string responseBody = response.Content.ReadAsStringAsync().Result;
            return JsonSerializer.Deserialize<Dictionary<string, int>>(responseBody);
        }
        catch {
            return null;
        }
    }

    public void AddSpawnCount(bool mainHouse = false, bool mainBasement = false, bool beachHouse = false,
        bool mineshaft = false) {
        try {
            ModContent.GetInstance<SpawnHousesMod>().Logger.Info("Sending spawn count info to Web API");
            var dict = new Dictionary<string, int> {
                ["main_house"] = mainHouse ? 1 : 0,
                ["main_basement"] = mainBasement ? 1 : 0,
                ["beach_house"] = beachHouse ? 1 : 0,
                ["mineshaft"] = mineshaft ? 1 : 0
            };
            StringContent content = new StringContent(JsonSerializer.Serialize(dict), Encoding.UTF8, "application/json");
            HttpResponseMessage response = _client.PostAsync("https://spawnhousescounter.xyz/api/add", content).Result;
            response.EnsureSuccessStatusCode();
        }
        catch {
            // ignored
        }
    }
}