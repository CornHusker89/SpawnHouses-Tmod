using System;
using SpawnHouses.Common.Modules.Components;
using SpawnHouses.Common.Types;
using SpawnHouses.Common.Types.Geometry;

namespace SpawnHouses.Common.Generation.Components;

public class StairwayGen {
    /// <summary>
    /// </summary>
    [InstanceGenerator(typeof(Stairway))]
    public class StairwayGenerator1 : PathComponentGenerator {
        public override HashSet<Tag> PossibleTags { get; } = ComponentTagSystem.NewTagSet(
            [
                Tags.StairwayToHorizontalGap
            ]
        );

        public override Shape GetBoundingShape(PathComponentParams param) => throw new NotImplementedException();

        public override bool Generate(PathComponentParams param) => true;
    }
}