using System;
using System.Collections.Generic;
using System.Reflection;

namespace SpawnHouses.Common.Types;

public static class GlobalGeneratorUtils {
    /// <summary>type corresponds to the final component's type</summary>
    public static readonly Dictionary<Type, List<IGenerator>> InstanceGenerators = new();


    public static void LoadGenerators(Assembly assembly) {
        var pluginTypes = assembly.GetTypes();
        foreach (Type type in pluginTypes) {
            ModuleGenerator moduleInfo = type.GetCustomAttribute<ModuleGenerator>();

            if (moduleInfo != null) {
                if (!InstanceGenerators.TryGetValue(moduleInfo.ModuleType, out var generatorList))
                    InstanceGenerators[moduleInfo.ModuleType] = generatorList = [];
                generatorList.Add((IGenerator)Activator.CreateInstance(type));
            }
        }
    }

    internal static void LoadGenerators() {
        LoadGenerators(Assembly.GetExecutingAssembly());
    }
}