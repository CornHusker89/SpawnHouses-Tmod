#nullable enable
using System.Collections.Generic;
using SpawnHouses.AdvStructures.AdvStructureParts;
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
    /// <param name="param"></param>
    /// <param name="geometry"></param>
    /// <returns></returns>
    public bool CanGenerate(object param, PointGeometry geometry);

    /// <summary>
    ///     get the shape that bounds the generation from <see cref="Generate" /> without actually generating anything
    /// </summary>
    /// <param name="param"></param>
    /// <param name="geometry"></param>
    /// <returns></returns>
    public Shape GetBoundingShape(object param, PointGeometry geometry);

    /// <summary>
    ///     execute the generator with the given parameters
    /// </summary>
    /// <param name="param"></param>
    /// <param name="geometry"></param>
    /// <returns></returns>
    public TagMap Generate(object param, PointGeometry geometry);
}

public interface IGenerator<in TParams, in TGeometry> : IGenerator
    where TGeometry : PointGeometry {
    bool IGenerator.CanGenerate(object param, PointGeometry geometry) => CanGenerate(param, geometry);

    Shape IGenerator.GetBoundingShape(object param, PointGeometry geometry) => GetBoundingShape(param, geometry);

    TagMap IGenerator.Generate(object param, PointGeometry geometry) => Generate(param, geometry);

    /// <inheritdoc cref="IGenerator.CanGenerate" />
    public bool CanGenerate(TParams param, TGeometry geometry);

    /// <inheritdoc cref="IGenerator.GetBoundingShape" />
    public Shape GetBoundingShape(TParams param, TGeometry geometry);

    /// <inheritdoc cref="IGenerator.Generate" />
    public TagMap Generate(TParams structureParams, TGeometry geometry);
}

public abstract class Generator<TParams, TGeometry> : IGenerator<TParams, TGeometry>
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

public abstract class VolumeComponentGenerator : Generator<AdvStructure, Shape> {
    public abstract override bool CanGenerate(AdvStructure structure, Shape geometry);

    public sealed override Shape GetBoundingShape(AdvStructure structure, Shape geometry) => geometry;

    public abstract override TagMap Generate(AdvStructure structure, Shape geometry);
}

public abstract class PathComponentGenerator : Generator<AdvStructure, Path> {
    public abstract override bool CanGenerate(AdvStructure structure, Path geometry);

    public abstract override Shape GetBoundingShape(AdvStructure structure, Path geometry);

    public abstract override TagMap Generate(AdvStructure structure, Path geometry);
}