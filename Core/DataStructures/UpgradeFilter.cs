namespace SpawnHouses.Core.DataStructures;

/// <summary>
///     represents the parameters for upgrading a structure
/// </summary>
public class UpgradeParameters {
    private int _value;

    public bool MustHaveMoreHousing => (_value & 1) == 1;
    public bool MustHaveMs => (_value & 2) == 2;
    public bool MustHaveSameEntryPoints => (_value & 4) == 4;

    /// <summary>
    ///     initializes a new instance with the default values
    /// </summary>
    public UpgradeParameters() {
        ResetToDefault();
    }

    public UpgradeParameters(bool mustHaveMoreHousing, bool mustHaveMs, bool mustHaveSameEntryPoints) {
        if (mustHaveMoreHousing) AddMustHaveMoreHousing();
        if (mustHaveMs) AddMustHaveMs();
        if (mustHaveSameEntryPoints) AddMustHaveSameEntryPoints();
    }

    public void AddMustHaveMoreHousing() => _value |= 1;
    public void AddMustHaveMs() => _value |= 2;
    public void AddMustHaveSameEntryPoints() => _value |= 4;

    public void RemoveMustHaveMoreHousing() => _value &= ~1;
    public void RemoveMustHaveMs() => _value &= ~2;
    public void RemoveMustHaveSameEntryPoints() => _value &= ~4;

    public void ResetToDefault() {
        Clear();
        AddMustHaveMoreHousing();
    }

    public void Clear() => _value = 0;
}