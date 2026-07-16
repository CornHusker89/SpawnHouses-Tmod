using System;
using System.Collections.Generic;
using System.Reflection;
using SpawnHouses.Common.Types.Interfaces;

namespace SpawnHouses.Common.Types;

public static class GlobalGeneratorUtils {
    /// <summary>type corresponds to the final component's type</summary>
    public static readonly Dictionary<Type, List<IAdvGenerator>> InstanceGenerators = new();


    public static void LoadGenerators(Assembly assembly) {
        var pluginTypes = assembly.GetTypes();
        foreach (Type type in pluginTypes) {
            ModuleGenerator moduleInfo = type.GetCustomAttribute<ModuleGenerator>();

            if (moduleInfo != null) {
                if (!InstanceGenerators.TryGetValue(moduleInfo.ModuleType, out var generatorList))
                    InstanceGenerators[moduleInfo.ModuleType] = generatorList = [];
                generatorList.Add((IAdvGenerator)Activator.CreateInstance(type));
            }
        }
    }

    internal static void LoadGenerators() {
        LoadGenerators(Assembly.GetExecutingAssembly());
    }
}