using System.Collections.Generic;
using SpawnHouses.Types;

namespace SpawnHouses.AdvStructures.AdvStructureParts;

public class Floor : IComponent {
    public ushort Id { get; set; }
    public Shape Volume { get; set; }
    public List<ComponentTag> TagsRequired { get; set; }
    public List<ComponentTag> TagsBlacklist { get; set; }

    public bool IsExterior;

    public Floor(Shape volume, bool isExterior = false) {
        Volume = volume;
        IsExterior = isExterior;
        if (IsExterior) {
            TagsRequired = [ComponentTag.External];
        }
        else {
            TagsRequired = [];
        }
        TagsBlacklist = [];
    }
}