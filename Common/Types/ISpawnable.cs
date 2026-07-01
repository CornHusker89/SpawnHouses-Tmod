using SpawnHouses.Common.DataStructures;
using Terraria.DataStructures;

namespace SpawnHouses.Common.Types;

public interface ISpawnable {
    public TileBox BoundingBox { get; }

    public bool IsFound(Point16 playerPos);

    public void OnFound();
}