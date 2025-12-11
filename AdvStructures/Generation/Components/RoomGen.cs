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
        public override HashSet<ComponentTag> PossibleTags { get; } = [
            ComponentTag.RoomHousingValid
        ];

        public override bool Generate(VolumeComponentParams param) {
            int bottomY = param.Component.Volume.BoundingBox.bottomRight.Y;
            param.Component.Volume.ExecuteInArea((x, y) => {
                param.Tilemap.PlaceWall(x, y,
                    y == bottomY ? param.Palette.LivingRoom.BottomBackgroundAccent : param.Palette.LivingRoom.Primary);
            });

            return true;
        }
    }
}