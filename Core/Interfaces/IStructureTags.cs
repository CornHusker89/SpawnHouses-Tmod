using SpawnHouses.Core.Tagging;

namespace SpawnHouses.Core.Interfaces;

public interface IStructureTags {
    /// <summary>
    ///     tags that this instance currently has
    /// </summary>
    public TagMap TagsCurrent { get; }
}