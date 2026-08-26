using SpawnHouses.Core.AdvGeneratables;
using SpawnHouses.Core.Tiles;

namespace SpawnHouses.Core.Debug;

public class NonSolidDebugTile : StructureTile, IDebugTile {
    public IComponent Component { get; private set; }
    public bool IsComponentDoubleAssigned { get; private set; }

    public void SetComponent(IComponent component) {
        if (Component != null) IsComponentDoubleAssigned = true;
        Component = component;
    }
}