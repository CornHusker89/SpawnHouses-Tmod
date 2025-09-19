using System.Collections.Generic;
using SpawnHouses.Types;

namespace SpawnHouses.AdvStructures.AdvStructureParts;

public interface IComponent {
    /// <summary>
    ///     unique identifier given to each component in a structure
    /// </summary>
    /// <remarks>set in <see cref="AdvStructure.FillComponents" /> during structure generation</remarks>
    public ushort Id { get; set; }

    public Shape Volume { get; set; }
    public List<ComponentTag> TagsRequired { get; set; }
    public List<ComponentTag> TagsBlacklist { get; set; }
}