using System;

namespace SpawnHouses.Common.Types.Attributes;

[AttributeUsage(AttributeTargets.Class)]
public class AdvGeneratorLoadable(Type moduleType) : Attribute {
    public readonly Type ModuleType = moduleType;
}