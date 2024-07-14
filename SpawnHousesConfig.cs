using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using Newtonsoft.Json;
using Terraria.ModLoader.Config;

namespace SpawnHouses;

public class SpawnHousesConfig : ModConfig
{
	// values values
	private const byte NpcBase = 11;
	
	
	
	private bool _doesAutoSetSize;
	[JsonIgnore] public float SizeMultiplier => (float)Size / NpcBase;
	
	public override ConfigScope Mode => ConfigScope.ServerSide;
	
	
	
	[Header("SizeHeader")]
	
	
	[DefaultValue(true)]
	public bool AutoSetSize
	{
		get
		{
			if (_doesAutoSetSize && Size == NpcBase)
				return true;
			
			else
			{
				_doesAutoSetSize = false;
				return false;
			}
		}
		set 
		{
			if (value) {
				Size = NpcBase;
				_doesAutoSetSize = true;
			}
			else
				_doesAutoSetSize = false;
			
		}
	}
	
	[DefaultValue(NpcBase)]
	public byte Size { get; set; }
	

	
	
	[Header("StructuresHeader")]

	
	[DefaultValue(true)]
	[ReloadRequired]
	public bool EnableSpawnPointHouse { get; set; }
	
	[DefaultValue(true)]
	[ReloadRequired]
	public bool EnableSpawnPointBasement { get; set; }
	
	[DefaultValue(true)]
	[ReloadRequired]
	public bool EnableBeachHouse { get; set; }
}