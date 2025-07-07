using SpawnHouses.Types;

namespace SpawnHouses.AdvStructures.Generation;

public interface IComponentGenerator {
    public bool CanGenerate(ComponentParams componentParams) {
        return true;
    }

    public ComponentTag[] GetPossibleTags();

    public bool Generate(ComponentParams componentParams);
}