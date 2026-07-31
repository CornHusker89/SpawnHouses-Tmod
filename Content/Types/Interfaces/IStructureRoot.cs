#nullable enable

using System;
using SpawnHouses.Content.Tiles;
using SpawnHouses.Content.Types.Enums;
using Terraria.DataStructures;

namespace SpawnHouses.Content.Types.Interfaces;

public interface IStructureRoot : IDebugDraw {
    
    /// <summary>
    ///     unique id for this instance. automatically assigned on instance creation
    /// </summary>
    public ushort Id { get; }
    
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
    ///     returns true if the player is close enough to "find" the structure.
    ///     will only be called if structure is not already found, should not set <see cref="HasBeenFound"/>
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

    /// <summary>
    ///     sets top left position of the structure in the world
    /// </summary>
    /// <param name="position"></param>
    public void SetPosition(Point16 position);

    /// <summary>
    ///     moves the structure to align the entry points with the specified ground positions
    /// </summary>
    /// <param name="entry1"></param>
    /// <param name="groundEntryTarget1"></param>
    /// <param name="entry2"></param>
    /// <param name="groundEntryTarget2"></param>
    /// <param name="entryPointAnchor"> </param>
    public void AlignEntryPoints(EntryPoint entry1, Point16 groundEntryTarget1, EntryPoint? entry2, Point16? groundEntryTarget2 = null, EntryPointPositionAnchor entryPointAnchor = EntryPointPositionAnchor.Neutral) {
        if (entryPointAnchor is EntryPointPositionAnchor.Point2 && entry2 is null)
            throw new ArgumentException("entry point anchor cannon be on point 2 if point 2 is null");

        Point16 positionOffset;

        if (entryPointAnchor is EntryPointPositionAnchor.Point1 || entry2 is null) {
            positionOffset = groundEntryTarget1 - entry1.LowerOutside;
        }
        else if (entryPointAnchor is EntryPointPositionAnchor.Point2) {
            positionOffset = groundEntryTarget1 - entry2.LowerOutside;
        }
        else if (entryPointAnchor is EntryPointPositionAnchor.Neutral) {
            Point16 offset1 = groundEntryTarget1 - entry1.LowerOutside;
            Point16 offset2 = groundEntryTarget2!.Value - entry2.LowerOutside;
            positionOffset = new Point16(
                (short)((offset1.X + offset2.X) / 2),
                (short)((offset1.Y + offset2.Y) / 2)
            );
        }
        else {
            throw new Exception("unexpected value for entry point anchor");
        }

        SetPosition(positionOffset);
    }
}