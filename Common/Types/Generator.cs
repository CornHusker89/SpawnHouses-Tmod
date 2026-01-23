#nullable enable
using System.Collections.Generic;
using SpawnHouses.AdvStructures.AdvStructureParts;
using SpawnHouses.Common.Modules;
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
    ///     execute the generator with the given parameters
    /// </summary>
    /// <param name="param"></param>
    /// <returns></returns>
    public IGeneratable Generate(object param);
}

public interface IGenerator<in TParams, out TGeneratable> : IGenerator
    where TParams : IParams
    where TGeneratable : IGeneratable {
    bool IGenerator.CanGenerate(object param) => CanGenerate(param);

    IGeneratable IGenerator.Generate(object param) => Generate(param);

    /// <inheritdoc cref="IGenerator.CanGenerate" />
    public bool CanGenerate(TParams param);

    /// <inheritdoc cref="IGenerator.Generate" />
    public TGeneratable Generate(TParams param);
}

public abstract class Generator<TParams, TGeneratable> : IGenerator<TParams, TGeneratable>
    where TParams : IParams
    where TGeneratable : IGeneratable {
    public abstract HashSet<Tag> PossibleTags { get; }

    public abstract bool CanGenerate(TParams param);

    public abstract TGeneratable Generate(TParams param);
}

public abstract class StructureLayoutGenerator : Generator<StructureLayoutParams, StructureLayout> {
    public abstract override bool CanGenerate(StructureLayoutParams param);

    public abstract override StructureLayout Generate(StructureLayoutParams param);
}

public abstract class VolumeComponentGenerator : Generator<VolumeComponentParams, VolumeComponent> {
    public abstract override bool CanGenerate(VolumeComponentParams param);

    public abstract override VolumeComponent Generate(VolumeComponentParams param);
}

public abstract class PathComponentGenerator : Generator<PathComponentParams, PathComponent> {
    public abstract override bool CanGenerate(PathComponentParams param);

    public abstract override PathComponent Generate(PathComponentParams param);

    /// <summary>
    /// </summary>
    /// <param name="param"></param>
    /// <param name="geometry"></param>
    /// <param name="structureId">leave 0 for this structure's id</param>
    /// <returns></returns>
    public abstract Shape GetBoundingShape(PathComponentParams param, Path geometry, ushort structureId = 0);
}