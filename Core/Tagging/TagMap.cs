#nullable enable
using System;
using System.Collections.Generic;
using System.Linq;
using SpawnHouses.Core.Interfaces;

namespace SpawnHouses.Core.Tagging;

public record Tag(string? Name = null) {
    public string? Name { get; set; } = Name;

    public override string ToString() => "Tag " + Name;
}

public sealed record Tag<TValue>(string? Name = null) : Tag(Name);

public sealed class TagMap {
    private static readonly HashSet<Tag>[] ExclusiveTagsRequired = [
        [Tags.Component_HasCustomSloping, Tags.Component_SlopingAlgorithm],
        [Tags.Component_HasCustomSloping, Tags.Component_SlopeGrouping]
    ];

    private static readonly HashSet<Tag>[] ExclusiveTagsCurrent = [
    ];

    public static HashSet<Tag> NewTagSet(HashSet<Tag> tagSet, params HashSet<Tag>[] otherTagSets) {
        foreach (var otherTagSet in otherTagSets) tagSet.UnionWith(otherTagSet);
        return tagSet;
    }

    public static HashSet<Tag> NewTagSet(HashSet<Tag> tagSet, HashSet<Tag>[] excludedSets, params HashSet<Tag>[] otherTagSets) {
        foreach (var otherTagSet in otherTagSets) tagSet.UnionWith(otherTagSet);
        foreach (var excludedSet in excludedSets)
        foreach (Tag excludedTag in excludedSet)
            tagSet.Remove(excludedTag);
        return tagSet;
    }

    /// <summary>
    ///     adds the given tag to the <see cref="IAdvGeneratable.Params.TagsRequired" /> of each advGeneratable
    /// </summary>
    /// <param name="generatables"></param>
    /// <param name="tag"></param>
    /// <param name="value"></param>
    /// <typeparam name="T"></typeparam>
    public static void AddRequiredToEach<T>(IEnumerable<IAdvGeneratable> generatables, Tag<T> tag, T? value) {
        foreach (IAdvGeneratable generatable in generatables) generatable.Params.TagsRequired.Add(tag, value);
    }

    /// <summary>
    ///     adds the given tag to the <see cref="IAdvGeneratable.Params.TagsRequired" /> of each advGeneratable
    /// </summary>
    /// <param name="generatables"></param>
    /// <param name="tag"></param>
    public static void AddRequiredToEach(IEnumerable<IAdvGeneratable> generatables, Tag tag) {
        foreach (IAdvGeneratable generatable in generatables) generatable.Params.TagsRequired.Add(tag);
    }

    private readonly Dictionary<Tag, object?> _data;

    /// <summary>
    ///     if true, this TagMap cannot be modified but can be accessed. initialized as false
    /// </summary>
    public bool IsLocked = false;

    public Tag[] Keys => _data.Keys.ToArray();
    public HashSet<Tag> KeysSet => _data.Keys.ToHashSet();

    /// <summary>
    ///     enumerates the tag entries in this map as pairs of Tag -> value.
    /// </summary>
    public IEnumerable<KeyValuePair<Tag, object?>> GetEntries() => _data.ToArray();

    public TagMap() {
        _data = new Dictionary<Tag, object?>();
    }

    public TagMap(HashSet<Tag> tags) {
        _data = new Dictionary<Tag, object?>();
        foreach (Tag tag in tags) _data.Add(tag, null);
    }

    /// <summary>
    ///     set the value for a specific untyped tag
    /// </summary>
    /// <param name="tag"></param>
    /// <param name="value">dummy param, not used. will always be replaced with null</param>
    /// <remarks>will throw if TagMap <see cref="IsLocked"/> is true</remarks>
    private void Add(Tag tag, object? value) {
        if (IsLocked)
            throw new InvalidOperationException("TagMap is locked");
        _data[tag] = null;
    }

    /// <summary>
    ///     set the value for a specific tag
    /// </summary>
    /// <param name="tag"></param>
    /// <param name="value"></param>
    /// <typeparam name="T"></typeparam>
    /// <remarks>will throw if TagMap <see cref="IsLocked"/> is true</remarks>
    public void Add<T>(Tag<T> tag, T? value) {
        if (IsLocked)
            throw new InvalidOperationException("TagMap is locked");
        _data[tag] = value!;
    }

    /// <summary>
    ///     sets a specific untyped tag. data value set is always null
    /// </summary>
    /// <param name="tag"></param>
    /// <remarks>will throw if TagMap <see cref="IsLocked"/> is true</remarks>
    public void Add(Tag tag) {
        if (IsLocked)
            throw new InvalidOperationException("TagMap is locked");
        // Disallow typed Tag<T> instances here; only allow exactly Tag
        if (tag.GetType() != typeof(Tag))
            throw new ArgumentException("this tag is typed, and must include a value", nameof(tag));
        _data[tag] = null;
    }

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
    /// <param name="value">the data value, or default if not found</param>
    /// <typeparam name="T"></typeparam>
    /// <returns>true if the value was found</returns>
    public bool GetValueSafe<T>(Tag<T> tag, out T value) {
        if (_data.TryGetValue(tag, out object? obj)) {
            value = (T)obj!; // because the tag is typed (because the whole point of the function is to grab a value), this shouldn't ever be null
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
    /// <remarks>will throw if TagMap <see cref="IsLocked"/> is true</remarks>
    public void AddRange(TagMap tagMap, bool throwException = false) {
        if (IsLocked)
            throw new InvalidOperationException("TagMap is locked");
        var thisKeys = Keys;
        foreach (Tag tag in tagMap.Keys) {
            if (throwException && thisKeys.Contains(tag))
                throw new ArgumentException($"this TagMap already contains key {tag}");
            Add(tag, tagMap._data[tag]);
        }
    }

    /// <summary>
    ///     gets the delta between two TagMaps.
    ///     "additions" will contain tags present in otherMap but not in the original map.
    ///     "removals" will contain tags present in the original map but not in otherMap.
    /// </summary>
    /// <param name="otherMap"></param>
    /// <param name="additions">output TagMap containing added tags</param>
    /// <param name="removals">output TagMap containing removed tags</param>
    public void GetDelta(TagMap otherMap, out TagMap additions, out TagMap removals) {
        additions = new TagMap();
        removals = new TagMap();

        // tags present in otherMap but not in oldMap
        foreach (Tag tag in otherMap.Keys)
            if (!HasTag(tag))
                // preserve associated value if any
                additions._data[tag] = otherMap._data[tag];

        // tags present in oldMap but not in otherMap
        foreach (Tag tag in Keys)
            if (!otherMap.HasTag(tag))
                removals._data[tag] = _data[tag];
    }

    /// <summary>
    ///     internal method to check this map's tags against mutually exclusive tag sets
    /// </summary>
    /// <param name="exclusiveSets"></param>
    /// <exception cref="Exception"></exception>
    private void ValidateTagSet(HashSet<Tag>[] exclusiveSets) {
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
    public void ValidateRequiredTags() {
        ValidateTagSet(ExclusiveTagsRequired);
    }

    /// <summary>
    ///     throws error if any tags are mutually exclusive in the TagsCurrent category
    /// </summary>
    public void ValidateCurrentTags() {
        ValidateTagSet(ExclusiveTagsCurrent);
    }
}