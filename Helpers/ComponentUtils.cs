using System;
using System.Linq;
using SpawnHouses.AdvStructures.AdvStructureParts;
using SpawnHouses.Types;
using SpawnHouses.Types.Palette;

namespace SpawnHouses.Helpers;

public static class ComponentUtils {
    public static ComponentParams CreateComponentParamsForType(Component component, TilePalette tilePalette, StructureTilemap tilemap) {
        Type componentType = component.GetType();
        if (componentType.IsSubclassOf(typeof(VolumeComponent))) return new VolumeComponentParams((VolumeComponent)component, tilePalette, tilemap);

        if (componentType.IsSubclassOf(typeof(PathComponent))) return new PathComponentParams((PathComponent)component, tilePalette, tilemap);

        throw new Exception($"Component type \"{componentType.FullName}\" does not have an associated parameter type");
    }

    /// <summary>
    ///     ensures that the passed component has no misconfigured data. throws error if it is misconfigured
    /// </summary>
    public static void ValidateComponent(Component component) {
        var tagsRequiredSet = component.TagsRequired.Keys.ToHashSet();
        TagUtils.ValidateExclusiveTagsRequired(tagsRequiredSet);
        TagUtils.ValidateTagDataTypes(component.TagsRequired);
    }
}