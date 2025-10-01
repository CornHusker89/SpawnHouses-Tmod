using System;
using SpawnHouses.AdvStructures.AdvStructureParts;
using SpawnHouses.Types;

namespace SpawnHouses.Helpers;

public static class ComponentUtils {
    public static ComponentParams CreateComponentParamsForType(IComponent component, TilePalette tilePalette, StructureTilemap tilemap) {
        Type componentType = component.GetType();
        if (componentType == typeof(VolumeComponentParams)) return new VolumeComponentParams((IVolumeComponent)component, tilePalette, tilemap);

        if (componentType == typeof(PathComponentParams)) return new PathComponentParams((IPathComponent)component, tilePalette, tilemap);

        throw new Exception($"Component type \"{componentType.FullName}\" does not have an associated parameter type");
    }

    /// <summary>
    ///     ensures that the passed component has no misconfigured data. throws error if it is misconfigured
    /// </summary>
    public static void ValidateComponent(IComponent component) {
        TagUtils.ValidateTagsRequired(component.TagsRequired);
        TagUtils.ValidateTagsBlacklist(component.TagsBlacklist);
    }
}