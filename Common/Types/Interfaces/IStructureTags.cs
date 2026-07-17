using SpawnHouses.Common.Tagging;

namespace SpawnHouses.Common.Types.Interfaces;

public interface IStructureTags {
    /// <summary>
    ///     tags that this instance currently has
    /// </summary>
    public TagMap TagsCurrent { get; }
}