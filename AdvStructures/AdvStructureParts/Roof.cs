#nullable enable
using SpawnHouses.Types;

namespace SpawnHouses.AdvStructures.AdvStructureParts;

public class Roof : PathComponent {
    // IComponentExternalExt
    public bool IsExterior { get; set; } = true;

    public Roof(Path line) {
        Line = line;
        AddRequiredTag(ComponentTag.External);
    }
}