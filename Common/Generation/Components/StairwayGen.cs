using System;
using System.Collections.Generic;
using SpawnHouses.Common.Modules;
using SpawnHouses.Common.Modules.Components;
using SpawnHouses.Common.Palette;
using SpawnHouses.Common.Parameters;
using SpawnHouses.Common.Tagging;
using SpawnHouses.Common.Tiles;
using SpawnHouses.Common.Types;
using SpawnHouses.Common.Types.Geometry;
using Terraria.Utilities;

namespace SpawnHouses.Common.Generation.Components;

public class StairwayGen {
    /// <summary>
    /// </summary>
    [ModuleGenerator(typeof(Stairway))]
    public class StairwayGenerator1 : PathComponentGenerator {
        public override HashSet<Tag> PossibleTags { get; } = TagMap.NewTagSet(
            [
                Tags.StairwayToHorizontalGap
            ]
        );

        public override Shape GetBoundingShape(PathComponentParams param, Path geometry, UnifiedRandom random) => throw new NotImplementedException();

        public override bool Generate(PathComponent component, PathComponentParams param, UnifiedRandom random, TilePalette palette, StructureTilemap tilemap) => true;
    }
}