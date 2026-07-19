using System;
using System.Collections.Generic;
using SpawnHouses.StructureCommon.Modules.Components;
using SpawnHouses.StructureCommon.Palette;
using SpawnHouses.StructureCommon.Parameters;
using SpawnHouses.StructureCommon.Tagging;
using SpawnHouses.StructureCommon.Tiles;
using SpawnHouses.StructureCommon.Types;
using SpawnHouses.StructureCommon.Types.Attributes;
using SpawnHouses.StructureCommon.Types.Geometry;
using SpawnHouses.StructureCommon.Types.Interfaces;
using Terraria.Utilities;

namespace SpawnHouses.StructureCommon.Generation.Components;

public class StairwayGen {
    /// <summary>
    /// </summary>
    [AdvGeneratorLoadable(typeof(Stairway), false)]
    public class StairwayGenerator1 : PathComponentAdvGenerator {
        public override HashSet<Tag> PossibleTags { get; } = TagMap.NewTagSet(
            [
                Tags.StairwayToHorizontalGap
            ]
        );

        public override Shape GetBoundingShape(PathComponentParams param, Path geometry, UnifiedRandom random) => throw new NotImplementedException();

        public override bool Generate(PathComponent component, PathComponentParams param, UnifiedRandom random, TilePalette palette, StructureTilemap tilemap) => true;
    }
}