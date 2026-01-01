using SpawnHouses.Common.Parameters;
using SpawnHouses.Common.Types.Geometry;
using SpawnHouses.Types.TagTypes;

namespace SpawnHouses.Common.Modules.Components;

public class Floor : VolumeComponent {
    public Floor(VolumeComponentParams param, Shape volume, bool isExterior = false) : base(param, volume) {
        if (isExterior)
            Params.TagsRequired.Add(Tags.External);
    }
}