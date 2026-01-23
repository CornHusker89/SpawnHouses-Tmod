using SpawnHouses.Common.Parameters;
using SpawnHouses.Common.Types.Geometry;

namespace SpawnHouses.Common.Modules.Components;

public class Wall : VolumeComponent {
    /// <summary>
    ///     constructor that requires params object
    /// </summary>
    /// <param name="p"></param>
    /// <param name="shape"></param>
    public Wall(VolumeComponentParams p, Shape shape) : base(p, shape) {
    }

    /// <summary>
    ///     constructor that automatically creates a new set of params
    /// </summary>
    /// <param name="structure"></param>
    /// <param name="shape"></param>
    public Wall(AdvStructure structure, Shape shape) : base(new VolumeComponentParams(structure), shape) {
    }
}