using System;
using System.Collections.Generic;
using System.Linq;
using SpawnHouses.AdvStructures.AdvStructureParts;
using SpawnHouses.AdvStructures.Generation;
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

    public StructureTilemap Tilemap;

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
        if (result) HasLayout = true;

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

    /// <summary>
    ///     sets the outside/exterior component/inside tile data within the tilemap, based on the current external layout
    /// </summary>
    public void EvaluateTilemapData() {
        foreach (Shape shape in ExternalLayout.Floors.Select(floor => floor.Volume))
            shape.ExecuteInArea((x, y) => {
                StructureTile tile = Tilemap[x, y];
                tile.IsExteriorComponent = true;
                tile.IsFloor = true;
            });

        foreach (Shape shape in ExternalLayout.Walls.Select(wall => wall.Volume))
            shape.ExecuteInArea((x, y) => {
                StructureTile tile = Tilemap[x, y];
                tile.IsExteriorComponent = true;
                tile.IsWall = true;
            });

        foreach (Shape shape in ExternalLayout.Gaps.Select(gap => gap.Volume))
            shape.ExecuteInArea((x, y) => {
                StructureTile tile = Tilemap[x, y];
                tile.IsExteriorComponent = true;
                tile.IsGap = true;
            });

        SearchOutside(0, 0);
        for (int x = 0; x < Tilemap.Width; x++)
        for (int y = 0; y < Tilemap.Height; y++) {
            StructureTile tile = Tilemap[x, y];
            if (!tile.IsOutside && !tile.IsExteriorComponent) tile.IsInside = true;
        }

        return;

        void SearchOutside(int x, int y) {
            foreach ((int dx, int dy) in ((int, int)[]) [(1, 0), (-1, 0), (0, 1), (0, -1)]) {
                if (!Tilemap.IsValidTile(x + dx, y + dy)) continue;

                StructureTile tile = Tilemap[x + dx, y + dy];
                if (tile.IsOutside || tile.IsExteriorComponent) continue;

                tile.IsOutside = true;
                SearchOutside(x + dx, y + dy);
            }
        }
    }

    private void FillComponentSet(List<Shape> volumes, ComponentTag[] tagsRequired, ComponentTag[] tagsBlacklist) {
        if (volumes.Count == 0)
            return;

        ComponentParams componentParams = new(
            tagsRequired,
            tagsBlacklist,
            volumes[0],
            Params.Palette,
            Tilemap
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

        FillComponentSet(ExternalLayout.Floors.Select(floor => floor.Volume).ToList(), [ComponentTag.IsFloor], []);
        FillComponentSet(ExternalLayout.Walls.Select(wall => wall.Volume).ToList(), [ComponentTag.IsWall], []);
        FillComponentSet(ExternalLayout.Gaps.FindAll(gap => !gap.IsHorizontal).Select(gap => gap.Volume).ToList(), [ComponentTag.IsFloorGap], []);
        FillComponentSet(ExternalLayout.Gaps.FindAll(gap => gap.IsHorizontal).Select(gap => gap.Volume).ToList(), [ComponentTag.IsWallGap], []);

        // FillComponentSet(ExternalLayout.Gaps.FindAll(gap => !gap.IsHorizontal).Select(gap => gap.Volume).ToList(), [ComponentTag.IsFloorGap], [],
        //     Params.Palette);
        // FillComponentSet(ExternalLayout.Gaps.FindAll(gap => gap.IsHorizontal).Select(gap => gap.Volume).ToList(), [ComponentTag.IsWallGap], [],
        //     Params.Palette);

        // FillComponentSet(Layout.Floors.Select(floor => floor.Volume).ToList(), [ComponentTag.IsFloor], [], Params.Palette);
        // FillComponentSet(Layout.Walls.Select(wall => wall.Volume).ToList(), [ComponentTag.IsWall], [], Params.Palette);
        // FillComponentSet(Layout.Gaps.FindAll(gap => !gap.IsHorizontal).Select(gap => gap.Volume).ToList(), [ComponentTag.IsDebugBlocks], [],
        //     Params.Palette);
        // FillComponentSet(Layout.Gaps.FindAll(gap => gap.IsHorizontal).Select(gap => gap.Volume).ToList(), [ComponentTag.IsDebugBlocks], [],
        //     Params.Palette);
        //
        // List<Shape> roomVolumes = [];
        // foreach (Room room in Layout.Rooms) {
        //     roomVolumes.Add(room.Volume);
        // }
        //
        // FillComponentSet(roomVolumes, [ComponentTag.IsBackground], [],
        //     Params.Palette);

        BoundingBox.ExecuteInArea((x, y) => {
            WorldUtils.TileFrame(x, y);
            Framing.WallFrame(x, y);
        });
    }

    public void PlaceTilemap() {
        int xOffset = Tilemap.WorldTileOffset.X;
        int yOffset = Tilemap.WorldTileOffset.Y;
        for (int x = 0; x < Tilemap.Width; x++)
        for (int y = 0; y < Tilemap.Height; y++)
            Tilemap[x, y].CopyTile(x + xOffset, y + yOffset);
    }

    public void FinishHousing() {
    }
}