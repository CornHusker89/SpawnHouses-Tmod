using System;
using System.Collections.Generic;
using System.Linq;
using SpawnHouses.AdvStructures.AdvStructureParts;
using SpawnHouses.AdvStructures.Generation;
using SpawnHouses.Structures;
using SpawnHouses.Types;
using SpawnHouses.Types.TagTypes;
using Terraria;

namespace SpawnHouses.AdvStructures;

public interface IGeneratable : ITagSystem {
    /// <summary>
    ///     the generation parameters for this generatable object
    /// </summary>
    public Params Params { get; }

    /// <summary>
    ///     the executable generator for this component
    /// </summary>
    /// <remarks>set in <see cref="AdvStructure.ApplyLayoutMethod" /> during structure generation</remarks>
    public IGenerator Generator { get; }

    /// <summary>
    ///     the geometry that this generatable will occupy
    /// </summary>
    public PointGeometry Geometry { get; }

    /// <summary>
    ///     gets a random generator from the list of applicable generators for this generatable type
    /// </summary>
    /// <param name="generators"></param>
    /// <returns></returns>
    public IGenerator GetGenerator(List<IGenerator> generators);
}

public interface IGeneratable<TParams, TGeometry, TGenerator> : IGeneratable
    where TParams : Params
    where TGeometry : PointGeometry
    where TGenerator : Generator<TParams, TGeometry> {
    Params IGeneratable.Params => Params;

    /// <inheritdoc cref="IGeneratable.Params" />
    public new TParams Params { get; init; }

    IGenerator IGeneratable.Generator => Generator;

    /// <inheritdoc cref="IGeneratable.Generator" />
    public new TGenerator Generator { get; init; }

    PointGeometry IGeneratable.Geometry => Geometry;

    /// <inheritdoc cref="IGeneratable.Geometry" />
    public new TGeometry Geometry { get; init; }

    IGenerator IGeneratable.GetGenerator(List<IGenerator> generators) => Generator;

    /// <inheritdoc cref="IGeneratable.GetGenerator" />
    public TGenerator GetGenerator(List<TGenerator> generators);
}

public abstract class Generatable<TParams, TGeometry, TGenerator> : IGeneratable<TParams, TGeometry, TGenerator>
    where TParams : Params
    where TGeometry : PointGeometry
    where TGenerator : Generator<TParams, TGeometry> {
    public required TParams Params { get; init; }
    public required TagMap TagsRequired { get; init; }
    public required TagMap TagsCurrent { get; init; }

    public required TGenerator Generator { get; init; }
    public required TGeometry Geometry { get; init; }

    protected Generatable(TParams param, TagMap tagsRequired, TagMap tagsCurrent, TGeometry geometry) {
        Params = param;
        TagsRequired = tagsRequired;
        TagsCurrent = tagsCurrent;
        Geometry = geometry;
    }

    public TGenerator GetGenerator(List<TGenerator> generators) {
        TagsRequired.ValidateExclusiveRequiredTags();
        TagsCurrent.ValidateExclusiveCurrentTags();
        var validGenerators = generators.Where(gen => gen.CanGenerate(Params, Geometry)
                                                      && TagsRequired.KeysSet.IsSubsetOf(gen.PossibleTags))
            .ToArray();

        if (validGenerators.Length == 0)
            throw new Exception($"No component generators were found that are compatible with given parameters. type: {GetType().FullName}, required tags: {EnumHelper.ToString(TagsRequired.Keys)}");

        return Params.Structure.RandomGen.NextFromList(validGenerators);
    }
}