using System.Collections.Generic;
using SpawnHouses.Types;
using Terraria.DataStructures;

namespace SpawnHouses.AdvStructures.AdvStructureParts;

public class Roof : IPathComponent, IComponentExternalExt {
    
    // IComponent
    public ushort Id { get; set; }
    public HashSet<ComponentTag> TagsRequired { get; set; }
    public HashSet<ComponentTag> TagsBlacklist { get; set; }
    
    // IPathComponent
    public Path Line { get; set; }
    public (Point16 topLeft, Point16 bottomRight) BoundingBox { get; set; }

    // IComponentExternalExt
    public bool IsExterior { get; set; } = true;

    public Roof(Path line) {
        TagsRequired = [ComponentTag.External];
        TagsBlacklist = [];
        Line = line;
    }
}