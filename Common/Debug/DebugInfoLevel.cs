namespace SpawnHouses.Common.Debug;

public class DebugInfoLevel {
    private int value;

    public DebugInfoLevel(int value) {
        this.value = value;
    }

    public DebugInfoLevel() {
        value = 0;
    }

    public bool HasHitboxes => (value & 1) == 1;
    public bool HasCorners => (value & 2) == 2;
    public bool HasComponentType => (value & 4) == 4;
    public bool HasNames => (value & 8) == 8;

    /// <summary>
    ///     progresses through the options (adds 1 to internal value)
    /// </summary>
    public void Cycle() {
        value++;
        if (value > 15)
            value = 0;
    }

    public void Clear() => value = 0;
    public void AddHitBoxes() => value |= 1;
    public void AddCorners() => value |= 2;
    public void AddComponentType() => value |= 4;
    public void AddNames() => value |= 8;
    public void RemoveHitBoxes() => value &= ~1;
    public void RemoveCorners() => value &= ~2;
    public void RemoveComponentType() => value &= ~4;
    public void RemoveNames() => value &= ~8;

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