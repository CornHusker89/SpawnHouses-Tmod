#nullable enable
using SpawnHouses.Types;

namespace SpawnHouses.AdvStructures.AdvStructureParts;

public class Floor : VolumeComponent {
    public Floor(Shape volume, bool isExterior = false) {
        Volume = volume;
        if (isExterior)
            AddRequiredTag(ComponentTag.External);
    }
}