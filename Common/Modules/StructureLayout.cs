using System.Collections.Generic;
using JetBrains.Annotations;
using Microsoft.Xna.Framework;
using SpawnHouses.Common.Modules.Components;
using SpawnHouses.Common.Parameters;
using SpawnHouses.Common.Tagging;
using SpawnHouses.Common.Types;
using SpawnHouses.Helpers;
using Terraria;
using Terraria.DataStructures;

namespace SpawnHouses.Common.Modules;

public class StructureLayout : Generatable<StructureLayout, StructureLayoutParams, StructureLayoutGenerator> {
    public string Name => Params.Structure.Name + "_Layout";
    
    [CanBeNull]
    public List<Floor> ExternalFloors { get; private set; }

    [CanBeNull]
    public List<Wall> ExternalWalls { get; private set; }

    [CanBeNull]
    public List<Gap> ExternalGaps { get; private set; }

    [CanBeNull]
    public List<Roof> Roofs { get; private set; }

    [CanBeNull]
    public List<RoomLayout> RoomLayouts { get; private set; }
    
    public (Point16 topLeft, Point16 bottomRight) BoundingBox { get; private set; }

    [CanBeNull]
    public List<IComponent> ExternalComponents { get; private set; }
    /// <summary>
    ///     any <see cref="Room" />s are at the very end of the list
    /// </summary>
    [CanBeNull]
    public List<IComponent> AllComponents { get; private set; }
    
    public Room[] Rooms {
        get {
            int len = 0;
            foreach (RoomLayout roomLayout in RoomLayouts!) len += roomLayout.Rooms.Count;
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
            Point16 topLeftWorldPos = Params.Structure.Tilemap.ConvertToGlobal(BoundingBox.topLeft) * new Point16(16);
            DrawHelper.DrawWorldBasedBorder(
                new Rectangle(
                    topLeftWorldPos.X - DrawHelper.DebugDrawWidth,
                    topLeftWorldPos.Y - DrawHelper.DebugDrawWidth,
                    (BoundingBox.bottomRight.X - BoundingBox.topLeft.X + 1) * 16 + DrawHelper.DebugDrawWidth * 2,
                    (BoundingBox.bottomRight.Y - BoundingBox.topLeft.Y + 1) * 16 + DrawHelper.DebugDrawWidth * 2
                ),
                color,
                DrawHelper.DebugDrawWidth
            );
        }

        if (DebugInfoVisibility.DisplayName) DrawHelper.DrawWorldBasedText(Name, BoundingBox.topLeft.ToPoint() * new Point(16, 16) - new Point(16, 16), color);

        if (AllComponents != null)
            foreach (IComponent component in AllComponents)
                component?.DrawDebugInfo();
        else if (ExternalComponents != null) {
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
        
        UpdateComponentList();
    }

    /// <summary>
    ///     sets the EXTERNAL gap components of this structure layout, calls <see cref="UpdateComponentList" />
    /// </summary>
    /// <param name="externalGaps"></param>
    public void SetExternalGapComponents(List<Gap> externalGaps) {
        ExternalGaps = externalGaps;
        
        UpdateComponentList();
    }

    /// <summary>
    ///     sets the INTERIOR components of this structure layout, and calls <see cref="UpdateComponentList" />
    /// </summary>
    /// <param name="roomLayouts"></param>
    public void SetInternalComponents(List<RoomLayout> roomLayouts) {
        RoomLayouts = roomLayouts;
        
        UpdateComponentList();
    }

    /// <summary>
    ///     rebuilds <see cref="ExternalComponents"/> and <see cref="AllComponents" /> using the current lists of components.
    ///     also updates the bounding box
    /// </summary>
    public void UpdateComponentList() {
        if (ExternalFloors != null && ExternalWalls != null && Roofs != null) {
            ExternalComponents = [];
            ExternalComponents!.AddRange(ExternalFloors);
            ExternalComponents.AddRange(ExternalWalls);
            ExternalComponents.AddRange(Roofs);

            if (ExternalGaps != null)
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

        if (ExternalComponents != null && ExternalGaps != null && RoomLayouts != null) {
            AllComponents = [];
            AllComponents!.AddRange(ExternalComponents);
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
        if (ExternalFloors != null)
            foreach (Floor floor in ExternalFloors)
                floor.Geometry.Move(offset);
        if (ExternalWalls != null)
            foreach (Wall wall in ExternalWalls)
                wall.Geometry.Move(offset);
        if (ExternalGaps != null)
            foreach (Gap gap in ExternalGaps)
                gap.Geometry.Move(offset);
        if (Roofs != null)
            foreach (Roof roof in Roofs)
                roof.Geometry.Move(offset);
        if (RoomLayouts != null)
            foreach (RoomLayout roomSection in RoomLayouts)
                roomSection.Offset(offset);
    }
}