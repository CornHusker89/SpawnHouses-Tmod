namespace SpawnHouses.Content.Types.Enums;

/// <summary>
///     where a structure's entry points' are placed to match their target positions
/// </summary>
public enum EntryPointPositionAnchor {
    /// <summary>
    ///     neither entry point will be exactly on-target, but both will be close
    /// </summary>
    Neutral,

    /// <summary>
    ///     target point 1 will have an entry point directly on it
    /// </summary>
    Point1,

    /// <summary>
    ///     target point 2 will have na entry point directly on it
    /// </summary>
    Point2
}