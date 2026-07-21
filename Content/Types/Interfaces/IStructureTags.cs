using SpawnHouses.Content.Tagging;

namespace SpawnHouses.Content.Types.Interfaces;

public interface IStructureTags {
    /// <summary>
    ///     tags that this instance currently has
    /// </summary>
    public TagMap TagsCurrent { get; }
}