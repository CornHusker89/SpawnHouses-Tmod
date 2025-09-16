using System.Collections.Generic;
using SpawnHouses.Types;

namespace SpawnHouses.AdvStructures.AdvStructureParts;

public class Roof : IComponent {
    public ushort Id { get; set; }
    public Shape Volume { get; set; }
    public List<ComponentTag> TagsRequired { get; set; }
    public List<ComponentTag> TagsBlacklist { get; set; }

    public Roof(Shape volume) {
        Volume = volume;
        TagsRequired = [ComponentTag.External];
        TagsBlacklist = [];
    }
}