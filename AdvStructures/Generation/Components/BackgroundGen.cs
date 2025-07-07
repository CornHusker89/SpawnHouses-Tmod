using SpawnHouses.Types;

namespace SpawnHouses.AdvStructures.Generation.Components;

public static class BackgroundGen {
    /// <summary>
    ///     Fills mostly with random walls, but has specific walls on bottom edge
    /// </summary>
    public class BackgroundGenerator1 : IComponentGenerator {
        public ComponentTag[] GetPossibleTags() {
            return [
                ComponentTag.IsBackground
            ];
        }

        public bool Generate(ComponentParams componentParams) {
            componentParams.Volume.ExecuteInArea((x, y) => {
                if (y == componentParams.Volume.BoundingBox.bottomRight.Y)
                    PaintedType.PlaceWall(x, y, PaintedType.PickRandom(componentParams.TilePalette.BackgroundRoomAlt), componentParams.Tilemap);
                else if (y == componentParams.Volume.BoundingBox.bottomRight.Y - 1)
                    PaintedType.PlaceWall(x, y, componentParams.TilePalette.BackgroundRoomAccent, componentParams.Tilemap);
                else
                    PaintedType.PlaceWall(x, y, componentParams.TilePalette.BackgroundRoomMain, componentParams.Tilemap);
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
                ComponentTag.IsBackground
            ];
        }

        public bool Generate(ComponentParams componentParams) {
            int bottomY = componentParams.Volume.BoundingBox.bottomRight.Y;
            componentParams.Volume.ExecuteInArea((x, y) => {
                if (y == bottomY || y == bottomY - 1 || y == bottomY - 2)
                    PaintedType.PlaceWall(x, y, PaintedType.PickRandom(componentParams.TilePalette.BackgroundRoomAlt), componentParams.Tilemap);
                else
                    PaintedType.PlaceWall(x, y, componentParams.TilePalette.BackgroundRoomMain, componentParams.Tilemap);
            });

            return true;
        }
    }
}