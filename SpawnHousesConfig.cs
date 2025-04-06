using System.ComponentModel;
using Newtonsoft.Json;
using Terraria.ModLoader.Config;

namespace SpawnHouses;

public class SpawnHousesConfig : ModConfig {
    private bool _enableBeachHouse = true;

    private bool _enableSpawnPointBasement = true;


    private bool _enableSpawnPointHouse = true; //this is in the StructuresHeader

    private int _spawnPointBasementSize = 6;

    private int _spawnPointHouseSize = 4;
    public override ConfigScope Mode => ConfigScope.ServerSide;

    [Header("SizeHeader")]
    [DefaultValue(4)]
    [Range(2, 4)]
    [Slider]
    public int SpawnPointHouseSize {
        get => _spawnPointHouseSize;
        set {
            _spawnPointHouseSize = value;
            SetTotalNpcs();
        }
    }

    [DefaultValue(6)]
    [Range(3, 25)]
    [Slider]
    public int SpawnPointBasementSize {
        get => _spawnPointBasementSize;
        set {
            _spawnPointBasementSize = value;
            SpawnPointBasementMultiplier = value / 6f;
            SetTotalNpcs();
        }
    }

    [JsonIgnore] public float SpawnPointBasementMultiplier { get; set; } = 1;

    [DefaultValue(2)]
    [Range(2, 2)]
    [Slider]
    [JsonIgnore]
    //[ShowDespiteJsonIgnore]
    public int BeachHouseSize { get; set; } = 2;


    [Header("ModifiersHeader")]
    [ReloadRequired]
    [DefaultValue(true)]
    public bool MagicStorageIntegrations { get; set; }

    [DefaultValue(false)] public bool SpawnPointHouseOffset { get; set; }

    [DefaultValue(true)] public bool SpawnPointHouseSetsSpawn { get; set; }

    [DefaultValue(0.5f)]
    [Slider]
    [Range(0.1f, 1.0f)]
    [Increment(0.1f)]
    public float SpawnPointBasementShape { get; set; }

    [Header("StructuresHeader")]
    [DefaultValue(true)]
    public bool EnableSpawnPointHouse {
        get => _enableSpawnPointHouse;
        set {
            _enableSpawnPointHouse = value;
            SetTotalNpcs();
        }
    }

    [DefaultValue(true)]
    public bool EnableSpawnPointBasement {
        get => _enableSpawnPointBasement;
        set {
            _enableSpawnPointBasement = value;
            SetTotalNpcs();
        }
    }

    [DefaultValue(true)] public bool EnableMineshaft { get; set; } = true;

    [DefaultValue(true)]
    public bool EnableBeachHouse {
        get => _enableBeachHouse;
        set {
            _enableBeachHouse = value;
            SetTotalNpcs();
        }
    }

    // [JsonIgnore]
    // [ShowDespiteJsonIgnore]
    // public int TotalNpcs { get; set; }

    private void SetTotalNpcs() {
        // Console.WriteLine(_enableSpawnPointHouse);
        // int size = 0;
        // if (_enableSpawnPointHouse)
        // {
        // 	Console.WriteLine("adding spawn point house");
        // 	size += _spawnPointHouseSize;
        // }
        // Console.WriteLine(size);
        // if (_enableSpawnPointBasement)
        // 	size += _spawnPointBasementSize;
        // if (_enableBeachHouse)
        // 	size += BeachHouseSize;
        // Console.WriteLine(size);
        // TotalNpcs = size;
        // Console.WriteLine(TotalNpcs);


        // Console.WriteLine(Main.MenuUI.CurrentState);
        // Terraria.ModLoader.Config.UI.

        // foreach (UIElement element in ))
        // {
        // 	if (element is UICheckbox checkbox && checkbox.Text == "Enable Feature")
        // 	{
        // 		return checkbox;
        // 	}
        // }
    }
}