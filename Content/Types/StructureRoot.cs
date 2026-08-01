#nullable enable

using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using SpawnHouses.Content.Debug;
using SpawnHouses.Content.Tagging;
using SpawnHouses.Content.Tiles;
using SpawnHouses.Content.Types.Enums;
using SpawnHouses.Content.Types.Interfaces;
using SpawnHouses.Helpers;
using Terraria.DataStructures;

namespace SpawnHouses.Content.Types;

/// <summary>
///     the root of any generatable structure. all structures must have one, and only one
/// </summary>
public abstract class StructureRoot : IGeneratable, IStructureTags, IDebugDraw {
    // IGeneratable

    public ushort Id { get; protected set; }

    // IStructureTags

    public abstract TagMap TagsCurrent { get; protected set; }

    // IDebugDraw

    public abstract string Name { get; protected set; }

    /// <summary>
    ///     <inheritdoc cref="IDebugDraw.DebugInfoVisibility" />. is cloned from <see cref="StructureManager.DefaultDebugInfoLevel" /> on instance creation
    /// </summary>
    public DebugInfoLevel DebugInfoVisibility { get; set; } = StructureManager.DefaultDebugInfoLevel.Clone();


    /// <summary>
    ///     the container for this structure root, if there is one
    /// </summary>
    public IStructureContainer? Container { get; set; } = null;
    
    /// <summary>
    ///     full tilemap of the structure. can be set anytime before being placed in the world
    /// </summary>
    public StructureTilemap Tilemap { get; set; }

    /// <summary>
    ///     the ways in/out of a structure
    /// </summary>
    public EntryPoint[] EntryPoints { get; protected set; }

    /// <summary>
    ///     if the structure has been "found" by a player. not set inside <see cref="IsFound"/> or <see cref="OnFound"/>
    /// </summary>
    public bool HasBeenFound { get; set; }

    public Color GetDrawColor() => DrawHelper.GetColor(Id);

    public abstract List<DebugLabel> DrawDebugGeometry();

    /// <summary>
    ///     returns true if the player is close enough to "find" the structure.
    ///     will only be called if structure is not already found, should not set <see cref="HasBeenFound"/>
    /// </summary>
    /// <param name="playerPos"></param>
    /// <returns></returns>
    public abstract bool IsFound(Point16 playerPos);

    /// <summary>
    ///     called when a structure is "found". <see cref="IsFound" /> is set to true before this method is called
    /// </summary>
    public abstract void OnFound();

    /// <summary>
    ///     puts tiles into the structure's tilemap
    /// </summary>
    public abstract void LoadTilemap();

    /// <summary>
    ///     paste tiles from the structure's tilemap into game tilemap
    /// </summary>
    public abstract void ApplyTilemap();

    /// <summary>
    ///     sets top left position of the structure in the world
    /// </summary>
    /// <param name="position"></param>
    public abstract void SetPosition(Point16 position);

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

    public int GetUpgradePriority() => 0;
}