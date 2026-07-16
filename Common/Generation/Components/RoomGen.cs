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
    public class RoomGenerator2 : VolumeComponentAdvGenerator {
        public override HashSet<Tag> PossibleTags { get; } = TagMap.NewTagSet(
            [
                Tags.RoomTypeBedroom
            ],
            ComponentHelper.PlaceBeams.PossibleTags,
            ComponentHelper.FillShapeWalls.PossibleTags,
            ComponentHelper.FurnishRooms.PossibleTags
        );

        public override bool CanGenerate(VolumeComponent component, VolumeComponentParams param, UnifiedRandom random) => param.TagsRequired.HasTag(Tags.RoomTypeBedroom);

        public override bool Generate(VolumeComponent component, VolumeComponentParams param, UnifiedRandom random, TilePalette palette, StructureTilemap tilemap) {
            int endY = component.Geometry.BoundingBox.Bottom;
            int mainTopY = component.Geometry.BoundingBox.Top;
            int mainBottomY = endY - 1;

            ComponentHelper.FillShapeWalls.Action(component.Geometry, param.Structure, (_, y) => {
                if (y == endY) return palette.BedroomRoom.BottomBackgroundAccent;
                if (y == mainTopY || y == mainBottomY) return palette.BedroomRoom.HorizontalBeamBackground;
                return palette.BedroomRoom.Primary;
            });

            component.TagsCurrent.Add(Tags.RoomTypeBedroom);

            if (param.TagsRequired.HasTag(Tags.RoomHasBeams))
                ComponentHelper.PlaceBeams.Action(component.Geometry, component, 4, palette.BedroomRoom);
            ComponentHelper.FurnishRooms.Action((Room)component);
            return true;
        }
    }

    /// <summary>
    ///     Fills in sections of 3-wide primary tiles, with accents and beams
    /// </summary>
    [ModuleGenerator(typeof(Room))]
    public class RoomGenerator3 : VolumeComponentAdvGenerator {
        public override HashSet<Tag> PossibleTags { get; } = TagMap.NewTagSet(
            [
                Tags.RoomTypeLiving,
            ],
            ComponentHelper.FillShapeWalls.PossibleTags,
            ComponentHelper.FurnishRooms.PossibleTags
        );

        public override bool CanGenerate(VolumeComponent component, VolumeComponentParams param, UnifiedRandom random) => param.TagsRequired.HasTag(Tags.RoomTypeLiving);

        public override bool Generate(VolumeComponent component, VolumeComponentParams param, UnifiedRandom random, TilePalette palette, StructureTilemap tilemap) {
            int endY = component.Geometry.BoundingBox.Bottom;
            int mainTopY = component.Geometry.BoundingBox.Top;
            int mainBottomY = endY - 1;

            component.TagsCurrent.Add(Tags.RoomTypeLiving);
            
            ComponentHelper.FillShapeWalls.Action(component.Geometry, param.Structure, (x, y) => {
                if (y == endY) return palette.LivingRoom.BottomBackgroundAccent;
                if (y == mainTopY || y == mainBottomY) return palette.LivingRoom.HorizontalBeamBackground;
                return palette.LivingRoom.Primary;
            });
            ComponentHelper.FurnishRooms.Action((Room)component);
            return true;
        }
    }
}