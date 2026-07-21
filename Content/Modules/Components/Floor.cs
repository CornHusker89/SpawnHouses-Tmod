using SpawnHouses.Content.Parameters;
using SpawnHouses.Content.Types.Geometry;
using SpawnHouses.Content.Types.Interfaces;
using SpawnHouses.Content.Types.RootStructureTypes;

namespace SpawnHouses.Content.Modules.Components;

public class Floor : VolumeComponent {
    /// <summary>
    ///     constructor that requires params object
    /// </summary>
    /// <param name="p"></param>
    /// <param name="shape"></param>
    /// <param name="name"></param>
    public Floor(VolumeComponentParams p, Shape shape, string name) : base(p, shape, name) {
    }

    /// <summary>
    ///     constructor that automatically creates a new set of params
    /// </summary>
    /// <param name="structure"></param>
    /// <param name="shape"></param>
    /// <param name="name"></param>
    public Floor(AdvStructure structure, Shape shape, string name) : base(new VolumeComponentParams(structure), shape, name) {
    }
}