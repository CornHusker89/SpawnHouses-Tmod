using SpawnHouses.Content.Tagging;
using SpawnHouses.Content.Types.RootStructureTypes;

namespace SpawnHouses.Content.Parameters;

public interface IParams {
    /// <summary>
    ///     the parent structure of the instance
    /// </summary>
    public AdvStructure Structure { get; set; }

    /// <summary>
    ///     tags that are required for this object to exist
    /// </summary>
    public TagMap TagsRequired { get; }
}