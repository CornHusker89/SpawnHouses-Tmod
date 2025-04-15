using System;
using System.Collections.Generic;
using System.Linq;
using SpawnHouses.AdvStructures.AdvStructureParts;
using SpawnHouses.Types;
using Terraria.DataStructures;

namespace SpawnHouses.AdvStructures;

public class AdvStructure {
    public Shape BoundingBox;
    public ExternalLayout ExternalLayout;
    public bool FilledComponents;
    public bool HasLayout;
    public RoomLayout Layout;

    public StructureParams Params;

    // public Tilemap TileMap;
    /// <summary>Position of the top-left of the outer bounding box</summary>
    public Point16 Position;

    public int XSize, YSize, HousingCount;

    public AdvStructure(StructureParams structureParams) {
        Params = structureParams;
    }

    /// <summary>
    ///     calculates a structure's layout, and does not alter tiles
    /// </summary>
    /// <param name="method">method to be used. leave null for a random method</param>
    public bool ApplyLayoutMethod(Func<StructureParams, AdvStructure, bool> method = null) {
        method ??= AdvStructureLayouts.GetRandomMethod(Params);
        var result = method(Params, this);
        if (result)
            HasLayout = true;
        return result;
    }

    private void FillComponentSet(List<Shape> volumes, StructureTag[] tagsRequired, StructureTag[] tagsBlacklist,
        TilePalette palette) {
        if (volumes.Count == 0)
            return;

        var componentParams = new ComponentParams(
            tagsRequired,
            tagsBlacklist,
            volumes[0],
            palette
        );
        var genMethod = ComponentGen.GetRandomMethod(componentParams);

        foreach (var volume in volumes) {
            componentParams.Volume = volume;
            genMethod(componentParams);
        }
    }

    /// <summary>
    ///     Fills current layout with tiles
    /// </summary>
    /// <exception cref="Exception">Throws when no layout has been set</exception>
    public void FillComponents() {
        if (!HasLayout)
            throw new Exception("No layout has been set");

        FillComponentSet(ExternalLayout.WallVolumes, [StructureTag.HasWall], [], Params.Palette);
        FillComponentSet(ExternalLayout.WallGaps.Select(gap => gap.Volume).ToList(), [StructureTag.IsWallGap], [],
            Params.Palette);
        FillComponentSet(ExternalLayout.WallVolumes, [StructureTag.HasWall], [], Params.Palette);
        FillComponentSet(ExternalLayout.WallGaps.Select(gap => gap.Volume).ToList(), [StructureTag.IsWallGap], [],
            Params.Palette);

        FillComponentSet(Layout.FloorVolumes, [StructureTag.HasFloor], [], Params.Palette);
        FillComponentSet(Layout.FloorGaps.Select(gap => gap.Volume).ToList(), [StructureTag.IsFloorGap], [],
            Params.Palette);
        FillComponentSet(Layout.WallVolumes, [StructureTag.HasWall], [], Params.Palette);
        FillComponentSet(Layout.WallGaps.Select(gap => gap.Volume).ToList(), [StructureTag.IsWallGap], [],
            Params.Palette);

        FillComponentSet(Layout.Rooms.Select(room => room.Volume).ToList(), [StructureTag.IsWallGap], [],
            Params.Palette);
    }


    public void FinishHousing() {
    }

    public Shape GetBoundingShape(StructureParams structureParams) {
        var boundingShape = new Shape(
            new Point16(structureParams.Start.X, Math.Max(structureParams.Start.Y, structureParams.End.Y)),
            new Point16(structureParams.End.X,
                Math.Min(structureParams.Start.Y, structureParams.End.Y) + structureParams.Height + 3)
        );
        return boundingShape;
    }
}