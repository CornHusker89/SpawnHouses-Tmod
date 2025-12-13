#nullable enable
using SpawnHouses.Types;

namespace SpawnHouses.AdvStructures.AdvStructureParts;

public class Wall : VolumeComponent {
    public Wall(Shape volume, bool isExterior = false) {
        Volume = volume;
        if (isExterior)
            AddRequiredTag(ComponentTag.External);
    }
}