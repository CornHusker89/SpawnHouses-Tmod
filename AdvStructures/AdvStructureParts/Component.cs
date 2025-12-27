using SpawnHouses.AdvStructures.Generation;
using SpawnHouses.Types;
using SpawnHouses.Types.TagTypes;

namespace SpawnHouses.AdvStructures.AdvStructureParts;

public interface IComponent : IGeneratable {
    /// <summary>
    ///     unique identifier given to each component in a structure. a value of 0 represents unassigned
    ///     get id with (componentGenerator.GetType().FullName ?? componentGenerator.GetType().Name).GetHashCode();
    /// </summary>
    /// <remarks>set in <see cref="AdvStructure.ApplyLayoutMethod" /> during structure generation</remarks>
    public ushort Id { get; init; }
}

public abstract class VolumeComponent : Generatable<VolumeComponentParams, Shape, VolumeComponentGenerator>, IComponent {
    public ushort Id { get; init; }

    protected VolumeComponent(VolumeComponentParams param, TagMap tagsRequired, TagMap tagsCurrent, Shape shape) : base(param, tagsRequired, tagsCurrent, shape) {
    }
}

public abstract class PathComponent : Generatable<PathComponentParams, Path, PathComponentGenerator>, IComponent {
    public ushort Id { get; init; }

    protected PathComponent(PathComponentParams param, TagMap tagsRequired, TagMap tagsCurrent, Path path) : base(param, tagsRequired, tagsCurrent, path) {
    }
}