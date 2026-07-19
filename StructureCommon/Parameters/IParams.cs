using SpawnHouses.StructureCommon.Tagging;
using SpawnHouses.StructureCommon.Types.RootStructureTypes;

namespace SpawnHouses.StructureCommon.Parameters;

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