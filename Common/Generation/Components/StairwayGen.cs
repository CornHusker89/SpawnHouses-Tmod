using System;
using SpawnHouses.AdvStructures.AdvStructureParts;
using SpawnHouses.Common;
using SpawnHouses.Common.Types;
using SpawnHouses.Common.Types.Geometry;

namespace SpawnHouses.AdvStructures.Generation.Components;

public class StairwayGen {
    /// <summary>
    /// </summary>
    [InstanceGenerator(typeof(Stairway))]
    public class StairwayGenerator1 : PathComponentGenerator {
        public override ComponentTagPartialSet PossibleTags { get; } = ComponentTagSystem.NewTagSet(
            [
                ComponentTags.StairwayToHorizontalGap
            ]
        );

        public override Shape GetBoundingShape(PathComponentParams param) => throw new NotImplementedException();

        public override bool Generate(PathComponentParams param) => true;
    }
}