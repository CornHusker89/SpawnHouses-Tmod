using SpawnHouses.Types;
using SpawnHouses.Types.TagTypes;

namespace SpawnHouses.AdvStructures.AdvStructureParts;

public class Floor : VolumeComponent {
    public Floor(VolumeComponentParams param, Shape volume, bool isExterior = false) : base(param, volume,) {
        if (isExterior)
            Tags.AddRequiredTag(ComponentTags.External);
    }
}