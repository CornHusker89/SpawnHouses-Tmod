#nullable enable
using System.Linq;
using SpawnHouses.AdvStructures.AdvStructureParts;
using SpawnHouses.Common;
using SpawnHouses.Common.Types;
using SpawnHouses.Common.Types.Geometry;
using SpawnHouses.Helpers;

namespace SpawnHouses.AdvStructures.Generation.Components;

public static class RoomGen {
    /// <summary>
    ///     Fills in sections of 3-wide primary tiles, with accents and beams
    /// </summary>
    [InstanceGenerator(typeof(Room))]
    public class RoomGenerator2 : VolumeComponentGenerator {
        public override ComponentTagPartialSet PossibleTags { get; } = ComponentTagSystem.NewPartialTagSet(
            [
                ComponentTags.RoomTypeLiving,
                ComponentTags.RoomHousingValid,
                ComponentTags.RoomBeamsAreTiles
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

            if (param.Component.Required.ContainsKey(ComponentTags.RoomBeamsAreTiles))
                param.Component.Volume.ExecuteInArea((x, y) => {
                    if (beamXPositions.Contains(x))
                        param.Tilemap.PlaceTile(x, y, param.Palette.LivingRoom.BeamTile, actuated: param.Palette.LivingRoom.BeamTileActuation);
                });

            return true;
        }
    }
}