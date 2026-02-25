#nullable enable
using System.Collections.Generic;
using SpawnHouses.Common.Modules;
using SpawnHouses.Common.Modules.Components;
using SpawnHouses.Common.Palette;
using SpawnHouses.Common.Parameters;
using SpawnHouses.Common.Tagging;
using SpawnHouses.Common.Tiles;
using SpawnHouses.Common.Types;
using SpawnHouses.Helpers.Complex;
using Terraria.Utilities;

namespace SpawnHouses.Common.Generation.Components;

public static class RoomGen {
    /// <summary>
    ///     Fills in sections of 3-wide primary tiles, with accents and beams
    /// </summary>
    [ModuleGenerator(typeof(Room))]
    public class RoomGenerator2 : VolumeComponentGenerator {
        public override HashSet<Tag> PossibleTags { get; } = TagMap.NewTagSet(
            [
                Tags.RoomTypeBedroom,
                Tags.RoomHousingValid,
                Tags.RoomBeamsAreTiles,
                Tags.RoomBeamsAreWalls
            ],
            ComponentHelper.PlaceBeams.PossibleTags,
            ComponentHelper.FillShapeWalls.PossibleTags
        );

        public override bool Generate(VolumeComponent component, VolumeComponentParams param, UnifiedRandom random, TilePalette palette, StructureTilemap tilemap) {
            int endY = component.Geometry.BoundingBox.bottomRight.Y;
            int mainTopY = component.Geometry.BoundingBox.topLeft.Y;
            int mainBottomY = endY - 1;

            ComponentHelper.FillShapeWalls.Action(component.Geometry, param.Structure, (x, y) => {
                if (y == endY) return palette.BedroomRoom.BottomBackgroundAccent;
                if (y == mainTopY || y == mainBottomY) return palette.BedroomRoom.HorizontalBeamBackground;
                return palette.BedroomRoom.Primary;
            });

            ComponentHelper.PlaceBeams.Action(component.Geometry, component, 4, palette.BedroomRoom);

            return true;
        }
    }

    /// <summary>
    ///     Fills in sections of 3-wide primary tiles, with accents and beams
    /// </summary>
    [ModuleGenerator(typeof(Room))]
    public class RoomGenerator3 : VolumeComponentGenerator {
        public override HashSet<Tag> PossibleTags { get; } = TagMap.NewTagSet(
            [
                Tags.RoomTypeLiving,
                Tags.RoomHousingValid,
            ],
            ComponentHelper.FillShapeWalls.PossibleTags
        );

        public override bool Generate(VolumeComponent component, VolumeComponentParams param, UnifiedRandom random, TilePalette palette, StructureTilemap tilemap) {
            int endY = component.Geometry.BoundingBox.bottomRight.Y;
            int mainTopY = component.Geometry.BoundingBox.topLeft.Y;
            int mainBottomY = endY - 1;

            ComponentHelper.FillShapeWalls.Action(component.Geometry, param.Structure, (x, y) => {
                if (y == endY) return palette.LivingRoom.BottomBackgroundAccent;
                if (y == mainTopY || y == mainBottomY) return palette.LivingRoom.HorizontalBeamBackground;
                return palette.LivingRoom.Primary;
            });

            return true;
        }
    }
}