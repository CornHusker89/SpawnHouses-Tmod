using SpawnHouses.Core.RootStructureTypes;
using SpawnHouses.Core.Tagging;

namespace SpawnHouses.Core.Parameters;

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