using SpawnHouses.AdvStructures.AdvStructureParts;
using SpawnHouses.Common.Parameters;
using SpawnHouses.Common.Tagging;
using SpawnHouses.Common.Types;
using SpawnHouses.Common.Types.Geometry;

namespace SpawnHouses.Common.Modules;

public interface IComponent : IGeneratable {
    /// <summary>
    ///     geometry that this component will occupy. set by the shape in the params at the time of component generation
    /// </summary>
    public PointGeometry Geometry { get; }
}

public interface IComponent<out TGeometry> : IComponent
    where TGeometry : PointGeometry {
    PointGeometry IComponent.Geometry => Geometry;

    /// <inheritdoc cref="IComponent.Geometry" />
    public new TGeometry Geometry { get; }
}

public abstract class VolumeComponent : Generatable<VolumeComponentParams, VolumeComponentGenerator>, IComponent<Shape> {
    public Shape Geometry { get; set; }

    protected VolumeComponent(VolumeComponentParams param, Shape geometry) : base(param, new TagMap()) {
        Geometry = geometry;
    }
}

public abstract class PathComponent : Generatable<PathComponentParams, PathComponentGenerator>, IComponent<Path> {
    public Path Geometry { get; set; }

    protected PathComponent(PathComponentParams param, Path geometry) : base(param, new TagMap()) {
        Geometry = geometry;
    }
}