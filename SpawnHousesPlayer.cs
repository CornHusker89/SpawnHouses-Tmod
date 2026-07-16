using Microsoft.Xna.Framework;
using SpawnHouses.Legacy.Helpers;
using Terraria;
using Terraria.DataStructures;
using Terraria.ModLoader;

namespace SpawnHouses;

public class SpawnHousesPlayer : ModPlayer {
    private int _frameCounter;

    public override void OnEnterWorld() {
        if (CompatabilityHelper.ErrorLoadingMS) Main.NewText("Generated Housing had an issue loading Magic Storage content, so Magic Storage features in Generated Houses are disabled. Please contact the author about this issue!", Color.Red);
    }

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