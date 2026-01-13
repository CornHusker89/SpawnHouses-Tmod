using SpawnHouses.Common.Parameters;
using SpawnHouses.Common.Tagging;
using SpawnHouses.Common.Types.Geometry;

namespace SpawnHouses.Common.Modules.Components;

public class Wall : VolumeComponent {
    /// <summary>
    ///     constructor that requires params object
    /// </summary>
    /// <param name="p"></param>
    /// <param name="isExterior"></param>
    public Wall(VolumeComponentParams p, bool isExterior = false) : base(p) {
        if (isExterior)
            TagsCurrent.Add(Tags.External);
    }

    /// <summary>
    ///     constructor that automatically creates a new set of params
    /// </summary>
    /// <param name="structure"></param>
    /// <param name="shape"></param>
    /// <param name="isExterior"></param>
    public Wall(AdvStructure structure, Shape shape, bool isExterior = false) : base(new VolumeComponentParams(structure, shape)) {
        if (isExterior)
            TagsCurrent.Add(Tags.External);
    }
}