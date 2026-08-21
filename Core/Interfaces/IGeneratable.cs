namespace SpawnHouses.Core.Interfaces;

/// <summary>
///     the root of any instance or sub-instance of a structure
/// </summary>
public interface IGeneratable {
    /// <summary>
    ///     unique number given to each instance in the world. automatically assigned on instance creation,
    /// </summary>
    public ushort Id { get; }
}