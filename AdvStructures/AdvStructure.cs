using System;
using System.Collections.Generic;
using System.Linq;
using SpawnHouses.AdvStructures.AdvStructureParts;
using SpawnHouses.Types;
using Terraria;
using Terraria.DataStructures;
using Terraria.WorldBuilding;

namespace SpawnHouses.AdvStructures;

public class AdvStructure {
    /// <summary>Extends a few blocks out further in each direction</summary>
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
    public bool ApplyLayoutMethod(Func<AdvStructure, bool> method = null) {
        method ??= AdvStructureLayoutsGen.GetRandomMethod(Params);
        bool result = method(this);
        if (result) {
            HasLayout = true;
        }

        // set the bounding box
        var edgeVolumes = ExternalLayout.Floors.Select(floor => floor.Volume).ToList();
        edgeVolumes.AddRange(ExternalLayout.Walls.Select(wall => wall.Volume).ToList());
        edgeVolumes.AddRange(ExternalLayout.Gaps.Select(gap => gap.Volume).ToList());
        BoundingBox = new Shape(
            new Point16(edgeVolumes.Min(volume => volume.BoundingBox.topLeft.X) - 5, edgeVolumes.Min(volume => volume.BoundingBox.topLeft.Y) - 5),
            new Point16(edgeVolumes.Max(volume => volume.BoundingBox.bottomRight.X) + 5, edgeVolumes.Max(volume => volume.BoundingBox.bottomRight.Y) + 5)
        );

        return result;
    }

    private void FillComponentSet(List<Shape> volumes, ComponentTag[] tagsRequired, ComponentTag[] tagsBlacklist,
        TilePalette palette) {
        if (volumes.Count == 0)
            return;

        ComponentParams componentParams = new ComponentParams(
            tagsRequired,
            tagsBlacklist,
            volumes[0],
            palette
        );
        var genMethod = ComponentGen.GetRandomMethod(componentParams);

        foreach (Shape volume in volumes) {
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

        FillComponentSet(ExternalLayout.Floors.Select(floor => floor.Volume).ToList(), [ComponentTag.IsFloor], [], Params.Palette);
        FillComponentSet(ExternalLayout.Walls.Select(wall => wall.Volume).ToList(), [ComponentTag.IsWall], [], Params.Palette);
        FillComponentSet(ExternalLayout.Gaps.FindAll(gap => !gap.IsHorizontal).Select(gap => gap.Volume).ToList(), [ComponentTag.IsFloorGap], [],
            Params.Palette);
        FillComponentSet(ExternalLayout.Gaps.FindAll(gap => gap.IsHorizontal).Select(gap => gap.Volume).ToList(), [ComponentTag.IsWallGap], [],
            Params.Palette);

        FillComponentSet(Layout.Floors.Select(floor => floor.Volume).ToList(), [ComponentTag.IsFloor], [], Params.Palette);
        FillComponentSet(Layout.Walls.Select(wall => wall.Volume).ToList(), [ComponentTag.IsWall], [], Params.Palette);
        FillComponentSet(Layout.Gaps.FindAll(gap => !gap.IsHorizontal).Select(gap => gap.Volume).ToList(), [ComponentTag.IsDebugBlocks], [],
            Params.Palette);
        FillComponentSet(Layout.Gaps.FindAll(gap => gap.IsHorizontal).Select(gap => gap.Volume).ToList(), [ComponentTag.IsDebugBlocks], [],
            Params.Palette);

        List<Shape> roomVolumes = [];
        foreach (Room room in Layout.Rooms) {
            roomVolumes.Add(room.Volume);
        }
        FillComponentSet(roomVolumes, [ComponentTag.IsBackground], [],
            Params.Palette);

        BoundingBox.ExecuteInArea((x, y) => {
            WorldUtils.TileFrame(x, y);
            Framing.WallFrame(x, y);
        });
    }

    public void FinishHousing() {
    }
}