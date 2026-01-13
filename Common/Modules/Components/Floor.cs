using SpawnHouses.Common.Parameters;
using SpawnHouses.Common.Tagging;
using SpawnHouses.Common.Types.Geometry;

namespace SpawnHouses.Common.Modules.Components;

public class Floor : VolumeComponent {
    /// <summary>
    ///     constructor that requires params object
    /// </summary>
    /// <param name="param"></param>
    /// <param name="isExterior"></param>
    public Floor(VolumeComponentParams param, bool isExterior = false) : base(param) {
        if (isExterior)
            Params.TagsRequired.Add(Tags.External);
    }

    /// <summary>
    ///     constructor that automatically creates a new set of params
    /// </summary>
    /// <param name="structure"></param>
    /// <param name="geometry"></param>
    /// <param name="isExterior"></param>
    public Floor(AdvStructure structure, Shape geometry, bool isExterior = false) : base(new VolumeComponentParams(structure, geometry)) {
        if (isExterior)
            Params.TagsRequired.Add(Tags.External);
    }
}