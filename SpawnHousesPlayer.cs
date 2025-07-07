using System;
using Microsoft.Xna.Framework;
using SpawnHouses.Helpers;
using SpawnHouses.Structures.Chains;
using SpawnHouses.Structures.Structures;
using Terraria;
using Terraria.DataStructures;
using Terraria.ModLoader;
using BoundingBox = SpawnHouses.Types.BoundingBox;

namespace SpawnHouses;

public class SpawnHousesPlayer : ModPlayer {
    private int _frameCounter;

    public override void OnEnterWorld() {
        if (CompatabilityHelper.ErrorLoadingMS) Main.NewText("Generated Houses had an issue loading Magic Storage content, so Magic Storage features in Generated Houses are disabled. Please contact the author about this issue!", Color.Red);
    }

    public override void PostUpdate() {
        _frameCounter++;
        if (_frameCounter >= 45) {
            _frameCounter = 0;
            Point16 pos = Player.Center.ToTileCoordinates16();

            if (StructureManager.MainBasement is not null && StructureManager.MainBasement.Status == StructureStatus.GeneratedButNotFound) {
                if (StructureManager.MainBasementBoundingBoxes.Exists(boundingBox => BoundingBox.IsPointInside(boundingBox, pos)))
                    StructureManager.MainBasement.OnFound();
            }

            if (StructureManager.BeachHouse is not null && StructureManager.BeachHouse.Status == StructureStatus.GeneratedButNotFound) {
                int houseCenterX = StructureManager.BeachHouse.X + BeachHouse._structureXSize / 2;
                int houseCenterY = StructureManager.BeachHouse.Y + BeachHouse._structureYSize / 2;

                if (
                    pos.X > houseCenterX - 70
                    && pos.X < houseCenterX + 70
                    && pos.Y > houseCenterY - 44
                    && pos.Y < houseCenterY + 44
                )
                    StructureManager.BeachHouse.OnFound();
            }
        }
    }
}