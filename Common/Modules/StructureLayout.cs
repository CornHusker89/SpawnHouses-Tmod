using System.Collections.Generic;
using System.Linq;
using SpawnHouses.Common.Modules.Components;
using SpawnHouses.Common.Parameters;
using SpawnHouses.Common.Tagging;
using SpawnHouses.Common.Tiles;
using SpawnHouses.Common.Types;
using SpawnHouses.Common.Types.Geometry;
using SpawnHouses.Types;
using Terraria.DataStructures;

namespace SpawnHouses.Common.Modules;

public class StructureLayout : Generatable<StructureLayoutParams, StructureLayoutGenerator> {
    public List<Floor> Floors { get; private set; }
    public List<Wall> Walls { get; private set; }
    public List<Gap> Gaps { get; private set; }
    public List<Roof> Roofs { get; private set; }
    public List<RoomLayout> RoomSections { get; private set; }

    public StructureLayout(StructureLayoutParams param) : base(param, new TagMap()) {
    }

    public void SetComponents(List<Floor> floors, List<Wall> walls, List<Gap> gaps, List<Roof> roofs, List<RoomLayout> roomSections) {
        Floors = floors;
        Walls = walls;
        Gaps = gaps;
        Roofs = roofs;
        RoomSections = roomSections;
    }

    /// <summary>
    ///     adds <see cref="Tags.External" /> to every component
    /// </summary>
    public void SetComponentTagsExternal() {
        foreach (Floor floor in Floors)
            floor.Params.TagsRequired.Add(Tags.External);
        foreach (Wall wall in Walls)
            wall.Params.TagsRequired.Add(Tags.External);
        foreach (Gap gap in Gaps)
            gap.Params.TagsRequired.Add(Tags.External);
        // roofs are automatically marked as external
    }

    /// <summary>
    ///     sets the outside/exterior component/inside tile data within the tilemap, based on the current external layout
    /// </summary>
    public void SetTilesExternalStatus() {
        StructureTilemap tilemap = Params.Structure.Tilemap;
        foreach (Shape shape in Floors.Where(floor => floor.TagsCurrent.HasTag(Tags.External)).Select(floor => floor.Geometry))
            shape.ExecuteInArea((x, y) => {
                StructureTile tile = tilemap[x, y];
                tile.IsExteriorComponent = true;
                tile.IsFloor = true;
            });

        foreach (Shape shape in Walls.Where(wall => wall.TagsCurrent.HasTag(Tags.External)).Select(wall => wall.Geometry))
            shape.ExecuteInArea((x, y) => {
                StructureTile tile = tilemap[x, y];
                tile.IsExteriorComponent = true;
                tile.IsWall = true;
            });

        foreach (Shape shape in Gaps.Where(gap => gap.TagsCurrent.HasTag(Tags.External)).Select(gap => gap.Geometry))
            shape.ExecuteInArea((x, y) => {
                StructureTile tile = tilemap[x, y];
                tile.IsExteriorComponent = true;
                tile.IsGap = true;
            });

        SearchOutside(0, 0);
        for (int x = 0; x < tilemap.Width; x++)
        for (int y = 0; y < tilemap.Height; y++) {
            StructureTile tile = tilemap[x, y];
            if (!tile.IsOutside && !tile.IsExteriorComponent) tile.IsInside = true;
        }

        return;

        void SearchOutside(int x, int y) {
            StructureTile thisTile = tilemap[x, y];
            thisTile.IsOutside = true;
            thisTile.IsNullTile = true;
            thisTile.IsNullWall = true;

            foreach ((int dx, int dy) in ((int, int)[]) [(1, 0), (-1, 0), (0, 1), (0, -1)]) {
                if (!tilemap.InBounds(x + dx, y + dy)) continue;
                StructureTile nextTile = tilemap[x + dx, y + dy];
                if (nextTile.IsOutside || nextTile.IsExteriorComponent) continue;

                SearchOutside(x + dx, y + dy);
            }
        }
    }

    /// <summary>
    ///     moves everything in the layout by the offset. ex. if offset = (3, 0) will move everything in the layout 3 to the right in world coordinates
    /// </summary>
    /// <param name="offset"></param>
    public void Offset(Point16 offset) {
        foreach (Floor floor in Floors) floor.Geometry.Offset(offset);
        foreach (Wall wall in Walls) wall.Geometry.Offset(offset);
        foreach (Gap gap in Gaps) gap.Geometry.Offset(offset);
        foreach (Roof roof in Roofs) roof.Geometry.Offset(offset);
        foreach (RoomLayout roomSection in RoomSections) roomSection.Offset(offset);
    }
}