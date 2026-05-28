using Microsoft.Xna.Framework;
using SpawnHouses.Legacy;
using SpawnHouses.Legacy.Structures;
using SpawnHouses.Structures.Structures;
using Terraria;
using Terraria.ModLoader;

namespace SpawnHouses;

public class SpawnHousesPlayer : ModPlayer {
    private int _frameCounter;

    public override void OnEnterWorld() {
        if (ModHelper.ErrorLoadingMS)
            Main.NewText(
                "Generated Houses had an issue loading Magic Storage content, so Magic Storage features in Generated Houses are disabled. Please contact the author about this issue!",
                Color.Red);
    }

    public override void PostUpdate() {
        _frameCounter++;
        if (_frameCounter >= 16) {
            _frameCounter = 0;
            int x = (int)Player.Center.X / 16;
            int y = (int)Player.Center.Y / 16;

            if (LegacyStructureManager.MainBasement is not null &&
                LegacyStructureManager.MainBasement.Status == StructureStatus.GeneratedButNotFound)
                if (
                    x > LegacyStructureManager.MainBasement.EntryPosX - 7
                    && x < LegacyStructureManager.MainBasement.EntryPosX + 7
                    && y > LegacyStructureManager.MainBasement.EntryPosY + 6
                    && y < LegacyStructureManager.MainBasement.EntryPosY + 20
                )
                    LegacyStructureManager.MainBasement.OnFound();

            if (LegacyStructureManager.BeachHouse is not null &&
                LegacyStructureManager.BeachHouse.Status == StructureStatus.GeneratedButNotFound) {
                int houseCenterX = LegacyStructureManager.BeachHouse.X + BeachHouse._structureXSize / 2;
                int houseCenterY = LegacyStructureManager.BeachHouse.Y + BeachHouse._structureYSize / 2;

                if (
                    x > houseCenterX - 70
                    && x < houseCenterX + 70
                    && y > houseCenterY - 44
                    && y < houseCenterY + 44
                )
                    LegacyStructureManager.BeachHouse.OnFound();
            }
        }
    }
}