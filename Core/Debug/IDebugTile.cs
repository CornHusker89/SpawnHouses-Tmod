using SpawnHouses.Core.AdvGeneratables;

namespace SpawnHouses.Core.Debug;

public interface IDebugTile {
    public IComponent Component { get; }
    public bool IsComponentDoubleAssigned { get; }

    public void SetComponent(IComponent component);
}