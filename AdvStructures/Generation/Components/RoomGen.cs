using System.Collections.Generic;
using SpawnHouses.AdvStructures.AdvStructureParts;
using SpawnHouses.Types;

namespace SpawnHouses.AdvStructures.Generation.Components;

public static class RoomGen {
    /// <summary>
    ///     Fills mostly with random walls, but has specific walls on bottom edge
    /// </summary>
    [ComponentGenerator(typeof(Room))]
    public class BackgroundGenerator1 : VolumeComponentGenerator {
        public override HashSet<ComponentTag> GetPossibleTags() {
            return [
                ComponentTag.BackgroundIsHousingValid
            ];
        }

        public override bool Generate(VolumeComponentParams param) {
            param.Component.Volume.ExecuteInArea((x, y) => {
                if (y == param.Component.Volume.BoundingBox.bottomRight.Y)
                    param.Tilemap.PlaceWall(x, y, param.Palette.BackgroundRoomAlt);
                else if (y == param.Component.Volume.BoundingBox.bottomRight.Y - 1)
                    param.Tilemap.PlaceWall(x, y, param.Palette.BackgroundRoomAccent);
                else
                    param.Tilemap.PlaceWall(x, y, param.Palette.BackgroundRoomMain);
            });

            return true;
        }
    }

    /// <summary>
    ///     Fills mostly with random walls, but places main walls on bottom 3
    /// </summary>
    [ComponentGenerator(typeof(Room))]
    public class BackgroundGenerator2 : VolumeComponentGenerator {
        public override HashSet<ComponentTag> GetPossibleTags() {
            return [
                ComponentTag.BackgroundIsHousingValid
            ];
        }

        public override bool Generate(VolumeComponentParams param) {
            int bottomY = param.Component.Volume.BoundingBox.bottomRight.Y;
            param.Component.Volume.ExecuteInArea((x, y) => {
                if (y == bottomY || y == bottomY - 1 || y == bottomY - 2)
                    param.Tilemap.PlaceWall(x, y, param.Palette.BackgroundRoomAlt);
                else
                    param.Tilemap.PlaceWall(x, y, param.Palette.BackgroundRoomMain);
            });

            return true;
        }
    }
}