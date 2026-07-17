namespace SpawnHouses.Common.Types.DataStructures;

/// <summary>
///     data-carrying struct for a <see cref="FileStructure" />'s substructures. can have 1 or more in a structure
/// </summary>
public readonly struct FileSubstructureData {
    /// <summary>
    ///     friendly name :)
    /// </summary>
    public readonly string Name;
    
    public readonly string FilePath;

    /// <summary>
    ///     the position IDs that this substructure is valid for. if empty, it is valid for all position IDs.
    /// </summary>
    public readonly string[] ValidPositionIds;
}