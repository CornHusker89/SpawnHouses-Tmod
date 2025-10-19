#nullable enable
using System;
using System.Collections.Generic;
using SpawnHouses.Types;

namespace SpawnHouses.AdvStructures.AdvStructureParts;

public abstract class Component {
    /// <summary>
    ///     unique identifier given to each component in a structure
    /// </summary>
    /// <remarks>set in <see cref="AdvStructure.FillComponents" /> during structure generation</remarks>
    public ushort Id { get; set; }

    public Dictionary<ComponentTag, object?> TagsRequired { get; set; }
    public HashSet<ComponentTag> TagsBlocklist { get; set; }

    public void AddRequiredTag(ComponentTag tag) {
        TagsRequired[tag] = null;
    }

    public void AddRequiredTag<T>(ComponentTag tag, T tagData) {
        TagsRequired[tag] = tagData;
    }

    public T GetTagData<T>(ComponentTag targetTag) {
        if (!TagsRequired.TryGetValue(targetTag, out object? value)) throw new Exception("targetTag not found within given tag list");

        if (value == null) throw new Exception($"tag data for {targetTag} is null, could not return any data");
        T typedValue = (T)value;
        if (typedValue == null) throw new Exception($"tag data for {targetTag} could not be cast to target type of \"{typeof(T).Name}\"");
        return typedValue;
    }
}

public abstract class VolumeComponent : Component {
    public Shape Volume { get; set; }
}

public abstract class PathComponent : Component {
    public Path Line { get; set; }
}

public interface IExternalComponent {
    public bool IsExterior { get; set; }
}