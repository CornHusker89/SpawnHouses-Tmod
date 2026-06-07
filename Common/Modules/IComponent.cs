using System.Collections.Generic;
using Microsoft.Xna.Framework;
using SpawnHouses.Common.Debug;
using SpawnHouses.Common.Parameters;
using SpawnHouses.Common.Tagging;
using SpawnHouses.Common.Types;
using SpawnHouses.Common.Types.Geometry;
using SpawnHouses.Helpers;
using Terraria;
using Terraria.Utilities;

namespace SpawnHouses.Common.Modules;

public interface IComponent : IGeneratable {
    /// <summary>
    ///     geometry that this component will occupy. set by the shape in the params at the time of component generation
    /// </summary>
    public PointGeometry Geometry { get; }
}

public interface IComponent<out TGeometry> : IComponent
    where TGeometry : PointGeometry {
    PointGeometry IComponent.Geometry => Geometry;

    /// <inheritdoc cref="IComponent.Geometry" />
    public new TGeometry Geometry { get; }
}

public abstract class VolumeComponent : Generatable<VolumeComponent, VolumeComponentParams, VolumeComponentGenerator>, IComponent<Shape> {
    private readonly DebugLabel _label;
    public Shape Geometry { get; set; }

    protected VolumeComponent(VolumeComponentParams param, Shape geometry, string name) : base(param, new TagMap(), name) {
        _label = new DebugLabel(geometry.BoundingBox.CenterPoint16(), this);
        Geometry = geometry;
    }

    public override List<DebugLabel> DrawDebugInfo() {
        Color color = DrawHelper.GetColor(this);
        Point tilemapOffsetWorldCoords = Params.Structure.Tilemap.GlobalTileOffset.ToPoint() * new Point(16, 16);
        
        if (DebugInfoVisibility.DisplayBounds) {
            var path = new Point[Geometry.ExteriorDrawPath.Length];
            for (int i = 0; i < path.Length; i++) path[i] = Geometry.ExteriorDrawPath[i].ToPoint() + tilemapOffsetWorldCoords;
            DrawHelper.DrawWorldBasedRectangularPath(path, color, DrawHelper.DebugDrawWidth);
        }

        if (DebugInfoVisibility.DisplayPoints) {
            var points = new Point[Geometry.Points.Length];
            for (int i = 0; i < points.Length; i++) points[i] = Geometry.Points[i].ToPoint() * new Point(16, 16) + tilemapOffsetWorldCoords + new Point(8, 8);
            DrawHelper.DrawWorldBasedPoints(points, color, DrawHelper.DebugDrawWidth * 2);
        }

        return _label.IsVisible(Geometry.BoundingBox) ? [_label] : [];
    }
}

public abstract class PathComponent : Generatable<PathComponent, PathComponentParams, PathComponentGenerator>, IComponent<Path> {
    private readonly DebugLabel _label;
    public Path Geometry { get; set; }

    protected PathComponent(PathComponentParams param, Path geometry, string name) : base(param, new TagMap(), name) {
        _label = new DebugLabel(geometry.Points[0], this);
        Geometry = geometry;
    }

    public Shape GetBoundingShape() {
        if (Generator == null) SetGenerator();

        return Generator!.GetBoundingShape(Params, Geometry, new UnifiedRandom(Id));
    }

    public override List<DebugLabel> DrawDebugInfo() {
        Color color = DrawHelper.GetColor(this);

        if (DebugInfoVisibility.DisplayBounds) {
        }

        if (DebugInfoVisibility.DisplayPoints) {
            var points = new Point[Geometry.Points.Length];
            for (int i = 0; i < points.Length; i++) points[i] = Geometry.Points[i].ToPoint() * new Point(16, 16) + new Point(8, 8);
            DrawHelper.DrawWorldBasedPoints(points, color, 6);
        }

        return _label.IsVisible(Geometry.BoundingBox) ? [_label] : [];
    }
}