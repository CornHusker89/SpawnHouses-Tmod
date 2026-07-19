using SpawnHouses.StructureCommon.Tiles;
using SpawnHouses.StructureCommon.Types.Interfaces;

namespace SpawnHouses.StructureCommon.Debug;

public class SolidDebugTile : StructureTile, IDebugTile {
    public IComponent Component { get; private set; }
    public bool IsComponentDoubleAssigned { get; private set; }

    public void SetComponent(IComponent component) {
        if (Component != null) IsComponentDoubleAssigned = true;
        Component = component;
    }
}