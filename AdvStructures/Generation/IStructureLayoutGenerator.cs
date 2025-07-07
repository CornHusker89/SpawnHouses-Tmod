using SpawnHouses.Types;

namespace SpawnHouses.AdvStructures.Generation;

public interface IStructureLayoutGenerator {
    public StructureTag[] GetPossibleTags();

    public bool CanGenerate(StructureParams structureParams);

    public bool Generate(AdvStructure advStructure);
}