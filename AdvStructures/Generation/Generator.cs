#nullable enable
using System.Collections.Generic;
using SpawnHouses.AdvStructures.AdvStructureParts;
using SpawnHouses.Types;
using SpawnHouses.Types.TagTypes;

namespace SpawnHouses.AdvStructures.Generation;

public interface IGenerator {
    /// <summary>
    ///     all tags that could be created by using this generator
    /// </summary>
    public HashSet<Tag> PossibleTags { get; }

    /// <summary>
    ///     if this generator is allowed to generate the given parameters
    /// </summary>
    /// <param name="structureParams"></param>
    /// <param name="geometry"></param>
    /// <returns></returns>
    public bool CanGenerate(Params structureParams, PointGeometry geometry);

    /// <summary>
    ///     get the shape that bounds the generation from <see cref="Generate" /> without actually generating anything
    /// </summary>
    /// <param name="componentParams"></param>
    /// <param name="geometry"></param>
    /// <returns></returns>
    public Shape GetBoundingShape(Params componentParams, PointGeometry geometry);

    /// <summary>
    ///     execute the generator with the given parameters
    /// </summary>
    /// <param name="structureParams"></param>
    /// <param name="geometry"></param>
    /// <returns></returns>
    public TagMap Generate(Params structureParams, PointGeometry geometry);
}

public interface IGenerator<in TParams, in TGeometry> : IGenerator
    where TParams : Params
    where TGeometry : PointGeometry {
    bool IGenerator.CanGenerate(Params structureParams, PointGeometry geometry) => CanGenerate(structureParams, geometry);

    /// <inheritdoc cref="IGenerator.CanGenerate" />
    public bool CanGenerate(TParams param, TGeometry geometry);

    Shape IGenerator.GetBoundingShape(Params componentParams, PointGeometry geometry) => GetBoundingShape(componentParams, geometry);

    /// <inheritdoc cref="IGenerator.GetBoundingShape" />
    public Shape GetBoundingShape(TParams param, TGeometry geometry);

    TagMap IGenerator.Generate(Params structureParams, PointGeometry geometry) => Generate(structureParams, geometry);

    /// <inheritdoc cref="IGenerator.Generate" />
    public TagMap Generate(TParams structureParams, TGeometry geometry);
}

public abstract class Generator<TParams, TGeometry> : IGenerator<TParams, TGeometry>
    where TParams : Params
    where TGeometry : PointGeometry {
    public abstract HashSet<Tag> PossibleTags { get; }

    public abstract bool CanGenerate(TParams param, TGeometry geometry);

    public abstract Shape GetBoundingShape(TParams param, TGeometry geometry);

    public abstract TagMap Generate(TParams structureParams, TGeometry geometry);
}

public abstract class StructureLayoutGenerator : Generator<StructureParams, Path> {
    public abstract override bool CanGenerate(StructureParams param, Path geometry);

    public abstract override Shape GetBoundingShape(StructureParams param, Path geometry);

    public abstract override TagMap Generate(StructureParams componentParams, Path geometry);
}

public abstract class VolumeComponentGenerator : Generator<VolumeComponentParams, Shape> {
    public abstract override bool CanGenerate(VolumeComponentParams param, Shape geometry);

    public sealed override Shape GetBoundingShape(VolumeComponentParams param, Shape geometry) => geometry;

    public abstract override TagMap Generate(VolumeComponentParams componentParams, Shape geometry);
}

public abstract class PathComponentGenerator : Generator<PathComponentParams, Path> {
    public abstract override bool CanGenerate(PathComponentParams param, Path geometry);

    public abstract override Shape GetBoundingShape(PathComponentParams param, Path geometry);

    public abstract override TagMap Generate(PathComponentParams componentParams, Path geometry);
}