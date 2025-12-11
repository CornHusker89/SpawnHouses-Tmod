using System.Collections.Generic;
using SpawnHouses.Types;

namespace SpawnHouses.AdvStructures.Generation;

public interface IStructureLayoutGenerator {
    public HashSet<StructureTag> PossibleTags { get; }

    public bool CanGenerate(StructureParams structureParams);

    public bool Generate(AdvStructure advStructure);
}