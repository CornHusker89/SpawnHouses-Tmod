using SpawnHouses.StructureCommon.Types.DataStructures;

namespace SpawnHouses.StructureCommon.Types.Interfaces;

public interface IBoundingBox {
    public TileBox BoundingBox { get; }
}