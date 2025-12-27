#nullable enable

namespace SpawnHouses.Types.TagTypes;

public interface ITagSystem {
     /// <summary>
     ///     tags that are required for this object to exist
     /// </summary>
     public TagMap TagsRequired { get; }

     /// <summary>
     ///     tags currently applicable to this object
     /// </summary>
     public TagMap TagsCurrent { get; }
}

// public interface ITagSystem {
//     public TagSystem<Enum> Tags { get; init; }
// }
//
// public interface ITagSystem<TTag> : ITagSystem
//     where TTag : Enum {
//     public new TagSystem<TTag> Tags { get; init; }
// }
//
// public sealed class TagSystem<TTag>
//     where TTag : Enum {
//     private static HashSet<TTag>[] ExclusiveTagPartialSets;
//
//     static TagSystem() {
//         if (typeof(TTag) == typeof(StructureTag)) {
//             ExclusiveTagPartialSets = [
//             ];
//         }
//         else if (typeof(TTag) == typeof(ComponentTag)) {
//             ExclusiveTagPartialSets = [
//             ];
//         }
//         else
//             throw new ArgumentException($"{typeof(TTag).Name} is not a supported tag type");
//     }
//
//     public readonly TagMap TagsCurrent = new();
//     public readonly TagMap TagsRequired = new();
//
//     public static HashSet<TTag> NewPartialTagSet(HashSet<TTag> tagPartialSet) => tagPartialSet;
//
//     public static HashSet<TTag> NewPartialTagSet(HashSet<TTag> tagPartialSet, params HashSet<TTag>[] otherTagSets) {
//         foreach (var otherTagSet in otherTagSets) tagPartialSet.UnionWith(otherTagSet);
//         return tagPartialSet;
//     }
//
//     public void AddRequiredTag(TTag tag) => TagsRequired[tag] = null;
//
//     public void AddRequiredTag<TData>(TTag tag, TData tagData) => TagsRequired[tag] = tagData;
//
//     public void AddCurrentTag(TTag tag) => TagsCurrent[tag] = null;
//
//     public void AddCurrentTag<TData>(TTag tag, TData tagData) => TagsCurrent[tag] = tagData;
//
//     /// <summary>
//     ///     "appends" tag set onto another
//     /// </summary>
//     /// <param name="tagMap"></param>
//     /// <param name="addingTagMap"></param>
//     /// <param name="throwException">if true, will throw with duplicate tags. otherwise will just overwrite</param>
//     private static void AddTags(TagMap tagMap, TagMap addingTagMap, bool throwException) {
//         foreach (TTag addingTag in addingTagMap.Keys) {
//             if (throwException && tagMap.ContainsKey(addingTag))
//                 throw new Exception("duplicate tag: " + addingTag);
//
//             tagMap[addingTag] = addingTagMap[addingTag];
//         }
//     }
//
//     public void AddRequiredTags(HashSet<TTag> tagPartialSet) => AddTags(TagsRequired, tagPartialSet, false);
//
//     public void AddRequiredTagsUnsafe(HashSet<TTag> tagPartialSet) => AddTags(TagsRequired, tagPartialSet, false);
//
//     public void AddCurrentTags(HashSet<TTag> tagPartialSet) => AddTags(TagsCurrent, tagPartialSet, false);
//
//     public void AddCurrentTagsUnsafe(HashSet<TTag> tagPartialSet) => AddTags(TagsCurrent, tagPartialSet, false);
//
//     /// <summary>
//     ///     "appends" tag set onto another
//     /// </summary>
//     /// <param name="tagSet"></param>
//     /// <param name="addingTagPartialSet"></param>
//     /// <param name="throwException">if true, will throw with duplicate tags. otherwise will just overwrite</param>
//     private static void AddTags(typedtagdictplaceholder tagSet, HashSet<TTag> addingTagPartialSet, bool throwException) {
//         foreach (TTag addingTag in addingTagPartialSet) {
//             if (throwException && tagSet.ContainsKey(addingTag))
//                 throw new Exception("duplicate tag: " + addingTag);
//
//             tagSet[addingTag] = null;
//         }
//     }
//
//     public void AddRequiredTags(typedtagdictplaceholder tagSet) => AddTags(TagsRequired, tagSet, false);
//
//     public void AddRequiredTagsUnsafe(typedtagdictplaceholder tagSet) => AddTags(TagsRequired, tagSet, false);
//
//     public void AddCurrentTags(typedtagdictplaceholder tagSet) => AddTags(TagsCurrent, tagSet, false);
//
//     public void AddCurrentTagsUnsafe(typedtagdictplaceholder tagSet) => AddTags(TagsCurrent, tagSet, false);
//
//     /// <summary>
//     ///     gets tag data from component's tag. throws when tag doesn't exist, has no data, or when cast fails.
//     ///     for safe version see <see cref="GetDataSafe{T}" />
//     /// </summary>
//     /// <param name="targetTag"></param>
//     /// <param name="tagSet"></param>
//     /// <typeparam name="TData"></typeparam>
//     /// <returns></returns>
//     /// <exception cref="Exception"></exception>
//     private static TData GetData<TData>(TTag targetTag, typedtagdictplaceholder tagSet) {
//         if (!tagSet.TryGetValue(targetTag, out object? value)) throw new ArgumentException("targetTag not found within given tag list");
//
//         if (value == null) throw new Exception($"Target tag \"{targetTag}\" found but had no associated data");
//         if (value is not TData typedValue) throw new Exception($"tag data for {targetTag} could not be cast to target type of \"{typeof(TTag).Name}\"");
//         return typedValue;
//     }
//
//     /// <summary>
//     ///     gets tag data from component's tag. throws when tag doesn't exist, has no data, or when cast fails.
//     ///     for safe version see <see cref="GetTagRequiredDataSafe{T}" />
//     /// </summary>
//     /// <param name="targetTag"></param>
//     /// <typeparam name="TData"></typeparam>
//     /// <returns></returns>
//     public TData GetTagRequiredData<TData>(TTag targetTag) => GetData<TData>(targetTag, TagsRequired);
//
//     /// <summary>
//     ///     gets tag data from component's tag. throws when tag doesn't exist, has no data, or when cast fails.
//     ///     for safe version see <see cref="GetTagCurrentDataSafe{T}" />
//     /// </summary>
//     /// <param name="targetTag"></param>
//     /// <typeparam name="TData"></typeparam>
//     /// <returns></returns>
//     public TData GetTagCurrentData<TData>(TTag targetTag) => GetData<TData>(targetTag, TagsCurrent);
//
//     /// <summary>
//     ///     gets tag data from component's tag. returns default (recommended to use nullable types) when tag isn't found, but throws if tag is found but has no data or if cast fails.
//     ///     for unsafe version see <see cref="GetData{T}" />
//     /// </summary>
//     /// <param name="targetTag"></param>
//     /// <param name="tagSet"></param>
//     /// <typeparam name="TData"></typeparam>
//     /// <returns></returns>
//     /// <exception cref="Exception"></exception>
//     private static TData? GetDataSafe<TData>(TTag targetTag, typedtagdictplaceholder tagSet) {
//         if (!tagSet.TryGetValue(targetTag, out object? value)) return default;
//
//         if (value == null) throw new Exception($"Target tag \"{targetTag}\" found but had no associated data");
//         if (value is not TData typedValue) throw new ArgumentException($"tag data for {targetTag} could not be cast to target type of \"{typeof(TTag).Name}\"");
//         return typedValue;
//     }
//
//     /// <summary>
//     ///     gets tag data from component's tag. returns default (recommended to use nullable types) when tag isn't found, but throws if tag is found but has no data or if cast fails.
//     ///     for unsafe version see <see cref="GetTagRequiredData{T}" />
//     /// </summary>
//     /// <param name="targetTag"></param>
//     /// <typeparam name="TData"></typeparam>
//     /// <returns></returns>
//     public TData? GetTagRequiredDataSafe<TData>(TTag targetTag) => GetDataSafe<TData?>(targetTag, TagsRequired);
//
//     /// <summary>
//     ///     gets tag data from component's tag. returns default (recommended to use nullable types) when tag isn't found, but throws if tag is found but has no data or if cast fails.
//     ///     for unsafe version see <see cref="GetTagRequiredData{T}" />
//     /// </summary>
//     /// <param name="targetTag"></param>
//     /// <typeparam name="TData"></typeparam>
//     /// <returns></returns>
//     public TData? GetTagCurrentDataSafe<TData>(TTag targetTag) => GetDataSafe<TData?>(targetTag, TagsCurrent);
//
//     /// <summary>
//     ///     ensures tag set does not have any mutually exclusive tags. throws if it does
//     /// </summary>
//     /// <param name="tagPartialSet"></param>
//     /// <exception cref="Exception"></exception>
//     private void ValidateExclusiveTags(HashSet<TTag> tagPartialSet) {
//         foreach (var exclusiveSet in ExclusiveTagPartialSets) {
//             if (!exclusiveSet.IsSubsetOf(tagPartialSet)) continue;
//
//             string message = "Component has mutually exclusive component tags required {";
//             foreach (TTag tag in exclusiveSet) message += $"{tag}), ";
//             throw new Exception(message.Remove(message.Length - 2));
//         }
//     }
//
//     /// <summary>
//     ///     ensures that there are no tags (in either required or current) that are mutually exclusive or have incorrect data types. throws if fails validation
//     /// </summary>
//     public void ValidateTags() {
//         ValidateExclusiveTags(TagsRequired.Keys.ToHashSet());
//         ValidateExclusiveTags(TagsCurrent.Keys.ToHashSet());
//     }
// }