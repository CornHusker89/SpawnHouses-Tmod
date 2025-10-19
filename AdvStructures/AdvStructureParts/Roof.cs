#nullable enable
using System.Collections.Generic;
using SpawnHouses.Types;

namespace SpawnHouses.AdvStructures.AdvStructureParts;

public class Roof : PathComponent, IExternalComponent {

    // IComponentExternalExt
    public bool IsExterior { get; set; } = true;

    public Roof(Path line) {
        TagsRequired = new Dictionary<ComponentTag, object?>([new KeyValuePair<ComponentTag, object?>(ComponentTag.External, null)]);
        TagsBlocklist = [];
        Line = line;
    }
}