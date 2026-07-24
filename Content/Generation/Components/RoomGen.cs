#nullable enable
using System.Collections.Generic;
using SpawnHouses.Content.Modules.Components;
using SpawnHouses.Content.Palette;
using SpawnHouses.Content.Parameters;
using SpawnHouses.Content.Tagging;
using SpawnHouses.Content.Tiles;
using SpawnHouses.Content.Types;
using SpawnHouses.Content.Types.Attributes;
using SpawnHouses.Content.Types.Interfaces;
using SpawnHouses.Helpers.Complex;
using Terraria.Utilities;

namespace SpawnHouses.Content.Generation.Components;

public static class RoomGen {
    /// <summary>
    ///     Fills in sections of 3-wide primary tiles, with accents and beams
    /// </summary>
    [AdvGeneratorLoadable(typeof(Room), false)]
    public class RoomGenerator2 : VolumeComponentAdvGenerator {
        public override HashSet<Tag> PossibleTags { get; } = TagMap.NewTagSet(
            [
                Tags.Room_TypeBedroom
            ],
            ComponentHelper.PlaceBeams.PossibleTags,
            ComponentHelper.FillShapeWalls.PossibleTags,
            ComponentHelper.FurnishRooms.PossibleTags
        );

        public override bool CanGenerate(VolumeComponent component, VolumeComponentParams param, UnifiedRandom random) => param.TagsRequired.HasTag(Tags.Room_TypeBedroom);

        public override bool Generate(VolumeComponent component, VolumeComponentParams param, UnifiedRandom random, TilePalette palette, StructureTilemap tilemap) {
            int endY = component.Geometry.BoundingBox.Bottom;
            int mainTopY = component.Geometry.BoundingBox.Top;
            int mainBottomY = endY - 1;

            ComponentHelper.FillShapeWalls.Action(component.Geometry, param.Structure, (_, y) => {
                if (y == endY) return palette.BedroomRoom.BottomBackgroundAccent;
                if (y == mainTopY || y == mainBottomY) return palette.BedroomRoom.HorizontalBeamBackground;
                return palette.BedroomRoom.Primary;
            });

            component.TagsCurrent.Add(Tags.Room_TypeBedroom);

            if (param.TagsRequired.HasTag(Tags.Room_HasBeams))
                ComponentHelper.PlaceBeams.Action(component.Geometry, component, 4, palette.BedroomRoom);
            ComponentHelper.FurnishRooms.Action((Room)component);
            return true;
        }
    }

    /// <summary>
    ///     Fills in sections of 3-wide primary tiles, with accents and beams
    /// </summary>
    [AdvGeneratorLoadable(typeof(Room), false)]
    public class RoomGenerator3 : VolumeComponentAdvGenerator {
        public override HashSet<Tag> PossibleTags { get; } = TagMap.NewTagSet(
            [
                Tags.Room_TypeLiving
            ],
            ComponentHelper.FillShapeWalls.PossibleTags,
            ComponentHelper.FurnishRooms.PossibleTags
        );

        public override bool CanGenerate(VolumeComponent component, VolumeComponentParams param, UnifiedRandom random) => param.TagsRequired.HasTag(Tags.Room_TypeLiving);

        public override bool Generate(VolumeComponent component, VolumeComponentParams param, UnifiedRandom random, TilePalette palette, StructureTilemap tilemap) {
            int endY = component.Geometry.BoundingBox.Bottom;
            int mainTopY = component.Geometry.BoundingBox.Top;
            int mainBottomY = endY - 1;

            component.TagsCurrent.Add(Tags.Room_TypeLiving);
            
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