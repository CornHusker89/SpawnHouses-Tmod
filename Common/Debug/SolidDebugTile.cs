using SpawnHouses.Common.Modules;
using SpawnHouses.Common.Tiles;

namespace SpawnHouses.Common.Debug;

public class SolidDebugTile : StructureTile, IDebugTile {
    public IComponent Component { get; private set; }
    public bool IsComponentDoubleAssigned { get; private set; }

    public void SetComponent(IComponent component) {
        if (Component != null) IsComponentDoubleAssigned = true;
        Component = component;
    }
}