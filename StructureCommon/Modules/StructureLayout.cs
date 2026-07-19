using System.Collections.Generic;
using Microsoft.Xna.Framework;
using SpawnHouses.Helpers;
using SpawnHouses.StructureCommon.Debug;
using SpawnHouses.StructureCommon.Modules.Components;
using SpawnHouses.StructureCommon.Parameters;
using SpawnHouses.StructureCommon.Tagging;
using SpawnHouses.StructureCommon.Types;
using SpawnHouses.StructureCommon.Types.DataStructures;
using SpawnHouses.StructureCommon.Types.Interfaces;
using Terraria.DataStructures;

namespace SpawnHouses.StructureCommon.Modules;

#nullable enable

public class StructureLayout : AdvGeneratable<StructureLayout, StructureLayoutParams, StructureLayoutAdvGenerator>, IBoundingBox {
    private readonly DebugLabel _label;
    
    public string Name => Params.Structure.Name + "_Layout";

    public TileBox BoundingBox { get; private set; }

    public List<Floor>? ExternalFloors { get; private set; }

    public List<Wall>? ExternalWalls { get; private set; }

    public List<Gap>? ExternalGaps { get; private set; }

    public List<Roof>? Roofs { get; private set; }

    public List<RoomLayout>? RoomLayouts { get; private set; }

    public List<IComponent>? ExternalComponents { get; private set; }
    
    /// <summary>
    ///     any <see cref="Room" />s are at the very end of the list
    /// </summary>
    public List<IComponent>? AllComponents { get; private set; }
    
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
        _label = new DebugLabel(BoundingBox.TopLeftPoint16, this);
    }

    public override Color GetDrawColor() => DrawHelper.GetColor(Id);

    public override List<DebugLabel> DrawDebugGeometry() {
        if (DebugInfoVisibility.DisplayBounds) {
            Point16 topLeftLocalPos = Params.Structure.Tilemap.ConvertToGlobal(BoundingBox.TopLeftPoint16);
            DrawHelper.DrawWorldBasedBorder(
                new Rectangle(
                    topLeftLocalPos.X * 16 - DrawHelper.DebugDrawWidth,
                    topLeftLocalPos.Y * 16 - DrawHelper.DebugDrawWidth,
                    BoundingBox.Width * 16 + DrawHelper.DebugDrawWidth * 2,
                    BoundingBox.Height * 16 + DrawHelper.DebugDrawWidth * 2
                ),
                GetDrawColor(),
                DrawHelper.DebugDrawWidth
            );
        }

        List<DebugLabel> labels = [];
        if (AllComponents != null)
            foreach (IComponent component in AllComponents) {
                if (component != null)
                    labels.AddRange(component.DrawDebugGeometry());
            }
        else if (ExternalComponents != null) {
            foreach (IComponent component in ExternalComponents)
                if (component != null)
                    labels.AddRange(component.DrawDebugGeometry());
        }

        if (_label.IsVisible(Params.Structure.Tilemap.ConvertToGlobal(BoundingBox)))
            labels.Add(_label);

        return labels;
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
                if (component.Geometry.BoundingBox.Left < minX) minX = component.Geometry.BoundingBox.Left;
                if (component.Geometry.BoundingBox.Top < minY) minY = component.Geometry.BoundingBox.Top;
                if (component.Geometry.BoundingBox.Right > maxX) maxX = component.Geometry.BoundingBox.Right;
                if (component.Geometry.BoundingBox.Bottom > maxY) maxY = component.Geometry.BoundingBox.Bottom;
            }

            BoundingBox = new TileBox(new Point16(minX, minY), new Point16(maxX, maxY));
            _label.Root = BoundingBox.TopLeftPoint16;
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

        if (AllComponents != null)
            foreach (IComponent component in AllComponents)
                component.UpdateLabelRoot();
        else if (ExternalComponents != null)
            foreach (IComponent component in ExternalComponents)
                component.UpdateLabelRoot();
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