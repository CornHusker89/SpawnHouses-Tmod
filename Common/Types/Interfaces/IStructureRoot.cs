using SpawnHouses.Common.Tiles;
using Terraria.DataStructures;

namespace SpawnHouses.Common.Types.Interfaces;

public interface IStructureRoot {
    /// <summary>
    ///     full tilemap of the structure. can be set anytime before being placed in the world
    /// </summary>
    public StructureTilemap Tilemap { get; }

    /// <summary>
    ///     the ways in/out of a structure
    /// </summary>
    public EntryPoint[] EntryPoints { get; }

    /// <summary>
    ///     if the structure has been "found" by a player. not affected inside <see cref="IsFound"/> or <see cref="OnFound"/>
    /// </summary>
    public bool HasBeenFound { get; }

    /// <summary>
    ///     returns true if the player is close enough to "find" the structure. should not set <see cref="HasBeenFound"/>
    /// </summary>
    /// <param name="playerPos"></param>
    /// <returns></returns>
    public bool IsFound(Point16 playerPos);

    /// <summary>
    ///     called when a structure is "found". <see cref="IsFound" /> is set to true before this method is called
    /// </summary>
    public void OnFound();

    /// <summary>
    ///     puts tiles into the structure's tilemap
    /// </summary>
    public void LoadTilemap();

    /// <summary>
    ///     paste tiles from the structure's tilemap into game tilemap
    /// </summary>
    public void ApplyTilemap();
}