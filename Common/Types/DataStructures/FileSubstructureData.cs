namespace SpawnHouses.Common.Types.DataStructures;

/// <summary>
///     data-carrying struct for a <see cref="FileStructure" />'s substructures. can have 1 or more in a structure
/// </summary>
public readonly struct FileSubstructureData {
    public readonly string FilePath;

    /// <summary>
    ///     the regions that this substructure is valid for. if empty, it is valid for all regions.
    /// </summary>
    public readonly string ValidRegions;
}