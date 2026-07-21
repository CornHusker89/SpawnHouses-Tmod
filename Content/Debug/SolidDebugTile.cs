using SpawnHouses.Content.Tiles;
using SpawnHouses.Content.Types.Interfaces;

namespace SpawnHouses.Content.Debug;

public class SolidDebugTile : StructureTile, IDebugTile {
    public IComponent Component { get; private set; }
    public bool IsComponentDoubleAssigned { get; private set; }

    public void SetComponent(IComponent component) {
        if (Component != null) IsComponentDoubleAssigned = true;
        Component = component;
    }
}