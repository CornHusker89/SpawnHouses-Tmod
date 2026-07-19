using System;

namespace SpawnHouses.StructureCommon.Types.Attributes;

/// <summary>
///     Marks a class as loadable by the advanced generator system.
/// </summary>
/// <param name="standalone">if this file can be placed standalone</param>
/// <param name="depreciated">if true, generator will only be used for rebuilding existing structures, and not for making new structures</param>
[AttributeUsage(AttributeTargets.Class)]
public class StructureTemplateLoadable(bool standalone, bool depreciated) : Attribute {
    /// <summary>
    ///     if this file can be placed standalone
    /// </summary>
    public readonly bool Standalone = standalone;

    /// <summary>
    ///     if true, generator will only be used for rebuilding existing structures, and not for making new structures
    /// </summary>
    public readonly bool Depreciated = depreciated;
}