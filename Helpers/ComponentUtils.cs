using System;
using SpawnHouses.AdvStructures;
using SpawnHouses.AdvStructures.AdvStructureParts;
using SpawnHouses.Types;

namespace SpawnHouses.Helpers;

public static class ComponentUtils {
    public static ComponentParams CreateParamsForComponent(IComponent component, AdvStructure structure) {
        Type componentType = component.GetType();
        if (componentType.IsSubclassOf(typeof(VolumeComponent)))
            return new VolumeComponentParams(structure);

        if (componentType.IsSubclassOf(typeof(PathComponent)))
            return new PathComponentParams(structure);

        throw new Exception($"Component type \"{componentType.FullName}\" does not have an associated parameter type");
    }
}