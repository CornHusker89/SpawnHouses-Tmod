using SpawnHouses.AdvStructures;
using Terraria.ModLoader;

namespace SpawnHouses;

public class SpawnHouses : Mod {
    public override void Load() {
        AdvStructure.PopulateGenerators();
    }
}