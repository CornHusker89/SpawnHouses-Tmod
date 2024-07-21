using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using Newtonsoft.Json;
using Terraria.ModLoader.Config;

namespace SpawnHouses;

public class SpawnHousesConfig : ModConfig
{
	// values values
	private const byte NpcBase = 6;
	
	private bool _doesAutoSetSize;
	[JsonIgnore] public float SizeMultiplier => (float)NpcEstimate / NpcBase;
	
	public override ConfigScope Mode => ConfigScope.ServerSide;
	
	
	
	[Header("SizeHeader")]
	
	
	[DefaultValue(true)]
	public bool AutoSetSize
	{
		get
		{
			if (_doesAutoSetSize && NpcEstimate == NpcBase)
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
				NpcEstimate = NpcBase;
				_doesAutoSetSize = true;
			}
			else
				_doesAutoSetSize = false;
			
		}
	}
	
	[DefaultValue(NpcBase)]
	[Terraria.ModLoader.Config.Range(4, 6)]
	[Slider]
	public int NpcEstimate { get; set; }
	

	
	
	[Header("StructuresHeader")]

	
	[DefaultValue(true)]
	public bool EnableSpawnPointHouse { get; set; }
	
	[DefaultValue(false)]
	[JsonIgnore]
	public bool EnableSpawnPointBasement { get; set; }
	
	[DefaultValue(true)]
	public bool EnableBeachHouse { get; set; }
}