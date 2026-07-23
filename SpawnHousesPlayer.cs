using Terraria;
using Terraria.DataStructures;
using Terraria.ModLoader;

namespace SpawnHouses;

public class SpawnHousesPlayer : ModPlayer {
    private int _frameCounter;

    public override void PostUpdate() {
        _frameCounter++;
        if (_frameCounter >= 45) {
            _frameCounter = 0;
            Point16 pos = Player.Center.ToTileCoordinates16();

            // foreach (LegacyStructure structure in StructureManager.LegacyStructures)
            //     if (structure.Status == StructureStatus.GeneratedButNotFound && structure.IsFound(pos))
            //         structure.OnFound();
            //
            // foreach (LegacyStructureChain structure in StructureManager.LegacyStructureChains) {
            //     if (structure.Status == StructureStatus.GeneratedButNotFound && structure.IsFound(pos)) structure.OnFound();
            // }
        }
    }
}