using System.Collections.Generic;
using Microsoft.Xna.Framework;
using SpawnHouses.Common.Modules.Components;
using SpawnHouses.Common.Parameters;
using SpawnHouses.Common.Tagging;
using SpawnHouses.Common.Types;
using SpawnHouses.Helpers;
using Terraria.DataStructures;

namespace SpawnHouses.Common.Modules;

public class StructureLayout : Generatable<StructureLayout, StructureLayoutParams, StructureLayoutGenerator> {
    /// <summary>
    ///     if external walls, floors, and roofs have been created and assigned. excludes any gaps, see <see cref="HasExternalGaps" />
    /// </summary>
    public bool HasExternalComponents;

    /// <summary>
    ///     if external gaps have been created and assigned
    /// </summary>
    public bool HasExternalGaps;

    /// <summary>
    ///     if all final components have been created and assigned
    /// </summary>
    public bool HasAllComponents;
    
    public List<Floor> ExternalFloors { get; private set; }
    public List<Wall> ExternalWalls { get; private set; }
    public List<Gap> ExternalGaps { get; private set; }
    public List<Roof> Roofs { get; private set; }
    public List<RoomLayout> RoomLayouts { get; private set; }
    
    public (Point16 topLeft, Point16 bottomRight) BoundingBox { get; private set; }

    public List<IComponent> ExternalComponents { get; private set; }
    /// <summary>
    ///     any <see cref="Room" />s are at the very end of the list
    /// </summary>
    public List<IComponent> AllComponents { get; private set; }
    
    public Room[] Rooms {
        get {
            int len = 0;
            foreach (RoomLayout roomLayout in RoomLayouts) len += roomLayout.Rooms.Count;
            var rooms = new Room[len];
            int count = 0;
            foreach (RoomLayout roomLayout in RoomLayouts) {
                foreach (Room room in roomLayout.Rooms) {
                    rooms[count] = room;
                    count++;
                }
            }

            return rooms;
        }
    }

    public StructureLayout(StructureLayoutParams param, string name) : base(param, new TagMap(), name) {
    }

    public override void DrawDebugInfo() {
        Color color = DrawHelper.GetColor(Id);

        if (DebugInfoVisibility.DisplayBounds) {
            Point16 topLeftWorldPos = BoundingBox.topLeft * new Point16(16);
            DrawHelper.DrawRectangle(
                new Rectangle(
                    topLeftWorldPos.X,
                    topLeftWorldPos.Y,
                    (BoundingBox.bottomRight.X - BoundingBox.topLeft.X) * 16,
                    (BoundingBox.bottomRight.Y - BoundingBox.topLeft.Y) * 16
                ),
                color,
                DrawHelper.DebugDrawWidth
            );
        }

        if (DebugInfoVisibility.DisplayName) DrawHelper.DrawText(Name, BoundingBox.topLeft * new Point16(16) - new Point16(16, 16), color);

        if (HasAllComponents)
            foreach (IComponent component in AllComponents)
                component?.DrawDebugInfo();
        else if (HasExternalComponents) {
            foreach (IComponent component in ExternalComponents)
                component?.DrawDebugInfo();
        }
    }

    /// <summary>
    ///     sets the EXTERNAL components of this structure layout excluding gaps, and calls <see cref="UpdateComponentList" />
    /// </summary>
    /// <param name="externalFloors"></param>
    /// <param name="externalWalls"></param>
    /// <param name="roofs"></param>
    public void SetExternalComponents(List<Floor> externalFloors, List<Wall> externalWalls, List<Roof> roofs) {
        ExternalFloors = externalFloors;
        ExternalWalls = externalWalls;
        Roofs = roofs;
        
        ExternalComponents = [];
        HasExternalComponents = true;
        UpdateComponentList();
    }

    /// <summary>
    ///     sets the EXTERNAL gap components of this structure layout, calls <see cref="UpdateComponentList" />
    /// </summary>
    /// <param name="externalGaps"></param>
    public void SetExternalGapComponents(List<Gap> externalGaps) {
        ExternalGaps = externalGaps;

        HasExternalGaps = true;
        UpdateComponentList();
    }

    /// <summary>
    ///     sets the INTERIOR components of this structure layout, and calls <see cref="UpdateComponentList" />
    /// </summary>
    /// <param name="roomLayouts"></param>
    public void SetInternalComponents(List<RoomLayout> roomLayouts) {
        RoomLayouts = roomLayouts;

        AllComponents = [];
        HasAllComponents = true;
        UpdateComponentList();
    }

    /// <summary>
    ///     rebuilds <see cref="ExternalComponents"/> and <see cref="AllComponents" /> using the current lists of components.
    ///     also updates the bounding box
    /// </summary>
    public void UpdateComponentList() {
        if (HasExternalComponents) {
            ExternalComponents.Clear();
            ExternalComponents.AddRange(ExternalFloors);
            ExternalComponents.AddRange(ExternalWalls);
            ExternalComponents.AddRange(Roofs);

            if (HasExternalGaps)
                ExternalComponents.AddRange(ExternalGaps);

            int minX = int.MaxValue, minY = int.MaxValue, maxX = 0, maxY = 0;
            foreach (IComponent component in ExternalComponents) {
                if (component.Geometry.BoundingBox.topLeft.X < minX) minX = component.Geometry.BoundingBox.topLeft.X;
                if (component.Geometry.BoundingBox.topLeft.Y < minY) minY = component.Geometry.BoundingBox.topLeft.Y;
                if (component.Geometry.BoundingBox.bottomRight.X > maxX) maxX = component.Geometry.BoundingBox.bottomRight.X;
                if (component.Geometry.BoundingBox.bottomRight.Y > maxY) maxY = component.Geometry.BoundingBox.bottomRight.Y;
            }

            BoundingBox = (new Point16(minX, minY), new Point16(maxX, maxY));
        }

        if (HasAllComponents) {
            AllComponents.Clear();
            AllComponents.AddRange(ExternalComponents);
            foreach (RoomLayout roomLayout in RoomLayouts) {
                AllComponents.AddRange(roomLayout.Floors);
                AllComponents.AddRange(roomLayout.Walls);
                AllComponents.AddRange(roomLayout.Gaps);
            }

            // put rooms at the very end of the list
            foreach (RoomLayout roomLayout in RoomLayouts) AllComponents.AddRange(roomLayout.Rooms);
        }
    }

    /// <summary>
    ///     moves everything in the layout by the offset. ex. if offset = (3, 0) will move everything in the layout 3 to the right in world coordinates
    /// </summary>
    /// <param name="offset"></param>
    public void Offset(Point16 offset) {
        foreach (Floor floor in ExternalFloors) floor.Geometry.Move(offset);
        foreach (Wall wall in ExternalWalls) wall.Geometry.Move(offset);
        foreach (Gap gap in ExternalGaps) gap.Geometry.Move(offset);
        foreach (Roof roof in Roofs) roof.Geometry.Move(offset);
        foreach (RoomLayout roomSection in RoomLayouts) roomSection.Offset(offset);
    }
}