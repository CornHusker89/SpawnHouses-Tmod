#nullable enable
using SpawnHouses.Types;

namespace SpawnHouses.AdvStructures.AdvStructureParts;

public abstract class Component : ComponentTagSystem {
    /// <summary>
    ///     unique identifier given to each component in a structure
    /// </summary>
    /// <remarks>set in <see cref="AdvStructure.FillComponents" /> during structure generation</remarks>
    public ushort Id;

    /// <summary>
    ///     hash for the generator's signature used to fill this component
    /// </summary>
    /// <remarks>set in <see cref="AdvStructure.FillComponents" /> during structure generation</remarks>
    public int GeneratorId;
}

public abstract class VolumeComponent : Component {
    public Shape Volume { get; set; }
}

public abstract class PathComponent : Component {
    public Path Line { get; set; }
}

public interface IExternalComponent {
    public bool IsExterior { get; set; }
}