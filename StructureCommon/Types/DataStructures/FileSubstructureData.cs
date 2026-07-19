using SpawnHouses.StructureCommon.Types.RootStructureTypes;
using Terraria.DataStructures;

namespace SpawnHouses.StructureCommon.Types.DataStructures;

/// <summary>
///     data-carrying struct for a <see cref="FileStructure" />'s substructures. can have 1 or more in a structure
/// </summary>
public readonly struct FileSubstructureData {
    /// <summary>
    ///     friendly name :)
    /// </summary>
    public readonly string Name;
    
    public readonly string FilePath;

    public readonly Point16 Size;

    /// <summary>
    ///     the position IDs that this substructure is valid for. if empty, it is valid for all position IDs.
    /// </summary>
    public readonly string[] ValidPositionIds;
}