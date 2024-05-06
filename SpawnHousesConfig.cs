using System.ComponentModel;
using Terraria.ModLoader.Config;

public class SpawnHousesConfig : ModConfig
{
	public override ConfigScope Mode => ConfigScope.ServerSide;
	
	[DefaultValue(true)]
	[ReloadRequired]
	public bool EnableSpawnPointHouse { get; set; }
	
	[DefaultValue(true)]
	[ReloadRequired]
	public bool EnableBeachHouse { get; set; }
}