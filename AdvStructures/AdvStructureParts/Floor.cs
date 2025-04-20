namespace SpawnHouses.AdvStructures.AdvStructureParts;

public class Floor {
    public Shape Volume;
    public bool IsExterior;

    public Floor(Shape volume, bool isExterior = false) {
        Volume = volume;
        IsExterior = isExterior;
    }
}