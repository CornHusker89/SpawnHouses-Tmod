using System.Collections.Generic;
using SpawnHouses.Types;
using Terraria.DataStructures;

namespace SpawnHouses.AdvStructures.AdvStructureParts;

public interface IComponent {
    /// <summary>
    ///     unique identifier given to each component in a structure
    /// </summary>
    /// <remarks>set in <see cref="AdvStructure.FillComponents"/> during structure generation</remarks>
    public ushort Id { get; set; }

    public HashSet<ComponentTag> TagsRequired { get; set; }
    public HashSet<ComponentTag> TagsBlacklist { get; set; }
}

public interface IVolumeComponent : IComponent {
    public Shape Volume { get; set; }
}

public interface IPathComponent : IComponent {
    public List<Point16> Path { get; set; }
    public (Point16 topLeft, Point16 bottomRight) BoundingBox { get; set; }
}

public interface IComponentExternalExt : IComponent {
    public bool IsExterior { get; set; }
}