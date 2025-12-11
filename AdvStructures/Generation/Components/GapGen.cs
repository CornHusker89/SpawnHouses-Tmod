using System.Collections.Generic;
using SpawnHouses.AdvStructures.AdvStructureParts;
using SpawnHouses.Types;
using Terraria.DataStructures;

namespace SpawnHouses.AdvStructures.Generation.Components;

public class GapGen {
    /// <summary>
    ///     A gap floor, places platforms and places walls if external
    /// </summary>
    [ComponentGenerator(typeof(Gap))]
    public class FloorGapGenerator1 : VolumeComponentGenerator {
        public override HashSet<ComponentTag> PossibleTags { get; } = [
            ComponentTag.IsFloorGap,
            ComponentTag.External
        ];

        public override bool Generate(VolumeComponentParams param) {
            int xStart = param.Component.Volume.BoundingBox.topLeft.X;
            int[] topY = new int[param.Component.Volume.Size.X];
            int[] bottomY = new int[param.Component.Volume.Size.X];
            bool external = !param.Component.TagsRequired.ContainsKey(ComponentTag.External);

            param.Component.Volume.ExecuteInArea((x, y) => {
                param.Tilemap[x, y].ClearTile(false);
                if (!external)
                    param.Tilemap.PlaceWall(x, y, param.Palette.InternalFloor.PrimaryBackground);
                StructureTile tile = param.Tilemap[x, y];
                tile.HasTile = false;

                if (topY[x - xStart] == 0)
                    topY[x - xStart] = y;
                if (bottomY[x - xStart] == 0)
                    bottomY[x - xStart] = y;

                if (y < topY[x - xStart])
                    topY[x - xStart] = y;
                if (y > bottomY[x - xStart])
                    bottomY[x - xStart] = y;
            });

            for (int index = 0; index < topY.Length; index++) {
                param.Tilemap.PlaceTile(xStart + index, topY[index], param.Palette.LivingRoom.Platform);
                param.Tilemap.PlaceTile(xStart + index, bottomY[index], param.Palette.LivingRoom.Platform);
            }

            return true;
        }
    }

    /// <summary>
    ///     gap wall, places door an places walls if external
    /// </summary>
    [ComponentGenerator(typeof(Gap))]
    public class WallGapGenerator1 : VolumeComponentGenerator {
        public override HashSet<ComponentTag> PossibleTags { get; } = [
            ComponentTag.IsWallGap,
            ComponentTag.External
        ];

        public override bool CanGenerate(VolumeComponentParams componentParams) {
            (int min, int max, double average) = componentParams.Component.Volume.GetDetailedAxisSizes(false);
            return min == 3 && max == 3;
        }

        public override bool Generate(VolumeComponentParams param) {
            if (!param.Component.TagsRequired.ContainsKey(ComponentTag.External))
                param.Component.Volume.ExecuteInArea((x, y) => {
                    param.Tilemap.PlaceWall(x, y, param.Palette.InternalWall.PrimaryBackground);
                    param.Tilemap[x, y].ClearTile(false);
                });
            param.Tilemap.PlaceMultiTile(param.Component.Volume.BoundingBox.topLeft, new Point16(1, 3),
                param.Palette.LivingRoom.Door, false, MultiTile.DoorOrigin);
            return true;
        }
    }
}