using SpawnHouses.Types.TagTypes;

namespace SpawnHouses.Common.Parameters;

public interface IParams {
    /// <summary>
    ///     the parent structure of the instance
    /// </summary>
    public AdvStructure Structure { get; init; }

    /// <summary>
    ///     tags that are required for this object to exist
    /// </summary>
    public TagMap TagsRequired { get; }
}