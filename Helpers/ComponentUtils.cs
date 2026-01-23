using System;
using SpawnHouses.Common;
using SpawnHouses.Common.Modules;
using SpawnHouses.Common.Parameters;

namespace SpawnHouses.Helpers;

public static class ComponentUtils {
    public static IParams CreateParamsForComponent(IComponent component, AdvStructure structure) {
        Type componentType = component.GetType();
        if (componentType.IsSubclassOf(typeof(VolumeComponent)))
            return new VolumeComponentParams(structure);

        if (componentType.IsSubclassOf(typeof(PathComponent)))
            return new PathComponentParams(structure);

        throw new Exception($"Component type \"{componentType.FullName}\" does not have an associated parameter type");
    }
}