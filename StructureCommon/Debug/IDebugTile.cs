using SpawnHouses.StructureCommon.Types.Interfaces;

namespace SpawnHouses.StructureCommon.Debug;

public interface IDebugTile {
    public IComponent Component { get; }
    public bool IsComponentDoubleAssigned { get; }

    public void SetComponent(IComponent component);
}