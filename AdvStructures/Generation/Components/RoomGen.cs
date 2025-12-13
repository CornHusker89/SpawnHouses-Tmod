#nullable enable
using System.Collections.Generic;
using System.Linq;
using SpawnHouses.AdvStructures.AdvStructureParts;
using SpawnHouses.Helpers;
using SpawnHouses.Types;

namespace SpawnHouses.AdvStructures.Generation.Components;

public static class RoomGen {
    /// <summary>
    ///     Fills in sections of 3-wide primary tiles, with accents and beams
    /// </summary>
    [ComponentGenerator(typeof(Room))]
    public class RoomGenerator2 : VolumeComponentGenerator {
        public override HashSet<ComponentTag> PossibleTags { get; } = ComponentTagSystem.NewTagSet(
            [
                ComponentTag.RoomTypeLiving,
                ComponentTag.RoomHousingValid,
                ComponentTag.RoomBeamsAreTiles
            ],
            ComponentHelper.CreateBeams.PossibleTags,
            ComponentHelper.FillShapeWalls.PossibleTags
        );

        public override bool Generate(VolumeComponentParams param) {
            Shape shape = param.Component.Volume;
            int endY = shape.BoundingBox.bottomRight.Y;
            int mainTopY = shape.BoundingBox.topLeft.Y;
            int mainBottomY = endY - 1;
            int[] beamXPositions = ComponentHelper.CreateBeams.Action(shape, param);

            ComponentHelper.FillShapeWalls.Action(shape, param, (x, y) => {
                if (y == endY) return param.Palette.LivingRoom.BottomBackgroundAccent;
                if (beamXPositions.Contains(x)) return param.Palette.LivingRoom.VerticalBeamBackground;
                if (y == mainTopY || y == mainBottomY) return param.Palette.LivingRoom.HorizontalBeamBackground;
                return param.Palette.LivingRoom.Primary;
            });

            return true;
        }
    }
}