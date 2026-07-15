namespace SpawnHouses.Common.DataStructures;

public readonly struct NumRange {
    public readonly int Min;
    public readonly int Max;

    public NumRange(int min, int max) {
        Min = min;
        Max = max;
    }
}