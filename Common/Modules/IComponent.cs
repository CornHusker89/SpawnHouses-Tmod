using SpawnHouses.Common.Parameters;
using SpawnHouses.Common.Tagging;
using SpawnHouses.Common.Types;
using SpawnHouses.Common.Types.Geometry;
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
    public Shape Geometry { get; set; }

    protected VolumeComponent(VolumeComponentParams param, Shape geometry) : base(param, new TagMap()) {
        Geometry = geometry;
    }

    public override void DrawDebugInfo() {
        // Color color = DrawHelper.GetColor(this);
        //
        // if (DebugInfoVisibility.HasHitboxes) {
        //     Point16 tilemapOffsetWorldCoords = Params.Structure.Tilemap.globalTileOffset * new Point16(16);
        //     var path = new Point16[Geometry.ExteriorDrawPath.Length];
        //     for (int i = 0; i < path.Length; i++) {
        //         path[i] = Geometry.ExteriorDrawPath[i] + tilemapOffsetWorldCoords;
        //     }
        //     DrawHelper.DrawPath(path, color, 3);
        // }
    }
}

public abstract class PathComponent : Generatable<PathComponent, PathComponentParams, PathComponentGenerator>, IComponent<Path> {
    public Path Geometry { get; set; }

    protected PathComponent(PathComponentParams param, Path geometry) : base(param, new TagMap()) {
        Geometry = geometry;
    }

    public Shape GetBoundingShape() {
        if (Generator == null) SetGenerator();

        return Generator!.GetBoundingShape(Params, Geometry, new UnifiedRandom(Id));
    }
}