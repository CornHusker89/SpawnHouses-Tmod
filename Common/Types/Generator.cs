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
    /// <param name="generatable"></param>
    /// <returns></returns>
    public bool CanGenerate(IParams param, IGeneratable generatable);

    /// <summary>
    ///     execute the generator with the given parameters. should not be called directly, use <see cref="Generatable{TParams,TGenerator}.ExecuteGenerator"/>
    /// </summary>
    /// <param name="param"></param>
    /// <param name="generatable"></param>
    /// <returns></returns>
    public bool Generate(IParams param, IGeneratable generatable);
}

public interface IGenerator<TParams, TGeneratable> : IGenerator
    where TParams : IParams
    where TGeneratable : IGeneratable {
    bool IGenerator.CanGenerate(IParams param, IGeneratable generatable) => CanGenerate(param, generatable);

    bool IGenerator.Generate(IParams param, IGeneratable generatable) => Generate(param, generatable);

    /// <inheritdoc cref="IGenerator.CanGenerate" />
    public bool CanGenerate(TParams param, TGeneratable generatable);

    /// <inheritdoc cref="IGenerator.Generate" />
    public bool Generate(TParams param, TGeneratable generatable);
}

public abstract class Generator<TParams, TGeneratable> : IGenerator<TParams, TGeneratable>
    where TParams : IParams
    where TGeneratable : IGeneratable {
    public abstract HashSet<Tag> PossibleTags { get; }

    public abstract bool CanGenerate(TParams param, TGeneratable generatable);

    public abstract bool Generate(TParams param, TGeneratable generatable);
}

public abstract class StructureLayoutGenerator : Generator<StructureLayoutParams, StructureLayout> {
    public abstract override bool CanGenerate(StructureLayoutParams param, StructureLayout structureLayout);

    public abstract override bool Generate(StructureLayoutParams param, StructureLayout structureLayout);
}

public abstract class VolumeComponentGenerator : Generator<VolumeComponentParams, VolumeComponent> {
    public abstract override bool CanGenerate(VolumeComponentParams param, VolumeComponent component);

    public abstract override bool Generate(VolumeComponentParams param, VolumeComponent component);
}

public abstract class PathComponentGenerator : Generator<PathComponentParams, PathComponent> {
    public abstract override bool CanGenerate(PathComponentParams param, PathComponent component);

    public abstract override bool Generate(PathComponentParams param, PathComponent component);

    /// <summary>
    /// </summary>
    /// <param name="param"></param>
    /// <param name="geometry"></param>
    /// <param name="structureId">leave 0 for this structure's id</param>
    /// <returns></returns>
    public abstract Shape GetBoundingShape(PathComponentParams param, Path geometry, ushort structureId = 0);
}