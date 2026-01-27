#nullable enable
using System.Collections.Generic;
using System.Linq;
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
                Tags.RoomTypeLiving,
                Tags.RoomHousingValid,
                Tags.RoomBeamsAreTiles
            ],
            ComponentHelper.CreateBeams.PossibleTags,
            ComponentHelper.FillShapeWalls.PossibleTags
        );

        public override bool Generate(VolumeComponent component, VolumeComponentParams param, UnifiedRandom random, TilePalette palette, StructureTilemap tilemap) {
            int endY = component.Geometry.BoundingBox.bottomRight.Y;
            int mainTopY = component.Geometry.BoundingBox.topLeft.Y;
            int mainBottomY = endY - 1;
            int[] beamXPositions = ComponentHelper.CreateBeams.Action(component.Geometry, component);

            ComponentHelper.FillShapeWalls.Action(component.Geometry, param.Structure, (x, y) => {
                if (y == endY) return palette.LivingRoom.BottomBackgroundAccent;
                if (beamXPositions.Contains(x)) return palette.LivingRoom.VerticalBeamBackground;
                if (y == mainTopY || y == mainBottomY) return palette.LivingRoom.HorizontalBeamBackground;
                return palette.LivingRoom.Primary;
            });

            if (param.TagsRequired.HasTag(Tags.RoomBeamsAreTiles))
                component.Geometry.ExecuteInArea((x, y) => {
                    if (beamXPositions.Contains(x))
                        tilemap.PlaceTile(x, y, palette.LivingRoom.BeamTile, actuated: palette.LivingRoom.BeamTileActuation);
                });

            return true;
        }
    }
}