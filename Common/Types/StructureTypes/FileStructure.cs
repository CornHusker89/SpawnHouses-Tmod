using SpawnHouses.Common.DataStructures;
using SpawnHouses.Common.Tiles;
using SpawnHouses.Common.Types.Interfaces;
using Terraria.DataStructures;

namespace SpawnHouses.Common.Types.StructureTypes;

public class FileStructure : IBoundingBox, IGeneratable, IStructureRoot {
    // IBoundingBox
    public TileBox BoundingBox { get; private set; }

    // IGeneratable
    public ushort Id { get; private set; }

    // IStructureRoot
    public StructureTilemap Tilemap { get; }
    public bool IsTilesPlaced { get; private set; }
    public bool IsFound(Point16 playerPos) => true;

    public void OnFound() {
    }
}