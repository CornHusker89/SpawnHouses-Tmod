#nullable enable
using SpawnHouses.Common.Modules;

namespace SpawnHouses.AdvStructures.AdvStructureParts;

public class Roof : PathComponent {
    public Roof(Path line) {
        Line = line;
        AddRequiredTag(ComponentTags.External);
    }
}