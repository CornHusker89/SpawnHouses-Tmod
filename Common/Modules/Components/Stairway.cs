using SpawnHouses.AdvStructures.AdvStructureParts;
using SpawnHouses.Common.Parameters;
using SpawnHouses.Common.Tagging;

namespace SpawnHouses.Common.Modules.Components;

public class Stairway : PathComponent {
    /// <summary>
    ///     constructor that requires params object
    /// </summary>
    /// <param name="p"></param>
    /// <param name="path"></param>
    /// <param name="placeTilesLowerX"></param>
    public Stairway(PathComponentParams p, Path path, bool placeTilesLowerX) : base(p, path) {
        TagsCurrent.Add(placeTilesLowerX ? Tags.StairwayTilesLowerX : Tags.StairwayTilesHigherX);
    }

    /// <summary>
    ///     constructor that automatically creates a new set of params
    /// </summary>
    /// <param name="structure"></param>
    /// <param name="path"></param>
    /// <param name="placeTilesLowerX"></param>
    public Stairway(AdvStructure structure, Path path, bool placeTilesLowerX) : base(new PathComponentParams(structure), path) {
        TagsCurrent.Add(placeTilesLowerX ? Tags.StairwayTilesLowerX : Tags.StairwayTilesHigherX);
    }
}