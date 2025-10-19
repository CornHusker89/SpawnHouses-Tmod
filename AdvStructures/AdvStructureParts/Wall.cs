#nullable enable
using System.Collections.Generic;
using SpawnHouses.Types;

namespace SpawnHouses.AdvStructures.AdvStructureParts;

public class Wall : VolumeComponent, IExternalComponent {
    
    // IExternalComponent
    public bool IsExterior { get; set; }

    public Wall(Shape volume, bool isExterior = false) {
        TagsRequired = new Dictionary<ComponentTag, object?>(IsExterior ? [new KeyValuePair<ComponentTag, object?>(ComponentTag.External, null)] : []);
        TagsBlocklist = [];
        Volume = volume;
        IsExterior = isExterior;
    }
}