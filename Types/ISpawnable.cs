using Terraria.DataStructures;

namespace SpawnHouses.Types;

public interface ISpawnable {
    public TileBox BoundingBox { get; }

    public bool IsFound(Point16 playerPos);

    public void OnFound();
}