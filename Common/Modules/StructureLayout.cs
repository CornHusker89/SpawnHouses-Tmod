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
    public List<Floor> ExternalFloors { get; private set; }
    public List<Wall> ExternalWalls { get; private set; }
    public List<Gap> ExternalGaps { get; private set; }
    public List<Roof> Roofs { get; private set; }
    public List<RoomLayout> RoomLayouts { get; private set; }

    public StructureLayout(StructureLayoutParams param) : base(param, new TagMap()) {
    }

    public Room[] Rooms {
        get {
            int len = 0;
            foreach (RoomLayout roomLayout in RoomLayouts) len += roomLayout.Rooms.Count;
            var rooms = new Room[len];
            int count = 0;
            foreach (RoomLayout roomLayout in RoomLayouts) {
                foreach (Room room in roomLayout.Rooms)
                    rooms[count] = room;
                count++;
            }

            return rooms;
        }
    }

    public void SetComponents(List<Floor> floors, List<Wall> walls, List<Gap> gaps, List<Roof> roofs, List<RoomLayout> roomLayouts) {
        ExternalFloors = floors;
        ExternalWalls = walls;
        ExternalGaps = gaps;
        Roofs = roofs;
        RoomLayouts = roomLayouts;
    }

    /// <summary>
    ///     sets the outside/exterior component/inside tile data within the tilemap, based on the current external layout
    /// </summary>
    public void SetTilesExternalStatus() {
        StructureTilemap tilemap = Params.Structure.Tilemap;
        foreach (Shape shape in ExternalFloors.Where(floor => floor.TagsCurrent.HasTag(Tags.External)).Select(floor => floor.Geometry))
            shape.ExecuteInArea((x, y) => {
                StructureTile tile = tilemap[x, y];
                tile.IsExteriorComponent = true;
                tile.IsFloor = true;
            });

        foreach (Shape shape in ExternalWalls.Where(wall => wall.TagsCurrent.HasTag(Tags.External)).Select(wall => wall.Geometry))
            shape.ExecuteInArea((x, y) => {
                StructureTile tile = tilemap[x, y];
                tile.IsExteriorComponent = true;
                tile.IsWall = true;
            });

        foreach (Shape shape in ExternalGaps.Where(gap => gap.TagsCurrent.HasTag(Tags.External)).Select(gap => gap.Geometry))
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
        foreach (Floor floor in ExternalFloors) floor.Geometry.Offset(offset);
        foreach (Wall wall in ExternalWalls) wall.Geometry.Offset(offset);
        foreach (Gap gap in ExternalGaps) gap.Geometry.Offset(offset);
        foreach (Roof roof in Roofs) roof.Geometry.Offset(offset);
        foreach (RoomLayout roomSection in RoomLayouts) roomSection.Offset(offset);
    }
}