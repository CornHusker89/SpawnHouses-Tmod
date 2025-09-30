using System.Collections.Generic;
using SpawnHouses.Types;

namespace SpawnHouses.AdvStructures.AdvStructureParts;

public class Floor : IVolumeComponent, IComponentExternalExt {
    // IVolumeComponent
    public ushort Id { get; set; }
    public HashSet<ComponentTag> TagsRequired { get; set; }
    public HashSet<ComponentTag> TagsBlacklist { get; set; }
    public Shape Volume { get; set; }

    // IExternalComponent
    public bool IsExterior { get; set; }

    public Floor(Shape volume, bool isExterior = false) {
        TagsRequired = IsExterior ? [ComponentTag.External] : [];
        TagsBlacklist = [];
        Volume = volume;
        IsExterior = isExterior;
    }
}