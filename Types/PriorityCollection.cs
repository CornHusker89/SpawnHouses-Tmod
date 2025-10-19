#nullable enable
using System.Collections.Generic;
using System.Linq;

namespace SpawnHouses.Types;

/// <summary>
///     contains multiple hashsets, sorted based on priority. intended for use when determining priority splits with the bsp algorithm.
///     has a blocklist feature to exclude specific items
/// </summary>
public class PriorityCollection<T> {
    private readonly Dictionary<int, HashSet<T>> _blocklistedItems = new();
    private readonly Dictionary<int, HashSet<T>> _sets = new();

    /// <summary>
    ///     the total number of items within all sets
    /// </summary>
    /// <remarks>respects blocklisted items</remarks>
    public int TotalLength => _sets.Keys.Sum(priority => GetHashSet(priority).Count);
    
    /// <summary>
    ///     the number of sets in the collection
    /// </summary>
    /// <remarks>excludes sets that are empty due to blocklisted items</remarks>
    public int SetsLength => _sets.Keys.Count(priority => GetHashSet(priority).Count != 0);

    /// <summary>
    ///     retrieves the hashset of the given priority
    /// </summary>
    /// <param name="priority"></param>
    /// <remarks>respects item blocklists</remarks>
    /// <returns></returns>
    public HashSet<T> GetHashSet(int priority) {
        if (!_blocklistedItems.TryGetValue(priority, out var value)) {
            value = [];
            _blocklistedItems[priority] = value;
        }
        
        return _sets[priority].Except(value).ToHashSet();
    }

    /// <summary>
    ///     gets an array of valid priorities
    /// </summary>
    /// <remarks>gives in ascending order, respects blocklist</remarks>
    public int[] GetValidPriorities() {
        return _sets.Keys.Where(priority => GetHashSet(priority).Count != 0)
            .OrderBy(priority => priority)
            .ToArray();
    }

    /// <summary>
    ///     sets the hashset at the given priority
    /// </summary>
    /// <param name="set"></param>
    /// <param name="priority"></param>
    public void SetHashSet(HashSet<T> set, int priority) {
        _sets[priority] = set;
    }

    /// <summary>
    ///     removes the hashset at the given priority
    /// </summary>
    /// <param name="priority"></param>
    public bool RemoveHashSet(int priority) {
        return _sets.Remove(priority);
    }

    /// <summary>
    ///     adds an item to the hashset at the given priority
    /// </summary>
    /// <param name="item"></param>
    /// <param name="priority"></param>
    /// <returns></returns>
    public bool AddItem(T item, int priority) {
        if (!_sets.TryGetValue(priority, out var set)) {
            _sets.Add(priority, [item]);
            return true;
        }

        return set.Add(item);
    }

    /// <summary>
    ///     removes a specific item, at the lowest priority at which it is found
    /// </summary>
    /// <param name="item"></param>
    /// <returns></returns>
    public bool RemoveItem(T item) {
        var sortedKeys = _sets.Keys.ToList();
        sortedKeys.Sort();
        foreach (int key in sortedKeys)
            if (_sets[key].Remove(item)) {
                if (_sets.Count == 0) // make sure there's not an empty set
                    _sets.Remove(key);

                return true;
            }

        return false;
    }

    /// <summary>
    ///     removes a specific item at the given priority
    /// </summary>
    /// <param name="item"></param>
    /// <param name="priority"></param>
    /// <returns></returns>
    public bool RemoveItem(T item, int priority) {
        if (!_sets.TryGetValue(priority, out var set)) return false;

        bool result = set.Remove(item);
        if (set.Count == 0) // make sure there's not an empty set
            _sets.Remove(priority);

        return result;
    }

    /// <summary>
    ///     adds an item to the blocklist, at the lowest priority it is found in the normal sets where the value is not already blocklisted
    /// </summary>
    /// <param name="item"></param>
    /// <returns>returns false if item is not found in the normal sets</returns>
    public bool AddToBlocklist(T item) {
        foreach (int priority in GetValidPriorities().Where(key => _sets[key].Contains(item))) {
            if (!_blocklistedItems.TryGetValue(priority, out var set))
                _blocklistedItems.Add(priority, [item]);
            else
                set.Add(item);
            return true;
        }

        return false;
    }

    /// <summary>
    ///     adds an item at a specific priority the blocklist
    /// </summary>
    /// <param name="priority"></param>
    /// <param name="item"></param>
    public bool AddToBlocklist(int priority, T item) {
        if (!_blocklistedItems.TryGetValue(priority, out var set)) {
            _blocklistedItems.Add(priority, [item]);
            return true;
        }

        return set.Add(item);
    }

    /// <summary>
    ///     removes all items from the blocklist
    /// </summary>
    public void ClearBlocklist() {
        _blocklistedItems.Clear();
    }

    /// <summary>
    ///     creates an array of all contained hashsets, in ascending priority order.
    /// </summary>
    /// <returns></returns>
    public (int priority, HashSet<T> set)[] ToSortedHashSetArray() {
        int[] priorities = GetValidPriorities();
        var result = new (int priority, HashSet<T> set)[priorities.Length];
        int count = 0;
        foreach (int key in priorities) {
            var set = GetHashSet(key);
            if (set.Count == 0) continue;
            result[count] = (key, set);
            count++;
        }

        return result;
    }

    /// <summary>
    ///     creates an array of all contained items, in ascending priority order
    /// </summary>
    /// <returns></returns>
    public T[] ToSortedArray() {
        var result = new T[TotalLength];
        var sets = ToSortedHashSetArray();
        int count = 0;
        foreach (var tuple in sets)
        foreach (T item in tuple.set) {
            result[count] = item;
            count++;
        }

        return result;
    }
}