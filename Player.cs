using Microsoft.Xna.Framework;
using SpawnHouses.Structures;
using SpawnHouses.Structures.Structures;
using Terraria;
using Terraria.ModLoader;

namespace SpawnHouses;

public class Player : ModPlayer {
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

            if (StructureManager.MainBasement is not null &&
                StructureManager.MainBasement.Status == StructureStatus.GeneratedButNotFound)
                if (
                    x > StructureManager.MainBasement.EntryPosX - 7
                    && x < StructureManager.MainBasement.EntryPosX + 7
                    && y > StructureManager.MainBasement.EntryPosY + 6
                    && y < StructureManager.MainBasement.EntryPosY + 20
                )
                    StructureManager.MainBasement.OnFound();

            if (StructureManager.BeachHouse is not null &&
                StructureManager.BeachHouse.Status == StructureStatus.GeneratedButNotFound) {
                int houseCenterX = StructureManager.BeachHouse.X + BeachHouse._structureXSize / 2;
                int houseCenterY = StructureManager.BeachHouse.Y + BeachHouse._structureYSize / 2;

                if (
                    x > houseCenterX - 70
                    && x < houseCenterX + 70
                    && y > houseCenterY - 44
                    && y < houseCenterY + 44
                )
                    StructureManager.BeachHouse.OnFound();
            }
        }
    }
}