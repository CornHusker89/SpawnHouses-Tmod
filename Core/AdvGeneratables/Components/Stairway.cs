using SpawnHouses.Core.Geometry;
using SpawnHouses.Core.Interfaces;
using SpawnHouses.Core.Parameters;
using SpawnHouses.Core.RootStructureTypes;
using SpawnHouses.Core.Tagging;

namespace SpawnHouses.Core.AdvGeneratables.Components;

public class Stairway : PathComponent {
    /// <summary>
    ///     constructor that requires params object
    /// </summary>
    /// <param name="p"></param>
    /// <param name="path"></param>
    /// <param name="placeTilesLowerX"></param>
    /// <param name="name"></param>
    public Stairway(PathComponentParams p, Path path, bool placeTilesLowerX, string name) : base(p, path, name) {
        p.TagsRequired.Add(placeTilesLowerX ? Tags.Stairway_TilesLowerX : Tags.Stairway_TilesHigherX);
    }

    /// <summary>
    ///     constructor that automatically creates a new set of params
    /// </summary>
    /// <param name="structure"></param>
    /// <param name="path"></param>
    /// <param name="placeTilesLowerX"></param>
    /// <param name="name"></param>
    public Stairway(AdvStructure structure, Path path, bool placeTilesLowerX, string name) : base(new PathComponentParams(structure), path, name) {
        Params.TagsRequired.Add(placeTilesLowerX ? Tags.Stairway_TilesLowerX : Tags.Stairway_TilesHigherX);
    }
}