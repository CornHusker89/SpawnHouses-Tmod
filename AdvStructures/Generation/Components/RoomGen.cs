using System.Collections.Generic;
using SpawnHouses.AdvStructures.AdvStructureParts;
using SpawnHouses.Types;

namespace SpawnHouses.AdvStructures.Generation.Components;

public static class RoomGen {
    /// <summary>
    ///     Fills mostly with random walls, but places main walls on bottom 2
    /// </summary>
    [ComponentGenerator(typeof(Room))]
    public class RoomGenerator2 : VolumeComponentGenerator {
        public new readonly HashSet<ComponentTag> PossibleTags = [
            ComponentTag.RoomHousingValid
        ];

        public override bool Generate(VolumeComponentParams param) {
            int bottomY = param.Component.Volume.BoundingBox.bottomRight.Y;
            param.Component.Volume.ExecuteInArea((x, y) => {
                if (y == bottomY || y == bottomY - 1 || y == bottomY - 2)
                    param.Tilemap.PlaceWall(x, y, param.Palette.LivingRoom.BottomBackgroundAccent);
                else
                    param.Tilemap.PlaceWall(x, y, param.Palette.LivingRoom.Primary);
            });

            return true;
        }
    }
}