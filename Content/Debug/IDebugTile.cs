using SpawnHouses.Content.Types.Interfaces;

namespace SpawnHouses.Content.Debug;

public interface IDebugTile {
    public IComponent Component { get; }
    public bool IsComponentDoubleAssigned { get; }

    public void SetComponent(IComponent component);
}