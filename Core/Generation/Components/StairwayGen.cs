using System;
using System.Collections.Generic;
using SpawnHouses.Core.AdvGeneratables;
using SpawnHouses.Core.AdvGeneratables.Components;
using SpawnHouses.Core.AdvStructureCore;
using SpawnHouses.Core.Attributes;
using SpawnHouses.Core.Geometry;
using SpawnHouses.Core.Palette;
using SpawnHouses.Core.Parameters;
using SpawnHouses.Core.Tagging;
using SpawnHouses.Core.Tiles;
using Terraria.Utilities;

namespace SpawnHouses.Core.Generation.Components;

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