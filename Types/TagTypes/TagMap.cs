#nullable enable
using System;
using System.Collections.Generic;
using System.Linq;

namespace SpawnHouses.Types.TagTypes;

public record Tag(ushort Id);

public sealed record Tag<TValue>(ushort Id) : Tag(Id);

public abstract record TagNull;

public sealed class TagMap {
    private static readonly HashSet<Tag>[] ExclusiveTagsRequired = [
    ];

    private static readonly HashSet<Tag>[] ExclusiveTagsCurrent = [
    ];

    private readonly Dictionary<Tag, object?> _data = new();

    public Tag[] Keys => _data.Keys.ToArray();
    public HashSet<Tag> KeysSet => _data.Keys.ToHashSet();

    /// <summary>
    ///     set the value for a specific untyped tag
    /// </summary>
    /// <param name="tag"></param>
    /// <param name="value">dummy param, not used. will always be replaced with null</param>
    private void Set(Tag tag, object? value) => _data[tag] = null;

    /// <summary>
    ///     set the value for a specific tag
    /// </summary>
    /// <param name="tag"></param>
    /// <param name="value"></param>
    /// <typeparam name="T"></typeparam>
    public void Set<T>(Tag<T> tag, T? value) => _data[tag] = value!;

    /// <summary>
    ///     sets a specific untyped tag. data value set is always null
    /// </summary>
    /// <param name="tag"></param>
    public void Set(Tag tag) => _data[tag] = null;

    /// <summary>
    ///     if a tag exists in this tag map
    /// </summary>
    /// <param name="tag"></param>
    /// <returns></returns>
    public bool HasTag(Tag tag) => _data.ContainsKey(tag);

    /// <summary>
    ///     gets the data associated with a tag. throws if tag isn't in map. see <see cref="GetValueSafe{T}" /> for safe version
    /// </summary>
    /// <param name="tag"></param>
    /// <typeparam name="T"></typeparam>
    /// <returns></returns>
    public T GetValue<T>(Tag<T> tag) => (T)_data[tag]!;

    /// <summary>
    ///     gets the data associated with a tag. returns true if tag is in the map. see <see cref="GetValue{T}" /> for unsafe version
    /// </summary>
    /// <param name="tag"></param>
    /// <param name="value"></param>
    /// <typeparam name="T"></typeparam>
    /// <returns></returns>
    public bool GetValueSafe<T>(Tag<T> tag, out T value) {
        if (_data.TryGetValue(tag, out object? obj)) {
            value = (T)obj!; // because the tag is typed, this shouldn't ever be null
            return true;
        }

        value = default!;
        return false;
    }

    /// <summary>
    ///     merge another tag map into this one
    /// </summary>
    /// <param name="tagMap"></param>
    /// <param name="throwException">if true, will throw if there is duplicate tags. otherwise, the other <paramref name="tagMap" /> will overwrite this one</param>
    /// <exception cref="Exception"></exception>
    public void Append(TagMap tagMap, bool throwException) {
        var thisKeys = Keys;
        foreach (Tag tag in tagMap.Keys) {
            if (throwException && thisKeys.Contains(tag))
                throw new Exception($"this TagMap already contains key {tag}");
            Set(tag, tagMap._data[tag]);
        }
    }

    /// <summary>
    ///     internal method to check this map's tags against a specific mutually exclusive tag set
    /// </summary>
    /// <param name="exclusiveSets"></param>
    /// <exception cref="Exception"></exception>
    private void ValidateExclusiveTags(HashSet<Tag>[] exclusiveSets) {
        foreach (var exclusiveSet in exclusiveSets) {
            if (!exclusiveSet.IsSubsetOf(KeysSet)) continue;

            string message = "Component has mutually exclusive component tags required {";
            foreach (Tag tag in exclusiveSet) message += $"{tag}), ";
            throw new Exception(message.Remove(message.Length - 2));
        }
    }

    /// <summary>
    ///     throws error if any tags are mutually exclusive in the TagsRequired category
    /// </summary>
    public void ValidateExclusiveRequiredTags() {
        ValidateExclusiveTags(ExclusiveTagsRequired);
    }

    /// <summary>
    ///     throws error if any tags are mutually exclusive in the TagsCurrent category
    /// </summary>
    public void ValidateExclusiveCurrentTags() {
        ValidateExclusiveTags(ExclusiveTagsCurrent);
    }
}