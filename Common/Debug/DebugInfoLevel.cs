namespace SpawnHouses.Common.Debug;

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

    public override string ToString() => _value.ToString();

    public string GetDetailedString() => $"value: {_value}, Bounds: {DisplayBounds}, Points: {DisplayPoints}, DisplayName: {DisplayName}, " +
                                         $"DisplayType: {DisplayType}, DisplayId: {DisplayId}, DisplayGenerator: {DisplayGenerator}";

    /// <summary>
    ///     progresses through all possible options (adds 1 to internal value)
    /// </summary>
    public void Cycle() {
        _value++;
        if (_value >= 64)
            _value = 0;
    }

    public void Clear() => _value = 0;
    
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
    
    //     namespace SpawnHouses.Common.Types;
    //
    // public enum DebugInfoTypes : byte {
    //     Hitboxes = 1,
    //     Corners = 2,
    //     ComponentType = 4,
    //     Names = 8
    // }
    //
    // public class DebugInfoLevel {
    //     private int value;
    //
    //     public DebugInfoLevel(int value) {
    //         this.value = value;
    //     }
    //
    //     public DebugInfoLevel() {
    //         value = 0;
    //     }
    //
    //     public bool HasHitboxes => (value & 1) == 1;
    //     public bool HasCorners => (value & 2) == 2;
    //     public bool HasComponentType => (value & 4) == 4;
    //     public bool HasNames => (value & 8) == 8;
    //
    //     /// <summary>
    //     ///     progresses through the options (adds 1 to internal value)
    //     /// </summary>
    //     public void Cycle() {
    //         value++;
    //         if (value > 15)
    //             value = 0;
    //     }
    //
    //     public void Clear() => value = 0;
    //     public void Add(DebugInfoTypes debugType) => value |= (byte)debugType;
    //     public void RemoveHitBoxes(DebugInfoTypes debugType) => value &= ~(byte)debugType;
    // }
}