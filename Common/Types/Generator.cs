#nullable enable
using System.Collections.Generic;
using SpawnHouses.Common.Parameters;
using SpawnHouses.Common.Tagging;
using SpawnHouses.Common.Types.Geometry;

namespace SpawnHouses.Common.Types;

public interface IGenerator {
    /// <summary>
    ///     all tags that could be created by using this generator
    /// </summary>
    public HashSet<Tag> PossibleTags { get; }

    /// <summary>
    ///     if this generator is allowed to generate the given parameters
    /// </summary>
    /// <param name="param"></param>
    /// <returns></returns>
    public bool CanGenerate(object param);

    /// <summary>
    ///     get the shape that bounds the generation from <see cref="Generate" /> without actually generating anything
    /// </summary>
    /// <param name="param"></param>
    /// <returns></returns>
    public Shape GetBoundingShape(object param);

    /// <summary>
    ///     execute the generator with the given parameters
    /// </summary>
    /// <param name="param"></param>
    /// <returns></returns>
    public TagMap Generate(object param);
}

public interface IGenerator<in TParams> : IGenerator
    where TParams : IParams {
    bool IGenerator.CanGenerate(object param) => CanGenerate(param);

    Shape IGenerator.GetBoundingShape(object param) => GetBoundingShape(param);

    TagMap IGenerator.Generate(object param) => Generate(param);

    /// <inheritdoc cref="IGenerator.CanGenerate" />
    public bool CanGenerate(TParams param);

    /// <inheritdoc cref="IGenerator.GetBoundingShape" />
    public Shape GetBoundingShape(TParams param);

    /// <inheritdoc cref="IGenerator.Generate" />
    public TagMap Generate(TParams param);
}

public abstract class Generator<TParams> : IGenerator<TParams>
    where TParams : IParams {
    public abstract HashSet<Tag> PossibleTags { get; }

    public abstract bool CanGenerate(TParams param);

    public abstract Shape GetBoundingShape(TParams param);

    public abstract TagMap Generate(TParams param);
}

public abstract class StructureLayoutGenerator : Generator<StructureLayoutParams> {
    public abstract override bool CanGenerate(StructureLayoutParams param);

    public abstract override Shape GetBoundingShape(StructureLayoutParams param);

    public abstract override TagMap Generate(StructureLayoutParams param);
}

public abstract class VolumeComponentGenerator : Generator<VolumeComponentParams> {
    public abstract override bool CanGenerate(VolumeComponentParams param);

    public sealed override Shape GetBoundingShape(VolumeComponentParams param) => param.Geometry;

    public abstract override TagMap Generate(VolumeComponentParams param);
}

public abstract class PathComponentGenerator : Generator<PathComponentParams> {
    public abstract override bool CanGenerate(PathComponentParams param);

    public abstract override Shape GetBoundingShape(PathComponentParams param);

    public abstract override TagMap Generate(PathComponentParams param);
}