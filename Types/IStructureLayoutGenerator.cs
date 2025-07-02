using SpawnHouses.AdvStructures;

namespace SpawnHouses.Types;

public interface IStructureLayoutGenerator {
    public StructureTag[] GetPossibleTags();

    public bool CanGenerate(StructureParams structureParams);

    public bool Generate(AdvStructure advStructure);
}
