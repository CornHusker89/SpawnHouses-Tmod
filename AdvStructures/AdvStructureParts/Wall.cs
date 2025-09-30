using System.Collections.Generic;
using SpawnHouses.Types;

namespace SpawnHouses.AdvStructures.AdvStructureParts;

public class Wall : IComponent, IVolumeComponent, IComponentExternalExt {
    // IComponent
    public ushort Id { get; set; }
    public HashSet<ComponentTag> TagsRequired { get; set; }
    public HashSet<ComponentTag> TagsBlacklist { get; set; }

    // IVolumeComponent
    public Shape Volume { get; set; }

    // IExternalComponent
    public bool IsExterior { get; set; }

    public Wall(Shape volume, bool isExterior = false) {
        TagsRequired = IsExterior ? [ComponentTag.External] : [];
        TagsBlacklist = [];
        Volume = volume;
        IsExterior = isExterior;
    }
}