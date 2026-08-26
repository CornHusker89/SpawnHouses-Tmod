using SpawnHouses.Core.AdvStructureCore;
using SpawnHouses.Core.Debug;
using SpawnHouses.Core.Parameters;
using SpawnHouses.Core.Tagging;

namespace SpawnHouses.Core.AdvGeneratables;

/// <summary>
///     a more complex version of the <see cref="IGeneratable" />, supporting debug drawing and dynamic tag-based generation. intended for AdvStructures
/// </summary>
public interface IAdvGeneratable : IGeneratable, IDebugDraw, IStructureTags {
    /// <summary>
    ///     if the advGeneratable instance has had a advGenerator run on it at least once
    /// </summary>
    public bool HasGenerated { get; protected set; }

    /// <summary>
    ///     generation parameters for this advGeneratable object
    /// </summary>
    public IParams Params { get; }

    /// <summary>
    ///     creates and executes a component's advGenerator, unlocks it's current tags, and marks the component as generated. correct way to generate modules
    /// </summary>
    public void ExecuteGenerator();

    /// <summary>
    ///     gets the hashcode of a advGenerator's name and namespace. <see cref="ExecuteGenerator" /> must have been called, and <see cref="HasGenerated" /> must be true
    /// </summary>
    /// <returns></returns>
    public int GetGeneratorHash();

    /// <summary>
    ///     gets the name of the advGenerator
    /// </summary>
    /// <returns></returns>
    public string GetGeneratorName();
}

public interface IAdvGeneratable<TSelf, TParams, TGenerator> : IAdvGeneratable
    where TSelf : IAdvGeneratable<TSelf, TParams, TGenerator>
    where TParams : IParams
    where TGenerator : IAdvGenerator<TParams, TSelf> {
    IParams IAdvGeneratable.Params => Params;

    /// <inheritdoc cref="IAdvGeneratable.Params" />
    public new TParams Params { get; }
}