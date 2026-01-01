using SpawnHouses.AdvStructures.AdvStructureParts;
using SpawnHouses.Common.Parameters;
using SpawnHouses.Common.Types;
using SpawnHouses.Common.Types.Geometry;
using SpawnHouses.Types.TagTypes;

namespace SpawnHouses.Common.Modules;

public interface IComponent : IGeneratable {
    /// <summary>
    ///     geometry that this component will occupy
    /// </summary>
    public PointGeometry Geometry { get; }
}

public interface IComponent<TGeometry> : IComponent
    where TGeometry : PointGeometry {
    PointGeometry IComponent.Geometry => Geometry;

    /// <inheritdoc cref="IComponent.Geometry" />
    public new TGeometry Geometry { get; init; }
}

public abstract class VolumeComponent : Generatable<VolumeComponentParams, VolumeComponentGenerator>, IComponent<Shape> {
    public Shape Geometry { get; init; }

    protected VolumeComponent(VolumeComponentParams param, Shape shape) : base(param, new TagMap()) {
        Geometry = shape;
    }
}

public abstract class PathComponent : Generatable<PathComponentParams, PathComponentGenerator>, IComponent<Path> {
    public Path Geometry { get; init; }

    protected PathComponent(PathComponentParams param, Path path) : base(param, new TagMap()) {
        Geometry = path;
    }
}