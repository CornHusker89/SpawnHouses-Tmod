using System.Collections.Generic;
using SpawnHouses.Types;
using Terraria.DataStructures;

namespace SpawnHouses.AdvStructures.AdvStructureParts;

public class Roof : IComponent, IPathComponent, IComponentExternalExt {
    // IComponent
    public ushort Id { get; set; }
    public HashSet<ComponentTag> TagsRequired { get; set; }
    public HashSet<ComponentTag> TagsBlacklist { get; set; }

    // IPathComponent
    public List<Point16> Path { get; set; }
    public (Point16 topLeft, Point16 bottomRight) BoundingBox { get; set; }

    // IExternalComponent
    public bool IsExterior { get; set; } = true;

    public Roof(List<Point16> path, (Point16 topLeft, Point16 bottomRight) boundingBox) {
        TagsRequired = [ComponentTag.External];
        TagsBlacklist = [];
        Path = path;
        BoundingBox = boundingBox;
    }
}