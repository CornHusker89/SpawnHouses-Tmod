using System;

namespace SpawnHouses.StructureCommon.Types.Attributes;

/// <summary>
///     Marks a class as loadable by the advanced generator system.
/// </summary>
/// <param name="moduleType">the type of generatable this class makes</param>
/// <param name="depreciated">if true, generator will only be used for rebuilding existing structures, and not for making new structures</param>
[AttributeUsage(AttributeTargets.Class)]
public class AdvGeneratorLoadable(Type moduleType, bool depreciated) : Attribute {
    /// <summary>
    ///     the type of generatable this class makes
    /// </summary>
    public readonly Type ModuleType = moduleType;

    /// <summary>
    ///     if true, generator will only be used for rebuilding existing structures, and not for making new structures
    /// </summary>
    public readonly bool Depreciated = depreciated;
}