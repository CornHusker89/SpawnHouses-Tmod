using System;
using System.Collections.Generic;
using SpawnHouses.Common.Debug;
using SpawnHouses.Common.Parameters;
using SpawnHouses.Common.Tagging;
using SpawnHouses.Common.Types.Interfaces;
using Terraria;
using Terraria.Utilities;

namespace SpawnHouses.Common.Types;

public abstract class AdvGeneratable<TSelf, TParams, TGenerator> : IAdvGeneratable<TSelf, TParams, TGenerator>
    where TSelf : AdvGeneratable<TSelf, TParams, TGenerator>
    where TParams : IParams
    where TGenerator : IAdvGenerator<TParams, TSelf> {
    public DebugInfoLevel DebugInfoVisibility { get; set; }

    /// (ideally) a unique identifier. during assignment, any "#" get replaced with advGeneratable's id
    public string Name { get; }
    
    public bool HasGenerated { get; set; }
    public ushort Id { get; }
    public TParams Params { get; }
    public TagMap TagsCurrent { get; }

    protected TGenerator Generator;

    protected AdvGeneratable(TParams param, TagMap tagsCurrent, string name) {
        Id = StructureManager.NextGeneratableId();
        Params = param;
        TagsCurrent = tagsCurrent;
        TagsCurrent.IsLocked = true;
        DebugInfoVisibility = new DebugInfoLevel();
        Name = name.Replace("#", Id.ToString());
    }

    public abstract List<DebugLabel> DrawDebugGeometry();

    /// <summary>
    ///     gets a advGenerator for this module
    /// </summary>
    /// <param name="generators"></param>
    /// <returns></returns>
    /// <exception cref="Exception"></exception>
    private IAdvGenerator FindValidGenerator(IAdvGenerator[] generators) {
        TagsCurrent.ValidateRequiredTags();
        List<IAdvGenerator> validGeneratorsList = [];
        foreach (IAdvGenerator gen in generators)
            if (gen.CanGenerate(this, Params, new UnifiedRandom(Id)) && Params.TagsRequired.KeysSet.IsSubsetOf(gen.PossibleTags))
                validGeneratorsList.Add(gen);

        var validGenerators = validGeneratorsList.ToArray();

        if (validGenerators.Length == 0)
            throw new Exception($"No instance generators were found that are compatible with given parameters. type: {GetType().FullName}");

        return Params.Structure.LayoutRandom.NextFromList(validGenerators);
    }

    /// <summary>
    ///     ensures that all the param's required tags are in <see cref="TagsCurrent" />
    /// </summary>
    private void ValidateTagGeneration() {
        foreach (Tag tag in Params.TagsRequired.Keys)
            if (!TagsCurrent.HasTag(tag))
                throw new Exception($"missing advGeneratable {this} current tag \"{tag}\" which was required in params");
    }

    /// <summary>
    ///     sets this module's advGenerator
    /// </summary>
    /// <exception cref="Exception"></exception>
    protected void SetGenerator() {
        Type instanceType = GetType();
        if (!GlobalGeneratorUtils.InstanceGenerators.TryGetValue(instanceType, out var generators)) throw new Exception($"instance type {GetType().FullName} generators not found");
        var typedGenerators = generators.Cast<IAdvGenerator>().ToArray();

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
            throw new Exception($"advGenerator execution failed on module {ToString()}");
        ValidateTagGeneration();
        TagsCurrent.ValidateCurrentTags();
        TagsCurrent.IsLocked = true;
        HasGenerated = true;
    }

    public string GetGeneratorName() => Generator.GetType().Name;
    
    public int GetGeneratorHash() => Generator.GetType().FullName!.GetHashCode();
}