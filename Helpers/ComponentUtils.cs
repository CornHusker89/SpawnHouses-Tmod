using System;
using SpawnHouses.AdvStructures;
using SpawnHouses.AdvStructures.AdvStructureParts;
using SpawnHouses.Types;

namespace SpawnHouses.Helpers;

public static class ComponentUtils {
    public static ComponentParams CreateParamsForComponent(Component component, AdvStructure structure) {
        Type componentType = component.GetType();
        if (componentType.IsSubclassOf(typeof(VolumeComponent)))
            return new VolumeComponentParams((VolumeComponent)component, structure);

        if (componentType.IsSubclassOf(typeof(PathComponent)))
            return new PathComponentParams((PathComponent)component, structure);

        throw new Exception($"Component type \"{componentType.FullName}\" does not have an associated parameter type");
    }
}