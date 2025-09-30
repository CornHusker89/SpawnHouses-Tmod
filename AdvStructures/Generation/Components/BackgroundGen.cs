using System.Collections.Generic;
using SpawnHouses.Types;

namespace SpawnHouses.AdvStructures.Generation.Components;

public static class BackgroundGen {
    /// <summary>
    ///     Fills mostly with random walls, but has specific walls on bottom edge
    /// </summary>
    public class BackgroundGenerator1 : IVolumeComponentGenerator {
        public HashSet<ComponentTag> GetPossibleTags() {
            return [
                ComponentTag.BackgroundIsHousingValid
            ];
        }

        public bool Generate(VolumeComponentParams componentParams) {
            componentParams.Component.Volume.ExecuteInArea((x, y) => {
                if (y == componentParams.Component.Volume.BoundingBox.bottomRight.Y)
                    componentParams.Tilemap.PlaceWall(x, y, PaintedType.PickRandom(componentParams.Palette.BackgroundRoomAlt));
                else if (y == componentParams.Component.Volume.BoundingBox.bottomRight.Y - 1)
                    componentParams.Tilemap.PlaceWall(x, y, componentParams.Palette.BackgroundRoomAccent);
                else
                    componentParams.Tilemap.PlaceWall(x, y, componentParams.Palette.BackgroundRoomMain);
            });

            return true;
        }
    }

    /// <summary>
    ///     Fills mostly with random walls, but places main walls on bottom 3
    /// </summary>
    public class BackgroundGenerator2 : IVolumeComponentGenerator {
        public HashSet<ComponentTag> GetPossibleTags() {
            return [
                ComponentTag.BackgroundIsHousingValid
            ];
        }

        public bool Generate(VolumeComponentParams componentParams) {
            int bottomY = componentParams.Component.Volume.BoundingBox.bottomRight.Y;
            componentParams.Component.Volume.ExecuteInArea((x, y) => {
                if (y == bottomY || y == bottomY - 1 || y == bottomY - 2)
                    componentParams.Tilemap.PlaceWall(x, y, PaintedType.PickRandom(componentParams.Palette.BackgroundRoomAlt));
                else
                    componentParams.Tilemap.PlaceWall(x, y, componentParams.Palette.BackgroundRoomMain);
            });

            return true;
        }
    }
}