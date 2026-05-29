using System;

namespace SpawnHouses.Common.Types;

[AttributeUsage(AttributeTargets.Class)]
public class ModuleGenerator(Type moduleType) : Attribute {
    public readonly Type ModuleType = moduleType;
}