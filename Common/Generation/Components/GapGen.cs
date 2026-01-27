using System.Collections.Generic;
using SpawnHouses.Common.Modules;
using SpawnHouses.Common.Modules.Components;
using SpawnHouses.Common.Palette;
using SpawnHouses.Common.Parameters;
using SpawnHouses.Common.Tagging;
using SpawnHouses.Common.Tiles;
using SpawnHouses.Common.Types;
using Terraria.DataStructures;
using Terraria.Utilities;

namespace SpawnHouses.Common.Generation.Components;

public class GapGen {
    /// <summary>
    ///     A gap floor, places platforms and places walls if external
    /// </summary>
    [ModuleGenerator(typeof(Gap))]
    public class FloorGapGenerator1 : VolumeComponentGenerator {
        public override HashSet<Tag> PossibleTags { get; } = TagMap.NewTagSet(
            [
                Tags.External
            ]
        );

        public override bool CanGenerate(VolumeComponent component, VolumeComponentParams param, UnifiedRandom random) => !((Gap)component).IsHorizontal;

        public override bool Generate(VolumeComponent component, VolumeComponentParams param, UnifiedRandom random, TilePalette palette, StructureTilemap tilemap) {
            int xStart = component.Geometry.BoundingBox.topLeft.X;
            int[] topY = new int[component.Geometry.Size.X];
            int[] bottomY = new int[component.Geometry.Size.X];
            bool external = !param.TagsRequired.HasTag(Tags.External);

            component.Geometry.ExecuteInArea((x, y) => {
                tilemap[x, y].ClearTile(false);
                if (!external)
                    tilemap.PlaceWall(x, y, palette.InternalFloor.PrimaryBackground);
                StructureTile tile = tilemap[x, y];
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
                tilemap.PlaceTile(xStart + index, topY[index], (external ? palette.ExternalFloor : palette.InternalFloor).Platform);
                tilemap.PlaceTile(xStart + index, bottomY[index], (external ? palette.ExternalFloor : palette.InternalFloor).Platform);
            }

            return true;
        }
    }

    /// <summary>
    ///     gap wall, places door and places walls if external
    /// </summary>
    [ModuleGenerator(typeof(Gap))]
    public class WallGapGenerator1 : VolumeComponentGenerator {
        public override HashSet<Tag> PossibleTags { get; } = TagMap.NewTagSet(
            [
                Tags.External
            ]
        );

        public override bool CanGenerate(VolumeComponent component, VolumeComponentParams param, UnifiedRandom random) {
            (int min, int max, _) = component.Geometry.GetDetailedAxisSizes(false);
            return min == 3 && max == 3 && ((Gap)component).IsHorizontal;
        }

        public override bool Generate(VolumeComponent component, VolumeComponentParams param, UnifiedRandom random, TilePalette palette, StructureTilemap tilemap) {
            bool external = param.TagsRequired.HasTag(Tags.External);
            if (!external)
                component.Geometry.ExecuteInArea((x, y) => {
                    tilemap.PlaceWall(x, y, palette.InternalWall.PrimaryBackground);
                    tilemap[x, y].ClearTile(false);
                });
            tilemap.PlaceMultiTile(component.Geometry.BoundingBox.topLeft, new Point16(1, 3),
                (external ? palette.ExternalWall : palette.InternalWall).Door, false, MultiTile.DoorOrigin);
            return true;
        }
    }
}