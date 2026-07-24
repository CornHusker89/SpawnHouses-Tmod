using System;
using System.Collections.Generic;
using SpawnHouses.Content.Modules.Components;
using SpawnHouses.Content.Palette;
using SpawnHouses.Content.Parameters;
using SpawnHouses.Content.Tagging;
using SpawnHouses.Content.Tiles;
using SpawnHouses.Content.Types;
using SpawnHouses.Content.Types.Attributes;
using SpawnHouses.Content.Types.Geometry;
using SpawnHouses.Content.Types.Interfaces;
using Terraria.Utilities;

namespace SpawnHouses.Content.Generation.Components;

public class StairwayGen {
    /// <summary>
    /// </summary>
    [AdvGeneratorLoadable(typeof(Stairway), false)]
    public class StairwayGenerator1 : PathComponentAdvGenerator {
        public override HashSet<Tag> PossibleTags { get; } = TagMap.NewTagSet(
            [
                Tags.Stairway_ToHorizontalGap
            ]
        );

        public override Shape GetBoundingShape(PathComponentParams param, Path geometry, UnifiedRandom random) => throw new NotImplementedException();

        public override bool Generate(PathComponent component, PathComponentParams param, UnifiedRandom random, TilePalette palette, StructureTilemap tilemap) => true;
    }
}