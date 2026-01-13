using SpawnHouses.AdvStructures.AdvStructureParts;
using SpawnHouses.Common.Tagging;
using SpawnHouses.Common.Types.Geometry;

namespace SpawnHouses.Common.Parameters;

public interface IComponentParams : IParams {
    /// <summary>
    ///     geometry that this component occupies
    /// </summary>
    public PointGeometry Geometry { get; }
}

public interface IComponentParams<TGeometry> : IComponentParams
    where TGeometry : PointGeometry {
    PointGeometry IComponentParams.Geometry => Geometry;

    /// <inheritdoc cref="IComponentParams.Geometry" />
    public new TGeometry Geometry { get; init; }
}

public class VolumeComponentParams : IComponentParams<Shape> {
    public AdvStructure Structure { get; init; }
    public TagMap TagsRequired { get; init; }
    public Shape Geometry { get; init; }

    public VolumeComponentParams(AdvStructure structure, Shape shape, TagMap tagsRequired = null) {
        Structure = structure;
        TagsRequired = tagsRequired ?? new TagMap();
        Geometry = shape;
    }
}

public class PathComponentParams : IComponentParams<Path> {
    public AdvStructure Structure { get; init; }
    public TagMap TagsRequired { get; init; }
    public Path Geometry { get; init; }

    public PathComponentParams(AdvStructure structure, Path path, TagMap tagsRequired = null) {
        Structure = structure;
        TagsRequired = tagsRequired ?? new TagMap();
        Geometry = path;
    }
}