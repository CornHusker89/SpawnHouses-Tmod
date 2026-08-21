namespace SpawnHouses.Core.Debug;

public class DebugInfoLevel {
    private int _value;

    public DebugInfoLevel(int value) {
        _value = value;
    }

    public DebugInfoLevel() {
        _value = 0;
    }

    public bool DisplayBounds => (_value & 1) == 1;
    public bool DisplayPoints => (_value & 2) == 2;
    public bool DisplayName => (_value & 4) == 4;
    public bool DisplayType => (_value & 8) == 8;
    public bool DisplayId => (_value & 16) == 16;
    public bool DisplayGenerator => (_value & 32) == 32;

    public bool IsDisplayingText => _value > 3;

    public DebugInfoLevel Clone() => new(_value);

    public override string ToString() => _value.ToString();

    public string GetDetailedString() => $"value: {_value}, Bounds: {DisplayBounds}, Points: {DisplayPoints}, DisplayName: {DisplayName}, " +
                                         $"DisplayType: {DisplayType}, DisplayId: {DisplayId}, DisplayGenerator: {DisplayGenerator}";

    public void AddBounds() => _value |= 1;
    public void AddPoints() => _value |= 2;
    public void AddName() => _value |= 4;
    public void AddType() => _value |= 8;
    public void AddId() => _value |= 16;
    public void AddGenerator() => _value |= 32;

    public void RemoveBounds() => _value &= ~1;
    public void RemovePoints() => _value &= ~2;
    public void RemoveName() => _value &= ~4;
    public void RemoveType() => _value &= ~8;
    public void RemoveId() => _value &= ~16;
    public void RemoveGenerator() => _value &= ~32;

    /// <summary>
    ///     progresses through all possible options (adds 1 to internal value)
    /// </summary>
    public void Cycle() {
        _value++;
        if (_value >= 64)
            _value = 0;
    }

    /// <summary>
    ///     enables next display type, disables all if every display type is enabled
    /// </summary>
    public void EnableNext() {
        _value = _value switch {
            < 1 => 1,
            < 2 => 3,
            < 4 => 7,
            < 8 => 15,
            < 16 => 31,
            < 32 => 63,
            _ => 0
        };
    }

    public void Clear() => _value = 0;
}