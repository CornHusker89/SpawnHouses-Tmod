using System.ComponentModel;
using Terraria.ModLoader.Config;

namespace SpawnHouses;

public class SpawnHousesConfig : ModConfig
{
	public override ConfigScope Mode => ConfigScope.ServerSide;
	
	[DefaultValue(true)]
	[ReloadRequired]
	public bool EnableSpawnPointHouse { get; set; }
	
	[DefaultValue(true)]
	[ReloadRequired]
	public bool EnableMainBasement { get; set; }
	
	[DefaultValue(true)]
	[ReloadRequired]
	public bool EnableBeachHouse { get; set; }
}