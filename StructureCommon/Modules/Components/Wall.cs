using SpawnHouses.StructureCommon.Parameters;
using SpawnHouses.StructureCommon.Types.Geometry;
using SpawnHouses.StructureCommon.Types.Interfaces;
using SpawnHouses.StructureCommon.Types.RootStructureTypes;

namespace SpawnHouses.StructureCommon.Modules.Components;

public class Wall : VolumeComponent {
    /// <summary>
    ///     constructor that requires params object
    /// </summary>
    /// <param name="p"></param>
    /// <param name="shape"></param>
    /// <param name="name"></param>
    public Wall(VolumeComponentParams p, Shape shape, string name) : base(p, shape, name) {
    }

    /// <summary>
    ///     constructor that automatically creates a new set of params
    /// </summary>
    /// <param name="structure"></param>
    /// <param name="shape"></param>
    /// <param name="name"></param>
    public Wall(AdvStructure structure, Shape shape, string name) : base(new VolumeComponentParams(structure), shape, name) {
    }
}