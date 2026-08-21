using SpawnHouses.Core.DataStructures;

namespace SpawnHouses.Core.Interfaces;

public interface IBoundingBox {
    public TileBox BoundingBox { get; }
}