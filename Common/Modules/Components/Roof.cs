#nullable enable
using SpawnHouses.Common.Parameters;
using SpawnHouses.Common.Tagging;
using SpawnHouses.Common.Types.Geometry;

namespace SpawnHouses.Common.Modules.Components;

/// <summary>
///     all roofs' required tags has Tags.External in it on object init
/// </summary>
public class Roof : PathComponent {
    /// <summary>
    ///     constructor that requires params object
    /// </summary>
    /// <param name="p"></param>
    /// <param name="path"></param>
    public Roof(PathComponentParams p, Path path) : base(p, path) {
        p.TagsRequired.Add(Tags.External);
    }

    /// <summary>
    ///     constructor that automatically creates a new set of params
    /// </summary>
    /// <param name="structure"></param>
    /// <param name="path"></param>
    public Roof(AdvStructure structure, Path path) : base(new PathComponentParams(structure), path) {
        Params.TagsRequired.Add(Tags.External);
    }
}