using SpawnHouses.AdvStructures.Generation;
using SpawnHouses.Types;
using SpawnHouses.Types.TagTypes;

namespace SpawnHouses.AdvStructures.AdvStructureParts;

public interface IComponent : IGeneratable {
}

public abstract class VolumeComponent : Generatable<VolumeComponentParams, Shape, VolumeComponentGenerator>, IComponent {
    protected VolumeComponent(VolumeComponentParams param, TagMap tagsRequired, TagMap tagsCurrent, Shape shape) : base(param, tagsRequired, tagsCurrent, shape) {
        Generator = Params.Structure.GetGeneratorForComponent<VolumeComponentGenerator>(this);
    }
}

public abstract class PathComponent : Generatable<PathComponentParams, Path, PathComponentGenerator>, IComponent {
    protected PathComponent(PathComponentParams param, TagMap tagsRequired, TagMap tagsCurrent, Path path) : base(param, tagsRequired, tagsCurrent, path) {
        Generator = Params.Structure.GetGeneratorForComponent<PathComponentGenerator>(this);
    }
}