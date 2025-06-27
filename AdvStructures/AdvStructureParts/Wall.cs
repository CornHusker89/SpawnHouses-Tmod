namespace SpawnHouses.AdvStructures.AdvStructureParts;

public class Wall {
    public bool IsExterior;
    public Shape Volume;

    public Wall(Shape volume, bool isExterior = false) {
        Volume = volume;
        IsExterior = isExterior;
    }
}