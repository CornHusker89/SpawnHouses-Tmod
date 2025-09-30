using System.Collections.Generic;
using SpawnHouses.Types;

namespace SpawnHouses.AdvStructures.Generation;

/// <summary>
///     base component generator with default types is <see cref="IComponentGenerator" />
/// </summary>
public interface IComponentGenerator<TParams> where TParams : ComponentParams {
    public HashSet<ComponentTag> GetPossibleTags();

    public bool CanGenerate(TParams componentParams) {
        return true;
    }

    public bool Generate(TParams componentParams);
}

public interface IComponentGenerator : IComponentGenerator<ComponentParams>;

public interface IVolumeComponentGenerator : IComponentGenerator<VolumeComponentParams>;

public interface IPathComponentGenerator : IComponentGenerator<PathComponentParams>;