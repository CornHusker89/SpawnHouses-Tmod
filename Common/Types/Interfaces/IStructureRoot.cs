using SpawnHouses.Common.Tiles;
using Terraria.DataStructures;

namespace SpawnHouses.Common.Types.Interfaces;

public interface IStructureRoot {
    public StructureTilemap Tilemap { get; }

    /// <summary>
    ///     true if this structure has placed tiles in the world
    /// </summary>
    public bool IsTilesPlaced { get; }

    /// <summary>
    ///     if the structure has been "found" by a player
    /// </summary>
    public bool HasBeenFound { get; }

    /// <summary>
    ///     returns true if the player is close enough to "find" the structure
    /// </summary>
    /// <param name="playerPos"></param>
    /// <returns></returns>
    public bool IsFound(Point16 playerPos);

    /// <summary>
    ///     called when a structure is "found". <see cref="IsFound" /> is set to true before this method is called
    /// </summary>
    public void OnFound();
}