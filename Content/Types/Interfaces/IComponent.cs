using System.Collections.Generic;
using Microsoft.Xna.Framework;
using SpawnHouses.Content.Debug;
using SpawnHouses.Content.Parameters;
using SpawnHouses.Content.Tagging;
using SpawnHouses.Content.Types.Geometry;
using SpawnHouses.Helpers;
using Terraria;
using Terraria.DataStructures;
using Terraria.Utilities;

namespace SpawnHouses.Content.Types.Interfaces;

public interface IComponent : IAdvGeneratable {
    /// <summary>
    ///     geometry that this component will occupy. set by the shape in the params at the time of component generation
    /// </summary>
    public PointGeometry Geometry { get; }

    /// <summary>
    ///     updates the internal debug label's root point
    /// </summary>
    public void UpdateLabelRoot();
}

public interface IComponent<out TGeometry> : IComponent
    where TGeometry : PointGeometry {
    PointGeometry IComponent.Geometry => Geometry;

    /// <inheritdoc cref="IComponent.Geometry" />
    public new TGeometry Geometry { get; }
}

public abstract class VolumeComponent : AdvGeneratable<VolumeComponent, VolumeComponentParams, VolumeComponentAdvGenerator>, IComponent<Shape> {
    private readonly DebugLabel _label;
    public Shape Geometry { get; set; }

    protected VolumeComponent(VolumeComponentParams param, Shape geometry, string name) : base(param, new TagMap(), name) {
        _label = new DebugLabel(Point16.Zero, this);
        Geometry = geometry;
    }

    public override Color GetDrawColor() => DrawHelper.GetColor(this);

    public override List<DebugLabel> DrawDebugGeometry() {
        Color color = DrawHelper.GetColor(this);
        Point tilemapOffsetWorldCoords = Params.Structure.Tilemap.GlobalTileOffset.ToPoint() * new Point(16, 16);
        
        if (DebugInfoVisibility.DisplayBounds) {
            var path = new Point[Geometry.ExteriorDrawPath.Length];
            for (int i = 0; i < path.Length; i++) path[i] = Geometry.ExteriorDrawPath[i] + tilemapOffsetWorldCoords;
            DrawHelper.DrawWorldBasedRectangularPath(path, color, DrawHelper.DebugDrawWidth);
        }

        if (DebugInfoVisibility.DisplayPoints) {
            var points = new Point[Geometry.Points.Length];
            for (int i = 0; i < points.Length; i++) points[i] = Geometry.Points[i].ToPoint() * new Point(16, 16) + tilemapOffsetWorldCoords + new Point(8, 8);
            DrawHelper.DrawWorldBasedPoints(points, color, DrawHelper.DebugDrawWidth * 2);
        }

        return _label.IsVisible(Params.Structure.Tilemap.ConvertToGlobal(Geometry.BoundingBox)) ? [_label] : [];
    }

    public void UpdateLabelRoot() {
        _label.Root = Params.Structure.Tilemap.ConvertToGlobal(Geometry.BoundingBox.CenterPoint16);
    }
}

public abstract class PathComponent : AdvGeneratable<PathComponent, PathComponentParams, PathComponentAdvGenerator>, IComponent<Path> {
    private readonly DebugLabel _label;
    public Path Geometry { get; set; }

    protected PathComponent(PathComponentParams param, Path geometry, string name) : base(param, new TagMap(), name) {
        _label = new DebugLabel(Point16.Zero, this);
        Geometry = geometry;
    }
    
    public Shape GetBoundingShape() {
        if (Generator == null) SetGenerator();

        return Generator!.GetBoundingShape(Params, Geometry, new UnifiedRandom(Id));
    }

    public override Color GetDrawColor() => DrawHelper.GetColor(this);

    public override List<DebugLabel> DrawDebugGeometry() {
        Color color = DrawHelper.GetColor(this);

        if (DebugInfoVisibility.DisplayBounds) {
        }

        if (DebugInfoVisibility.DisplayPoints) {
            var points = new Point[Geometry.Points.Length];
            for (int i = 0; i < points.Length; i++) points[i] = Geometry.Points[i].ToPoint() * new Point(16, 16) + new Point(8, 8);
            DrawHelper.DrawWorldBasedPoints(points, color, 6);
        }

        return _label.IsVisible(Params.Structure.Tilemap.ConvertToGlobal(Geometry.BoundingBox)) ? [_label] : [];
    }

    public void UpdateLabelRoot() {
        _label.Root = Params.Structure.Tilemap.ConvertToGlobal(Geometry.Points[0]);
    }
}