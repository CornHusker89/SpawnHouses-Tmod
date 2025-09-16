using System;
using SpawnHouses.Types;

namespace SpawnHouses.AdvStructures.Generation.Components;

public static class BackgroundGen {
    /// <summary>
    ///     Fills mostly with random walls, but has specific walls on bottom edge
    /// </summary>
    public class BackgroundGenerator1 : IComponentGenerator {
        public ComponentTag[] GetPossibleTags() {
            return [
                ComponentTag.BackgroundIsHousingValid
            ];
        }

        public bool Generate(ComponentParams componentParams) {
            componentParams.Component.Volume.ExecuteInArea((x, y) => {
                if (y == componentParams.Component.Volume.BoundingBox.bottomRight.Y)
                    componentParams.Tilemap.PlaceWall(x, y, PaintedType.PickRandom(componentParams.TilePalette.BackgroundRoomAlt));
                else if (y == componentParams.Component.Volume.BoundingBox.bottomRight.Y - 1)
                    componentParams.Tilemap.PlaceWall(x, y, componentParams.TilePalette.BackgroundRoomAccent);
                else
                    componentParams.Tilemap.PlaceWall(x, y, componentParams.TilePalette.BackgroundRoomMain);
            });

            return true;
        }
    }

    /// <summary>
    ///     Fills mostly with random walls, but places main walls on bottom 3
    /// </summary>
    public class BackgroundGenerator2 : IComponentGenerator {
        public ComponentTag[] GetPossibleTags() {
            return [
                ComponentTag.BackgroundIsHousingValid
            ];
        }

        public bool Generate(ComponentParams componentParams) {
            int bottomY = componentParams.Component.Volume.BoundingBox.bottomRight.Y;
            componentParams.Component.Volume.ExecuteInArea((x, y) => {
                if (y == bottomY || y == bottomY - 1 || y == bottomY - 2)
                    componentParams.Tilemap.PlaceWall(x, y, PaintedType.PickRandom(componentParams.TilePalette.BackgroundRoomAlt));
                else
                    componentParams.Tilemap.PlaceWall(x, y, componentParams.TilePalette.BackgroundRoomMain);
            });

            return true;
        }
    }
}