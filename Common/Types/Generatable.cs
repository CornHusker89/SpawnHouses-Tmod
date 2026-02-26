using System;
using System.Collections.Generic;
using System.Linq;
using SpawnHouses.Common.Parameters;
using SpawnHouses.Common.Tagging;
using SpawnHouses.Structures;
using Terraria;
using Terraria.Utilities;

namespace SpawnHouses.Common.Types;

public interface IGeneratable {
    /// <summary>
    ///     if the generatable instance has had a generator run on it at least once
    /// </summary>
    public bool HasGenerated { get; protected set; }
    
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
    ///     creates and executes a component's generator, unlocks it's current tags, and marks the component as generated. correct way to generate modules
    /// </summary>
    public void ExecuteGenerator();

    /// <summary>
    ///     gets the hashcode of a generator's name and namespace. <see cref="ExecuteGenerator"/> must have been called, and <see cref="HasGenerated"/> must be true
    /// </summary>
    /// <returns></returns>
    public int GetGeneratorHash();
}

public interface IGeneratable<TSelf, TParams, TGenerator> : IGeneratable
    where TSelf : IGeneratable<TSelf, TParams, TGenerator>
    where TParams : IParams
    where TGenerator : IGenerator<TParams, TSelf> {
    
    IParams IGeneratable.Params => Params;
    /// <inheritdoc cref="IGeneratable.Params" />
    public new TParams Params { get; }
}

public abstract class Generatable<TSelf, TParams, TGenerator> : IGeneratable<TSelf, TParams, TGenerator>
    where TSelf : Generatable<TSelf, TParams, TGenerator>
    where TParams : IParams
    where TGenerator : IGenerator<TParams, TSelf> {
    public bool HasGenerated { get; set; }
    public ushort Id { get; }
    public TParams Params { get; }
    public TagMap TagsCurrent { get; }

    protected TGenerator Generator;

    protected Generatable(TParams param, TagMap tagsCurrent) {
        Id = StructureManager.NextGeneratableId();
        Params = param;
        TagsCurrent = tagsCurrent;
        TagsCurrent.IsLocked = true;
    }

    /// <summary>
    ///     gets a generator for this module
    /// </summary>
    /// <param name="generators"></param>
    /// <returns></returns>
    /// <exception cref="Exception"></exception>
    private IGenerator FindValidGenerator(IGenerator[] generators) {
        TagsCurrent.ValidateRequiredTags();
        List<IGenerator> validGeneratorsList = [];
        foreach (IGenerator gen in generators)
            if (gen.CanGenerate(this, Params, new UnifiedRandom(Id)) && Params.TagsRequired.KeysSet.IsSubsetOf(gen.PossibleTags))
                validGeneratorsList.Add(gen);

        var validGenerators = validGeneratorsList.ToArray();

        if (validGenerators.Length == 0)
            throw new Exception($"No instance generators were found that are compatible with given parameters. type: {GetType().FullName}, required tags: {EnumHelper.ToString(Params.TagsRequired.Keys)}");

        return Params.Structure.LayoutRandom.NextFromList(validGenerators);
    }

    /// <summary>
    ///     ensures that all the param's required tags are in <see cref="TagsCurrent" />
    /// </summary>
    private void ValidateTagGeneration() {
        foreach (Tag tag in Params.TagsRequired.Keys)
            if (!TagsCurrent.HasTag(tag))
                throw new Exception($"missing generatable current tag \"{tag}\" which was required in params");
    }

    /// <summary>
    ///     sets this module's generator
    /// </summary>
    /// <exception cref="Exception"></exception>
    protected void SetGenerator() {
        Type instanceType = GetType();
        if (!AdvStructure.InstanceGenerators.TryGetValue(instanceType, out var generators)) throw new Exception($"instance type {GetType().FullName} generators not found");
        var typedGenerators = generators.Cast<IGenerator>().ToArray();

        int generatorIndex = 0;
        if (!Params.Structure.InstanceGeneratorQueue.TryGetValue(instanceType, out var generatorList)) {
            generatorList = [FindValidGenerator(typedGenerators)];
            Params.Structure.InstanceGeneratorQueue[instanceType] = generatorList;
        }
        else {
            while (!generatorList[generatorIndex].CanGenerate(this, Params, new UnifiedRandom(Id))) {
                generatorIndex++;
                if (generatorIndex >= generatorList.Count)
                    generatorList.Add(FindValidGenerator(typedGenerators));
            }
        }

        Generator = (TGenerator)generatorList[generatorIndex];
    }
    
    public void ExecuteGenerator() {
        if (Generator == null) {
            SetGenerator();
        }

        TagsCurrent.IsLocked = false;
        if (!Generator!.Generate(this, Params, new UnifiedRandom(Id), Params.Structure.Palette, Params.Structure.Tilemap))
            throw new Exception($"generator execution failed on module {ToString()}");
        ValidateTagGeneration();
        TagsCurrent.ValidateCurrentTags();
        TagsCurrent.IsLocked = true;
        HasGenerated = true;
    }

    public int GetGeneratorHash() => Generator.GetType().FullName!.GetHashCode();
}