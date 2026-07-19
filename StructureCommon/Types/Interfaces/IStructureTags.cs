using SpawnHouses.StructureCommon.Tagging;

namespace SpawnHouses.StructureCommon.Types.Interfaces;

public interface IStructureTags {
    /// <summary>
    ///     tags that this instance currently has
    /// </summary>
    public TagMap TagsCurrent { get; }
}