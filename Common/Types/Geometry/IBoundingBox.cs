using SpawnHouses.Common.DataStructures;

namespace SpawnHouses.Common.Types.Geometry;

public interface IBoundingBox {
    public TileBox BoundingBox { get; }
}