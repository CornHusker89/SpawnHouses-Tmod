namespace SpawnHouses.AdvStructures.AdvStructureParts;

public class Floor {
    public bool IsExterior;
    public Shape Volume;

    public Floor(Shape volume, bool isExterior = false) {
        Volume = volume;
        IsExterior = isExterior;
    }
}