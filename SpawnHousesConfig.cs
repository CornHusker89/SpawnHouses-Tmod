using System;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using Microsoft.Xna.Framework;
using Newtonsoft.Json;
using Terraria;
using Terraria.ModLoader;
using Terraria.ModLoader.Config;
using Terraria.UI;

namespace SpawnHouses;

public class SpawnHousesConfig : ModConfig
{
	public override ConfigScope Mode => ConfigScope.ServerSide;
	
	// [JsonIgnore]
	// [ShowDespiteJsonIgnore]
	// public int TotalNpcs { get; set; }
	
	void SetTotalNpcs()
	{
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
	
	private int _spawnPointHouseSize = 4;
	[Header("SizeHeader")]
	
	[DefaultValue(4)]
	[Terraria.ModLoader.Config.Range(2, 4)]
	[Slider]
	public int SpawnPointHouseSize
	{
		get => _spawnPointHouseSize;
		set
		{
			_spawnPointHouseSize = value;
			SetTotalNpcs();
		}
	}
	
	private int _spawnPointBasementSize = 6;
	[DefaultValue(6)]
	[Terraria.ModLoader.Config.Range(3, 15)]
	[Slider]
	public int SpawnPointBasementSize
	{
		get => _spawnPointBasementSize;
		set
		{
			_spawnPointBasementSize = value;
			SpawnPointBasementMultiplier = value / 6f;
			SetTotalNpcs();
		}
	}
	[JsonIgnore] public float SpawnPointBasementMultiplier { get; set; } = 1;
	
	[DefaultValue(2)]
	[Terraria.ModLoader.Config.Range(2, 2)]
	[Slider]
	[JsonIgnore]
	//[ShowDespiteJsonIgnore]
	public int BeachHouseSize { get; set; } = 2;
	
	
	
	
	
	
	
	
	[Header("ModifiersHeader")]
	
	
	[ReloadRequired]
	[DefaultValue(true)]
	public bool MagicStorageIntegrations { get; set; }
	
	[DefaultValue(false)]
	public bool SpawnPointHouseOffset { get; set; }
	
	[DefaultValue(true)]
	public bool SpawnPointHouseSetsSpawn { get; set; }
	
	[DefaultValue(0.5f)]
	[Slider]
	[Terraria.ModLoader.Config.Range(0, 1)]
	[Increment(0.1f)]
	public float MainBasementShape { get; set; }

	
	
	
	
	
	
	

	
	
	private bool _enableSpawnPointHouse = true; //this is in the StructuresHeader
	[Header("StructuresHeader")]
	
	[DefaultValue(true)]
	public bool EnableSpawnPointHouse
	{
		get => _enableSpawnPointHouse;
		set
		{
			_enableSpawnPointHouse = value;
			SetTotalNpcs();
		}
	}
	
	private bool _enableSpawnPointBasement = true;
	[DefaultValue(true)]
	public bool EnableSpawnPointBasement 	
	{
		get => _enableSpawnPointBasement;
		set
		{
			_enableSpawnPointBasement = value;
			SetTotalNpcs();
		}
	}

	[DefaultValue(true)] 
	public bool EnableMineshaft { get; set; }= true;
	
	private bool _enableBeachHouse  = true;
	[DefaultValue(true)]
	public bool EnableBeachHouse 
	{
		get => _enableBeachHouse;
		set
		{
			_enableBeachHouse = value;
			SetTotalNpcs();
		}
	}
}