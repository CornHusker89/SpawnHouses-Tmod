using SpawnHouses.Common.Modules;

namespace SpawnHouses.Common.Debug;

public interface IDebugTile {
    public IComponent Component { get; }
    public bool IsComponentDoubleAssigned { get; }

    public void SetComponent(IComponent component);
}