using System;
using System.Linq;
using SpawnHouses.Common.Parameters;
using SpawnHouses.Common.Tagging;
using SpawnHouses.Structures;
using Terraria;

namespace SpawnHouses.Common.Types;

public interface IGeneratable {
    /// <summary>
    ///     if the generatable instance has had a generator run on it at least once
    /// </summary>
    public bool HasGenerated { get; }
    
    /// <summary>
    ///     unique number given to each generatable instance in the world. automatically assigned on instance creation, 
    /// </summary>
    public ushort Id { get; }

    /// <summary>
    ///     generation parameters for this generatable object
    /// </summary>
    public IParams Params { get; }
    
    /// <summary>
    ///      tags that this instance currently has
    /// </summary>
    public TagMap TagsCurrent { get; }
    
    /// <summary>
    ///     executable generator for this component. automatically assigned on instance creation by <see cref="SetGenerator"/>
    /// </summary>
    public IGenerator Generator { get; }

    /// <summary>
    ///     sets <see cref="Generator"/>. called on instance creation
    /// </summary>
    public void SetGenerator();

    /// <summary>
    ///     gets the hashcode of a generator's name and namespace
    /// </summary>
    /// <returns></returns>
    public int GetGeneratorHash() => Generator.GetType().FullName!.GetHashCode();
}

public interface IGeneratable<out TParams, out TGenerator> : IGeneratable
    where TParams : IParams
    where TGenerator : Generator<TParams> {
    IParams IGeneratable.Params => Params;
    /// <inheritdoc cref="IGeneratable.Params" />
    public new TParams Params { get; }

    IGenerator IGeneratable.Generator => Generator;
    /// <inheritdoc cref="IGeneratable.Generator" />
    public new TGenerator Generator { get; }

    void IGeneratable.SetGenerator() => SetGenerator();

    /// <inheritdoc cref="IGeneratable.SetGenerator" />
    public new void SetGenerator();
}

public abstract class Generatable<TParams, TGenerator> : IGeneratable<TParams, TGenerator>
    where TParams : IParams
    where TGenerator : Generator<TParams> {
    public bool HasGenerated { get; private set; }
    public ushort Id { get; init; }
    public TParams Params { get; init; }
    public TagMap TagsCurrent { get; init; }
    public TGenerator Generator { get; private set; }

    protected Generatable(TParams param, TagMap tagsCurrent) {
        Id = StructureManager.NextGeneratableId();
        Params = param;
        TagsCurrent = tagsCurrent;
        // TagsCurrent.IsLocked = true; TODO
        SetGenerator();
    }

    private TGenerator FindValidGenerator(TGenerator[] generators) {
        TagsCurrent.ValidateExclusiveCurrentTags();
        var validGenerators = generators.Where(gen => gen.CanGenerate(Params) && Params.TagsRequired.KeysSet.IsSubsetOf(gen.PossibleTags))
            .ToArray();

        if (validGenerators.Length == 0)
            throw new Exception($"No instance generators were found that are compatible with given parameters. type: {GetType().FullName}, required tags: {EnumHelper.ToString(Params.TagsRequired.Keys)}");

        return Params.Structure.RandomGen.NextFromList(validGenerators);
    }

    public void SetGenerator() {
        int generatorIndex = 0;
        Type instanceType = GetType();
        if (!AdvStructure.InstanceGenerators.TryGetValue(instanceType, out var generators)) throw new Exception($"instance type {GetType().FullName} generators not found");
        var typedGenerators = generators.Cast<TGenerator>().ToArray();

        if (!Params.Structure.InstanceGeneratorQueue.TryGetValue(instanceType, out var generatorList)) {
            generatorList = [FindValidGenerator(typedGenerators)];
            Params.Structure.InstanceGeneratorQueue[instanceType] = generatorList;
        }
        else {
            while (!generatorList[generatorIndex].CanGenerate(Params)) {
                generatorIndex++;
                if (generatorIndex >= Params.Structure.InstanceGeneratorQueue.Count) generatorList.Add(FindValidGenerator(typedGenerators));
            }
        }

        Generator = (TGenerator)generatorList[generatorIndex];
    }

    public void ExecuteGenerator() {
        TagsCurrent.IsLocked = false;
        Generator.Generate(Params);
        HasGenerated = true;
    }
}